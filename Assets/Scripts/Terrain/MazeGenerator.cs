#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class MazeGenerator : MonoBehaviour
{
    public MazeNode node; // nodes to render

    public int nodeSize = 1; // distance between the walls of the node
    public int mazeSize = 10; // always square

    public bool useSeed = false;
    public int seed = 123;

    [HideInInspector]
    public HashSet<GameObject> walls; // store the walls locations, hashset to avoid duplicates
    private MazeNode[] sideNodesEntrance;
    private MazeNode[] sideNodesExit;
    [HideInInspector]
    public HashSet<GameObject> peripheral;
    [HideInInspector]
    public HashSet<GameObject> insideMountains;

    [HideInInspector]
    public Vector3 playerSpawn;

    // to clear the maze from the editor
    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
        {
            ClearOldMaze();
        }
#endif
    }

    void Awake()
    {
        ClearOldMaze();
        // init seed
        if (useSeed)
        {
            seed = Random.Range(0, 1_000);
        }

        Random.InitState(seed);
        Debug.Log($"Maze seed = {seed}");

        // get grid
        MazeNode[][] allNodes = RenderGrid(mazeSize);
        Backtracking(allNodes);

        PerlinTerrain terrain = FindObjectOfType<PerlinTerrain>();
        playerSpawn = CarveEntrance();
        terrain.GenerateTerrain();
    }

    private MazeNode[][] RenderGrid(int size)
    {
        MazeNode[][] allNodes = new MazeNode[size][];
        sideNodesEntrance = new MazeNode[size];
        sideNodesExit = new MazeNode[size];
        peripheral = new HashSet<GameObject>();
        insideMountains = new HashSet<GameObject>();
        int i = 0;
        int j = 0;
        // random perlin area
        int minBand = Mathf.Max(2, size / 9);
        int maxBand = Mathf.Max(2, size / 5 + 2);
        int bandX = Random.Range(minBand, maxBand);
        int bandZ = Random.Range(minBand, maxBand);

        // pick start positions so that start + band <= size
        int randXStart = Random.Range(0, size - bandX);
        int randXEnd = randXStart + bandX;
        int randZStart = Random.Range(0, size - bandZ);
        int randZEnd = randZStart + bandZ;
        for (int x = 0; x < size; x++)
        {
            allNodes[x] = new MazeNode[size];
            for (int z = 0; z < size; z++)
            {
                Vector3 localPos = new Vector3(x * nodeSize, 0f, z * nodeSize);
                MazeNode cell = Instantiate(node, transform);
                cell.transform.localPosition = localPos;
                cell.transform.localRotation = Quaternion.identity;

                allNodes[x][z] = cell;
                cell.MarkUnvisited();
                cell.gridX = x;
                cell.gridZ = z;
                if (x > randXStart && x < randXEnd)
                {
                    if (z > randZStart && z < randZEnd)
                    {
                        AddActiveWalls(cell, insideMountains);
                    }
                }
                if (z == 0) // entrance
                {
                    sideNodesEntrance[i] = cell;
                    i++;
                }
                else if (z == size - 1) // exit
                {
                    sideNodesExit[j] = cell;
                    j++;
                }
                if (z == 0 || z == size - 1 || x == 0 || x == size - 1)
                {
                    GameObject wall = null;
                    if (z == 0)
                        wall = cell.SouthWall;
                    else if (z == size - 1)
                        wall = cell.NorthWall;
                    else if (x == 0)
                        wall = cell.WestWall;
                    else if (x == size - 1)
                        wall = cell.EastWall;
                    if (wall != null)
                        peripheral.Add(wall);
                }
            }
        }
        return allNodes;
    }

    private void Backtracking(MazeNode[][] locationsList)
    {
        Stack<MazeNode> stack = new Stack<MazeNode>();
        // start at random point
        int x = Random.Range(0, mazeSize);
        int z = Random.Range(0, mazeSize);

        MazeNode currentNode = locationsList[x][z];
        stack.Push(currentNode);
        currentNode.MarkVisited();
        walls = new HashSet<GameObject>();
        while (stack.Count > 0)
        { // while there are unvisited nodes
            MazeNode nextNode = GetUnvisitedNeighbour(currentNode, locationsList);

            if (nextNode == null)
            { // no neighbours = backtracking
                stack.Pop();
                if (stack.Count == 0)
                {
                    // nothing left to process
                    break;
                }
                currentNode = stack.Peek(); // update currentNode
                continue;
            }
            // if there is a neighbour
            ClearWalls(currentNode, nextNode); // remove the extra walls
            // move to the next node
            nextNode.MarkVisited();
            currentNode = nextNode;
            stack.Push(currentNode);
        }
    }

    private void ClearOldMaze()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject old = transform.GetChild(i).gameObject;
#if UNITY_EDITOR
            DestroyImmediate(old);
#else
            Destroy(old);
#endif
        }
    }

    private void ClearWalls(MazeNode current, MazeNode next)
    {
        int dx = next.gridX - current.gridX;
        int dz = next.gridZ - current.gridZ;

        // east 
        if (dx == 1)
        {
            current.RemoveWall(current.EastWall);
            next.RemoveWall(next.WestWall);
        }
        // west
        else if (dx == -1)
        {
            current.RemoveWall(current.WestWall);
            next.RemoveWall(next.EastWall);
        }

        // north
        else if (dz == 1)
        {
            current.RemoveWall(current.NorthWall);
            next.RemoveWall(next.SouthWall);
        }
        // south
        else if (dz == -1)
        {
            current.RemoveWall(current.SouthWall);
            next.RemoveWall(next.NorthWall);
        }

        AddActiveWalls(current, walls);
        AddActiveWalls(next, walls);
    }

    // take the remaining walls that are not cleared and add them to walls locations list
    private void AddActiveWalls(MazeNode node, HashSet<GameObject> walls)
    {
        if (node.EastWall.activeSelf)
        {
            walls.Add(node.EastWall);
        }
        if (node.WestWall.activeSelf)
        {
            walls.Add(node.WestWall);
        }
        if (node.NorthWall.activeSelf)
        {
            walls.Add(node.NorthWall);
        }
        if (node.SouthWall.activeSelf)
        {
            walls.Add(node.SouthWall);
        }
    }

    private MazeNode GetUnvisitedNeighbour(MazeNode node, MazeNode[][] allNodes)
    {
        int x = node.gridX;
        int z = node.gridZ;

        List<MazeNode> neighbours = new();

        // east
        if (x + 1 < mazeSize)
        {
            var neighbour = allNodes[x + 1][z];
            if (!neighbour.isVisited) neighbours.Add(neighbour);
        }

        // west
        if (x - 1 >= 0)
        {
            var neighbour = allNodes[x - 1][z];
            if (!neighbour.isVisited) neighbours.Add(neighbour);
        }

        // north
        if (z + 1 < mazeSize)
        {
            var neighbour = allNodes[x][z + 1];
            if (!neighbour.isVisited) neighbours.Add(neighbour);
        }

        // south
        if (z - 1 >= 0)
        {
            var neighbour = allNodes[x][z - 1];
            if (!neighbour.isVisited) neighbours.Add(neighbour);
        }

        if (neighbours.Count == 0) return null;
        return neighbours[Random.Range(0, neighbours.Count)];
    }

    // clear one random wall for exit and one for entrance
    private Vector3 CarveEntrance()
    {
        int randEntrance = Random.Range(1, mazeSize - 1);
        MazeNode entrance = sideNodesEntrance[randEntrance];

        var mr = entrance.SouthWall.GetComponent<Renderer>();
        if (mr != null)
            mr.enabled = false;

        entrance.RemoveWall(entrance.SouthWall);
        peripheral.Remove(entrance.SouthWall);
        walls.Remove(entrance.SouthWall);

        Vector3 cellCenter = entrance.transform.position;
        Vector3 outsidePoint = cellCenter + new Vector3(0, 0, -nodeSize);
        Debug.Log(outsidePoint);
        return outsidePoint;
    }

}
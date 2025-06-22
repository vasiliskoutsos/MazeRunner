using UnityEngine;

public class MazeNode : MonoBehaviour
{
    public GameObject NorthWall;
    public GameObject SouthWall;
    public GameObject EastWall;
    public GameObject WestWall;
    public GameObject unvisitedCube; // this cube exists if the node is unvisited
    [HideInInspector]
    public bool isVisited = false;
    [HideInInspector]
    public int gridX;
    [HideInInspector]
    public int gridZ;

    public void RemoveWall(GameObject wall)
    {
        if (wall != null)
            wall.SetActive(false);
        else
            Debug.Log("Null wall");
    }

    public void MarkVisited()
    {
        isVisited = true;
        if (unvisitedCube != null)
            unvisitedCube.SetActive(false);
        else
            Debug.Log("Null cube");
    }

    public void MarkUnvisited()
    {
        isVisited = false;
        if (unvisitedCube != null)
            unvisitedCube.SetActive(true);
        else
            Debug.Log("Null cube");
    }
}
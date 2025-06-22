using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnRule
{
    public GameObject prefab;
    public float correctHeight;
}

public class Spawner : MonoBehaviour
{
    public PerlinTerrain perlinTerrain;
    public SpawnRule[] rules;
    private Terrain terrain;
    private Dictionary<GameObject, Vector2Int> prefabHalfCellSize = new Dictionary<GameObject, Vector2Int>();

    void Awake()
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
            return;

        var data = terrain.terrainData;

        if (rules == null || rules.Length == 0)
            return;

        foreach (var rule in rules)
        {
            if (rule.prefab == null)
                continue;

            var temp = Instantiate(rule.prefab, Vector3.zero, Quaternion.identity);
            Collider coll = temp.GetComponent<Collider>();

            if (coll == null)
                Debug.Log($"{rule.prefab.name} has no Collider to size ");

            int res = data.heightmapResolution;
            Vector3 terrainSize = data.size;
            float cellSize = terrainSize.x / (res - 1);

            if (cellSize <= 0) {
                Debug.Log($"spawner invalid cellSize {cellSize}. terrain size x {terrainSize.x}, resolution {res}");
                Destroy(temp);
                continue;
            }

            Vector3 halfSize = coll != null ? coll.bounds.extents : Vector3.zero;
            int halfCellsX = Mathf.CeilToInt(halfSize.x / cellSize);
            int halfCellsZ = Mathf.CeilToInt(halfSize.z / cellSize);

            Debug.Log($"spawner prefab {rule.prefab.name} world halfSize {halfSize}, cell size {cellSize}, halfCells {halfCellsX}, {halfCellsZ}");

            int shrink = 50; // shrink buffer
            halfCellsX = Mathf.Max(0, halfCellsX - shrink);
            halfCellsZ = Mathf.Max(0, halfCellsZ - shrink);

            prefabHalfCellSize[rule.prefab] = new Vector2Int(halfCellsX, halfCellsZ);
            
            Destroy(temp);
        }
    }

    void Start()
    {
        SpawnAll();
    }

    void SpawnAll()
    {
        terrain = Terrain.activeTerrain;
        var data = terrain.terrainData;
        var pos = terrain.transform.position;
        int res = data.heightmapResolution;
        Vector3 size = data.size;

        HashSet<Vector2Int> wallsToHills = perlinTerrain.wallsToHills;
        foreach (Vector2Int coord in perlinTerrain.peripheralHills)
        {
            if (wallsToHills.Contains(coord)) wallsToHills.Remove(coord);
        }
        HashSet<Vector2Int> availableRealEstate = new HashSet<Vector2Int>(wallsToHills);
        List<Vector2Int> cells = new List<Vector2Int>(availableRealEstate);

        if (cells.Count == 0)
            return;

        int spawned = 0; // counter for spawned
        float spacing = 2.5f; // buffer for spacing
        foreach (var square in cells)
        {
            if (!availableRealEstate.Contains(square))
                continue;

            if (square.x % spacing != 0 || square.y % spacing != 0)
                continue;

            List<SpawnRule> candidates = new List<SpawnRule>();
            foreach (var rule in rules)
            {
                candidates.Add(rule);
            }

            if (candidates.Count == 0)
                continue;

            SpawnRule chosen = candidates[Random.Range(0, candidates.Count)];

            if (!prefabHalfCellSize.TryGetValue(chosen.prefab, out Vector2Int halfCells))
            {
                Debug.Log($"spawner could not find prefab size for {chosen.prefab.name} in prefabHalfCellSize dictionary skipping rock for square {square}");
                continue;
            }

            bool canSpawnHere = true;
            if (halfCells.x < 0 || halfCells.y < 0)
            {
                Debug.Log($"spawner invalid negative halfCells for {chosen.prefab.name}: {halfCells}");
                canSpawnHere = false;
            }

            for (int dx = -halfCells.x; dx <= halfCells.x; dx++)
            {
                for (int dz = -halfCells.y; dz <= halfCells.y; dz++)
                {
                    Vector2Int cell = new Vector2Int(square.x + dx, square.y + dz);
                    if (cell.x < 0 || cell.x >= res || cell.y < 0 || cell.y >= res)
                    {
                        canSpawnHere = false;
                        break;
                    }
                }
                if (!canSpawnHere)
                    break;
            }

            if (!canSpawnHere)
                continue;

            float y = chosen.correctHeight;
            Vector3 worldPos = new Vector3(square.x * size.x / (res - 1) + (size.x / (res - 1) * 0.5f), y, square.y * size.z / (res - 1) + (size.z / (res - 1) * 0.5f)) + pos;
            //float spawnYOffset = -0.5f;
            //float terrainY = terrain.SampleHeight(worldPos);
            //worldPos.y = terrainY + spawnYOffset;

            GameObject house = Instantiate(chosen.prefab, worldPos, Quaternion.identity, transform);
            spawned++;

            Collider coll = house.GetComponent<Collider>();
            if (coll != null)
            {
                float bottomY = coll.bounds.min.y;
                float worldY = worldPos.y;
                if (bottomY < worldY)
                    house.transform.position += Vector3.up * (worldY - bottomY);
            }

            int buffer = 25; // extra cells to remove
            int removeX = halfCells.x + buffer;
            int removeZ = halfCells.y + buffer;

            // carve out both footprint + buffer
            for (int dx = -removeX; dx <= removeX; dx++)
            {
                for (int dz = -removeZ; dz <= removeZ; dz++)
                {
                    var cellToRemove = new Vector2Int(square.x + dx, square.y + dz);
                    availableRealEstate.Remove(cellToRemove);
                }
            }
        }
        Debug.Log($"Total houses spawned: {spawned}");
        if (spawned == 0 && cells.Count > 0)
        {
            Debug.Log("spawner processed all cells but no houses spawned");
        }
    }
}
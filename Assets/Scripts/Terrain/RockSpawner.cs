using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RockRule
{
    public GameObject prefab;
}

public class RockSpawner : MonoBehaviour
{
    public PerlinTerrain perlinTerrain;
    public RockRule[] rules;
    Terrain terrain;
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
                Debug.LogWarning($"{rule.prefab.name} has no collider to size from");

            int res = data.heightmapResolution;
            Vector3 terrainSize = data.size;
            float cellSize = terrainSize.x / (res - 1);

            if (cellSize <= 0) {
                Destroy(temp);
                continue;
            }

            Vector3 halfSize = coll != null ? coll.bounds.extents : Vector3.zero;
            int halfCellsX = Mathf.CeilToInt(halfSize.x / cellSize);
            int halfCellsZ = Mathf.CeilToInt(halfSize.z / cellSize);

            int shrink = 44; // shrink buffer
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

        if (perlinTerrain.peripheralHills == null) {
            Debug.LogError("rock spawner peripheralHills null");
            return;
        }

        HashSet<Vector2Int> availableRealEstate = new HashSet<Vector2Int>(perlinTerrain.peripheralHills);
        List<Vector2Int> cells = new List<Vector2Int>(availableRealEstate);

        if (cells.Count == 0) 
            return;

        int rocksSpawned = 0; // Counter for spawned rocks
        float spacing = 2.5f; // buffer for spacing
        foreach (var square in cells)
        {
            if (!availableRealEstate.Contains(square))
                continue;

            if (square.x % spacing != 0 || square.y % spacing != 0)
                continue;

            int res = data.heightmapResolution;
            Vector3 size = data.size;

            List<RockRule> candidates = new List<RockRule>();
            foreach (var rule in rules)
            {
                candidates.Add(rule);
            }

            if (candidates.Count == 0)
                continue;

            RockRule chosen = candidates[Random.Range(0, candidates.Count)];
            
            if (!prefabHalfCellSize.TryGetValue(chosen.prefab, out Vector2Int halfCells)) {
                Debug.Log($"rock spawner could not find prefab size for {chosen.prefab.name} in prefabHalfCellSize dictionary, skipping rock for square {square}.");
                continue;
            }

            bool canSpawnHere = true;
            if (halfCells.x < 0 || halfCells.y < 0) {
                Debug.Log($"rock spawner invalid negative half cells for {chosen.prefab.name}: {halfCells}");
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
                if (!canSpawnHere) break;
            }

            if (!canSpawnHere)
                continue;

            Vector3 worldPos = new Vector3(square.x * size.x / (res - 1) + (size.x / (res - 1) * 0.5f), 0, square.y * size.z / (res - 1) + (size.z / (res - 1) * 0.5f)) + pos;
            float spawnYOffset = -0.5f; // spawn rocks more down
            float terrainY = terrain.SampleHeight(worldPos);
            worldPos.y = terrainY + spawnYOffset;

            GameObject rock = Instantiate(chosen.prefab, worldPos, Quaternion.identity, transform);
            rocksSpawned++;

            Collider coll = rock.GetComponent<Collider>();
            if (coll != null)
            {
                float bottomY = coll.bounds.min.y;
                float worldY = worldPos.y;
                if (bottomY < worldY)
                    rock.transform.position += Vector3.up * (worldY - bottomY);
            }
            
            int buffer = 15; // extra cells to remove
            int removeX = halfCells.x + buffer;
            int removeZ = halfCells.y + buffer;

            // carve out both footprint and buffer
            for (int dx = -removeX; dx <= removeX; dx++)
            {
                for (int dz = -removeZ; dz <= removeZ; dz++)
                {
                    var cellToRemove = new Vector2Int(square.x + dx, square.y + dz);
                    availableRealEstate.Remove(cellToRemove);
                }
            }
        }
        Debug.Log($"Total rocks spawned: {rocksSpawned}");
        if (rocksSpawned == 0 && cells.Count > 0) {
            Debug.Log("rock spawner spawned zero rocks");
        }
    }
}
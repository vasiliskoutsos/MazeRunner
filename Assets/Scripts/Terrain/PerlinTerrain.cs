using System.Collections.Generic;
using UnityEngine;

public class PerlinTerrain : MonoBehaviour
{
    public float[,] terrainHeight;
    public MazeGenerator maze;
    
    Terrain terrain;
    public float refinement = 0.01f;
    public float multiplier = 0.8f;
    public int width = 5;
    public float height = 0.0005f; // hill height
    private float randomOffsetX;
    private float randomOffsetZ;
    [HideInInspector]
    public HashSet<Vector2Int> wallsToHills;

    // peripheral
    public float refinementPeripheral = 0.024f;
    public float multiplierPeripheral = 2.5f;
    public int widthPeripheral = 5;
    public float heightPeripheral = 0.0028f; // hill height
    private float randomPeripheralOffsetX;
    private float randomPeripheralOffsetZ;
    [HideInInspector]
    public HashSet<Vector2Int> peripheralHills;

    public void GenerateTerrain()
    {
        terrain = GetComponent<Terrain>();
        wallsToHills = new HashSet<Vector2Int>();
        peripheralHills = new HashSet<Vector2Int>();
        int resolution = terrain.terrainData.heightmapResolution;
        TerrainData data = terrain.terrainData;
        // convert world space wall pos to object space coords on the terrain
        foreach (GameObject wall in maze.walls)
        {
            if (wall == null || !wall.activeInHierarchy)
                continue;

            // get world space aabb of the wall
            Renderer[] allChildRenderers = wall.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in allChildRenderers)
            {
                // skip inactive walls
                if (!rend.gameObject.activeInHierarchy)
                    continue;
                // skip if renderer is disabled
                if (!rend.enabled)
                    continue;
                    
                Bounds b = rend.bounds; // corners of a walls bounding box 

                // bmin bmax = opposite corners of wall
                // from global pos to terrain object pos
                float terrainWidth = data.size.x;
                Vector3 min = (b.min - terrain.transform.position - maze.transform.position) * terrainWidth / maze.mazeSize;
                Vector3 max = (b.max - terrain.transform.position - maze.transform.position) * terrainWidth / maze.mazeSize;

                // normalize to get the percentage of wall to terrain size
                float normXMin = Mathf.Clamp01(min.x / data.size.x);
                float normZMin = Mathf.Clamp01(min.z / data.size.z);
                float normXMax = Mathf.Clamp01(max.x / data.size.x);
                float normZMax = Mathf.Clamp01(max.z / data.size.z);

                // terrain grid is resolution x resolution
                // we convert to norm to the size of the terrain
                float offset = 10f;
                int xMin = Mathf.RoundToInt(normXMin * (resolution - 1) + offset);
                int zMin = Mathf.RoundToInt(normZMin * (resolution - 1) + offset);
                int xMax = Mathf.RoundToInt(normXMax * (resolution - 1) + offset);
                int zMax = Mathf.RoundToInt(normZMax * (resolution - 1) + offset);

                // make sure the indexes are valid
                xMin = Mathf.Clamp(xMin, 0, resolution - 1);
                zMin = Mathf.Clamp(zMin, 0, resolution - 1);
                xMax = Mathf.Clamp(xMax, 0, resolution - 1);
                zMax = Mathf.Clamp(zMax, 0, resolution - 1);

                // add every cell within rectangle
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        Vector2Int cell = new Vector2Int(x, z);
                        wallsToHills.Add(cell);
                        if (maze.peripheral.Contains(wall))
                            peripheralHills.Add(cell);
                    }
                }
            }
        }

        // perlin noise
        randomOffsetX = Random.Range(1, 500);
        randomOffsetZ = Random.Range(1, 500);
        randomPeripheralOffsetX = Random.Range(1, 500);
        randomPeripheralOffsetZ = Random.Range(1, 500);
        terrainHeight = new float[resolution, resolution];
        foreach (var cell in wallsToHills)
        {
            int i = cell.x;
            int j = cell.y;
            for (int a = i - width; a < i + width; a++)
            {
                for (int b = j - width; b < j + width; b++)
                {
                    if (a < 0 || b < 0 || a >= resolution || b >= resolution) continue;
                    float dx = a - i;
                    float dz = b - j;
                    float dist = Mathf.Sqrt(dx * dx + dz * dz);
                    float radius = width;
                    float norm = dist / radius;
                    if (norm > 1f) continue;
                    float falloff = 1f - Mathf.SmoothStep(0f, 1f, norm);
                    float perlinNoise = 0f;
                    float contrib = 0;

                    if (peripheralHills.Contains(cell)) // make the peripheral hills
                    {
                        perlinNoise = Mathf.PerlinNoise((a + randomPeripheralOffsetX) * refinementPeripheral, (b + randomPeripheralOffsetZ) * refinementPeripheral);
                        contrib = heightPeripheral * perlinNoise * multiplierPeripheral * falloff;
                    }
                    else
                    {
                        perlinNoise = Mathf.PerlinNoise((a + randomOffsetX) * refinement, (b + randomOffsetZ) * refinement);
                        contrib = height * perlinNoise * multiplier * falloff;
                    }

                    terrainHeight[a, b] = Mathf.Max(terrainHeight[a, b], contrib);
                }
            }
        }
        data.SetHeights(0, 0, terrainHeight);
        // set textures according to height
        int w = data.alphamapWidth, h = data.alphamapHeight;
        float[,,] alpha = new float[h, w, 2];
        float threshold = 0.0002f; // height threshold
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                float nx = x / (float)(w - 1);
                float ny = y / (float)(h - 1);
                float ht = data.GetInterpolatedHeight(nx, ny) / data.size.y;
                alpha[y, x, 0] = ht < threshold ? 1f : 0f; // low height texture
                alpha[y, x, 1] = ht >= threshold ? 1f : 0f; // high height texture
            }
        }
        data.SetAlphamaps(0, 0, alpha);
    }
}
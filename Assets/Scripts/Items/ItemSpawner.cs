using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject healthKatana;
    public GameObject speedKatana;
    public GameObject cameraKatana;

    public float checkRadius = 0.5f;
    public int totalSpawns = 1; // total spawn for each item
    public float xShiftAmount = 1f;
    public PerlinTerrain perlinTerrain;

    private Terrain terrain;
    private GameObject[] items;

    void Start()
    {
        items = new GameObject[] {speedKatana, healthKatana, cameraKatana};
        terrain = Terrain.activeTerrain;
        if (terrain == null)
            Debug.Log("No terrain found");
        int maxAttempts = 100;
        foreach (GameObject item in items)
        {
            for (int i = 0; i < totalSpawns; i++)
            {
                Vector3 spawnPos = GetRandomPoint(terrain);
                int attempts = 0;
                while (attempts < maxAttempts)
                {
                    Collider[] hits = Physics.OverlapSphere(spawnPos, checkRadius);
                    if (hits.Length == 0)
                        break;
                    //spawnPos.x += xShiftAmount;
                    spawnPos.y = terrain.SampleHeight(new Vector3(spawnPos.x, 0, spawnPos.z)) + terrain.transform.position.y + 0.6f;
                    attempts++;
                }
                if (attempts >= maxAttempts)
                    Debug.LogWarning("spawnitem: couldnt find a free spot after " + maxAttempts + " attempts");
                Instantiate(item, spawnPos, Quaternion.identity);
            }
        }
    }

    private Vector3 GetRandomPoint(Terrain terrain)
    {
        // define world space spawn bounds
        float minXZ = 4f;
        float maxXZ = 16f;

        float randomX = Random.Range(minXZ, maxXZ);
        float randomZ = Random.Range(minXZ, maxXZ);

        // sample the terrain height at that point
        float y = terrain.SampleHeight(new Vector3(randomX, 0f, randomZ));

        return new Vector3(randomX, y, randomZ);
    }

}
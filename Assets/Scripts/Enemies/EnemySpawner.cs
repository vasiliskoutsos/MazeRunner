using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public float checkRadius = 0.5f;
    public int totalEnemies = 3;
    public float xShiftAmount = 1f;
    public PerlinTerrain perlinTerrain;
    private Terrain terrain;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
            Debug.Log("No terrain found");
        int maxAttempts = 100;
        for (int i = 0; i < totalEnemies; i++)
        {
            Vector3 spawnPos = GetRandomPoint(terrain);
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                Collider[] hits = Physics.OverlapSphere(spawnPos, checkRadius);
                if (hits.Length == 0)
                    break;
                //spawnPos.x += xShiftAmount;
                spawnPos.y = terrain.SampleHeight(new Vector3(spawnPos.x,0,spawnPos.z)) + terrain.transform.position.y;
                attempts++;
            }
            if (attempts >= maxAttempts)
                Debug.Log("spawnenemy couldnt find free spot after " + maxAttempts + " attempts");
            Instantiate(enemy, spawnPos, Quaternion.identity);
        }
    }

    private Vector3 GetRandomPoint(Terrain terrain)
    {
        // define world space spawn bounds
        float minXZ = 0f;
        float maxXZ = 16f;

        float randomX = Random.Range(minXZ, maxXZ);
        float randomZ = Random.Range(minXZ, maxXZ);

        // sample terrain height at that point
        float y = terrain.SampleHeight(new Vector3(randomX, 0f, randomZ));

        return new Vector3(randomX, y - 0.3f, randomZ);
    }
}
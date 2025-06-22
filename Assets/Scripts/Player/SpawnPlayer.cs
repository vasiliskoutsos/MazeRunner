using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject player;
    public MazeGenerator maze;

    // area to check for already there are objects
    public float checkRadius = 0.5f;

    // how much to shift on x axis when there is overlap
    public float xShiftAmount = 1f;

    void Start()
    {
        Vector3 spawnPos = maze.playerSpawn;
        
        float zOffset = 15f + 5f;
        spawnPos.z += zOffset;
        spawnPos.y += 3f;

        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Collider[] hits = Physics.OverlapSphere(spawnPos, checkRadius); // check overlap
            if (hits.Length == 0) // no overlaps player can spawn
                break;

            // otherwise shift x and try again
            spawnPos.x += xShiftAmount;
            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning("spawn player couldnt find a free spot after " + maxAttempts + " attempts");

        Instantiate(player, spawnPos, Quaternion.identity);
    }
}
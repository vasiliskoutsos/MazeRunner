using UnityEngine;

public class ExitSpawn : MonoBehaviour
{
    public GameObject exit;
    public MazeGenerator maze;

    // area to check for already there are objects
    public float checkRadius = 0.5f;

    void Start()
    {
        Vector3 spawnPos = maze.playerSpawn;
        
        float zOffset = 30f;
        spawnPos.z += zOffset;
        spawnPos.x = Random.Range(3, 15);

        int maxAttempts = 100;
        int attempts = 0;

        if (attempts >= maxAttempts)
            Debug.LogWarning("spawn exit couldnt find a free spot after " + maxAttempts + " attempts");

        Instantiate(exit, spawnPos, Quaternion.identity);
    }
}
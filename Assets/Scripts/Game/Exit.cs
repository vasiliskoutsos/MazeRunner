using UnityEngine;

public class Exit: MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        LevelManager.Instance.MazeCompleted();
    }
}

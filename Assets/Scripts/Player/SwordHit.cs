using UnityEngine;

public class SwordHit : MonoBehaviour
{
    private float destroyDelay = 1f;
    private PlayerMovement player;

    void Start()
    {
        Transform top = transform.root;

        player = top.GetComponent<PlayerMovement>();
        if (player == null)
            Debug.Log("no player movement");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && player.isAttacking)
            Destroy(other.gameObject, destroyDelay);
    }
}
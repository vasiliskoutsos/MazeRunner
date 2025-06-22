using UnityEngine;
using System.Collections;

public class GetHealth : PickupItem
{
    public float amount = 40f;
    public float duration = 5f;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {

            PlayerStats stats = collision.collider.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.Heal(amount);
                ToggleImage("Health");
                DisablePickup();
                StartCoroutine(WaitToggleImage(duration));
            }
            else
                Debug.LogWarning($"stats not found on '{collision.collider.name}'.");
        }
    }

    private void DisablePickup()
    {
        // stop the update from moving it
        enabled = false;

        // hide all renderers in object
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // turn off collider so it cant be triggered again
        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }
    
    private IEnumerator WaitToggleImage(float duration)
    {
        yield return new WaitForSeconds(duration);
        ToggleImage("Health");
        // destroy the item
        Destroy(gameObject);
    }
}

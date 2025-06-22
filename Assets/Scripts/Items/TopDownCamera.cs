using UnityEngine;

public class TopDownCamera : PickupItem
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            MouseLook camMove = collision.collider.transform.root.GetComponentInChildren<MouseLook>();
            if (camMove != null)
            {
                camMove.topEnabled = true;
                ToggleImage("Camera");
            }
            else
                Debug.LogWarning("not found Mouse look");

            // destroy the item
            Destroy(gameObject);
        }
    }
}

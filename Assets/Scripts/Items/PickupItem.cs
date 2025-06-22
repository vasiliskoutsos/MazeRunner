using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public float speed = 2f;
    public string playerTag = "Player";

    private GameObject poweupImage;
    private Transform playerTransform;
    private bool isComing = true;

    void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform imageTransform = canvas.transform.Find("Power Ups");
            if (imageTransform != null)
                poweupImage = imageTransform.gameObject;
            else
                Debug.LogWarning("could not find power ups");
        }
        else
            Debug.LogWarning("canvas not found in scene");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerTransform = other.transform;
            isComing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isComing = false;
            playerTransform = null;
        }
    }

    // if is active move toward player current position
    private void Update()
    {
        if (isComing && playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * (speed * Time.deltaTime);
        }
    }

    public void ToggleImage(string imageName)
    {
        if (poweupImage == null)
        {
            Debug.LogWarning("poweupimage root not assigned");
            return;
        }

        Transform child = poweupImage.transform.Find(imageName);
        if (child == null)
        {
            Debug.LogWarning($" '{imageName}' not found under {poweupImage.name}");
            return;
        }

        bool isActive = child.gameObject.activeSelf;
        child.gameObject.SetActive(!isActive);
        Debug.Log($"{imageName} is now {(!isActive ? "shown" : "hidden")}");
    }
}
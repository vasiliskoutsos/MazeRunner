using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float sensitivityX = 100f;
    public float sensitivityY = 50f;
    public float maxUpDownAngle = 80f;

    public Camera cam; // normal camera
    public Camera topDownCam; // top down camera
    public Transform playerBody;

    [HideInInspector]
    public bool topEnabled = false; // if top down camera is enabled the power up has been consumed
    private float xRotation = 0f;
    private bool isTop = false; // is top currenltly activated

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // initial camera positions
        if (cam != null)
        {
            cam.enabled = true;
            cam.GetComponent<AudioListener>().enabled = true;
        }
        if (topDownCam != null)
        {
            topDownCam.enabled = false;
            topDownCam.GetComponent<AudioListener>().enabled = false;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        playerBody.Rotate(Vector3.up * mouseX);

        if (!isTop)
        {
            float mouseY = Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxUpDownAngle, maxUpDownAngle);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.Q) && topEnabled && !isTop)
            {
                TopDownView();
                isTop = true;
            }
            else if (Input.GetKeyDown(KeyCode.Q) && topEnabled && isTop)
            {
                NormalView();
                isTop = false;
            }
    }

    public void TopDownView()
    {
        if (cam != null)
        {
            cam.enabled = false;
            cam.GetComponent<AudioListener>().enabled = false;
        }
        if (topDownCam != null)
        {
            topDownCam.enabled = true;
            topDownCam.GetComponent<AudioListener>().enabled = true;
        }

        // move the camera 
        topDownCam.transform.SetPositionAndRotation(playerBody.position + Vector3.up * 10f, Quaternion.Euler(90f, playerBody.eulerAngles.y, 0f));
    }
    
    public void NormalView()
    {
        if (topDownCam != null) topDownCam.enabled = false;
        if (cam != null) cam.enabled = true;
    }

}

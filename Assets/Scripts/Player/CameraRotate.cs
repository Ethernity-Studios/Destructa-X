using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public float MouseSensitivity;

    public Transform orientation;

    private float xRotation;
    private float yRotation;

    private bool isShopOpen;

    public PlayerEconomyManager PlayerEconomyManager;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (PlayerEconomyManager == null) return;
        if (orientation == null) return;
        isShopOpen = PlayerEconomyManager.IsShopOpen;
        if (isShopOpen) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * MouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * MouseSensitivity;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0,yRotation,0);
    }
}

using System;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public float MouseSensitivity;

    public Transform orientation;
    public Transform body;

    private float xRotation;
    private float yRotation;

    private bool isShopOpen;

    public PlayerEconomyManager PlayerEconomyManager;

    private PlayerInput playerInput;
    private Vector2 mouseLook;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

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

        mouseLook = playerInput.PlayerMovement.Look.ReadValue<Vector2>();
        float mouseX = mouseLook.x * MouseSensitivity * Time.deltaTime;
        float mouseY = mouseLook.y * MouseSensitivity * Time.deltaTime;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0,yRotation,0);
        body.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void RotateCamera(Vector3 rotation)
    {
        yRotation = rotation.y;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x,rotation.y,transform.eulerAngles.z);
        orientation.rotation = Quaternion.Euler(transform.eulerAngles.x,rotation.y,transform.eulerAngles.z);
        body.rotation = Quaternion.Euler(transform.eulerAngles.x,rotation.y,transform.eulerAngles.z);
    }

    private void OnEnable()
    {
        playerInput.PlayerMovement.Enable();
    }

    private void OnDisable()
    {
        playerInput.PlayerMovement.Disable();
    }
}

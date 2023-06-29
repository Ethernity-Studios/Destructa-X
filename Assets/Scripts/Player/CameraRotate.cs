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
    private bool isMenuOpen;

    public PlayerEconomyManager PlayerEconomyManager;

    private PlayerInput playerInput;
    public Vector2 MouseLook;

    public bool CanRotate = true;
    public bool CanRotateBody = true;

    public bool Spectating;
    public Transform SpectatingTransform;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        if (!Spectating)
        {
            if (PlayerEconomyManager == null) return;
            if (orientation == null) return;
            isShopOpen = PlayerEconomyManager.IsShopOpen;
            isMenuOpen = PlayerEconomyManager.SettingsMenu.IsOpened;
            if (isShopOpen) return;
            if (isMenuOpen) return;
            if (!CanRotate) return;

            MouseLook = playerInput.PlayerMovement.Look.ReadValue<Vector2>();
            float mouseX = MouseLook.x * MouseSensitivity * Time.deltaTime;
            float mouseY = MouseLook.y * MouseSensitivity * Time.deltaTime;

            yRotation += mouseX;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.parent.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0,yRotation,0);
            if (!CanRotateBody) return;
            body.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        else
        {
            transform.parent.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }

    }

    public void RotateCamera(Vector3 rotation)
    {
        yRotation = rotation.y;
        transform.parent.rotation = Quaternion.Euler(transform.eulerAngles.x,rotation.y,transform.eulerAngles.z);
        orientation.rotation = Quaternion.Euler(transform.eulerAngles.x,rotation.y,transform.eulerAngles.z);
        if (!CanRotateBody) return;
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

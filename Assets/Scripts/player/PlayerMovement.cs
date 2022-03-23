using UnityEngine;
using Mirror;
using System;

public enum PlayerState
{
    Idle, Walk, Run, Crouch, Jump, Planting, Dead
}

public class PlayerMovement : NetworkBehaviour
{
    public float MouseSens = 2f;
    float xRotation = 0f;
    Transform cameraTransform;


    CharacterController characterController;

    public float RunSpeed = 7f;
    public float WalkSpeed = 1f;
    public float JumpForce = 1.3f;

    float gravityMultiplier = -30f;

    [SerializeField]Transform groundCheck;
    [SerializeField]float groundDistance = 0.4f;
    [SerializeField]LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;

    PlayerManager playerManager;
    void Start()
    {
        if (!isLocalPlayer) return;
        playerManager = GetComponent<PlayerManager>();
        playerManager.PlayerState = PlayerState.Idle;
        cameraTransform = Camera.main.transform;
        cameraTransform.SetParent(transform);
        cameraTransform.position = new Vector3(transform.position.x,transform.position.y + .6f, transform.position.z);
        Cursor.lockState = CursorLockMode.Locked;
        characterController = FindObjectOfType<CharacterController>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (playerManager.PlayerState == PlayerState.Dead) return;
        movePlayer();
        jump();
        crouch();
    }

    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        rotateCamera();
    }

    void rotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSens*100 * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSens*100 * Time.fixedDeltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void movePlayer()
    {
        if (playerManager.PlayerState == PlayerState.Planting) return;
        float speed = 7;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = WalkSpeed;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = RunSpeed;
        }

        characterController.Move(move * speed * Time.deltaTime);
    }

    void jump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(JumpForce * -2f * gravityMultiplier);
        }
        
        velocity.y += gravityMultiplier * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void crouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && playerManager.PlayerState != PlayerState.Jump)
        {
            
        }
    }

}

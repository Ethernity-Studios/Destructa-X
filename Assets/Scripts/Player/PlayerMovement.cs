using Mirror;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

enum MovementState
{
    Idle, Walking, Sprinting, Jumping
}

[SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] Transform playerHead;

    Player playerManager;
    Rigidbody rb;

    [SerializeField] MovementState state;

    [Header("Mouse sens")]
    public float MouseSens = 1.7f;
    float xRotation = 0f;

    [Header("Movement")]
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    [SerializeField] float sprintSpeed = 5f;
    [SerializeField] float walkSpeed = 3f;
    float moveSpeed;

    [Header("Jump")]
    [SerializeField] float groundDrag;
    [SerializeField] bool grounded;

    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;
    bool readyToJump = true;
    float jumpEnergy;

    GameManager gameManager;

    [SerializeField] float mouseX;
    [SerializeField] float mouseY;
    [SerializeField] float rotX;

    GameObject mainCamera;

    Vector3 cameraRotation;
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public override void OnStartLocalPlayer()
    {
        mainCamera = Camera.main.gameObject;
        mainCamera.transform.position = transform.position + new Vector3(0, .6f, 0);
        mainCamera.transform.eulerAngles = new Vector3(playerHead.rotation.x, transform.rotation.y, 0);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (isLocalPlayer)
        {
            playerManager = GetComponent<Player>();
            playerManager.PlayerState = PlayerState.Idle;
            rb.freezeRotation = true;
            //cameraTransform = Camera.main.transform;
            //cameraTransform.SetParent(transform.GetChild(0));
            //cameraTransform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }
    }


    private void Update()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
        getInput();
        moveCamera();
        rotateHead();
        stateHandler();


        grounded = Physics.Raycast(origin: transform.position, direction: Vector3.down, maxDistance: 1.2f/*, layerMask: groundMask*/);

        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
        if (playerManager.PlayerState == PlayerState.Planting) return;
        if (playerManager.PlayerState == PlayerState.Defusing) return;
        speedControl();
        movePlayer();


    }

    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
    }

    void stateHandler()
    {
        if (grounded && rb.velocity == Vector3.zero) state = MovementState.Idle;
        else if (grounded && Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = walkSpeed;
            state = MovementState.Walking;
        }
        else if (grounded)
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
        }
        else
        {
            state = MovementState.Jumping;
        }
    }

    void rotateHead()
    {
        if (GetComponent<PlayerEconomyManager>().IsShopOpen) return;

        rotX = Mathf.Clamp(rotX, -90f, 90f);
        mainCamera.transform.eulerAngles = new Vector3(-rotX, mainCamera.transform.eulerAngles.y + mouseX, 0);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerHead.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void moveCamera()
    {
        mainCamera.transform.position = transform.position + new Vector3(0, .6f, 0);
    }

    void getInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && readyToJump && grounded && playerManager.PlayerState != PlayerState.Planting && playerManager.PlayerState != PlayerState.Defusing)
        {
            readyToJump = false;
            jump();
            Invoke("resetJump", jumpCooldown);
        }

        mouseX = Input.GetAxis("Mouse X") * MouseSens;
        mouseY = Input.GetAxis("Mouse Y") * MouseSens;
        rotX += Input.GetAxis("Mouse Y") * MouseSens;
    }


    void movePlayer()
    {
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
    }

    private void speedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void resetJump()
    {
        readyToJump = true;
    }

}

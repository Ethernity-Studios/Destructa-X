using System;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    Idle,
    Walking,
    Sprinting,
    Jumping
}

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] Transform playerHead;

    Player playerManager;
    Rigidbody rb;

    public MovementState state;

    [Header("Movement")] float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    [SerializeField] float sprintSpeed = 5f;
    [SerializeField] float walkSpeed = 3f;
    float moveSpeed;

    [Header("Jump")] [SerializeField] float groundDrag;
    [SerializeField] bool grounded;

    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;
    bool readyToJump = true;
    float jumpEnergy;


    GameObject mainCamera;

    Vector3 cameraRotation;

    [SerializeField] private Transform orientation;

    [SerializeField] private Transform body;

    private PlayerInput playerInput;

    [SerializeField] private LayerMask groundMask;

    public override void OnStartLocalPlayer()
    {
        playerInput = new PlayerInput();
        
        playerInput.PlayerMovement.Jump.performed += Jump;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!isLocalPlayer) return;
        mainCamera = Camera.main!.gameObject;
        mainCamera.transform.parent.position = transform.position + new Vector3(0, .6f, 0);
        mainCamera.transform.parent.eulerAngles = new Vector3(playerHead.rotation.x, transform.rotation.y, 0);
        mainCamera.GetComponent<CameraRotate>().orientation = orientation;
        mainCamera.GetComponent<CameraRotate>().body = body;
        mainCamera.GetComponent<CameraRotate>().PlayerEconomyManager = GetComponent<PlayerEconomyManager>();
        mainCamera.transform.parent.GetComponent<CameraMove>().cameraPosition = playerHead;
        playerManager = GetComponent<Player>();
        playerManager.PlayerState = PlayerState.Idle;
        rb.freezeRotation = true;

        playerInput.PlayerMovement.Enable();
        //cameraTransform = Camera.main.transform;
        //cameraTransform.SetParent(transform.GetChild(0));
        //cameraTransform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }
    private void OnDisable()
    {
        playerInput.PlayerMovement.Disable();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
        getInput();
        stateHandler();
        rotatePlayer();

        grounded = Physics.Raycast(origin: transform.position + new Vector3(0,1,0), direction: Vector3.down,
            maxDistance: 1.2f , layerMask: groundMask);

        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }


    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
        if (playerManager.PlayerState is PlayerState.Planting or PlayerState.Defusing) return;

        speedControl();
        movePlayer();
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
        else state = MovementState.Jumping; 
    }

    void rotatePlayer()
    {
        playerHead.rotation = mainCamera.transform.rotation;
    }


    void getInput()
    {
        horizontalInput = playerInput.PlayerMovement.Movement.ReadValue<Vector2>().x;
        verticalInput = playerInput.PlayerMovement.Movement.ReadValue<Vector2>().y;
        //horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");
    }


    void movePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
    }

    private void speedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (!(flatVel.magnitude > moveSpeed)) return;
        Vector3 limitedVel = flatVel.normalized * moveSpeed;
        rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!readyToJump || !grounded || playerManager.PlayerState is PlayerState.Planting or PlayerState.Defusing) return;
        if (!context.performed) return;
        readyToJump = false;
        Invoke(nameof(resetJump), jumpCooldown);
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void resetJump()
    {
        readyToJump = true;
    }
}
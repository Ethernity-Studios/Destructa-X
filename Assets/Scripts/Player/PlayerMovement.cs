using Mirror;
using Unity.Mathematics;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!isLocalPlayer) return;
        mainCamera = Camera.main!.gameObject;
        mainCamera.transform.parent.position = transform.position + new Vector3(0, .6f, 0);
        mainCamera.transform.parent.eulerAngles = new Vector3(playerHead.rotation.x, transform.rotation.y, 0);
        mainCamera.GetComponent<CameraRotate>().orientation = orientation;
        mainCamera.GetComponent<CameraRotate>().PlayerEconomyManager = GetComponent<PlayerEconomyManager>();
        mainCamera.transform.parent.GetComponent<CameraMove>().cameraPosition = playerHead;
        playerManager = GetComponent<Player>();
        playerManager.PlayerState = PlayerState.Idle;
        rb.freezeRotation = true;
        //cameraTransform = Camera.main.transform;
        //cameraTransform.SetParent(transform.GetChild(0));
        //cameraTransform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }


    private void Update()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
        getInput();
        stateHandler();
        rotateHead();

        grounded = Physics.Raycast(origin: transform.position, direction: Vector3.down,
            maxDistance: 1.2f /*, layerMask: groundMask*/);

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
        switch (grounded)
        {
            case true when rb.velocity == Vector3.zero:
                state = MovementState.Idle;
                break;
            case true when Input.GetKey(KeyCode.LeftShift):
                moveSpeed = walkSpeed;
                state = MovementState.Walking;
                break;
            case true:
                state = MovementState.Sprinting;
                moveSpeed = sprintSpeed;
                break;
            default:
                state = MovementState.Jumping;
                break;
        }
    }

    void rotateHead()
    {
        playerHead.rotation = mainCamera.transform.rotation;
    }

    void getInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (!Input.GetKeyDown(KeyCode.Space) || !readyToJump || !grounded ||
            playerManager.PlayerState is PlayerState.Planting or PlayerState.Defusing) return;
        readyToJump = false;
        jump();
        Invoke(nameof(resetJump), jumpCooldown);
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
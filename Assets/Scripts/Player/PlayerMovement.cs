using Mirror;
using UnityEngine;

enum MovementState
{
    Idle, Walking, Sprinting, Jumping
}

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
        speedControl();
        getInput();
        stateHandler();

        grounded = Physics.Raycast(origin: transform.position, direction: Vector3.down, maxDistance: 1.2f/*, layerMask: groundMask*/);

        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;

        movePlayer();
    }

    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
        rotateHead();
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
        float mouseX = Input.GetAxis("Mouse X") * MouseSens * 100 * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSens * 100 * Time.fixedDeltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerHead.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void getInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && readyToJump && grounded)
        {
            readyToJump = false;
            jump();
            Invoke("resetJump", jumpCooldown);
        }
    }


    void movePlayer()
    {
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
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

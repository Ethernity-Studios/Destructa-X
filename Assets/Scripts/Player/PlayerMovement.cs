using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    Idle,
    Walking,
    Sprinting,
    InAir
}

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] Transform playerHead;

    Player playerManager;
    Rigidbody rb;
    [SerializeField]  Animator anim;

    public MovementState state;

    [Header("Movement")]
    float horizontalInput;
    float verticalInput;

    private float smoothX;
    private float smoothZ;
    [SerializeField] private float smoothFactor;

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
    
    private static readonly int VelocityX = Animator.StringToHash("VelocityX");
    private static readonly int VelocityZ = Animator.StringToHash("VelocityZ");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
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
        if (!isLocalPlayer) return;
        playerInput.PlayerMovement.Disable();
    }

    private void OnEnable()
    {
        if (!isLocalPlayer) return;
        playerInput.PlayerMovement.Enable();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
        getInput();
        stateHandler();
        rotatePlayer();
        setAnimVelocity(verticalInput, horizontalInput);
        
        grounded = Physics.Raycast(origin: transform.position + new Vector3(0,1,0), direction: Vector3.down,
            maxDistance: 1.2f , layerMask: groundMask);

        if (grounded)
        {
            rb.drag = groundDrag;
            anim.SetBool(IsGrounded, true);
        }
        else
        {
            Debug.Log("Not grounded");

            rb.drag = 0;
            anim.SetBool(IsGrounded, false);
            anim.SetBool(IsJumping, false);
            anim.SetBool(IsFalling, false);
        }
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
            case true when playerInput.PlayerMovement.Walking.IsPressed():
                moveSpeed = walkSpeed;
                state = MovementState.Walking;
                break;
            case true when !playerInput.PlayerMovement.Walking.IsPressed():
                state = MovementState.Sprinting;
                moveSpeed = sprintSpeed;
                break;
            case false:
                state = MovementState.InAir;
                break;
        }
    }

    void rotatePlayer()
    {
        playerHead.rotation = mainCamera.transform.rotation;
    }


    void getInput()
    {
        horizontalInput = playerInput.PlayerMovement.Movement.ReadValue<Vector2>().x;
        verticalInput = playerInput.PlayerMovement.Movement.ReadValue<Vector2>().y;
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
        animateJump();
    }

    void resetJump()
    {
        readyToJump = true;
    }

    #region Animations

    void setAnimVelocity(float x, float z)
    {
        
        smoothX = Mathf.Lerp(smoothX, x, smoothFactor);
        smoothZ = Mathf.Lerp(smoothZ, z, smoothFactor);
        if (state == MovementState.Walking)
        {
            anim.speed = 1;
            anim.SetFloat(VelocityX, smoothZ/2);
            anim.SetFloat(VelocityZ, smoothX/2);
        }
        else
        {
            anim.speed = .85f;
            anim.SetFloat(VelocityX, smoothZ);
            anim.SetFloat(VelocityZ, smoothX);
        }
        
        
        if(x != 0 || z != 0) anim.SetBool(IsMoving, true);
        else anim.SetBool(IsMoving, false);
    }

    void animateJump()
    {
        anim.SetBool(IsJumping, true);
        anim.SetBool(IsFalling, true);
    }

    void land()
    {
        Debug.Log("Landing");
        anim.SetBool(IsFalling, false);
    }
    
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 3) return;
        if (anim.GetBool(IsFalling)) land();
        Debug.Log("Landed");

    }
}
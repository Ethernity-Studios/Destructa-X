using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Side
{
    Right,
    Left
}

public enum MovementState
{
    Idle,
    Walking,
    Sprinting,
    InAir,
    Crouching,
    CrouchingIdle
}

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] Transform playerHead;

    Player playerManager;
    private PlayerShootingManager playerShootingManager;
    Rigidbody rb;
    [SerializeField] Animator anim;

    public MovementState state;

    [Header("Movement")] float horizontalInput;
    float verticalInput;

    private float smoothX;
    private float smoothZ;
    [SerializeField] private float movementSmoothFactor;

    [SerializeField] private float crouchSmoothFactor;

    [SerializeField] private float capsuleSmoothFactor;

    Vector3 moveDirection;

    [SerializeField] float sprintSpeed = 5f;
    [SerializeField] float walkSpeed = 3f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] float moveSpeed;

    [Header("Jump")] [SerializeField] float groundDrag;
    [SerializeField] bool grounded;
    [SerializeField] private bool crouching;

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

    [SerializeField] private CapsuleCollider capsuleCollider;
    private float targetHeight;
    private float targetCrouch;

    private static readonly int VelocityX = Animator.StringToHash("VelocityX");
    private static readonly int VelocityZ = Animator.StringToHash("VelocityZ");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int IsCrouching = Animator.StringToHash("isCrouching");
    private static readonly int Crouch = Animator.StringToHash("Crouch");
 
    private CameraRotate cameraRotate;

    private Vector3 currentBodyRotationY;
    [SerializeField] private float bodyRotationSmoothFactor;
    private bool canRotateBody = false;

    public override void OnStartLocalPlayer()
    {
        playerInput = new PlayerInput();

        playerInput.PlayerMovement.Jump.performed += jump;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerShootingManager = GetComponent<PlayerShootingManager>();
        if (!isLocalPlayer) return;
        mainCamera = Camera.main!.gameObject;
        mainCamera.transform.parent.position = transform.position + new Vector3(0, .6f, 0);
        mainCamera.transform.parent.eulerAngles = new Vector3(playerHead.rotation.x, transform.rotation.y, 0);
        cameraRotate = mainCamera.GetComponent<CameraRotate>();
        cameraRotate.orientation = orientation;
        cameraRotate.body = body;
        cameraRotate.PlayerEconomyManager = GetComponent<PlayerEconomyManager>();
        mainCamera.transform.parent.GetComponent<CameraMove>().cameraPosition = playerHead;
        playerManager = GetComponent<Player>();
        playerManager.PlayerState = PlayerState.Idle;
        rb.freezeRotation = true;

        playerInput.PlayerMovement.Enable();

        Invoke(nameof(setBodyRotate), 3f);
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
        crouch();
        checkRotation();

        setAnimVelocity(verticalInput, horizontalInput);

        if (grounded)
        {
            rb.drag = groundDrag;
            anim.SetBool(IsGrounded, true);
            anim.SetBool(IsFalling, false);
        }
        else
        {
            rb.drag = 0;
            anim.SetBool(IsGrounded, true);
            anim.SetBool(IsJumping, false);
            anim.SetBool(IsFalling, true);
        }
    }


    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerManager.IsDead) return;
        if (playerManager.PlayerState is PlayerState.Planting or PlayerState.Defusing) return;

        grounded = Physics.Raycast(origin: transform.position + new Vector3(0, 1, 0), direction: Vector3.down, maxDistance: 1.2f, layerMask: groundMask);

        speedControl();
        movePlayer();
    }

    void setBodyRotate() => canRotateBody = true;

    void stateHandler()
    {
        switch (grounded)
        {
            case true when rb.velocity == Vector3.zero && !crouching:
                state = MovementState.Idle;
                cameraRotate.CanRotateBody = false;
                playerShootingManager.BloomModifier = 0;
                break;
            case true when playerInput.PlayerMovement.Walking.IsPressed() && !crouching:
                moveSpeed = walkSpeed;
                state = MovementState.Walking;
                cameraRotate.CanRotateBody = true;
                playerShootingManager.BloomModifier = 60;
                break;
            case true when !playerInput.PlayerMovement.Walking.IsPressed() && rb.velocity != Vector3.zero && !crouching:
                state = MovementState.Sprinting;
                moveSpeed = sprintSpeed;
                cameraRotate.CanRotateBody = true;
                playerShootingManager.BloomModifier = 80;
                break;
            case true when crouching && rb.velocity == Vector3.zero:
                state = MovementState.CrouchingIdle;
                cameraRotate.CanRotateBody = false;
                playerShootingManager.BloomModifier = -10;
                break;
            case true when crouching && rb.velocity != Vector3.zero:
                state = MovementState.Crouching;
                moveSpeed = crouchSpeed;
                cameraRotate.CanRotateBody = true;
                playerShootingManager.BloomModifier = 45;
                break;
            case false:
                state = MovementState.InAir;
                cameraRotate.CanRotateBody = true;
                playerShootingManager.BloomModifier = 120;
                break;
        }
    }

    void rotatePlayer()
    {
        playerHead.rotation = mainCamera.transform.rotation;
    }

    void checkRotation()
    {
        if (state != MovementState.Idle && state != MovementState.CrouchingIdle) return;
        if (!canRotateBody) return;

        currentBodyRotationY.y = cameraRotate.transform.rotation.eulerAngles.y;

        float smoothness = Mathf.Abs(cameraRotate.MouseLook.x) / bodyRotationSmoothFactor ;
        body.transform.rotation = Quaternion.Lerp(body.transform.rotation, Quaternion.Euler(currentBodyRotationY),smoothness);
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

    void crouch()
    {
        if (playerInput.PlayerMovement.Crouching.IsPressed())
        {
            crouching = true;
            anim.SetBool(IsCrouching, true);
            targetHeight = 1f;
            targetCrouch = 1;
        }
        else
        {
            crouching = false;
            anim.SetBool(IsCrouching, false);
            targetHeight = 2f;
            targetCrouch = 0;
        }


        anim.SetFloat(Crouch, Mathf.Lerp(anim.GetFloat(Crouch), targetCrouch, crouchSmoothFactor));
        capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, targetHeight, capsuleSmoothFactor);
    }

    void jump(InputAction.CallbackContext context)
    {
        if (!readyToJump || !grounded ||
            playerManager.PlayerState is PlayerState.Planting or PlayerState.Defusing) return;
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
        smoothX = Mathf.Lerp(smoothX, x, movementSmoothFactor);
        smoothZ = Mathf.Lerp(smoothZ, z, movementSmoothFactor);
        if (state == MovementState.Walking)
        {
            anim.speed = 1;
            anim.SetFloat(VelocityX, smoothZ / 2);
            anim.SetFloat(VelocityZ, smoothX / 2);
        }
        else
        {
            anim.speed = .85f;
            anim.SetFloat(VelocityX, smoothZ);
            anim.SetFloat(VelocityZ, smoothX);
        }


        if (x != 0 || z != 0) anim.SetBool(IsMoving, true);
        else anim.SetBool(IsMoving, false);
    }

    void animateJump()
    {
        anim.SetBool(IsJumping, true);
        anim.SetBool(IsFalling, true);
    }

    #endregion
}
using Mirror;
using UnityEngine;

public class PlayerMovementTest : NetworkBehaviour
{
    public float sens = 400f;
    public float speed = 8f;
    public float jumpf = 1f;
    public float gravity = 0.1f;

    [SerializeField] private Camera camyr;
    [SerializeField] private Transform ground_t;
    [SerializeField] private CharacterController controller;
    [SerializeField] private LayerMask ground;
    [SerializeField] private bool onground;
    [SerializeField] private Vector3 velocity;

    private float cameraY;

    private void Start()
    {
        if (!isLocalPlayer) return;
        camyr = Camera.main;
        camyr.gameObject.transform.parent = transform;
        camyr.transform.position = new Vector3(transform.position.x, transform.position.y + .6f, transform.position.z);
        Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        move();
        jump();
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;
        rotate();
    }

    private void rotate()
    {
        var mousex = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        var mousey = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        cameraY -= mousey;

        cameraY = Mathf.Clamp(cameraY, -90f, 90f);
        camyr.transform.localRotation = Quaternion.Euler(cameraY, 0, 0);
        transform.Rotate(Vector3.up * mousex);
    }

    private void move()
    {
        var lol = Input.GetAxis("Horizontal");
        var xd = Input.GetAxis("Vertical");
        var sus = speed;
        if (Input.GetKey(KeyCode.LeftShift)) sus *= 1.5f;

        controller.Move((transform.right * lol + transform.forward * xd) * sus * Time.deltaTime);
    }

    private void jump()
    {
        onground = Physics.CheckSphere(ground_t.position, 0.4f, ground);

        if (onground)
        {
            velocity.y = 0f;
            if (Input.GetButtonDown("Jump")) velocity.y = Mathf.Sqrt(jumpf * -2f * -gravity);
        }
        else
        {
            velocity.y -= gravity;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
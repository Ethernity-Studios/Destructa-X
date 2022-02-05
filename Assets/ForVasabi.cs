using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerMovement : NetworkBehaviour
{
    public CharacterController controller;
    public float speed = 8f;
    public float running = 15f;
    public float gravity = -30f;
    public float jumpH = 2f;

    public Transform GCheck;
    public float groundDist = 0.4f;
    public LayerMask groundMask;

    public Vector3 velocity;
    bool isGrounded;

    public float MouseSens = 400f;
    GameObject Camyr;
    float xRotation = 0f;

    [SerializeField] Material playerMat;

    [SyncVar(hook = nameof(SendText))]
    int value;

    Text text;

    void Start()
    {
        text = FindObjectOfType<Text>();
        text.text = value.ToString();
        if (!isLocalPlayer) return;
        Camyr = Camera.main.gameObject;
        Camyr.gameObject.transform.parent = this.transform;
        Camyr.transform.position = new Vector3(transform.position.x, transform.position.y + .6f, transform.position.z);
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        Move();
        if (Input.GetKeyDown(KeyCode.I))
        {
            IncreaseNumber();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            controller.enabled = false;
            transform.position = Vector3.zero;
            controller.enabled = true;
        }
    }
    void IncreaseNumber()
    {
        value++;
    }

    void SendText(int _, int newC)
    {
        text.text = newC.ToString();
    }

    void Move()
    {
        isGrounded = Physics.CheckSphere(GCheck.position, groundDist, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = running;
        }
        else
        {
            speed = 8f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = 8f;
        }
        if (Input.GetButton("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpH * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        float Mousex = Input.GetAxis("Mouse X") * MouseSens * Time.deltaTime;
        float MouseY = Input.GetAxis("Mouse Y") * MouseSens * Time.deltaTime;
        xRotation -= MouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        Camyr.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * Mousex);
    }

}

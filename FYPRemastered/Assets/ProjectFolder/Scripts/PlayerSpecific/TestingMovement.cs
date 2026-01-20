using UnityEngine;

public class TestingMovement : MonoBehaviour
{
    /*public float moveSpeed = 5f;
    public bool useWorldSpace = true;
    public bool moving = false;
    // Update is called once per frame
    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) { x -= 1f; moving = true; }
        else if (Input.GetKey(KeyCode.RightArrow)) { x += 1f; moving = true; }
        else if (Input.GetKey(KeyCode.UpArrow)) { z += 1f; moving = true; }
        else if (Input.GetKey(KeyCode.DownArrow)) { z -= 1f; moving = true; }
        else moving = false;
            Vector3 moveDirection = new Vector3(x, 0f, z).normalized;
        if (useWorldSpace)
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.Self);
        }
        if(moving)
        Debug.LogError("TestingMovement script is active.");
    }*/
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float sprintMultiplier = 1.6f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -19.62f;   // a bit stronger than default feels nicer
    [SerializeField] private float groundedStick = -2f; // keeps you grounded on slopes

    [Header("Optional")]
    [SerializeField] private Transform moveRelativeTo;  // e.g., your camera

    private CharacterController _cc;
    private float _verticalVelocity;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Horizontal input (arrow keys + WASD work with default Input axes)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Build a world-space movement direction
        Vector3 move = new Vector3(x, 0f, z);
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (moveRelativeTo != null)
        {
            Vector3 fwd = moveRelativeTo.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = moveRelativeTo.right; right.y = 0f; right.Normalize();
            move = (right * move.x + fwd * move.z);
        }
        else
        {
            // Move relative to this object's facing
            move = transform.TransformDirection(move);
        }

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        // Gravity
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = groundedStick;

        _verticalVelocity += gravity * Time.deltaTime;

        // Final motion (CharacterController resolves collisions)
        Vector3 velocity = move * speed;
        velocity.y = _verticalVelocity;

        _cc.Move(velocity * Time.deltaTime);
    }
}

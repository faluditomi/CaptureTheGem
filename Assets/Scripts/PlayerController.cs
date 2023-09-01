using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    private bool isGrounded;

    private Rigidbody myRigidbody;

    private Transform collisionCheckPoint;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        collisionCheckPoint = new GameObject("CollisionCheckPoint").transform;

        collisionCheckPoint.parent = transform;

        collisionCheckPoint.position = new Vector3(myRigidbody.position.x, myRigidbody.position.y - GetComponent<Collider>().bounds.extents.y + 0.1f, myRigidbody.position.z);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Check if the player is grounded
        isGrounded = Physics.Raycast(collisionCheckPoint.position, Vector3.down, 0.2f);
        
        // Player movement
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * movementSpeed * Time.deltaTime;

        transform.Translate(movement);

        // Jumping
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            myRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}

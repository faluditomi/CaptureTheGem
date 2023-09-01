using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GemController gemController;

    private Rigidbody myRigidbody;

    private Transform collisionCheckPoint;
    
    private GameObject myGem;

    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float speedMultiplierWithGem = 0.8f;
    [SerializeField] private float jumpForce = 6f;

    private bool isGrounded;

    private void Awake()
    {
        gemController = FindObjectOfType<GemController>();

        myRigidbody = GetComponent<Rigidbody>();

        myGem = transform.Find("Gem").gameObject;
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
        PlayerMovement();
    }

    private void OnTriggerEnter(Collider other)
    {
        switch(other.transform.tag)
        {
            case "Gem":
                myGem.SetActive(true);

                gemController.GemPickedUp();
                
                break;

            case "Player":
                if(myGem.activeInHierarchy)
                {
                    myGem.SetActive(false);

                    gemController.GemDropped(transform.position);
                }
                
                break;

            case "ScoreTrigger":
                if((other.transform.parent.name.Contains("One") && name.Contains("One") ||
                (other.transform.parent.name.Contains("Two") && name.Contains("Two"))) &&
                myGem.activeInHierarchy)
                {
                    myGem.SetActive(false);

                    gemController.GemReset();
                }

                break;
        }
    }

    private void PlayerMovement()
    {
        // Check if the player is grounded
        isGrounded = Physics.Raycast(collisionCheckPoint.position, Vector3.down, 0.2f);
        
        // Player movement
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * movementSpeed * Time.deltaTime;

        if(myGem.activeInHierarchy)
        {
            movement *= speedMultiplierWithGem;
        }

        transform.Translate(movement);

        // Jumping
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            myRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}

using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    private PlayerGameInstance gameInstance;

    private GemController gemController;

    private Rigidbody myRigidbody;

    private Transform collisionCheckPoint;
    
    private GameObject myGem;

    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float speedMultiplierWithGem = 0.8f;
    [SerializeField] private float jumpForce = 6f;

    private string goalIndicator = string.Empty;

    private bool isGrounded;

    [Client]
    public override void OnStartAuthority()
    {
        enabled = true;
    }

    [Client]
    private void Awake()
    {
        gemController = FindObjectOfType<GemController>();

        myRigidbody = GetComponent<Rigidbody>();

        myGem = transform.Find("Gem").gameObject;
    }

    [Client]
    private void Start()
    {
        collisionCheckPoint = new GameObject("CollisionCheckPoint").transform;

        collisionCheckPoint.parent = transform;

        collisionCheckPoint.position = new Vector3(myRigidbody.position.x, myRigidbody.position.y - GetComponent<Collider>().bounds.extents.y + 0.1f, myRigidbody.position.z);

        Cursor.lockState = CursorLockMode.Locked;
    }

    [Client]
    private void Update()
    {
        if(!isOwned)
        {
            return;
        }

        PlayerMovement();

        PlayerJump();
    }

    [Client]
    private void OnTriggerEnter(Collider other)
    {
        if(!isOwned)
        {
            return;
        }

        switch(other.transform.tag)
        {
            case "Gem":
                CmdPickUpGem();
                
                break;

            case "Player":
                CmdDropGem();
                
                break;

            case "ScoreTrigger":
                CmdResetGem(other.name);

                break;
        }
    }
    
    [Command]
    public void CmdSetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    [Command]
    private void CmdPickUpGem()
    {
        RpcToggleGemIndicator(true);

        gemController.RpcGemPickedUp();
    }

    [Command]
    private void CmdDropGem()
    {
        if(myGem.activeInHierarchy)
        {
            RpcToggleGemIndicator(false);

            gemController.RpcGemDropped(transform.position);
        }
    }

    [Command]
    private void CmdResetGem(string name)
    {
        if(name.Contains(goalIndicator) && myGem.activeInHierarchy)
        {
            RpcToggleGemIndicator(false);

            // gameInstance?.Score();

            gemController.RpcGemReset();
        }
    }

    [ClientRpc]
    private void RpcToggleGemIndicator(bool state)
    {
        myGem.SetActive(state);
    }

    [Command]
    private void CmdPlayerMovement(Vector3 movement)
    {
        //validate logic

        transform.Translate(movement);
    }

    [Command]
    private void CmdPlayerJump()
    {
        //validate logic
        
        myRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    [Client]
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

        CmdPlayerMovement(movement);
    }

    [Client]
    private void PlayerJump()
    {
        if(isGrounded && Input.GetButtonDown("Jump"))
        {
            CmdPlayerJump();
        }
    }

    public void SetGoalIndicator(string goalIndicator)
    {
        this.goalIndicator = goalIndicator;
    }

    public void SetGameInstance(PlayerGameInstance gameInstance)
    {
        this.gameInstance = gameInstance;
    }
}

using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    private GemController gemController;

    private Rigidbody myRigidbody;

    private Transform collisionCheckPoint;
    
    private GameObject myGem;

    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float speedMultiplierWithGem = 0.8f;
    [SerializeField] private float jumpForce = 6f;

    private bool isGrounded;

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
        // CmdAssignAuthorityOverGem();

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
                CmdResetGem(other.transform.parent.name);

                break;
        }
    }

    // [Command]
    // private void CmdAssignAuthorityOverGem()
    // {
    //     gemController.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    // }
    
    [Command]
    public void CmdSetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    [Command]
    private void CmdPickUpGem()
    {
        RpcPickUpGem();
    }

    [Command]
    private void CmdDropGem()
    {
        if(myGem.activeInHierarchy)
        {
            RpcDropGem();
        }
    }

    [Command]
    private void CmdResetGem(string name)
    {
        if(/*(name.Contains("One") && this.name.Contains("One") ||
        (name.Contains("Two") && this.name.Contains("Two"))) &&*/
        myGem.activeInHierarchy)
        {
            RpcResetGem();
        }
    }

    [ClientRpc]
    private void RpcPickUpGem()
    {
        myGem.SetActive(true);

        gemController.CmdGemPickedUp();
    }

    [ClientRpc]
    private void RpcDropGem()
    {
        myGem.SetActive(false);

        gemController.CmdGemDropped(transform.position);
    }

    [ClientRpc]
    private void RpcResetGem()
    {
        myGem.SetActive(false);

        gemController.CmdGemReset();
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
}

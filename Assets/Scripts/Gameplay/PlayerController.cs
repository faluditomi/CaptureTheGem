using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{  
    #region Attributes
    private PlayerGameInstance gameInstance;

    private GemController gemController;

    private Rigidbody myRigidbody;

    private Transform collisionCheckPoint;
    
    private GameObject myGem;

    [Tooltip("The speed at which the player moves forwards, backwards and sideways.")]
    [SerializeField] private float movementSpeed = 8f;
    [Tooltip("The speed is multiplied with this while carrying gem. Should be between 0 and 1.")]
    [SerializeField] private float speedMultiplierWithGem = 0.8f;
    [Tooltip("The size of the player's jump.")]
    [SerializeField] private float jumpForce = 6f;

    private string goalIndicator = string.Empty;

    private bool isGrounded;
    #endregion

    #region MonoBehaviour Methods
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
        // In my opinion, creating such simple object in code is safer, as this implementation
        // is immune to changes to the player object's transform, etc.
        collisionCheckPoint = new GameObject("CollisionCheckPoint").transform;

        collisionCheckPoint.parent = transform;

        collisionCheckPoint.position = new Vector3(myRigidbody.position.x, myRigidbody.position.y - GetComponent<Collider>().bounds.extents.y + 0.1f, myRigidbody.position.z);

        Cursor.lockState = CursorLockMode.Locked;
    }

    [Client]
    private void Update()
    {
        // With this, we make sure that our movement only runs on each player once.
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

        // This switch statement is the starting point for all gem behaviour. It would have
        // been prettier in the GemController, but considering the scope and time limit,
        // this seemed more efficient to me.
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
    #endregion

    #region Mirror Overrides
    /* This method simply turns on this component when the client gains authority
    over the player, making sure the controls of the clients don't get mixed up. */
    [Client]
    public override void OnStartAuthority()
    {
        enabled = true;
    }
    #endregion
    
    #region Regular Methods
    /* Currently the direction vector is not normalised which allows for faster strafing
    but as I was testing it both ways, I thought the strafing actually adds to the fun. */
    [Client]
    private void PlayerMovement()
    {
        // Player movement
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * movementSpeed * Time.deltaTime;

        if(myGem.activeInHierarchy)
        {
            movement *= speedMultiplierWithGem;
        }

        CmdPlayerMovement(movement);
    }

    /* If the player is touching the ground and they client is pressing the
    jump button, the player will jump */
    [Client]
    private void PlayerJump()
    {
        isGrounded = Physics.Raycast(collisionCheckPoint.position, Vector3.down, 0.2f);

        if(isGrounded && Input.GetButtonDown("Jump"))
        {
            CmdPlayerJump();
        }
    }

    #region Commands
    /* This is where we can do further logic validation to prevent cheating. */
    [Command]
    private void CmdPlayerMovement(Vector3 movement)
    {
        transform.Translate(movement);
    }

    /* This is where we can do further logic validation to prevent cheating. */
    [Command]
    private void CmdPlayerJump()
    {
        myRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    [Command]
    private void CmdPickUpGem()
    {
        RpcToggleGemIndicator(true);

        gemController.RpcGemPickedUp();
    }

    /* This gets called when a player touches another. If the client has the gem, they
    will drop it. */
    [Command]
    private void CmdDropGem()
    {
        if(myGem.activeInHierarchy)
        {
            RpcToggleGemIndicator(false);

            gemController.RpcGemDropped(transform.position);
        }
    }

    /* The server checks whether this client is holding the gem and if they brought it
    to the right base, then it resets the indicator, gives a point to the client and
    calls the gem's behaviour. */
    [Command]
    private void CmdResetGem(string name)
    {
        if(name.Contains(goalIndicator) && myGem.activeInHierarchy)
        {
            RpcToggleGemIndicator(false);

            gameInstance?.Score();

            StartCoroutine(gemController.ResetGemBehaviour());
        }
    }
    
    /* A helper method for the CameraController. Not very elegant, as it is a side-effect
    of me using a simplified implementation of the old input system. */
    [Command]
    public void CmdSetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
    #endregion

    #region RPCs
    /* This method just helps to save some lines of code when manipulatingthe indicators
    for all clients. */
    [ClientRpc]
    private void RpcToggleGemIndicator(bool state)
    {
        myGem.SetActive(state);
    }
    #endregion
    #endregion

    #region Setters
    public void SetGoalIndicator(string goalIndicator)
    {
        this.goalIndicator = goalIndicator;
    }

    public void SetGameInstance(PlayerGameInstance gameInstance)
    {
        this.gameInstance = gameInstance;
    }
    #endregion
}

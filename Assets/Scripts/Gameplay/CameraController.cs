using UnityEngine;
using Mirror;

public class CameraController : NetworkBehaviour
{
    #region Attributes
    private PlayerController playerController;

    private Quaternion rotationHorizontal;

    private Vector3 rotationVertical;

    [Tooltip("The speed of the player's looking around.")]
    [SerializeField] private float mouseSensitivity = 5f;

    private bool canLook = true;
    #endregion

    #region MonoBehaviour Methods
    [Client]
    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    [Client]
    private void Start()
    {
        rotationHorizontal = transform.rotation;
    }

    [Client]
    private void Update()
    {
        // We make sure only one client runs this and that we have time at the start to
        // set up the player properly.
        if(!isOwned || !canLook)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        
        float mouseY = Input.GetAxis("Mouse Y");

        rotationHorizontal *= Quaternion.Euler(0f, mouseX * mouseSensitivity, 0f);

        // This is where the horizontal rotation is set.
        playerController.CmdSetRotation(rotationHorizontal);

        rotationVertical = transform.eulerAngles;

        rotationVertical.x -= mouseY * mouseSensitivity;

        // We make sure that the player can't turn the camera to look behind themselves.
        // This is a long solution, as I ran into some trouble with how Unity handles Euler
        // angles differenty in the editor and at runtime.
        if(rotationVertical.x < 270f && rotationVertical.x > 180f)
        {
            rotationVertical.x = 270f;
        }
        else if(rotationVertical.x > 90f && rotationVertical.x < 180f)
        {
            rotationVertical.x = 90f;
        }

        // This is where the vertical rotation is set.
        transform.eulerAngles = rotationVertical;
    }
    #endregion

    #region Mirror Overrides
    /* This method simply turns on this, and the Camera component when the client gains
    authority over the player, making sure the controls of the clients don't get mixed up. */
    [Client]
    public override void OnStartAuthority()
    {
        enabled = true;

        GetComponent<Camera>().enabled = true;
    }
    #endregion

    #region Setters
    [Client]
    public void SetCanLook(bool canLook)
    {
        this.canLook = canLook;
    }
    #endregion
}

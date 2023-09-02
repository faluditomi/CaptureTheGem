using UnityEngine;
using Mirror;

public class CameraController : NetworkBehaviour
{
    private PlayerController playerController;

    [SerializeField] private float mouseSensitivity = 5f;

    private Quaternion rotationHorizontal;

    private Vector3 rotationVertical;

    private bool canLook = true;

    public override void OnStartAuthority()
    {
        enabled = true;

        GetComponent<Camera>().enabled = true;
    }

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
        if(!isOwned || !canLook)
        {
            return;
        }

        // Mouse-based character rotation (horizontal)
        float mouseX = Input.GetAxis("Mouse X");
        
        float mouseY = Input.GetAxis("Mouse Y");

        // Update player's rotation (horizontal)
        rotationHorizontal *= Quaternion.Euler(0f, mouseX * mouseSensitivity, 0f);

        playerController.CmdSetRotation(rotationHorizontal);

        // Camera rotation (vertical)
        rotationVertical = transform.eulerAngles;

        rotationVertical.x -= mouseY * mouseSensitivity;

        if(rotationVertical.x < 270f && rotationVertical.x > 180f)
        {
            rotationVertical.x = 270f;
        }
        else if(rotationVertical.x > 90f && rotationVertical.x < 180f)
        {
            rotationVertical.x = 90f;
        }

        transform.eulerAngles = rotationVertical;
    }

    [Client]
    public void SetCanLook(bool canLook)
    {
        this.canLook = canLook;
    }
}

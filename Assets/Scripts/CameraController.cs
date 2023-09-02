using UnityEngine;
using Mirror;

public class CameraController : NetworkBehaviour
{
    private PlayerController playerController;

    [SerializeField] private float mouseSensitivity = 5f;

    private Quaternion rotationHorizontal = Quaternion.identity;

    private Vector3 rotationVertical;

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
    private void Update()
    {
        if(!isOwned)
        {
            return;
        }

        // Mouse-based character rotation (horizontal)
        float mouseX = Input.GetAxis("Mouse X");
        
        float mouseY = Input.GetAxis("Mouse Y");

        // Update player's rotation (horizontal)
        rotationHorizontal *= Quaternion.Euler(0f, mouseX * mouseSensitivity, 0f);

        // transform.parent.rotation = rotationHorizontal;
        playerController.CmdSetRotation(rotationHorizontal);

        // Camera rotation (vertical)
        rotationVertical = transform.eulerAngles;

        rotationVertical.x -= mouseY * mouseSensitivity;

        CmdSetCameraRotation();
    }

    private void CmdSetCameraRotation()
    {
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
}

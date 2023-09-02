using UnityEngine;
using Mirror;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 5f;

    private Quaternion playerRotation = Quaternion.identity;

    public override void OnStartAuthority()
    {
        GetComponent<Camera>().enabled = true;
    }

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
        playerRotation *= Quaternion.Euler(0f, mouseX * mouseSensitivity, 0f);

        transform.parent.rotation = playerRotation;

        // Camera rotation (vertical)
        // Clamp camera's vertical rotation
        Vector3 currentRotation = transform.eulerAngles;

        currentRotation.x -= mouseY * mouseSensitivity;

        if(currentRotation.x < 270f && currentRotation.x > 180f)
        {
            currentRotation.x = 270f;
        }
        else if(currentRotation.x > 90f && currentRotation.x < 180f)
        {
            currentRotation.x = 90f;
        }

        transform.eulerAngles = currentRotation;
    }


}

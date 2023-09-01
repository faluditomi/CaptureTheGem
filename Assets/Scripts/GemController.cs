using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemController : MonoBehaviour
{
    private MeshRenderer myMeshRenderer;

    private Collider myCollider;

    private Rigidbody myRigidbody;

    [SerializeField] private float bounceForce = 2f;

    private void Awake()
    {
        myMeshRenderer = GetComponent<MeshRenderer>();

        myCollider = GetComponent<Collider>();

        myRigidbody = GetComponent<Rigidbody>();
    }

    //even tho barely anything happens in it, I made it a class so it's easier to trace and so that we can append it later
    public void GemPickedUp()
    {
        myMeshRenderer.enabled = false;

        myCollider.enabled = false;
    }

    public void GemDropped(Vector3 posOfCollision)
    {
        transform.position = new Vector3(posOfCollision.x, 6f, posOfCollision.z);

        myMeshRenderer.enabled = true;

        myCollider.enabled = true;

        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

        myRigidbody.AddForce(direction * bounceForce, ForceMode.Impulse);
    }

    public void GemReset()
    {
        transform.position = new Vector3(0f, 6f, 0f);

        myMeshRenderer.enabled = true;

        myCollider.enabled = true;
    }
}

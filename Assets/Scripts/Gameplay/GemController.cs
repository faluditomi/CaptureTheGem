using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GemController : NetworkBehaviour
{
    private MeshRenderer myMeshRenderer;

    private Collider[] myColliders = new Collider[2];

    private Rigidbody myRigidbody;

    [SerializeField] private float bounceForce = 2f;

    private void Awake()
    {
        myMeshRenderer = GetComponent<MeshRenderer>();

        myColliders = GetComponents<Collider>();

        myRigidbody = GetComponent<Rigidbody>();
    }

    //even tho barely anything happens in it, I made it a class so it's easier to trace and so that we can append it later
    [ClientRpc]
    public void RpcGemPickedUp()
    {
        myMeshRenderer.enabled = false;

        myRigidbody.isKinematic = true;

        foreach(Collider collider in myColliders)
        {
            collider.enabled = false;
        }
    }

    [ClientRpc]
    public void RpcGemDropped(Vector3 posOfCollision)
    {
        transform.position = new Vector3(posOfCollision.x, 6f, posOfCollision.z);

        myMeshRenderer.enabled = true;

        foreach(Collider collider in myColliders)
        {
            collider.enabled = true;
        }

        myRigidbody.isKinematic = false;

        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

        myRigidbody.AddForce(direction * bounceForce, ForceMode.Impulse);
    }

    [ClientRpc]
    public void RpcGemReset()
    {
        transform.position = new Vector3(0f, 6f, 0f);

        myRigidbody.isKinematic = false;

        myMeshRenderer.enabled = true;

        foreach(Collider collider in myColliders)
        {
            collider.enabled = true;
        }
    }
}

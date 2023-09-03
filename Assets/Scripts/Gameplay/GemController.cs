using System.Collections;
using Mirror;
using UnityEngine;

public class GemController : NetworkBehaviour
{
    #region Attributes
    private MeshRenderer myMeshRenderer;

    // We want to keep track of both the trigger and the normal collider of the gem.
    private Collider[] myColliders = new Collider[2];

    private Rigidbody myRigidbody;

    [Tooltip("How far the gem is thrown when it is dropped by a player.")]
    [SerializeField] private float bounceForce = 2f;
    [Tooltip("How much time passes between getting a score and the gem reseting.")]
    [SerializeField] private float gemResetDelay = 2f;
    #endregion

    #region MonoBehaviour Methods
    private void Awake()
    {
        myMeshRenderer = GetComponent<MeshRenderer>();

        myColliders = GetComponents<Collider>();

        myRigidbody = GetComponent<Rigidbody>();
    }
    #endregion

    #region RPCs
    /* Makes the gem invisible and untouchable while it's "picked up", instead
    of destroying it. */
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

    /* Brings the gem back in the game, placing it above the players and throwing
    it in a random direction, so that it doesn't predictably land on either player. */
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

    
    /* Similar to the RpcGemDropped, but the gem is returned to the middle of the Map and
    it isn't thrown. */
    [ClientRpc]
    private void RpcGemReset()
    {
        transform.position = new Vector3(0f, 6f, 0f);

        myRigidbody.isKinematic = false;

        myMeshRenderer.enabled = true;

        foreach(Collider collider in myColliders)
        {
            collider.enabled = true;
        }
    }
    #endregion

    #region Coroutines
    /* Takes care of the delay between scoring and resetting the gem. */
    public IEnumerator ResetGemBehaviour()
    {
        yield return new WaitForSeconds(gemResetDelay);

        RpcGemReset();
    }
    #endregion
}

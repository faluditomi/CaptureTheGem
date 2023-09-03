using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    #region Attributes
    private NetworkManagerCTG networkManager = NetworkManager.singleton as NetworkManagerCTG;

    private static List<Transform> spawnPoints = new List<Transform>();

    private GameObject playerPrefab;

    private int nextIndex = 0;
    #endregion
    
    #region MonoBehaviour Methods
    [Client]
    private void Awake()
    {
        Transform spawnPointContainer = GameObject.Find("SpawnPoints").transform;

        // This object gets spawed from code when the Map is loaded, thus we have to gather the scene references from code.
        for(int i = 0; i < spawnPointContainer.childCount; i++)
        {
            spawnPoints.Add(spawnPointContainer.GetChild(i));
        }
        
        // In my opinion, assigning references in code using Resources saves potential headaches with dragging references in the Inspector.
        playerPrefab = Resources.Load<GameObject>("Spawnables/Player");
    }

    /* OnDestroy gets called if the object that this behaviour is on gets destroyed.
    In which case, we want to stop the potential spawning of further players, so we
    unsubscribe from our event. */
    [Server]
    private void OnDestroy()
    {
        // This event gets triggered every time a client loads a scene. Which is when we need to spawn a player.
        NetworkManagerCTG.OnServerReadied -= SpawnPlayer;
    }
    #endregion

    #region Mirror Overrides
    /* OnStartServer is similar to Start() but it gets called on the server so it's
    a good place to subscribe to our event and set up our spawner. */
    [Server]
    public override void OnStartServer()
    {
        // This event gets triggered every time a client loads a scene. Which is when we need to spawn a player.
        NetworkManagerCTG.OnServerReadied += SpawnPlayer;
    }
    #endregion

    #region Regular Methods
    /* This method creates player instances for our clients, does initial set-up
    and makes sure the clients can take control of the player objects. */
    [Server]
    public void SpawnPlayer(NetworkConnectionToClient connection)
    {
        if (spawnPoints[nextIndex] == null)
        {
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);

        // The method gets run on the server, so we need this call below to spawn this player instance on all our clients.
        NetworkServer.Spawn(playerInstance, connection);

        if(nextIndex == 0)
        {
            playerInstance.GetComponent<MeshRenderer>().material.color = Color.red;

            playerInstance.GetComponent<PlayerController>().SetGoalIndicator("One");
        }
        else if(nextIndex == 1)
        {
            playerInstance.GetComponent<MeshRenderer>().material.color = Color.blue;

            playerInstance.GetComponent<PlayerController>().SetGoalIndicator("Two");
        }

        // This is the place I found where I could most easily link the PlayerGameInstance and the PlayerController,
        // so that we can manipulate the player scores later on.
        playerInstance.GetComponent<PlayerController>().SetGameInstance(networkManager.playersInGame[nextIndex].GetComponent<PlayerGameInstance>());

        // This is here because I had a bug with the rotations not being taken into account during instantiation
        // because the camera was already manipulating the variable.
        playerInstance.GetComponentInChildren<CameraController>().SetCanLook(true);

        nextIndex++;
    }
    #endregion
}

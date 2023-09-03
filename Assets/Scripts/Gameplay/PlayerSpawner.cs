using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    private NetworkManagerCTG networkManager = NetworkManager.singleton as NetworkManagerCTG;

    [SerializeField] private GameObject playerPrefab;

    private static List<Transform> spawnPoints = new List<Transform>();

    private int nextIndex = 0;
    
    private void Awake()
    {
        Transform spawnPointContainer = GameObject.Find("SpawnPoints").transform;

        for(int i = 0; i < spawnPointContainer.childCount; i++)
        {
            spawnPoints.Add(spawnPointContainer.GetChild(i));
        }
    }

    public override void OnStartServer()
    {
        NetworkManagerCTG.OnServerReadied += SpawnPlayer;
    }

    [Server]
    private void OnDestroy()
    {
        NetworkManagerCTG.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    public void SpawnPlayer(NetworkConnectionToClient connection)
    {
        if (spawnPoints[nextIndex] == null)
        {
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);

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

        playerInstance.GetComponent<PlayerController>().SetGameInstance(networkManager.playersInGame[nextIndex].GetComponent<PlayerGameInstance>());

        playerInstance.GetComponentInChildren<CameraController>().SetCanLook(true);

        nextIndex++;
    }
}

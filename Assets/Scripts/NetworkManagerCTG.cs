using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerCTG : NetworkManager
{
    [SerializeField] private PlayerNetworkInstance playerNetworkInstancePrefab;

    public List<PlayerNetworkInstance> players = new List<PlayerNetworkInstance>();

    [SerializeField] private SceneAsset menuScene;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("Spawnables").ToList();
    }

    public override void OnStartClient()
    {
        GameObject[] spawnablePrefabs = Resources.LoadAll<GameObject>("Spawnables");

        foreach(GameObject prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    { 
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnectionToClient connection)
    {
        if(numPlayers >= maxConnections || !SceneManager.GetActiveScene().name.Equals(menuScene.name))
        {
            connection.Disconnect();

            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient connection)
    {
        if(connection.identity)
        {
            PlayerNetworkInstance player = connection.identity.GetComponent<PlayerNetworkInstance>();

            players.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(connection);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient connection)
    {
        if(SceneManager.GetActiveScene().name.Equals(menuScene.name))
        {
            bool isLeader = players.Count == 0;

            PlayerNetworkInstance playerInstance = Instantiate(playerNetworkInstancePrefab);

            playerInstance.SetIsLeader(isLeader);

            NetworkServer.AddPlayerForConnection(connection, playerInstance.gameObject);
        }
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach(PlayerNetworkInstance player in players)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        foreach(PlayerNetworkInstance player in players)
        {
            if(!player.GetIsReady())
            {
                return false;
            }
        }

        return true;
    }

    public override void OnStopServer()
    {
        players.Clear();
    }

    public List<PlayerNetworkInstance> GetPlayersOnNetwork()
    {
        return players;
    }
}

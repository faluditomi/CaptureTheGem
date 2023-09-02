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
    [SerializeField] private PlayerLobbyInstance playerLobbyInstancePrefab;
    [SerializeField] private PlayerGameInstance playerGameInstancePrefab;

    public List<PlayerLobbyInstance> playersInLobby = new List<PlayerLobbyInstance>();
    public List<PlayerGameInstance> playersInGame = new List<PlayerGameInstance>();

    [SerializeField] private SceneAsset menuScene;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnectionToClient> OnServerReadied;

    [SerializeField] private GameObject playerSpawner;

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
            PlayerLobbyInstance player = connection.identity.GetComponent<PlayerLobbyInstance>();

            playersInLobby.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(connection);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient connection)
    {
        if(SceneManager.GetActiveScene().name.Equals(menuScene.name))
        {
            bool isLeader = playersInLobby.Count == 0;

            PlayerLobbyInstance playerInstance = Instantiate(playerLobbyInstancePrefab);

            playerInstance.SetIsLeader(isLeader);

            NetworkServer.AddPlayerForConnection(connection, playerInstance.gameObject);
        }
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach(PlayerLobbyInstance player in playersInLobby)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        foreach(PlayerLobbyInstance player in playersInLobby)
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
        playersInLobby.Clear();
    }

    public void StartGame()
    {
        if(SceneManager.GetActiveScene().name.Equals(menuScene.name) && IsReadyToStart())
        {
            ServerChangeScene("Map");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        for(int i = GetPlayersInLobby().Count - 1; i >= 0; i--)
        {
            NetworkConnectionToClient connection = GetPlayersInLobby()[i].connectionToClient;

            PlayerGameInstance playerGameInstance = Instantiate(playerGameInstancePrefab);

            playerGameInstance.SetDisplayName(GetPlayersInLobby()[i].GetDisplayName());

            NetworkServer.Destroy(connection.identity.gameObject);

            NetworkServer.ReplacePlayerForConnection(connection, playerGameInstance.gameObject);
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        GameObject playerSpawnerInstance = Instantiate(playerSpawner);

        NetworkServer.Spawn(playerSpawnerInstance);
    }

    public override void OnServerReady(NetworkConnectionToClient connection)
    {
        OnServerReadied?.Invoke(connection);
        
        base.OnServerReady(connection);
    }

    public List<PlayerLobbyInstance> GetPlayersInLobby()
    {
        return playersInLobby;
    }
}

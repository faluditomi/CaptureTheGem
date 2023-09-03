using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerCTG : NetworkManager
{
    #region Attributes
    private PlayerLobbyInstance playerLobbyInstancePrefab;
    private PlayerGameInstance playerGameInstancePrefab;

    // Keeping track of our clients both in the lobby and in the game has come in
    // really useful.
    private List<PlayerLobbyInstance> playersInLobby = new List<PlayerLobbyInstance>();
    private List<PlayerGameInstance> playersInGame = new List<PlayerGameInstance>();

    [SerializeField] private SceneAsset menuScene;

    // We save outselves a lot of method calls and lines of code by using these events.
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnectionToClient> OnServerReadied;

    private GameObject playerSpawner;
    #endregion

    #region MonoBehaviour Methods
    public override void Awake()
    {
        base.Awake();

        // Here we also use the resources folder to save the hassle of having to drag
        // references into the inspector.
        playerLobbyInstancePrefab = Resources.Load<PlayerLobbyInstance>("Spawnables/PlayerLobbyInstance");

        playerGameInstancePrefab = Resources.Load<PlayerGameInstance>("Spawnables/PlayerGameInstance");

        playerSpawner = Resources.Load<GameObject>("Spawnables/PlayerSpawner");
    }
    #endregion

    #region Mirror Overrides
    /* Here the server registers all the prefabs we will want to spawn in the
    NetworkManager. */ 
    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("Spawnables").ToList();
    }

    /* Here the client registers all the prefabs we will want to spawn in the
    NetworkManager. */ 
    public override void OnStartClient()
    {
        GameObject[] spawnablePrefabs = Resources.LoadAll<GameObject>("Spawnables");

        foreach(GameObject prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    
    /* Here we invoke an event that will help us navigate the main menu/lobby. */ 
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();
    }

    /* Here we invoke an event that will help us navigate the main menu/lobby. */ 
    public override void OnClientDisconnect()
    { 
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();
    }

    
    /* We won't allow new players to join if we are too many or if we are not
    in the lobby anymore. */ 
    public override void OnServerConnect(NetworkConnectionToClient connection)
    {
        if(numPlayers >= maxConnections || !SceneManager.GetActiveScene().name.Equals(menuScene.name))
        {
            connection.Disconnect();

            return;
        }
    }

    /* Here we invoke an event that will help us spawn in our players. */ 
    public override void OnServerReady(NetworkConnectionToClient connection)
    {
        OnServerReadied?.Invoke(connection);
        
        base.OnServerReady(connection);
    }
    
    /* When we add players to the main menu, we keep track of who the lobby leader is.
    The one that created the room and joined first. */ 
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

    /* When we move to the map scene, we exhange our clients' lobby instances for
    their new game instances. */ 
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

    /* When the new scene is loaded in, we spawn the PlayerSpawner, so that we
    can start populating the map with player objects. */ 
    public override void OnServerSceneChanged(string sceneName)
    {
        GameObject playerSpawnerInstance = Instantiate(playerSpawner);

        NetworkServer.Spawn(playerSpawnerInstance);
    }

    /* If a client leaves, they are removed from our list, our lobby, and the
    ready statuses are updated. */ 
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

    public override void OnStopServer()
    {
        playersInLobby.Clear();
    }
    #endregion

    #region Regular Methods
    /* Here we check whether everyone is readied up or not. */ 
    public void NotifyPlayersOfReadyState()
    {
        foreach(PlayerLobbyInstance player in playersInLobby)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    
    /* This method only returns true if everyone is ready. */ 
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

    /* If the client who created the lobby starts the game and we are in the lobby,
    we will move to the map. */ 
    public void StartGame()
    {
        if(SceneManager.GetActiveScene().name.Equals(menuScene.name) && IsReadyToStart())
        {
            ServerChangeScene("Map");
        }
    }
    #endregion

    #region Getters
    public List<PlayerLobbyInstance> GetPlayersInLobby()
    {
        return playersInLobby;
    }

    public List<PlayerGameInstance> GetPlayersInGame()
    {
        return playersInGame;
    }
    #endregion
}

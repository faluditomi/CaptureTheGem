using System;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerCTG : NetworkManager
{
    [SerializeField] private SceneAsset menuScene;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

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
}

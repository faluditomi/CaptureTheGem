using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGameInstance : NetworkBehaviour
{
    private NetworkManagerCTG networkManager = NetworkManager.singleton as NetworkManagerCTG;

    [SyncVar]
    private string displayName = "Loading...";

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        networkManager.playersInGame.Add(this);
    }

    public override void OnStopServer()
    {
        networkManager.playersInGame.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }
}

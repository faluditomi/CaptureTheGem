using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyInstance : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyUI;

    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[2];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[2];

    [SerializeField] private Button startGameButton = null;

    private NetworkManagerCTG networkManager = NetworkManager.singleton as NetworkManagerCTG;

    private bool isLeader;
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    private bool isReady;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    private string displayName = "Loading...";

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(NameInputMenu.GetDisplayName());

        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        networkManager.playersInLobby.Add(this);

        //gets rid of bug where start button stays active if a 2nd person joins after the first is readied up
        networkManager.NotifyPlayersOfReadyState();

        UpdateUI();
    }

    public override void OnStopServer()
    {
        networkManager.playersInLobby.Remove(this);

        UpdateUI();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue)
    {
        UpdateUI();
    }

    public void HandleDisplayNameChanged(string oldValue, string newValue)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if(!isOwned)
        {
            foreach(PlayerLobbyInstance player in networkManager.playersInLobby)
            {
                if(player.isOwned)
                {
                    player.UpdateUI();

                    break;
                }
            }

            return;
        }

        for(int i = 0; i < networkManager.playersInLobby.Count; i++)
        {
            playerNameTexts[i].text = networkManager.playersInLobby[i].GetDisplayName();

            playerReadyTexts[i].text = networkManager.playersInLobby[i].GetIsReady() ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if(!isLeader)
        {
            return;
        }

        startGameButton.gameObject.SetActive(readyToStart);
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        isReady = !isReady;

        networkManager.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if(networkManager.playersInLobby[0].connectionToClient.Equals(connectionToClient))
        {
            networkManager.StartGame();
        }
    }

    public void SetIsLeader(bool isLeader)
    {
        this.isLeader = isLeader;

        if(GetIsReady())
        {
            startGameButton.gameObject.SetActive(isLeader);
        }
    }

    public NetworkManagerCTG GetNetworkManager()
    {
        return networkManager;
    }

    public bool GetIsReady()
    {
        return isReady;
    }

    public string GetDisplayName()
    {
        return displayName;
    }
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyInstance : NetworkBehaviour
{
    private NetworkManagerCTG networkManager = NetworkManager.singleton as NetworkManagerCTG;

    [SerializeField] private GameObject lobbyUI;

    [SerializeField] private TMP_Text[] nameTexts = new TMP_Text[2];
    [SerializeField] private TMP_Text[] readyTexts = new TMP_Text[2];

    [SerializeField] private Button startGameButton = null;

    private bool isLeader;
    [SyncVar(hook = nameof(HandleReadyStatusChange))]
    private bool isReady;

    [SyncVar(hook = nameof(HandleDisplayNameChange))]
    private string displayName = "Loading...";

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(NameInputMenu.GetDisplayName());

        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        networkManager.GetPlayersInLobby().Add(this);

        //gets rid of bug where start button stays active if a 2nd person joins after the first is readied up
        networkManager.NotifyPlayersOfReadyState();

        UpdateUI();
    }

    public override void OnStopServer()
    {
        networkManager.GetPlayersInLobby().Remove(this);

        UpdateUI();
    }

    public void HandleReadyStatusChange(bool oldValue, bool newValue)
    {
        UpdateUI();
    }

    public void HandleDisplayNameChange(string oldValue, string newValue)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if(!isOwned)
        {
            foreach(PlayerLobbyInstance player in networkManager.GetPlayersInLobby())
            {
                if(player.isOwned)
                {
                    player.UpdateUI();

                    break;
                }
            }

            return;
        }

        for(int i = 0; i < networkManager.GetPlayersInLobby().Count; i++)
        {
            nameTexts[i].text = networkManager.GetPlayersInLobby()[i].GetDisplayName();

            readyTexts[i].text = networkManager.GetPlayersInLobby()[i].GetIsReady() ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
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
        if(networkManager.GetPlayersInLobby()[0].connectionToClient.Equals(connectionToClient))
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

    public bool GetIsReady()
    {
        return isReady;
    }

    public string GetDisplayName()
    {
        return displayName;
    }
}

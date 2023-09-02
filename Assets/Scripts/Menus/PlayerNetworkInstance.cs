using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNetworkInstance : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyUI;

    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[2];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[2];

    [SerializeField] private Button startGameButton = null;

    private NetworkManagerCTG lobby = NetworkManager.singleton as NetworkManagerCTG;

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
        lobby.players.Add(this);

        UpdateUI();
    }

    public override void OnStopServer()
    {
        lobby.players.Remove(this);

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
            foreach(PlayerNetworkInstance player in lobby.players)
            {
                if(player.isOwned)
                {
                    player.UpdateUI();

                    break;
                }
            }

            return;
        }

        // for(int i = 0; i < playerNameTexts.Length; i++)
        // {
        //     playerNameTexts[i].text = "Waiting For Player...";

        //     playerReadyTexts[i].text = string.Empty;
        // }

        for(int i = 0; i < lobby.players.Count; i++)
        {
            playerNameTexts[i].text = lobby.players[i].GetDisplayName();

            playerReadyTexts[i].text = lobby.players[i].GetIsReady() ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if(!isLeader)
        {
            return;
        }

        startGameButton.interactable = readyToStart;
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

        lobby.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if(lobby.players[0].connectionToClient.Equals(connectionToClient))
        {
            //start game
        }
    }

    public void SetIsLeader(bool isLeader)
    {
        this.isLeader = isLeader;

        startGameButton.gameObject.SetActive(isLeader);
    }

    public NetworkManagerCTG GetNetworkManager()
    {
        return lobby;
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

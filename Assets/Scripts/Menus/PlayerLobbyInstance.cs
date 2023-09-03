using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyInstance : NetworkBehaviour
{
    #region Attributes
    private NetworkManagerCTG networkManager = NetworkManager.singleton as NetworkManagerCTG;

    // For the menus I refrained from gathering references in code, since I
    // Would have to be searching in transform, which would defeat the purpose
    // in my opinion.
    [SerializeField] private GameObject lobbyUI;

    [SerializeField] private TMP_Text[] nameTexts = new TMP_Text[2];
    [SerializeField] private TMP_Text[] readyTexts = new TMP_Text[2];

    [SerializeField] private Button startGameButton = null;

    private bool isLeader;
    // SynchVars are a secure way of keeping important variable coonsistent throughout the server
    // and the hooks help us call a method when the attributes change.
    [SyncVar(hook = nameof(HandleReadyStatusChange))]
    private bool isReady;

    [SyncVar(hook = nameof(HandleDisplayNameChange))]
    private string displayName = "Loading...";
    #endregion

    #region Mirror Overrides
    /* Once a client gains authority over their lobby instance, we set their
    name in the lobby and turn on their lobby UI. */
    public override void OnStartAuthority()
    {
        CmdSetDisplayName(NameInputMenu.GetDisplayName());

        lobbyUI.SetActive(true);
    }

    /* when an instance is created, the client adds themselves to the list of lobby
    instances and updates the ready status serverwide. */ 
    public override void OnStartClient()
    {
        networkManager.GetPlayersInLobby().Add(this);

        // This line is here because I had a bug where a the start button stays active
        // when the 2nd person joins after the 1st readies up.
        networkManager.NotifyPlayersOfReadyState();

        UpdateUI();
    }

    /* when an instance is removed, the client removes themselves from the list of lobby
    instances and updates the ready status serverwide, along with the names. */ 
    public override void OnStopServer()
    {
        networkManager.GetPlayersInLobby().Remove(this);

        UpdateUI();
    }
    #endregion

    #region Regular Methods
    /* Having the oldValue and the newValue as inputs was the only way I could
    compile the code. */
    public void HandleReadyStatusChange(bool oldValue, bool newValue)
    {
        UpdateUI();
    }

    /* Having the oldValue and the newValue as inputs was the only way I could
    compile the code. */
    public void HandleDisplayNameChange(string oldValue, string newValue)
    {
        UpdateUI();
    }

    /* The code that turns the Start button on and off, depending on ready status. */
    public void HandleReadyToStart(bool readyToStart)
    {
        if(!isLeader)
        {
            return;
        }

        startGameButton.gameObject.SetActive(readyToStart);
    }

    /* The method that updates the Names and the Ready statuses in the lobby. */
    private void UpdateUI()
    {
        // This would be a temporary solution, for making sure every client runs the 
        // UI update once, due to it being overcomplicated. But I didn't yet have time to 
        // find a more elegant solution.
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

            readyTexts[i].text = networkManager.GetPlayersInLobby()[i].GetIsReady() ? "<color=green>Ready</color>" : "<color=black>Not Ready</color>";
        }
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

    /* If this is the lobby leader, the game will be started. */ 
    [Command]
    public void CmdStartGame()
    {
        if(networkManager.GetPlayersInLobby()[0].connectionToClient.Equals(connectionToClient))
        {
            networkManager.StartGame();
        }
    }
    #endregion

    #region Setters
    public void SetIsLeader(bool isLeader)
    {
        this.isLeader = isLeader;

        // When all players are ready, the Start buttn becomes visible to the lobby leader.
        if(GetIsReady())
        {
            startGameButton.gameObject.SetActive(isLeader);
        }
    }
    #endregion

    #region Getters
    public bool GetIsReady()
    {
        return isReady;
    }

    public string GetDisplayName()
    {
        return displayName;
    }
    #endregion
}

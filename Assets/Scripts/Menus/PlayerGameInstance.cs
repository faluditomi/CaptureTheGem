using Mirror;
using TMPro;
using UnityEngine;

public class PlayerGameInstance : NetworkBehaviour
{
    #region Attributes
    private NetworkManagerCTG networkManager = NetworkManager.singleton as NetworkManagerCTG;

    // For the menus I refrained from gathering references in code, since I
    // Would have to be searching in transform, which would defeat the purpose
    // in my opinion.
    [SerializeField] private GameObject gameHUD;

    [SerializeField] private TMP_Text[] nameTexts = new TMP_Text[2];
    [SerializeField] private TMP_Text[] scoreTexts = new TMP_Text[2];

    [SyncVar]
    private string displayName;

    // SynchVars are a secure way of keeping important variable coonsistent throughout the server
    // and the hooks help us call a method when the attributes change.
    [SyncVar(hook = nameof(HandleScoreChange))]
    private int score = 0;
    #endregion

    #region Mirror Overrides
    /* Just in case we would want to carry our players over to another scene,
    we call DontDestroyOnLoad, add them to our list of player instances and we 
    activate and update the HUD. */
    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        
        networkManager.GetPlayersInGame().Add(this);

        gameHUD.SetActive(true);

        UpdateHUD();
    }

    public override void OnStopServer()
    {
        networkManager.GetPlayersInGame().Remove(this);
    }
    #endregion

    #region Regular Methods
    /* Having the oldValue and the newValue as inputs was the only way I could
    compile the code. */
    public void HandleScoreChange(int oldValue, int newValue)
    {
        UpdateHUD();
    }

    /* The method that updates the Names and the Scores in-game. */
    private void UpdateHUD()
    {
        // This would be a temporary solution, for making sure every client runs the 
        // HUD update once, due to it being overcomplicated. But I didn't yet have time to 
        // find a more elegant solution.
        if(!isOwned)
        {
            foreach(PlayerGameInstance player in networkManager.GetPlayersInGame())
            {
                if(player.isOwned)
                {
                    player.UpdateHUD();

                    break;
                }
            }

            return;
        }

        for(int i = 0; i < networkManager.GetPlayersInGame().Count; i++)
        {
            nameTexts[i].text = networkManager.GetPlayersInGame()[i].GetDisplayName();

            // The scores are only visible if there is a player attached to them. Since
            // it is possible to enter the game alone.
            if(nameTexts[i].text != string.Empty)
            {
                scoreTexts[i].text = networkManager.GetPlayersInGame()[i].GetScore().ToString();
            }
        }
    }

    public void Score()
    {
        score++;
    }
    #endregion

    #region Setters
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }
    #endregion

    #region  Getters
    public int GetScore()
    {
        return score;
    }

    public string GetDisplayName()
    {
        return displayName;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGameInstance : NetworkBehaviour
{
    private NetworkManagerCTG networkManager = NetworkManager.singleton as NetworkManagerCTG;

    [SerializeField] private GameObject gameHUD;

    [SerializeField] private TMP_Text[] nameTexts = new TMP_Text[2];
    [SerializeField] private TMP_Text[] scoreTexts = new TMP_Text[2];

    [SyncVar]
    private string displayName;

    [SyncVar(hook = nameof(HandleScoreChange))]
    private int score = 0;

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        
        networkManager.playersInGame.Add(this);

        gameHUD.SetActive(true);

        UpdateHUD();
    }

    public override void OnStopServer()
    {
        networkManager.playersInGame.Remove(this);
    }

    public void HandleScoreChange(int oldValue, int newValue)
    {
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if(!isOwned)
        {
            foreach(PlayerGameInstance player in networkManager.playersInGame)
            {
                if(player.isOwned)
                {
                    player.UpdateHUD();

                    break;
                }
            }

            return;
        }

        for(int i = 0; i < networkManager.playersInGame.Count; i++)
        {
            nameTexts[i].text = networkManager.playersInGame[i].GetDisplayName();

            if(nameTexts[i].text != string.Empty)
            {
                scoreTexts[i].text = networkManager.playersInGame[i].GetScore().ToString();
            }
        }
    }

    public void Score()
    {
        score++;
    }

    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    public int GetScore()
    {
        return score;
    }

    public string GetDisplayName()
    {
        return displayName;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    
    private NetworkManagerCTG networkManager;

    [SerializeField] private GameObject landingPagePanel;

    [SerializeField] private TMP_InputField ipAddressInputField
    ;
    [SerializeField] private Button joinButton;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManagerCTG>();
    }

    private void OnEnable()
    {
        NetworkManagerCTG.OnClientConnected += HandleClientConnected;

        NetworkManagerCTG.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManagerCTG.OnClientConnected -= HandleClientConnected;

        NetworkManagerCTG.OnClientDisconnected -= HandleClientDisconnected;
    }


    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;

        networkManager.networkAddress = ipAddress;

        networkManager.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);

        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private NetworkManagerCTG networkManager;

    [SerializeField] private GameObject landingPagePanel;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManagerCTG>();
    }

    public void HostLobby()
    {
        networkManager.StartHost();

        landingPagePanel.SetActive(false);
    }
}

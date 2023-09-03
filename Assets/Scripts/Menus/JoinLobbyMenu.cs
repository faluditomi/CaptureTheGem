using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    #region Attributes
    private NetworkManagerCTG networkManager;

    // For the menus I refrained from gathering references in code, since I
    // Would have to be searching in transform, which would defeat the purpose
    // in my opinion.
    [SerializeField] private GameObject landingPagePanel;

    [SerializeField] private TMP_InputField ipAddressInputField;

    [SerializeField] private Button joinButton;
    #endregion

    #region MonoBehaviour Methods
    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManagerCTG>();
    }

    /* Whenever a client connects or disconnects, their menu UI will
    update automatically. */
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
    #endregion

    #region Regular Methods
    /* This is the behaviour that runs when a client presses the Join button. */
    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;

        networkManager.networkAddress = ipAddress;

        networkManager.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = false;

        gameObject.SetActive(false);

        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
    #endregion
}

using UnityEngine;

public class MainMenu : MonoBehaviour
{
    #region Attributes
    private NetworkManagerCTG networkManager;

    // For the menus I refrained from gathering references in code, since I
    // Would have to be searching in transform, which would defeat the purpose
    // in my opinion.
    [SerializeField] private GameObject landingPagePanel;
    #endregion

    #region MonoBehaviour Methods
    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManagerCTG>();
    }
    #endregion

    #region Regular Methods
    public void HostLobby()
    {
        networkManager.StartHost();

        landingPagePanel.SetActive(false);
    }
    #endregion
}

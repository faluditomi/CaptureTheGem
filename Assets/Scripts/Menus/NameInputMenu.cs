using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NameInputMenu : MonoBehaviour
{
    #region Attributes
    [SerializeField] private TMP_InputField nameInputField;

    [SerializeField] private Button continueButton;

    // It's safer to have a constant string tha nto type in the name multiple times.
    private const string playerPrefsNameKey = "Name";
    private static string displayName;
    #endregion

    #region MonoBehaviour Methods
    private void Start()
    {
        SetUpInputField();
    }
    #endregion

    #region Regular Methods
    /* This method gets called once at the start and it fills in the temporary text of the
    input field if the client has given a name previously. */
    private void SetUpInputField()
    {
        if(!PlayerPrefs.HasKey(playerPrefsNameKey))
        {
            return;
        }

        string defaultName = PlayerPrefs.GetString(playerPrefsNameKey);

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }

    /* This saves the client's name locally for future use. */
    public void SavePlayerName()
    {
        displayName = nameInputField.text;

        PlayerPrefs.SetString(playerPrefsNameKey, displayName);
    }
    #endregion

    #region Getters
    public static string GetDisplayName()
    {
        return displayName;
    }
    #endregion

    #region Setters
    public void SetPlayerName()
    {
        continueButton.interactable = !string.IsNullOrEmpty(nameInputField.text);
    }
    
    public void SetPlayerName(string name)
    {
        continueButton.interactable = !string.IsNullOrEmpty(name);
    }
    #endregion
}

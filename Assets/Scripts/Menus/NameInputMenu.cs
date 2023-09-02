using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class NameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    [SerializeField] private Button continueButton;

    private const string playerPrefsNameKey = "Name";
    private static string displayName;

    private void Start()
    {
        SetUpInputField();
    }

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

    public void SetPlayerName()
    {
        continueButton.interactable = !string.IsNullOrEmpty(nameInputField.text);
    }
    
    public void SetPlayerName(string name)
    {
        continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        displayName = nameInputField.text;

        PlayerPrefs.SetString(playerPrefsNameKey, displayName);
    }

    public static void SetDisplayName(string name)
    {
        displayName = name;
    }
}

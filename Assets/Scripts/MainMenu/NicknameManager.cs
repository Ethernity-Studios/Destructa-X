using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class NicknameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField nicknameInputField;
    [SerializeField] Button continueButton;

    public static string Nickname { get;private set; }

    const string DefaultNickname = "DefaultName";

    private void Start() => CheckNickname();

    void CheckNickname()
    {
        if (!PlayerPrefs.HasKey(DefaultNickname)) return;

        string defaultName = PlayerPrefs.GetString(DefaultNickname);

        nicknameInputField.text = defaultName;

        SetNickname(defaultName);
    }

    public void SetNickname(string name)
    {
        continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SaveNickname()
    {
        Nickname = nicknameInputField.text;

        PlayerPrefs.SetString(DefaultNickname,Nickname);
    }

    public string GetNickName()
    {
        return Nickname;
    }
}

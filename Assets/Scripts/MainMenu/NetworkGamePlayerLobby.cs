using UnityEngine;
using Mirror;

public class NetworkGamePlayerLobby : NetworkBehaviour
{
    public const string DefaultPlayerName = "DefaultName";

    public string PlayerName;

    [SerializeField] GameObject NicknameUI;

    private void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PlayerName = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void SetPlayerName(string name)
    {
        PlayerPrefs.SetString("PlayerName",name);
        PlayerName = name;
    }

    public void ConfirmNickname()
    {
        if(PlayerName != null)
        NicknameUI.SetActive(false);
    }
}

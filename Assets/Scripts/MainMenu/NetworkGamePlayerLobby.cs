using UnityEngine;
using Mirror;

public class NetworkGamePlayerLobby : NetworkBehaviour
{
    public const string DefaultPlayerName = "DefaultName";

    public string PlayerName;

    private void Start()
    {
        if (PlayerPrefs.GetString("PlayerName") != DefaultPlayerName) GetPlayerName();
    }

    public void SetPlayerName(string name)
    {
        PlayerPrefs.SetString("PlayerName",name);
    }

    public string GetPlayerName()
    {
        return PlayerPrefs.GetString("PlayerName");
    }

}

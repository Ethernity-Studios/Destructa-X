using UnityEngine;
using Mirror;

public enum Team
{
    Blue, Red
}

public class LobbyPlayer : NetworkBehaviour
{
    public static string DisplayName;
    public static Team SelectedTeam;

    private void Start()
    {
        DisplayName = NicknameManager.DisplayName;

        Debug.Log(DisplayName + SelectedTeam);
    }
}

using UnityEngine;
using Mirror;

public enum Team
{
    Non,Blue, Red
}

public class LobbyPlayer : NetworkBehaviour
{
    public string DisplayName;
    public static Team SelectedTeam;

    private void Start()
    {
        if (!isLocalPlayer) return;
        DisplayName = NicknameManager.DisplayName;
        Debug.Log(DisplayName + SelectedTeam);
    }
}

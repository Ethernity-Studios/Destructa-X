using UnityEngine;
using Mirror;
using TMPro;

public class LobbyPlayer : NetworkBehaviour
{
    public string DisplayName;
    public Team SelectedTeam;

    LobbyMenuManager lobbyMenuManager;

    private void Start()
    {
        lobbyMenuManager = FindObjectOfType<LobbyMenuManager>();
        if (!isLocalPlayer) return;
        DisplayName = NicknameManager.DisplayName;
        lobbyMenuManager.lobbyPlayers.Add(this);
    }

    /*public override void OnStopClient()
    {
        lobbyMenuManager.lobbyPlayers.Remove(this);
        if (SelectedTeam != Team.None)
        lobbyMenuManager.LeaveTeam(SelectedTeam);
        base.OnStopClient();
    }*/

}

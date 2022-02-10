using UnityEngine;
using Mirror;
using TMPro;
using System.Collections.Generic;

public enum Team
{
    None, Blue, Red
}

public class LobbyMenuManager : NetworkBehaviour
{
    [SerializeField] GameObject selectTeamUI;
    [SerializeField] GameObject agentSelectUI;


    [SerializeField] TMP_Text BlueTeamCountBtnText;
    [SerializeField] TMP_Text RedTeamCountBtnText;

    [SyncVar(hook = nameof(CmdUpdateTeamSizeUI))]
    public int BlueTeamSize;

    [SyncVar(hook = nameof(CmdUpdateTeamSizeUI))]
    public int RedTeamSize;

    public List<LobbyPlayer> lobbyPlayers = new();
    LobbyPlayer localPlayer;

    public void JoinTeam(int teamIndex)
    {
        selectTeamUI.SetActive(false);
        agentSelectUI.SetActive(true);
        foreach (var player in lobbyPlayers)
        {
            if (player.isLocalPlayer)localPlayer = player;
        }
        if (teamIndex == 1)
        {
            localPlayer.SelectedTeam = Team.Blue;
            CmdUpdateTeamSize(1, 0);
        }
        else if (teamIndex == 2)
        {
            localPlayer.SelectedTeam = Team.Red;
            CmdUpdateTeamSize(0, 1);
        }

    }
    public void LeaveTeam(Team team)
    {
        if(localPlayer.SelectedTeam == Team.Blue)
        {
            CmdUpdateTeamSize(-1,0);
            localPlayer.SelectedTeam = Team.None;
        }
        else if(localPlayer.SelectedTeam == Team.Red)
        {
            CmdUpdateTeamSize(0, -1);
            localPlayer.SelectedTeam = Team.None;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateTeamSize(int b, int r)
    {
        BlueTeamSize += b;
        RedTeamSize += r;
    }

    
    [Command(requiresAuthority = false)]
    public void CmdUpdateTeamSizeUI(int _, int newValue)
    {
        RpcUpdateTeamSize();
    }

    [ClientRpc]
    public void RpcUpdateTeamSize()
    {
        BlueTeamCountBtnText.text = BlueTeamSize.ToString();
        RedTeamCountBtnText.text = RedTeamSize.ToString();
    }

}

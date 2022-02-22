using UnityEngine;
using Mirror;
using TMPro;

public enum Team
{
    None, Blue, Red
}

public class RoomManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(CmdUpdateTeamSizeUI))]
    public int BlueTeamSize;
    [SyncVar(hook = nameof(CmdUpdateTeamSizeUI))]
    public int RedTeamSize;

    NetworkManagerRoom room;
    NetworkManagerRoom Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerRoom;
        }
    }

    [SerializeField]GameObject TeamSelectUI;
    [SerializeField]GameObject AgentSelectUI;

    [SerializeField]TMP_Text BlueTeamSizeText;
    [SerializeField]TMP_Text RedTeamSizeText;

    public void JoinTeam(int teamIndex)
    {
        foreach (var player in Room.roomSlots)
        {
            if (player.isLocalPlayer)
            {
                LobbyPlayer localPlayer = player.GetComponent<LobbyPlayer>();
                if (teamIndex == 1 && BlueTeamSize <5)
                {
                    localPlayer.PlayerTeam = Team.Blue;
                    CmdUpdateTeamSize(1,0);
                    TeamSelectUI.SetActive(false);
                    AgentSelectUI.SetActive(true);
                }
                else if (teamIndex == 2 &&RedTeamSize <5) 
                {
                    localPlayer.PlayerTeam = Team.Red;
                    CmdUpdateTeamSize(0, 1);
                    TeamSelectUI.SetActive(false);
                    AgentSelectUI.SetActive(true);
                } 
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateTeamSize(int b, int r)
    {
        BlueTeamSize += b;
        RedTeamSize += r;
    }

    [Command(requiresAuthority = false)]
    void CmdUpdateTeamSizeUI(int _, int newValue)
    {
        RpcUpdateTeamSizeUI();
    }
    [ClientRpc]
    public void RpcUpdateTeamSizeUI()
    {
        BlueTeamSizeText.text = BlueTeamSize.ToString();
        RedTeamSizeText.text = RedTeamSize.ToString();
    }
}

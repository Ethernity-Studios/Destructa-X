using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkRoomPlayer
{
    RoomManager roomManager;
    AgentManager agentManager;

    NetworkManagerRoom room;
    NetworkManagerRoom Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerRoom;
        }
    }
    [SyncVar]
    public string PlayerName;
    [SyncVar]
    public Team PlayerTeam;
    [SyncVar]
    public Agent PlayerPreselectedAgent;
    [SyncVar]
    public Agent PlayerSelectedAgent;

    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;
        roomManager = FindObjectOfType<RoomManager>();
        agentManager = FindObjectOfType<AgentManager>();

        CmdSetNickname(NicknameManager.DisplayName);

        base.OnStartClient();
    }

    public override void OnClientExitRoom()
    {
        roomManager = FindObjectOfType<RoomManager>();
        int tempB = 0, tempR = 0;
        foreach (var player in Room.roomSlots)
        {
            if (player.isLocalPlayer)
            {
                LobbyPlayer localPlayer = player.GetComponent<LobbyPlayer>();
                switch (localPlayer.PlayerTeam)
                {
                    case Team.None:
                        break;
                    case Team.Blue:
                        tempB++;
                        break;
                    case Team.Red:
                        tempR++;
                        break;
                }
                roomManager.BlueTeamSize = tempB;
                roomManager.RedTeamSize = tempR;
            }
        }

        base.OnClientExitRoom();
    }

    #region Command synchronization

    [Command]
    public void CmdJoinTeam(Team team)
    {
        PlayerTeam = team;
    }

    [Command]
    public void CmdSetNickname(string name)
    {
        PlayerName = name;
    }

    [Command]
    public void CmdPreselectAgent(Agent agent)
    {
        PlayerPreselectedAgent = agent;
    }

    [Command]
    public void CmdSelectAgent(Agent agent)
    {
        PlayerSelectedAgent = agent;
    }
    #endregion
}

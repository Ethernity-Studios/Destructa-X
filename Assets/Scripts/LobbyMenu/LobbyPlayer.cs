using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkRoomPlayer
{
    RoomManager roomManager;

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

    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;
        roomManager = FindObjectOfType<RoomManager>();

        PlayerName = NicknameManager.DisplayName;

        UpdateLocalPlayerInfo();

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

    public void UpdateLocalPlayerInfo()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.roomSlots)
            {
                if (player.hasAuthority)
                {
                    player.GetComponent<LobbyPlayer>().UpdateLocalPlayerInfo();
                    break;
                }
            }
            return;
        }


    }

    void UpdateUI()
    {

    }

}

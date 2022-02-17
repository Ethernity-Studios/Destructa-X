using UnityEngine;
using Mirror;
using TMPro;

public class LobbyPlayer : NetworkRoomPlayer
{
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
    string PlayerName;
    [SyncVar]
    Team PlayerTeam;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStopClient()
    {

    }

    void UpdateUI()
    {

    }

}

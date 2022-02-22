using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManagerRoom : NetworkRoomManager
{
    RoomManager roomManager;

    /*public override void OnRoomServerDisconnect(NetworkConnection conn)
    {
        LobbyPlayer player = conn.identity.GetComponent<LobbyPlayer>();
        if (player.PlayerTeam == Team.Blue) roomManager.BlueTeamSize--;
        else if (player.PlayerTeam == Team.Red) roomManager.RedTeamSize--;
        roomManager.RpcUpdateTeamSizeUI();
        base.OnRoomServerDisconnect(conn);
    }*/

}

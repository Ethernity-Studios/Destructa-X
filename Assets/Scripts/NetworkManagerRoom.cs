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

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }

        base.OnStartClient();
    }

    public override void OnRoomServerConnect(NetworkConnection conn)
    {
        NetworkServer.SetClientReady(conn);
        base.OnRoomServerConnect(conn);
    }
}

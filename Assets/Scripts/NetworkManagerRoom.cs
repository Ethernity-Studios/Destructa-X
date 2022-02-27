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

    public string SelectedMap;

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnRoomStartServer()
    {
        base.OnRoomStartServer();
    }

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

    public override void OnRoomServerPlayersReady()
    {
        roomManager = FindObjectOfType<RoomManager>();
        Debug.Log("Players ready!");
        roomManager.RpcCountdown(5);
    }

    public void StartGame(string mapName)
    {
        ServerChangeScene(mapName);
    }

}

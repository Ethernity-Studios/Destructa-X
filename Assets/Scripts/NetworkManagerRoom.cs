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

    public List<LobbyPlayer> Players = new();

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
        roomManager.RpcCountdown(1);

        foreach (var item in roomSlots)
        {
            Players.Add(item.GetComponent<LobbyPlayer>());
        }
    }

    public void StartGame(string mapName)
    {
        ServerChangeScene(mapName);
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
    }
}


using Mirror;
using System;
using System.Linq;
using UnityEngine;

public class NetworkManagerRoom : NetworkRoomManager
{
    RoomManager roomManager;

    public string SelectedMap;

    [SerializeField] GameObject loadingScreen;

    //public static event Action OnClientConnected;
    //public static event Action OnClientDisconnected;

    public static event Action<NetworkConnection> OnServerReadied;

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        
        base.OnStartServer();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        /*roomManager = FindObjectOfType<RoomManager>();
        if(roomManager != null)
            roomManager.PlayerDisconnect(conn);*/
        base.OnServerDisconnect(conn);
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
        
        base.OnRoomServerPlayersReady();
        
    }

    public void StartGame(string mapName)
    {
        ServerChangeScene(mapName);
    }
    
    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }
}

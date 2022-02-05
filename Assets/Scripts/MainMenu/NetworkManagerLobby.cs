using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkManagerLobby : NetworkManager
{
    [SerializeField] NetworkPlayer roomPlayerPrefab;

    [Scene] [SerializeField] string menuScene;
    public override void OnStartServer()
    {
        base.OnStartServer();
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList<GameObject>();
    }

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        if(numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name == menuScene)
        {
            NetworkPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);

            NetworkServer.AddPlayerForConnection(conn, roomPlayerPrefab.gameObject);
        }
    }
}

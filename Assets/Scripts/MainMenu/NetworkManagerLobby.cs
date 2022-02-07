using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkManagerLobby : NetworkRoomManager
{
    [Scene] [SerializeField] string GameScene;

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList<GameObject>();
        Debug.Log("OnStartServer");
    }

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    Button StartButton;

    /*public override void OnRoomServerPlayersReady()
    {
        StartButton = FindObjectOfType<Button>();
#if UNITY_SERVER
            base.OnRoomServerPlayersReady();
#else
        StartButton.interactable = true;
#endif
    }*/

    void StartGame()
    {
        ServerChangeScene(GameScene);
    }


}

using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkManagerLobby : NetworkRoomManager
{
    List<RoomPlayer> roomPlayers = new();
    [Scene] [SerializeField] string GameScene;

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

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

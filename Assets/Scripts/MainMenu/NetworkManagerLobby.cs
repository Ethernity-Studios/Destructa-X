using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkManagerLobby : NetworkRoomManager
{
    [Scene] [SerializeField] string GameScene;

    LobbyMenuManager lobbyMenuManager;

    new private void Start()
    {
        lobbyMenuManager = FindObjectOfType<LobbyMenuManager>();
    }

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

    public override void OnRoomClientExit()
    {
        /*foreach (var player in roomSlots)
        {
            if (player.isLocalPlayer)
            {
                LobbyPlayer localPlayer;
                localPlayer = player.GetComponent<LobbyPlayer>();
                if (localPlayer.SelectedTeam == Team.Blue) lobbyMenuManager.LeaveTeam(Team.Blue);
                else if (localPlayer.SelectedTeam == Team.Red) lobbyMenuManager.LeaveTeam(Team.Red);
            }
        }*/
        base.OnRoomClientDisconnect();
    }

    public override void OnRoomServerDisconnect(NetworkConnection conn)
    {
        LobbyPlayer localPlayer = conn.identity.GetComponent<LobbyPlayer>();
        if (localPlayer.SelectedTeam == Team.Blue)
        {
            lobbyMenuManager.BlueTeamSize--;
            Debug.Log("new Blue size" + lobbyMenuManager.BlueTeamSize);
            lobbyMenuManager.RpcUpdateTeamSize();
        }
        else if (localPlayer.SelectedTeam == Team.Red) 
        {
            lobbyMenuManager.RedTeamSize--;
            Debug.Log("new Red size" + lobbyMenuManager.RedTeamSize);
            lobbyMenuManager.RpcUpdateTeamSize();
        } 
        base.OnRoomServerDisconnect(conn);
    }
}

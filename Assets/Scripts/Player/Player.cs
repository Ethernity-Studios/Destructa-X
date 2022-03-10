using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;
    [SyncVar]
    public Team PlayerTeam;
    [SyncVar]
    public Agent PlayerAgent;

    [SyncVar]
    public int PlayerMoney;
    [SyncVar]
    public int PlayerKills;
    [SyncVar]
    public int PlayerDeaths;
    [SyncVar]
    public int PlayerAssists;

    public override void OnStartLocalPlayer()
    {
        if (!isLocalPlayer) return;
        SetPlayerInfo(NicknameManager.DisplayName,RoomManager.PTeam, RoomManager.PAgent);
        base.OnStartLocalPlayer();
    }

    [Command]
    public void SetPlayerInfo(string name, Team team, Agent agent)
    {
        PlayerName = name;
        PlayerTeam = team;
        PlayerAgent = agent;
    }
}

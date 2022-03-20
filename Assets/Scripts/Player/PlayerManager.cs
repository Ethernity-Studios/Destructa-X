using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerManager : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;
    [SyncVar]
    public Team PlayerTeam;
    [SyncVar]
    public Agent PlayerAgent;

    [SyncVar]
    public int PlayerMoney = 800;
    [SyncVar]
    public int PlayerKills;
    [SyncVar]
    public int PlayerDeaths;
    [SyncVar]
    public int PlayerAssists;

    [SyncVar]
    public bool IsDeath = false;

    [SerializeField] GameObject UIAgent;

    GameManager gameManager;
    AgentManager agentManager;
    public void Start()
    {

    }

    public override void OnStartLocalPlayer()
    {
        if (!isLocalPlayer) return;
        SetPlayerInfo(NicknameManager.DisplayName, RoomManager.PTeam, RoomManager.PAgent);
        base.OnStartLocalPlayer();
    }

    [Command]
    public void SetPlayerInfo(string name, Team team, Agent agent)
    {
        PlayerName = name;
        PlayerTeam = team;
        PlayerAgent = agent;
    }

    [Command]
    public void SwitchPlayerTeam(Team team)
    {
        PlayerTeam = team;
    }

    [Command]
    public void SwitchPlayerAgent(Agent agent)
    {
        PlayerAgent = agent;
    }

    [Command]
    public void AddMoney(int Money)
    {
        PlayerMoney = Money;
    }
}

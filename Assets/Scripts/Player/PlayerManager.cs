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
        test();
    }

    [Command(requiresAuthority = false)]
    void test()
    {
        testt();
    }
    [ClientRpc]
    void testt()
    {
        agentManager = FindObjectOfType<AgentManager>();
        gameManager = FindObjectOfType<GameManager>();
        UIAgent.transform.SetParent(gameManager.BlueAgents);
        UIAgent.GetComponent<RectTransform>().localScale = Vector3.one;
        UIAgent.transform.GetChild(0).GetComponent<Image>().sprite = agentManager.GetAgentMeta(PlayerAgent).Meta.Icon;
    }

    public override void OnStartLocalPlayer()
    {
        if (!isLocalPlayer) return;
        SetPlayerInfo(NicknameManager.DisplayName, RoomManager.PTeam, RoomManager.PAgent);
        //test(PlayerAgent);
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

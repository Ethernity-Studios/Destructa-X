using Mirror;
using UnityEngine;
using UnityEngine.UI;

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
    NetworkManagerRoom room;

    public PlayerState PlayerState;
    private void Awake()
    {
        if (!isLocalPlayer) return;

        PlayerState = PlayerState.Idle;
        gameManager = FindObjectOfType<GameManager>();  
        room = FindObjectOfType<NetworkManagerRoom>();
    }
    public void Start()
    {
        Invoke("SpawnUIAgent", .2f);
        if (!isLocalPlayer) return;
        CmdSetPlayerInfo(NicknameManager.DisplayName, RoomManager.PTeam, RoomManager.PAgent);

    }

    public override void OnStartLocalPlayer()
    {
        Invoke("CmdAddPlayer", .5f);
        Debug.Log("on start local");
        base.OnStartLocalPlayer();
    }

    void SpawnUIAgent()
    {
        gameManager = FindObjectOfType<GameManager>();
        agentManager = FindObjectOfType<AgentManager>();
        GameObject UIAgent = Instantiate(this.UIAgent);
        UIAgent.name = PlayerName;
        UIAgent.transform.GetChild(0).GetComponent<Image>().sprite = agentManager.GetAgentMeta(PlayerAgent).Meta.Icon;
        switch (PlayerTeam)
        {
            case Team.Blue:
                UIAgent.transform.SetParent(gameManager.BlueUIAgents);
                break;
            case Team.Red:
                UIAgent.transform.SetParent(gameManager.RedUIAgents);
                break;
        }
        UIAgent.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    [Command]
    public void CmdAddPlayer()
    {
        gameManager = FindObjectOfType<GameManager>();
        gameManager.Players.Add(this);
    }

    [Command]
    public void CmdSetPlayerInfo(string name, Team team, Agent agent)
    {
        PlayerName = name;
        PlayerTeam = team;
        PlayerAgent = agent;
    }

    [Command]
    public void CmdSwitchPlayerTeam(Team team)
    {
        PlayerTeam = team;
    }

    [Command]
    public void CmdSwitchPlayerAgent(Agent agent)
    {
        PlayerAgent = agent;
    }

    [Command]
    public void CmdAddMoney(int Money)
    {
        PlayerMoney = Money;
    }
}

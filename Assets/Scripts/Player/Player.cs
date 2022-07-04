using Mirror;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerState
{
    Idle, Walk, Run, Crouch, Jump, Planting, Defusing, Dead
}

public class Player : NetworkBehaviour, IDamageable
{
    [SyncVar]
    public string PlayerName;
    [SyncVar]
    public Team PlayerTeam;
    [SyncVar]
    public Agent PlayerAgent;

    [SyncVar(hook = nameof(updateMoneyText))]
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
    ShopManager shopManager;
    AgentManager agentManager;
    UIManager uiManager;
    CharacterController characterController;
    NetworkManagerRoom room;

    public PlayerState PlayerState;

    [SerializeField] GameObject playerBody;

    [SyncVar(hook = nameof(updateHealth))]
    public int Health;

    [SyncVar(hook = nameof(updateShield))]
    public int Shield;
    public int MaxShield;

    private void Awake()
    {
        PlayerState = PlayerState.Idle;
        uiManager = FindObjectOfType<UIManager>();
        shopManager = FindObjectOfType<ShopManager>();
        gameManager = FindObjectOfType<GameManager>();
        room = FindObjectOfType<NetworkManagerRoom>();
    }

    public void Start()
    {
        Invoke("setPlayerBody", 2f);
        Invoke("spawnUIAgent", .3f);
        if (!isLocalPlayer) return;
        Invoke("CmdAddPlayer", .3f);
        Cursor.lockState = CursorLockMode.Locked;
        CmdSetPlayerInfo(NicknameManager.DisplayName, RoomManager.PTeam, RoomManager.PAgent);
    }

    void setPlayerBody()
    {
        if(!isLocalPlayer)
        playerBody.layer = 10;
    }

    void spawnUIAgent()
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
    public void CmdSwitchPlayerTeam(Team team) => PlayerTeam = team;

    [Command]
    public void CmdSwitchPlayerAgent(Agent agent) => PlayerAgent = agent;

    [Command]
    public void CmdChangeMoney(int money) => PlayerMoney += money;

    [Command(requiresAuthority = false)]
    public void CmdTakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0) CmdKillPlayer();
    }

    [Command]
    public void CmdAddHealth(int health) => Health += health;

    [ClientRpc]
    public void RespawnPlayer(Vector3 position)
    {
        characterController = GetComponent<CharacterController>();
        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;
    }

    [Command(requiresAuthority = false)]
    public void CmdKillPlayer()
    {
        Debug.Log("Killing player: " + PlayerName);
        IsDeath = true;
    }

    void updateHealth(int _, int newValue)
    {
        uiManager.Health.text = newValue.ToString();
    }

    void updateShield(int _, int newValue)
    {
        uiManager.Shield.text = newValue.ToString();
    }

    void updateMoneyText(int _, int newValue)
    {
        uiManager.Money.text = newValue.ToString();
    }

    public void TakeDamage(int damage) => CmdTakeDamage(damage);

    public void AddHealth(int health) => CmdAddHealth(health);
}

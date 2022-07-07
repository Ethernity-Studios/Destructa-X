using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum PlayerState
{
    Idle, Walk, Run, Crouch, Jump, Planting, Defusing, Dead
}

public class Player : NetworkBehaviour, IDamageable
{
    [SyncVar]
    public string PlayerName = "";
    [SyncVar]
    public Team PlayerTeam;
    [SyncVar]
    public Agent PlayerAgent;

    [SyncVar(hook = nameof(updateMoneyText))]
    public int PlayerMoney = 800;
    [SyncVar]
    public int PlayerKills = 0;
    [SyncVar]
    public int PlayerDeaths = 0;
    [SyncVar]
    public int PlayerAssists = 0;

    [SyncVar]
    public bool IsDead = false;

    [SerializeField] GameObject UIAgent;

    GameManager gameManager;
    ShopManager shopManager;
    AgentManager agentManager;
    PlayerSpectateManager playerSpectateManager;
    PlayerInventoryManager playerInventoryManager;
    PlayerShootingManager playerShootingManager;
    PlayerBombManager playerBombManager;
    UIManager uiManager;
    CharacterController characterController;
    NetworkManagerRoom room;

    public PlayerState PlayerState;

    [SerializeField] GameObject playerBody;

    [SyncVar(hook = nameof(updateHealth))]
    public int Health = 100;

    [SyncVar(hook = nameof(updateShield))]
    public int Shield = 0;
    [SyncVar]
    public int PreviousRoundShield = 0;

    [SyncVar]
    public int RoundKills = 0;

    [SyncVar]
    public ShieldType ShieldType = ShieldType.None;

    public void Start()
    {
        PlayerState = PlayerState.Idle;
        playerSpectateManager = GetComponent<PlayerSpectateManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerShootingManager = GetComponent<PlayerShootingManager>();
        playerBombManager = GetComponent<PlayerBombManager>();
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        shopManager = FindObjectOfType<ShopManager>();

        room = FindObjectOfType<NetworkManagerRoom>();

        Invoke("setPlayerBody", 2f);
        Invoke("spawnUIAgent", .3f);
        if (!isLocalPlayer) return;
        Cursor.lockState = CursorLockMode.Locked;
        CmdSetPlayerInfo(NicknameManager.DisplayName, RoomManager.PTeam, RoomManager.PAgent);
    }

    public override void OnStartLocalPlayer()
    {
        Invoke("CmdAddPlayer", .3f);
        base.OnStartLocalPlayer();
    }

    void setPlayerBody()
    {
        if (!isLocalPlayer)
        {
            playerBody.layer = 10;
        }
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
        //gameManager.Players.Add(this);
        gameManager.PlayersID.Add(netId);
        if (PlayerTeam == Team.Blue)
        {
            gameManager.BlueTeam.Add(this);
            gameManager.AliveBluePlayers++;
            gameManager.BlueTeamPlayersIDs.Add(netId);
        }
        else if (PlayerTeam == Team.Red) 
        {
            gameManager.RedTeam.Add(this);
            gameManager.AliveRedPlayers++;
            gameManager.RedTeamPlayersIDs.Add(netId);
        } 
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

    [Command(requiresAuthority = false)]
    public void CmdChangeMoney(int money) 
    {
        PlayerMoney += money;
        if (PlayerMoney > 9000) PlayerMoney = 9000;
    } 

    [Command(requiresAuthority = false)]
    public void CmdTakeDamage(int damage)
    {
        Debug.Log(this + " Taking damage");
        Health -= damage;
        if (Health <= 0)
        {
            CmdKillPlayer();
        }
    }

    [Command]
    public void CmdAddHealth(int health) => Health += health;

    [Command]
    public void CmdSetShield(int shield) => Shield = shield;
    [Command]
    public void CmdSetShieldType(ShieldType shieldType) => ShieldType = shieldType;

    [Command]
    public void CmdAddKill() => PlayerKills++;
    [Command]
    public void CmdAddRoundKill() => RoundKills++;

    [ClientRpc]
    public void RpcRespawnPlayer(Vector3 position)
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
        IsDead = true;
        PlayerState = PlayerState.Dead;
        if (PlayerTeam == Team.Blue) gameManager.AliveBluePlayers--;
        else if(PlayerTeam == Team.Red) gameManager.AliveRedPlayers--;
        RpcKillPlayer();
        TargetRpcKillPlayer(connectionToClient);
    }

    [ClientRpc]
    void RpcKillPlayer()
    {
        playerSpectateManager.PlayerDeath();
    }

    [TargetRpc]
    void TargetRpcKillPlayer(NetworkConnection conn)
    {
        playerBombManager.StopAllCoroutines();
        playerShootingManager.StopAllCoroutines();
        playerInventoryManager.StopAllCoroutines();
        dropItems();
        playerSpectateManager.StartCoroutine(playerSpectateManager.PlayerDeathCoroutine());
    }

    void dropItems()
    {
        if (playerInventoryManager.Bomb != null) playerInventoryManager.CmdDropBomb();

        if (playerInventoryManager.PrimaryGun != null)
        {
            playerInventoryManager.CmdDropGun(GunType.Primary);
            playerInventoryManager.CmdDestroyGun(GunType.Secondary);
        }
        else if (playerInventoryManager.PrimaryGun == null && playerInventoryManager.SecondaryGun != null)
        {
            playerInventoryManager.CmdDropGun(GunType.Secondary);
        }
        playerInventoryManager.CmdSwitchItem(Item.Knife);
    }

    void updateHealth(int _, int newValue)
    {
        if (!isLocalPlayer) return;
        uiManager.Health.text = newValue.ToString();
        if (newValue < 0) uiManager.Health.text = 0.ToString();
    }

    void updateShield(int _, int newValue)
    {
        if (isLocalPlayer)
            uiManager.Shield.text = newValue.ToString();
    }

    void updateMoneyText(int _, int newValue)
    {
        if (isLocalPlayer)
            uiManager.Money.text = newValue.ToString();
    }

    public bool TakeDamage(int damage)
    {
        int h = Health;
        CmdTakeDamage(damage);
        return damage >= h;   
    }

    public void AddHealth(int health) => CmdAddHealth(health);

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(10);
    }
}

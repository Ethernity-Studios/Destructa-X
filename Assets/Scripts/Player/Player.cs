using System.Linq;
using Mirror;
using player;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Walk,
    Run,
    Crouch,
    Jump,
    Planting,
    Defusing,
    Dead
}

public class Player : NetworkBehaviour, IDamageable
{
    [SyncVar] public string PlayerName = "";
    [SyncVar] public Team PlayerTeam;
    [SyncVar] public Agent PlayerAgent;

    [SyncVar(hook = nameof(updateMoneyText))]
    public int PlayerMoney = 800;

    [SyncVar] public int EnemyPlayerMoney = 800;

    [SyncVar] public double Ping;


    [SyncVar] public int PlayerKills;
    [SyncVar] public int PlayerDeaths;
    [SyncVar] public int PlayerAssists;
    [SyncVar] public bool IsDead;

    //[SerializeField] GameObject UIAgent;

    GameManager gameManager;
    PlayerSpectateManager playerSpectateManager;
    PlayerInventoryManager playerInventoryManager;
    PlayerShootingManager playerShootingManager;
    PlayerBombManager playerBombManager;
    UIManager uiManager;
    CharacterController characterController;
    private PlayerUI playerUI;

    // TODO maybe syncVar
    public PlayerState PlayerState;

    [SerializeField] GameObject playerBody;

    [SyncVar(hook = nameof(updateHealth))] public int Health = 100;

    [SyncVar(hook = nameof(updateShield))] public int Shield = 0;
    [SyncVar] public int PreviousShield = 0;

    [SyncVar] public int RoundKills = 0;

    [SyncVar] public ShieldType ShieldType = ShieldType.None;

    private Camera _camera;


    [TargetRpc]
    public void RpcSetState(PlayerState state)
    {
        PlayerState = state;
    }

    public void Start()
    {
        _camera = Camera.main;
        if (isServer)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            CmdSetPlayerInfo(NicknameManager.DisplayName, RoomManager.PTeam, RoomManager.PAgent);
        }

        PlayerState = PlayerState.Idle;
        playerSpectateManager = GetComponent<PlayerSpectateManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerShootingManager = GetComponent<PlayerShootingManager>();
        playerBombManager = GetComponent<PlayerBombManager>();
        playerUI = GetComponent<PlayerUI>();
        GetComponent<PlayerCombatReport>();
        uiManager = FindObjectOfType<UIManager>();

        setPlayerBody();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        CmdUpdatePing(NetworkTime.rtt);
        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(10);
    }

    [Command]
    void CmdUpdatePing(double ping)
    {
        Ping = ping;
    }


    void setPlayerBody()
    {
        if (!isLocalPlayer)
        {
            playerBody.layer = 10;
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
        uiManager.ShopPlayerMoney.text = PlayerMoney.ToString();
        foreach (var p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            playerUI.RpcUpdatePlayerTeamMoney(p.netIdentity.connectionToClient, PlayerMoney);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdResetMoney()
    {
        PlayerMoney = 800;
        uiManager.ShopPlayerMoney.text = PlayerMoney.ToString();
        foreach (var p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            playerUI.RpcUpdatePlayerTeamMoney(p.netIdentity.connectionToClient, PlayerMoney);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdTakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            CmdKillPlayer();
        }
    }

    [Command]
    private void CmdAddHealth(int health) => Health += health;

    [Command]
    public void CmdSetShield(int shield) => Shield = shield;

    [Command]
    public void CmdSetPreviousShield(int shield) => PreviousShield = shield;

    [Command]
    public void CmdSetShieldType(ShieldType shieldType) => ShieldType = shieldType;

    [Command]
    public void CmdAddKill() => PlayerKills++;

    [Command]
    public void CmdAddAssist() => PlayerAssists++;

    [Command]
    public void CmdAddDeath() => PlayerDeaths++;

    [ClientRpc]
    public void RpcRespawnPlayer(Vector3 position, Vector3 rotation)
    {
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);

        _camera!.transform.GetComponent<CameraRotate>().RotateCamera(rotation);
    }

    [Command(requiresAuthority = false)]
    public void CmdKillPlayer()
    {
        Debug.Log("Killing player: " + PlayerName);
        IsDead = true;
        PlayerState = PlayerState.Dead;
        switch (PlayerTeam)
        {
            case Team.Blue:
                gameManager.AliveBluePlayers--;
                break;
            case Team.Red:
                gameManager.AliveRedPlayers--;
                break;
        }

        RpcKillPlayer();
        TargetRpcKillPlayer(connectionToClient);
    }

    [ClientRpc]
    void RpcKillPlayer()
    {
        playerSpectateManager.PlayerDeath();
        if (isLocalPlayer) CmdAddDeath();
    }

    [TargetRpc]
    void TargetRpcKillPlayer(NetworkConnection conn)
    {
        dropItems();
        playerUI.CmdDestroyPlayerHeader();
        playerBombManager.StopAllCoroutines();
        playerShootingManager.StopAllCoroutines();
        playerInventoryManager.StopAllCoroutines();
        playerShootingManager.CanShoot = true;
        playerShootingManager.Reloading = false;
        playerSpectateManager.StartCoroutine(playerSpectateManager.PlayerDeathCoroutine());
    }

    void dropItems()
    {
        if (playerInventoryManager.Bomb != null) playerInventoryManager.CmdDropBomb();

        if (playerInventoryManager.PrimaryGun != null)
        {
            playerInventoryManager.CmdSwitchItem(Item.Primary);
            playerInventoryManager.CmdDropGun(GunType.Primary);
            playerInventoryManager.CmdDestroyGun(GunType.Secondary);
        }
        else if (playerInventoryManager.PrimaryGun == null && playerInventoryManager.SecondaryGun != null)
        {
            playerInventoryManager.CmdSwitchItem(Item.Secondary);
            playerInventoryManager.CmdDropGun(GunType.Secondary);
        }

        playerInventoryManager.CmdSwitchItem(Item.Knife);
    }

    void updateHealth(int _, int newValue)
    {
        if (!isLocalPlayer) return;
        CmdUpdateHealth(newValue);
        uiManager.Health.text = newValue.ToString();
        uiManager.HealthBar.fillAmount = newValue * .01f;

        if (newValue >= 0) return;
        uiManager.Health.text = 0.ToString();
        uiManager.HealthBar.fillAmount = 0;
    }

    [Command]
    void CmdUpdateHealth(int value)
    {
        foreach (Player p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            if (p.PlayerTeam == PlayerTeam) RpcUpdateHealth(p.netIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    void RpcUpdateHealth(NetworkConnection conn, int value)
    {
        if (playerUI.HeaderPlayer == null) return;
        PlayerHeader playerHeader = playerUI.HeaderPlayer.GetComponent<PlayerHeader>();

        playerHeader.Health.fillAmount = value * .01f;
        if (value >= 0) return;
        playerHeader.Health.fillAmount = 0;
    }

    void updateShield(int _, int newValue)
    {
        if (!isLocalPlayer) return;
        uiManager.Shield.text = newValue.ToString();
        uiManager.ShieldBar.fillAmount = newValue / 100;
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
}
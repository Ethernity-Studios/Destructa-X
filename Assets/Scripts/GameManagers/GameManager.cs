using Mirror;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    StartGame, PreRound, Round, PostRound, EndGame
}

public enum BombState
{
    NotPlanted, Planted, Exploded, Defused
}

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public int Round = 0;

    public int RoundsPerHalf = 13;

    public float StartGameLenght = 10; //45s
    public float EndgameLenght = 5; //10s

    public float PreRoundLenght = 20; //30s
    public float RoundLenght = 10; //1m 40s
    public float PostRoundlenght = 5; //5s

    public float BombPlantTime = 5;
    public float BombHalfDefuseTime = 5;
    public float BombDefuseTime = 10;
    public float BombDetonationTime = 15; //40

    [SyncVar]
    public float GameTime = 5;

    [SyncVar]
    public GameState GameState = GameState.StartGame;
    [SyncVar]
    public BombState BombState = BombState.NotPlanted;

    [SerializeField] TMP_Text roundTimer;

    public Transform BlueUIAgents, RedUIAgents;

    [SyncVar]
    public int AliveBluePlayers = 0;
    [SyncVar]
    public int AliveRedPlayers = 0;

    public Slider PlantProgressSlider;
    public Slider DefuseProgressSlider;

    public GameObject ShopUI;
    [SerializeField] GameObject MOTD;

    [SerializeField] Transform[] blueSpawnPositions, redSpawnPositions;

    public GameObject BombPrefab;
    public GameObject GunHolder;
    [SerializeField] Transform bombSpawnLocation;

    public GameObject Bomb;

    [SerializeField] GunManager gunManager;
    public ShopManager shopManager;
    [SerializeField] MapController mapController;

    public GameObject BulletHolder;

    [SyncVar(hook = nameof(updateBlueTeamScore))]
    public int BlueTeamScore = 0;
    [SyncVar(hook = nameof(updateRedTeamScore))]
    public int RedTeamScore = 0;

    [SerializeField] TMP_Text BlueTeamScoreText;
    [SerializeField] TMP_Text RedTeamScoreText;

    [SyncVar]
    public bool BombPlanted = false;

    [SyncVar]
    public Team LosingTeam = Team.None;
    [SyncVar]
    public int LossStreak = 0;

    readonly public SyncList<uint> PlayersID = new();
    readonly public SyncList<uint> BlueTeamPlayersIDs = new();
    readonly public SyncList<uint> RedTeamPlayersIDs = new();

    [SyncVar]
    [SerializeField] bool GameReady;
    [SerializeField] GameObject loadingScreen;

    private NetworkManagerRoom room;
    private NetworkManagerRoom Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerRoom;
        }
    }


    private void Start()
    {
        ShopUI.SetActive(false);
        PlantProgressSlider.gameObject.SetActive(false);
        DefuseProgressSlider.gameObject.SetActive(false);
    }

    public override void OnStartServer()
    {
        NetworkManagerRoom.OnServerReadied += startGame;
        Debug.Log("on start server");
    }

    [Server]
    void startGame(NetworkConnection conn)
    {
        if(Room.roomSlots.Count(x => x.connectionToClient.isReady) != Room.roomSlots.Count) { return; }

        Debug.Log("Game ready! starting game in 3 second!");


        Invoke("setupGame", 3f);
    }

    void cmdSetupGame()
    {
        GameReady = true;
        GameTime = StartGameLenght;
        BombState = BombState.NotPlanted;
        Invoke("RpcSetupGame",2f);
        Invoke("spawnBomb", 1f);
        Invoke("spawnPlayers", 1.5f);
        Invoke("giveDefaultGun", 2f);
        StartRound(GameState.StartGame);
    }

    [ClientRpc]
    void RpcSetupGame()
    {
        loadingScreen.SetActive(false);
    }

    void spawnPlayers()
    {
        int b = 0;
        int r = 0;
        foreach (var playerID in PlayersID)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            if (player.PlayerTeam == Team.Blue)
            {
                player.RpcRespawnPlayer(blueSpawnPositions[b].position, blueSpawnPositions[b].rotation);
                setPlayerColor(player, Color.blue);
                b++;
            }
            else if (player.PlayerTeam == Team.Red)
            {
                player.RpcRespawnPlayer(redSpawnPositions[r].position, redSpawnPositions[r].rotation);
                setPlayerColor(player, Color.red);
                r++;
            }

        }
    }

    [ClientRpc]
    void setPlayerColor(Player player, Color color)
    {
        player.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = color;
    }

    private void Update()
    {
        if (!GameReady) return;
        updateRoundTimer();
        if (isServer) updateGameState();

        if (GameTime > 0) GameTime -= Time.deltaTime;
    }

    void giveDefaultGun()
    {
        foreach (var playerID in PlayersID)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            playerInventory.CmdGiveGun(gunManager.gunList[0].GunID);
            playerInventory.CmdSwitchItem(Item.Knife);
            playerInventory.CmdSwitchItem(Item.Secondary);
            playerInventory.CmdSwitchItem(Item.Knife);
        }
    }

    void spawnBomb()
    {
        Debug.Log("spawnBomb");
        GameObject bombInstance = Instantiate(BombPrefab);
        NetworkServer.Spawn(bombInstance);
        RpcSpawnBomb(bombInstance);
    }

    [ClientRpc]
    void RpcSpawnBomb(GameObject bombInstance)
    {
        Debug.Log("RpcSpawnBomb");
        Bomb = bombInstance;
        Bomb.transform.SetParent(gameObject.transform);
        Bomb.transform.position = bombSpawnLocation.position;
    }

    #region RoundManagement
    void updateRoundTimer()
    {
        var sec = Convert.ToInt32(GameTime % 60).ToString("00");
        var min = (Mathf.Floor(GameTime / 60) % 60).ToString("00");
        roundTimer.text = min + ":" + sec;
        if (GameTime <= 0) roundTimer.text = "00:00";
    }

    void updateBlueTeamScore(int _, int newValue) => BlueTeamScoreText.text = newValue.ToString();
    void updateRedTeamScore(int _, int newValue) => RedTeamScoreText.text = newValue.ToString();


    void updateGameState()
    {
        if (GameState == GameState.StartGame && GameTime <= 0)
        {
            //Buy phase - Start
            mapController.RpcDropWalls();
            closePlayerShopUI();
            CmdSetGameTime(RoundLenght);
            CmdChangeGameState(GameState.Round);
            RpcToggleMOTD(false);
        }
        else if (GameState == GameState.PreRound && GameTime <= 0)
        {
            //Buy phase
            mapController.RpcDropWalls();
            closePlayerShopUI();
            CmdChangeGameState(GameState.Round);
            CmdSetGameTime(RoundLenght);
            RpcToggleMOTD(false);
        }
        else if (BombState == BombState.Planted && GameTime <= 0)
        {
            //Bomb explosion
            //BombManager bombManager = FindObjectOfType<BombManager>();
            //bombManager.CmdDetonateBomb();
            CmdChangeBombState(BombState.Exploded);
            CmdSetGameTime(PostRoundlenght);
            CmdChangeGameState(GameState.PostRound);
        }
        else if (GameState == GameState.Round && GameTime <= 0)
        {
            //end round
            CmdSetGameTime(PostRoundlenght);
            CmdChangeGameState(GameState.PostRound);
        }
        else if (GameState == GameState.Round && AliveBluePlayers <= 0 && BlueTeamPlayersIDs.Count > 0 && PlayersID.Count > 1)
        {
            //All blue players dead
            Debug.Log("All blue players dead");
            CmdSetGameTime(PostRoundlenght);
            CmdChangeGameState(GameState.PostRound);
        }
        else if (GameState == GameState.Round && BombState == BombState.NotPlanted && AliveRedPlayers <= 0 && RedTeamPlayersIDs.Count > 0 && PlayersID.Count > 1)
        {
            //Bomb not planted and all red players dead
            Debug.Log("Bomb not planted and all red players dead");
            CmdSetGameTime(PostRoundlenght);
            CmdChangeGameState(GameState.PostRound);
        }
        else if (GameState == GameState.PostRound && GameTime <= 0)
        {
            //time's up
            Debug.Log("Time's up");
            startNewRound();
        }
    }

    void startNewRound()
    {
        AliveBluePlayers = BlueTeamPlayersIDs.Count;
        AliveRedPlayers = RedTeamPlayersIDs.Count;
        addScore();
        giveMoney();

        if (BombState == BombState.Exploded || BombState == BombState.Defused)
        {
            Debug.Log("Destroying planted bomb");
            NetworkServer.Destroy(gameObject.transform.GetChild(0).gameObject);
        }

        CmdSetGameTime(PreRoundLenght);
        CmdChangeGameState(GameState.PreRound);
        CmdChangeBombState(BombState.NotPlanted);
        Round++;


        RpcToggleMOTD(true);
        mapController.RpcResetWalls();

        NetworkServer.Destroy(Bomb);
        spawnBomb();
        if (GunHolder.transform.childCount > 0)
        {
            foreach (Transform gun in GunHolder.transform)
            {
                Debug.Log("Destroyingh gun: " + gun.name);
                NetworkServer.Destroy(gun.gameObject);
            }
        }


        foreach (var playerID in PlayersID)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            player.PreviousRoundShield = player.Shield;
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            RpcSetDefaultPlayerSettings(player);
            if (playerInventory.Bomb != null) NetworkServer.Destroy(playerInventory.Bomb);
            playerInventory.CmdSwitchItem(Item.Knife);

            if (playerInventory.SecondaryGun == null)
            {
                Debug.Log("Secondary gun is null, giving classic");
                playerInventory.CmdGiveGun(gunManager.gunList[0].GunID);
            }

            if (playerInventory.PrimaryGun != null)
            {
                Debug.Log("Primary gun is not null, switching to primary");
                playerInventory.CmdSwitchItem(Item.Primary);
            }
            else if (playerInventory.PrimaryGun == null)
            {
                Debug.Log("primary gun is null, switching to secondary");
                playerInventory.CmdSwitchItem(Item.Secondary);
            }

            PlayerSpectateManager playerSpectateManager = player.GetComponent<PlayerSpectateManager>();
            playerSpectateManager.SetPlayerTransform();

            player.IsDead = false;
            player.Health = 100;
            player.PlayerState = PlayerState.Idle;
            RpcGiveAmmo(player);
        }
        Debug.Log("new round spawning players");
        spawnPlayers();

        BombPlanted = false;
    }
    [ClientRpc]
    void RpcSetDefaultPlayerSettings(Player player)
    {
        PlayerShootingManager playerShootingManager = player.GetComponent<PlayerShootingManager>();
        playerShootingManager.CanShoot = true;
        playerShootingManager.Reloading = false;

        PlayerBombManager playerBombmanager = player.GetComponent<PlayerBombManager>();
        playerBombmanager.DefuseTimeLeft = 0;
        playerBombmanager.PlantTimeLeft = 0;

        PlantProgressSlider.value = 0;
        DefuseProgressSlider.value = 0;
        DefuseProgressSlider.enabled = false;
        PlantProgressSlider.enabled = false;
    }
    [ClientRpc]
    void RpcGiveAmmo(Player player)
    {
        PlayerInventoryManager playerInventoryManager = player.GetComponent<PlayerInventoryManager>();
        if(playerInventoryManager.PrimaryGun != null)
        {
            GunInstance gunInstance = playerInventoryManager.PrimaryGunInstance.GetComponent<GunInstance>();
            gunInstance.Ammo = gunInstance.Gun.Ammo;
            gunInstance.Magazine = gunInstance.Gun.MagazineAmmo;
        }
        if(playerInventoryManager.SecondaryGun != null)
        {
            GunInstance gunInstance = playerInventoryManager.SecondaryGunInstance.GetComponent<GunInstance>();
            gunInstance.Ammo = gunInstance.Gun.Ammo;
            gunInstance.Magazine = gunInstance.Gun.MagazineAmmo;
        }
    }

    void addScore()
    {
        Team tempTeam = LosingTeam;
        if (BombState == BombState.NotPlanted && AliveBluePlayers == 0)
        {
            RedTeamScore++;
            LosingTeam = Team.Blue;
        }
        else if(BombState == BombState.NotPlanted && AliveRedPlayers == 0)
        {
            BlueTeamScore++;
            LosingTeam = Team.Red;
        }
        else if(BombState == BombState.Defused)
        {
            BlueTeamScore++;
            LosingTeam = Team.Red;
        }
        else if(BombState == BombState.Exploded)
        {
            RedTeamScore++;
            LosingTeam = Team.Blue;
        }

        if(LosingTeam == tempTeam)
        {
            LossStreak++;
        }
        else if(LosingTeam != tempTeam)
        {
            LossStreak = 0;
        }

    }

    void giveMoney()
    {
        //KILLS
        foreach (var playerID in PlayersID)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            player.CmdChangeMoney(player.RoundKills * 200);
        }
        //RED TEAM BOMB PLANT
        if (BombPlanted)
        {
            foreach (var playerID in RedTeamPlayersIDs)
            {
                Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
                player.CmdChangeMoney(300);
            }
        }

        //WINNING TEAM
        if (BombState == BombState.Defused && GameState == GameState.PostRound)
        {
            foreach (var playerID in BlueTeamPlayersIDs)
            {
                Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
                player.CmdChangeMoney(3000);
            }
        }
        else if (BombState == BombState.Exploded && GameState == GameState.PostRound)
        {
            foreach (var playerID in RedTeamPlayersIDs)
            {
                Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
                player.CmdChangeMoney(3000);
            }
        }
        else if (BombState == BombState.NotPlanted)
        {
            foreach (var playerID in BlueTeamPlayersIDs)
            {
                Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
                player.CmdChangeMoney(3000);
            }
        }

        //LOSING TEAM
        foreach (var playerID in PlayersID)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            player.RoundKills = 0;
            if (player.PlayerTeam == LosingTeam)
            {
                if (player.PlayerTeam == Team.Red && !BombPlanted && !player.IsDead) player.CmdChangeMoney(1000);
                else if (player.PlayerTeam == Team.Blue && BombState == BombState.Exploded && !player.IsDead) player.CmdChangeMoney(1000);
                else
                {
                    player.CmdChangeMoney(1900);
                    if (LossStreak == 2) player.CmdChangeMoney(500);
                    else if (LossStreak >= 3) player.CmdChangeMoney(1000);
                }
            }
        }
    }



    [ClientRpc]
    void RpcToggleMOTD(bool statement)
    {
        MOTD.SetActive(statement);
    }

    void closePlayerShopUI()
    {
        foreach (var playerID in PlayersID)
        {
            RpcClosePlayerShopUI(NetworkServer.spawned[playerID].GetComponent<Player>());
        }
    }
    [ClientRpc]
    void RpcClosePlayerShopUI(Player player) => player.gameObject.GetComponent<PlayerEconomyManager>().CloseShopUI();

    [Server]
    public void StartRound(GameState gameState)
    {
        Round++;
        GameState = gameState;
    }

    [Command(requiresAuthority = false)]
    public void CmdSetGameTime(float time) => GameTime = time;

    [Command(requiresAuthority = false)]
    public void CmdChangeGameState(GameState gameState) => GameState = gameState;

    [Command(requiresAuthority = false)]
    public void CmdChangeBombState(BombState bombState) => BombState = bombState;

    [Command(requiresAuthority = false)]
    public void CmdSetBombPlanted() => BombPlanted = true;

    #endregion
}

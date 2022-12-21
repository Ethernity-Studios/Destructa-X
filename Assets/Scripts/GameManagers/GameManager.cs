using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using player;
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
    // [SyncVar]
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

    // [SyncVar(hook = nameof(DrawRoundTimer))]
    public float GameTime = 5;

    // [SyncVar]
    public GameState GameState = GameState.StartGame;
    // [SyncVar]
    public BombState BombState = BombState.NotPlanted;

    // TODO remove
    [SerializeField] TMP_Text roundTimer;

    public Transform BlueUIAgents, RedUIAgents;

    // [SyncVar]
    public int AliveBluePlayers = 0;
    // [SyncVar]
    public int AliveRedPlayers = 0;

    // public Slider PlantProgressSlider;
    // public Slider DefuseProgressSlider;

    public GameObject ShopUI;
    [SerializeField] GameObject MOTD;

    [SerializeField] Transform[] blueSpawnPositions, redSpawnPositions;

    public GameObject BombPrefab;
    public GameObject GunHolder;
    [SerializeField] Transform bombSpawnLocation;
    // [SyncVar]
    public GameObject Bomb;

    [SerializeField] GunManager gunManager;
    public ShopManager shopManager;
    private PlayerStateManger playerStateManger;
    [SerializeField] MapController mapController;
    private RoomManager roomManager;

    public GameObject BulletHolder;

    // [SyncVar(hook = nameof(updateBlueTeamScore))]
    public int BlueTeamScore = 0;
    // [SyncVar(hook = nameof(updateRedTeamScore))]
    public int RedTeamScore = 0;
    // [SyncVar]
    public bool BombPlanted = false;

    // [SyncVar]
    public Team LosingTeam = Team.None;
    // [SyncVar]
    public int LossStreak = 0;

    // FIXME edited with unsafe cmd
    public List<int> PlayersID = new();
    public List<int> BlueTeamPlayersIDs = new();
    public List<int> RedTeamPlayersIDs = new();
    public bool isEveryoneFuckingReady;

    // [SyncVar]
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
    
    // [SyncVar]
    public float PlantTimeLeft;
    // [SyncVar]
    public float DefuseTimeLeft;

    // [SyncVar] 
    public bool isPlanting;
    // [SyncVar]
    public bool isDefusing;


    private void Start()
    {
        playerStateManger = FindObjectOfType<PlayerStateManger>();
        roomManager = FindObjectOfType<RoomManager>();
        return;
        /*
        ShopUI.SetActive(false);
        // PlantProgressSlider.gameObject.SetActive(false);
        // DefuseProgressSlider.gameObject.SetActive(false);
        foreach (var con in NetworkServer.connections)
        {
            var p = con.Value.identity.GetComponent<Player>();
            PlayersID.Add(con.Value.identity.netId);
            if (p.PlayerTeam == Team.Blue)
            {
                BlueTeamPlayersIDs.Add(con.Value.identity.netId);
            }
            else if (p.PlayerTeam == Team.Red)
            {
                RedTeamPlayersIDs.Add(con.Value.identity.netId);
            }
            else
            {
                throw new Exception($"{p.name} player team is none");
            }
        }
        */
    }

    private void Update()
    {
        if (!isServer) return;
        if (!isEveryoneFuckingReady && Room.roomSlots.Count(x => x.connectionToClient.isReady) == Room.roomSlots.Count)
        {
            onPlayersLoaded();
            isEveryoneFuckingReady = true;
        }

        if (isEveryoneFuckingReady)
        {
            updateGameState();
            if (GameTime > 0) GameTime -= Time.deltaTime;   
        }
    }

    [Server]
    void InitializePlayer(int id)
    {
        var player = getPlayer(id);
        player.PlayerName = roomManager.playerNameMapping[id];
        player.PlayerAgent = roomManager.agentMapping[id];
        player.PlayerTeam = roomManager.bluePlayers.Contains(id) ? Team.Blue : Team.Red;
    }

    [Server]
    private void onPlayersLoaded()
    {
        // ShopUI.SetActive(false);
        
        PlayersID.Clear();
        BlueTeamPlayersIDs.Clear();
        RedTeamPlayersIDs.Clear();
        
        foreach (var con in NetworkServer.connections)
        {
            if (roomManager.bluePlayers.Contains(con.Key))
            {
                BlueTeamPlayersIDs.Add(con.Key);
            }
            else if (roomManager.redPlayers.Contains(con.Key))
            {
                RedTeamPlayersIDs.Add(con.Key);
            }
            PlayersID.Add(con.Key);
            InitializePlayer(con.Key);
        }
        
        setupGame();
    }

    /*
    public override void OnStartServer()
    {
        Debug.Log("on start server");
        
        if(Room.roomSlots.Count(x => x.connectionToClient.isReady) != Room.roomSlots.Count) { return; }

        //NetworkManagerRoom.OnServerReadied += startGame;
        Debug.Log("Game ready! starting game in 3 second!");

        // Invoke("setupGame", 3f);
        setupGame();
        //NetworkManagerRoom.OnServerReadied += startGame;
    }
    */

    #region Server

    [Server]
    void setupGame()
    {
        AliveBluePlayers = BlueTeamPlayersIDs.Count;
        AliveRedPlayers = RedTeamPlayersIDs.Count;   
        GameReady = true;
        
        GameTime = StartGameLenght;
        BombState = BombState.NotPlanted;
        // Invoke("RpcSetupGame",2f);
        playerStateManger.RpcSetupGame();
        // Invoke("ServerSpawnBomb", 1f);
        ServerSpawnBomb();
        // Invoke("spawnPlayers", 1.5f);
        spawnPlayers();
        // Invoke("giveDefaultGun", 2f);
        giveDefaultGun();
        StartRound(GameState.StartGame);
    }

    [Server]
    void updateGameState()
    {
        if (GameState == GameState.StartGame && GameTime <= 0)
        {
            //Buy phase - Start
            mapController.DropWalls();
            // closePlayerShopUI();
            GameTime = RoundLenght;
            GameState = GameState.Round;
            playerStateManger.RpcToggleMOTD(false);
        }
        else if (GameState == GameState.PreRound && GameTime <= 0)
        {
            //Buy phase
            mapController.DropWalls();
            // closePlayerShopUI();
            GameState = GameState.Round;
            GameTime = RoundLenght;
            playerStateManger.RpcToggleMOTD(false);
        }
        else if (BombState == BombState.Planted && GameTime <= 0)
        {
            //Bomb explosion
            //BombManager bombManager = FindObjectOfType<BombManager>();
            //bombManager.CmdDetonateBomb();
            BombState = BombState.Exploded;
            GameTime = PostRoundlenght;
            GameState = GameState.PostRound;
        }
        else if (GameState == GameState.Round && GameTime <= 0)
        {
            //end round
            GameTime = PostRoundlenght;
            GameState = GameState.PostRound;
        }
        else if (GameState == GameState.Round && AliveBluePlayers <= 0 && BlueTeamPlayersIDs.Count > 0 && PlayersID.Count > 1)
        {
            //All blue players dead
            Debug.Log("All blue players dead");
            GameTime = PostRoundlenght;
            GameState = GameState.PostRound;
        }
        else if (GameState == GameState.Round && BombState == BombState.NotPlanted && AliveRedPlayers <= 0 && RedTeamPlayersIDs.Count > 0 && PlayersID.Count > 1)
        {
            //Bomb not planted and all red players dead
            Debug.Log("Bomb not planted and all red players dead");
            GameTime = PostRoundlenght;
            GameState = GameState.PostRound;
        }
        else if (GameState == GameState.PostRound && GameTime <= 0)
        {
            //time's up
            Debug.Log("Time's up");
            startNewRound();
        }
    }
    
    [Server]
    public Player getPlayer(int id)
    {
        var player = NetworkServer.connections[id].identity.GetComponent<Player>();
        if (player == null)
        {
        }

        return player;
    }
    
    [Server]
    void startNewRound()
    {
        addScore();
        AliveBluePlayers = BlueTeamPlayersIDs.Count;
        AliveRedPlayers = RedTeamPlayersIDs.Count;
        giveMoney();

        if (BombState == BombState.Exploded || BombState == BombState.Defused)
        {
            NetworkServer.Destroy(gameObject.transform.GetChild(0).gameObject);
        }

        GameTime = PreRoundLenght;
        GameState = GameState.PreRound;
        BombState = BombState.NotPlanted;
        Round++;


        playerStateManger.RpcToggleMOTD(true);
        mapController.ResetWalls();

        NetworkServer.Destroy(Bomb);
        ServerSpawnBomb();
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
            Player player = getPlayer(playerID);
            player.PreviousRoundShield = player.Shield;
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            playerStateManger.RpcSetDefaultPlayerSettings(player);
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
            playerStateManger.RpcGiveAmmo(player);
        }
        Debug.Log("new round spawning players");
        spawnPlayers();

        BombPlanted = false;
    }
    
    [Server]
    void addScore()
    {
        Debug.Log("addScore");
        Team tempTeam = LosingTeam;
        if (BombState == BombState.NotPlanted && AliveBluePlayers == 0)
        {
            Debug.Log("losing1");
            RedTeamScore++;
            LosingTeam = Team.Blue;
        }
        else if(BombState == BombState.NotPlanted && AliveRedPlayers == 0)
        {
            Debug.Log("losing2");
            BlueTeamScore++;
            LosingTeam = Team.Red;
        }
        else if(BombState == BombState.Defused)
        {
            Debug.Log("losing3");
            BlueTeamScore++;
            LosingTeam = Team.Red;
        }
        else if(BombState == BombState.Exploded)
        {
            Debug.Log("losing4");
            RedTeamScore++;
            LosingTeam = Team.Blue;
        }

        if(LosingTeam == tempTeam)
        {
            Debug.Log("adding loss streak for team: " + tempTeam);
            LossStreak++;
        }
        else if(LosingTeam != tempTeam)
        {
            Debug.Log("reseting loss streak");
            LossStreak = 0;
        }
    }
    
    [Server]
    public void StartRound(GameState gameState)
    {
        Round++;
        GameState = gameState;
    }

    [Server]
    void giveMoney()
    {
        //KILLS
        foreach (var playerID in PlayersID)
        {
            Player player = getPlayer(playerID);
            player.CmdChangeMoney(player.RoundKills * 200);
        }
        //RED TEAM BOMB PLANT
        if (BombPlanted)
        {
            foreach (var playerID in RedTeamPlayersIDs)
            {
                Player player = getPlayer(playerID);
                player.CmdChangeMoney(300);
            }
        }

        //WINNING TEAM
        if (BombState == BombState.Defused && GameState == GameState.PostRound)
        {
            foreach (var playerID in BlueTeamPlayersIDs)
            {
                Player player = getPlayer(playerID);
                player.CmdChangeMoney(3000);
            }
        }
        else if (BombState == BombState.Exploded && GameState == GameState.PostRound)
        {
            foreach (var playerID in RedTeamPlayersIDs)
            {
                Player player = getPlayer(playerID);
                player.CmdChangeMoney(3000);
            }
        }
        else if (BombState == BombState.NotPlanted)
        {
            foreach (var playerID in BlueTeamPlayersIDs)
            {
                Player player = getPlayer(playerID);
                player.CmdChangeMoney(3000);
            }
        }

        //LOSING TEAM
        foreach (var playerID in PlayersID)
        {
            Player player = getPlayer(playerID);
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
    
    [Server]
    void ServerSpawnBomb()
    {
        var bombInstance = Instantiate(BombPrefab);
        // bombInstance.transform.SetParent(gameObject.transform);
        bombInstance.transform.position = bombSpawnLocation.position;
        NetworkServer.Spawn(bombInstance);
        Bomb = bombInstance;
    }
    
    [Server]
    void giveDefaultGun()
    {
        foreach (var playerID in PlayersID)
        {
            Player player = getPlayer(playerID);
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            playerInventory.CmdGiveGun(gunManager.gunList[0].GunID);
            playerInventory.CmdSwitchItem(Item.Knife);
            playerInventory.CmdSwitchItem(Item.Secondary);
            playerInventory.CmdSwitchItem(Item.Knife);
        }
    }
    
    [Server]
    void spawnPlayers()
    {
        int b = 0;
        int r = 0;
        foreach (var playerID in PlayersID)
        {
            Player player = getPlayer(playerID);
            if (player.PlayerTeam == Team.Blue)
            {
                player.RpcRespawnPlayer(blueSpawnPositions[b].position, blueSpawnPositions[b].rotation);
                playerStateManger.setPlayerColor(player, Color.blue);
                b++;
            }
            else if (player.PlayerTeam == Team.Red)
            {
                player.RpcRespawnPlayer(redSpawnPositions[r].position, redSpawnPositions[r].rotation);
                playerStateManger.setPlayerColor(player, Color.red);
                r++;
            }

        }
    }
    
    [Server]
    public void ServerSetPlantedState()
    {
        if (GameState == GameState.PostRound || GameState == GameState.EndGame) return;
        
        BombPlanted = true;
        GameTime = BombDetonationTime;
        BombState = BombState.Planted;
    }

    [Server]
    public void ServerSetDefusedState()
    {
        BombState = BombState.Defused;
        GameTime = PostRoundlenght;
        GameState = GameState.PostRound;
        // is null??
        var bombManager = Bomb.GetComponent<BombManager>();
        bombManager.canBoom = false;
        bombManager.StopAllCoroutines();
        bombManager.noBoomPwease();
        playerStateManger.RpcFuckOfBoom();
    }
    
    /*
    [Server]
    void closePlayerShopUI()
    {
        foreach (var playerID in PlayersID)
        {
            RpcClosePlayerShopUI(getPlayer(playerID));
        }
    }
    */

    #endregion

    #region rpcs
    /*
    [ClientRpc]
    void RpcSetupGame()
    {
        Debug.Log("RpcSetupGame");
        loadingScreen.SetActive(false);
    }
    
    [ClientRpc]
    void setPlayerColor(Player player, Color color)
    {
        player.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = color;
    }

    [ClientRpc]
    void RpcFuckOfBoom()
    {
        var bombManager = Bomb.GetComponent<BombManager>();
        bombManager.canBoom = false;
        bombManager.StopAllCoroutines();
        bombManager.noBoomPwease();
    }
    
    [ClientRpc]
    void RpcSetDefaultPlayerSettings(Player player)
    {
        Debug.Log("RpcSetDefaultPlayerSettings");
        PlayerShootingManager playerShootingManager = player.GetComponent<PlayerShootingManager>();
        playerShootingManager.CanShoot = true;
        playerShootingManager.Reloading = false;

        PlayerBombManager playerBombmanager = player.GetComponent<PlayerBombManager>();
        // FIXME
        // playerBombmanager.DefuseTimeLeft = 0;
        //playerBombmanager.PlantTimeLeft = 0;

        // PlantProgressSlider.value = 0;
        // DefuseProgressSlider.value = 0;
        // DefuseProgressSlider.enabled = false;
        // PlantProgressSlider.enabled = false;
    }
    [ClientRpc]
    void RpcGiveAmmo(Player player)
    {
        PlayerInventoryManager playerInventoryManager = player.GetComponent<PlayerInventoryManager>();
        if(playerInventoryManager.PrimaryGun != null)
        {
            GunInstance gunInstance = playerInventoryManager.PrimaryGunInstance.GetComponent<GunInstance>();
            // FIXME
            if (gunInstance == null)
            {
                playerInventoryManager.PrimaryGun = null;
            }
            else
            {
                gunInstance.Ammo = gunInstance.Gun.Ammo;
                gunInstance.Magazine = gunInstance.Gun.MagazineAmmo;
            }
        }
        if(playerInventoryManager.SecondaryGun != null)
        {
            GunInstance gunInstance = playerInventoryManager.SecondaryGunInstance.GetComponent<GunInstance>();
            if (gunInstance == null)
            {
                playerInventoryManager.SecondaryGun = null;
            }
            else
            {
                gunInstance.Ammo = gunInstance.Gun.Ammo;
                gunInstance.Magazine = gunInstance.Gun.MagazineAmmo;
            }
        }
    }
    
    [ClientRpc]
    void RpcToggleMOTD(bool statement)
    {
        MOTD.SetActive(statement);
    }
    
    // [ClientRpc]
    // void RpcClosePlayerShopUI(Player player) => player.gameObject.GetComponent<PlayerEconomyManager>().CloseShopUI();
    
    #endregion

    #region commands

    #endregion

    #region syncCallbacks

    // void updateBlueTeamScore(int _, int newValue) => BlueTeamScoreText.text = newValue.ToString();
    // void updateRedTeamScore(int _, int newValue) => RedTeamScoreText.text = newValue.ToString();
    
    void DrawRoundTimer(float _, float newValue)
    {
        var sec = Convert.ToInt32(newValue % 60).ToString("00");
        var min = (Mathf.Floor(newValue / 60) % 60).ToString("00");
        roundTimer.text = min + ":" + sec;
        if (newValue <= 0) roundTimer.text = "00:00";
    }
    */

    #endregion

    #region client

    #endregion
}
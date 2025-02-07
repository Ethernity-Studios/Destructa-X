using Mirror;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using player;
using UnityEngine;

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
    public int Round = 1;

    public int RoundsPerHalf = 12;

    public float StartGameLenght = 10; //45s
    public float EndgameLenght = 5; //10s

    public float PreRoundLenght = 20; //30s
    public float RoundLenght = 10; //1m 40s
    public float PostRoundLength = 5; //5s

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

    public Transform BlueUIAgents, RedUIAgents;

    [SyncVar]
    public int AliveBluePlayers = 0; 
    [SyncVar]
    public int AliveRedPlayers = 0;

    // public Slider PlantProgressSlider;
    // public Slider DefuseProgressSlider;

    public GameObject ShopUI;
    //[SerializeField] GameObject MOTD;

    [SerializeField] Transform[] blueSpawnPositions, redSpawnPositions;

    public GameObject BombPrefab;
    public GameObject GunHolder;
    [SerializeField] Transform bombSpawnLocation;
    public GameObject Bomb;

    [SerializeField] GunManager gunManager;
    public ShopManager shopManager;
    [SerializeField] public UIManager UIManager;
    private PlayerStateManger playerStateManger;
    [SerializeField] MapController mapController;
    //private RoomManager roomManager;

    public GameObject BulletHolder;
    [SyncVar]
    public int BlueTeamScore = 0;
    [SyncVar]
    public int RedTeamScore = 0;
    public bool BombPlanted = false;
    
    public Team LosingTeam = Team.None;
    public int LossStreak = 0;

    // FIXME edited with unsafe cmd
    public List<int> PlayersID = new();
    public List<int> BlueTeamPlayersIDs = new();
    public List<int> RedTeamPlayersIDs = new();
    public bool isEveryoneFuckingReady;
    
    //[SerializeField] GameObject loadingScreen;

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

    [SerializeField] GameObject mainCamera;

    public Transform ATeleportExit, BTeleportExit;

    private void Start()
    { 
        playerStateManger = FindObjectOfType<PlayerStateManger>();
        
        
        //roomManager = FindObjectOfType<RoomManager>();
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
            isEveryoneFuckingReady = true;
            onPlayersLoaded();
        }

        if (!isEveryoneFuckingReady) return;
        updateGameState();
        if (GameTime > 0) GameTime -= Time.deltaTime;
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
            PlayersID.Add(con.Key);
        }

        Invoke(nameof(addPlayerToTeam),1f);
        
        setupGame();
    }
    
    [Server]
    void addPlayerToTeam()
    {
        foreach (var con in NetworkServer.connections)
        {
            switch (con.Value.identity.GetComponent<Player>().PlayerTeam)
            {
                case Team.Blue:
                    BlueTeamPlayersIDs.Add(con.Key);
                    break;
                case Team.Red:
                    RedTeamPlayersIDs.Add(con.Key);
                    break;
            }
        }
        AliveBluePlayers = BlueTeamPlayersIDs.Count;
        AliveRedPlayers = RedTeamPlayersIDs.Count;
    }

    #region Server

    [Server]
    void setupGame()
    {

        GameTime = StartGameLenght;
        BombState = BombState.NotPlanted;
        // Invoke("RpcSetupGame",2f);
        playerStateManger.RpcSetupGame();
        Invoke(nameof(ServerSpawnBomb), 1f);
        //ServerSpawnBomb();
        Invoke(nameof(spawnPlayers), 1.5f);
        //spawnPlayers(); 
        Invoke(nameof(giveDefaultGun), 1f); // maybe fix me later ^^
        //Invoke(nameof(InitPlayerUI),1f);
        //giveDefaultGun();
        StartRound(GameState.StartGame);
    }

    /*[Server]
    void InitPlayerUI()
    {
        foreach (Player player in PlayersID.Select(GetPlayer))
        {
            PlayerUI playerUI = player.GetComponent<PlayerUI>();
            //playerUI.AddPlayerToHeader();
        }
    }*/


    [Server]
    [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
    void updateGameState()
    {
        if (GameState == GameState.StartGame && GameTime <= 0)
        {
            //Buy phase - Start
            Debug.Log("Buy phase ended");
            mapController.DropWalls();
            // closePlayerShopUI();
            GameTime = RoundLenght;
            GameState = GameState.Round;
            playerStateManger.RpcToggleMOTD(false, "", "");
        }
        else if (GameState == GameState.PreRound && GameTime <= 0)
        {
            //Buy phase
            Debug.Log("Buy phase ended");
            mapController.DropWalls();
            // closePlayerShopUI();
            GameState = GameState.Round;
            GameTime = RoundLenght;
            playerStateManger.RpcToggleMOTD(false, "", "");
        }
        else
        {
            if (BombState == BombState.Planted && GameTime <= 0)
            {
                //Bomb explosion
                //BombManager bombManager = FindObjectOfType<BombManager>();
                //bombManager.CmdDetonateBomb();
                BombState = BombState.Exploded;
                GameTime = PostRoundLength;
                GameState = GameState.PostRound;
                Debug.Log("Bomb explosion");
            }
            else
            {
                if (GameState == GameState.Round && GameTime <= 0)
                {
                    Debug.Log("Time's up");

                    //end round
                    GameTime = PostRoundLength;
                    GameState = GameState.PostRound;
                    addScore();
                }
                else if (GameState == GameState.Round && (AliveBluePlayers <= 0 && BlueTeamPlayersIDs.Count > 0))
                {
                    //All blue players dead
                    Debug.Log("All blue players dead");
                    GameTime = PostRoundLength;
                    GameState = GameState.PostRound;
                    addScore();
                }
                else if (GameState == GameState.Round && (BombState == BombState.NotPlanted && AliveRedPlayers <= 0 && RedTeamPlayersIDs.Count > 0))
                {
                    //Bomb not planted and all red players dead
                    Debug.Log("Bomb not planted and all red players dead");
                    GameTime = PostRoundLength;
                    GameState = GameState.PostRound;
                    addScore();
                }
                else if (GameState == GameState.PostRound && GameTime <= 0)
                {
                    //time's up
                    Debug.Log("Starting new round");
                    startNewRound();
                }
            }
        }
    }
    
    [Server]
    public Player GetPlayer(int id)
    {
        if (NetworkServer.connections[id] == null) return null;
        Player player = NetworkServer.connections[id].identity.GetComponent<Player>();
        if (player == null)
        {
        }

        return player;
    }
    
    [Server]
    void startNewRound()
    {
        giveMoney();
        if(Round == RoundsPerHalf) switchPlayerSides();
        AliveBluePlayers = BlueTeamPlayersIDs.Count;
        AliveRedPlayers = RedTeamPlayersIDs.Count;

        if (BombState is BombState.Exploded or BombState.Defused)
        {
            NetworkServer.Destroy(gameObject.transform.GetChild(0).gameObject);
        }

        GameTime = PreRoundLenght;
        GameState = GameState.PreRound;
        BombState = BombState.NotPlanted;
        Round++;


        playerStateManger.RpcToggleMOTD(true, UIManager.BuyPhaseText, UIManager.BuyPhaseSubText);
        mapController.ResetWalls();

        NetworkServer.Destroy(Bomb);
        ServerSpawnBomb();
        if (GunHolder.transform.childCount > 0)
        {
            foreach (Transform gun in GunHolder.transform)
            {
                Debug.Log("Destroying gun: " + gun.name);
                NetworkServer.Destroy(gun.gameObject);
            }
        }


        foreach (Player player in PlayersID.Select(GetPlayer))
        {
            player.EnemyPlayerMoney = player.PlayerMoney;
            PlayerUI playerUI = player.GetComponent<PlayerUI>();
            playerUI.RpcUpdatePlayerMoney();
            playerUI.CmdDestroyPlayerHeader();
            playerUI.CmdSpawnPlayerHeader();
            player.KillsThisRound = 0;
            
            //player.PreviousShield = player.Shield;
            player.CmdSetShieldType(ShieldType.None);
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            playerStateManger.RpcSetDefaultPlayerSettings(player);
            player.GetComponent<PlayerCombatReport>().RpcClearReports();
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
            playerSpectateManager.StopAllCoroutines();
            playerSpectateManager.ResetSpectate();

            player.IsDead = false;
            player.Health = 100;
            player.PlayerState = PlayerState.Idle;
            playerStateManger.RpcGiveAmmo(player);
        }
        Debug.Log("new round spawning players");
        spawnPlayers();

        BombPlanted = false;
    }

    void switchPlayerSides()
    {
        BlueTeamPlayersIDs.Clear();
        RedTeamPlayersIDs.Clear();

        var tmpBlueScore = BlueTeamScore;
        var tmpRedScore = RedTeamScore;

        BlueTeamScore = tmpRedScore;
        RedTeamScore = tmpBlueScore;
        
        foreach (var player in PlayersID.Select(GetPlayer))
        {
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            
            if(playerInventory.PrimaryGun != null) playerInventory.CmdDestroyGun(GunType.Primary);
            if(playerInventory.SecondaryGun != null) playerInventory.CmdDestroyGun(GunType.Secondary);
            
            playerInventory.CmdSwitchItem(Item.Knife);
            playerInventory.CmdSwitchItem(Item.Secondary);

            player.CmdResetMoney();
            switch (player.PlayerTeam)
            {
                case Team.Blue:
                    player.CmdSwitchPlayerTeam(Team.Red);
                    RedTeamPlayersIDs.Add(player.connectionToClient.connectionId);
                    break;
                case Team.Red:
                    player.CmdSwitchPlayerTeam(Team.Blue);
                    BlueTeamPlayersIDs.Add(player.connectionToClient.connectionId);
                    break;
            }
        }

        LossStreak = 0;
        LosingTeam = Team.None;
    }
    
    [Server]
    void addScore()
    {
        Debug.Log("addScore");
        Team tempTeam = LosingTeam;
        if (BombState == BombState.Defused)
        {
            Debug.Log("BLUE TEAM++ - bomb defused");
            BlueTeamScore++;
            LosingTeam = Team.Red;
        }
        else if (BombState == BombState.Exploded)
        {
            Debug.Log("Red TEAM++ - bomb exploded");
            RedTeamScore++;
            LosingTeam = Team.Blue;
        }
        else if (BombState == BombState.NotPlanted && AliveRedPlayers != 0 && AliveBluePlayers != 0)
        {
            Debug.Log("BLUE TEAM++ - bomb not planted && time's up");
            BlueTeamScore++;
            LosingTeam = Team.Red;  
        }
        else if (BombState == BombState.NotPlanted && AliveBluePlayers == 0 && AliveRedPlayers != 0)
        {
            Debug.Log("RED TEAM++ - bomb not planted && alive blue players = 0");
            RedTeamScore++;
            LosingTeam = Team.Blue;
        }
        else if (BombState == BombState.NotPlanted && AliveBluePlayers != 0)
        {
            Debug.Log("BLUE TEAM++ - bomb not planted && alive blue players != 0");
            BlueTeamScore++;
            LosingTeam = Team.Red;
        }
        else if (BombState == BombState.NotPlanted && AliveRedPlayers == 0)
        {
            Debug.Log("BLUE TEAM++ - bomb not planted && alive red players = 0");
            BlueTeamScore++;
            LosingTeam = Team.Red;
        }

        RpcUpdateScoreboardScore();

        if(LosingTeam == tempTeam)
        {
            Debug.Log("adding loss streak for team: " + tempTeam);
            LossStreak++;
        }
        else if(LosingTeam != tempTeam)
        {
            Debug.Log("resetting loss streak");
            LossStreak = 0;
        }
    }

    [ClientRpc]
    void RpcUpdateScoreboardScore()
    {
        UIManager.BlueScoreboardScore.text = BlueTeamScore.ToString();
        UIManager.RedScoreboardScore.text = RedTeamScore.ToString();
    }
    
    [Server]
    private void StartRound(GameState gameState)
    {
        Round++;
        GameState = gameState;
    }

    [Server]
    void giveMoney()
    {
        //KILLS
        foreach (Player player in PlayersID.Select(GetPlayer))
        {
            player.CmdChangeMoney(player.RoundKills * 200);
        }
        //RED TEAM BOMB PLANT
        if (BombPlanted)
        {
            foreach (Player player in RedTeamPlayersIDs.Select(GetPlayer))
            {
                player.CmdChangeMoney(300);
            }
        }

        switch (BombState)
        {
            //WINNING TEAM
            case BombState.Defused when GameState == GameState.PostRound:
            {
                foreach (Player player in BlueTeamPlayersIDs.Select(GetPlayer))
                {
                    player.CmdChangeMoney(3000);
                }

                break;
            }
            case BombState.Exploded when GameState == GameState.PostRound:
            {
                foreach (Player player in RedTeamPlayersIDs.Select(GetPlayer))
                {
                    player.CmdChangeMoney(3000);
                }

                break;
            }
            case BombState.NotPlanted:
            {
                foreach (Player player in BlueTeamPlayersIDs.Select(GetPlayer))
                {
                    player.CmdChangeMoney(3000);
                }

                break;
            }
        }

        //LOSING TEAM
        foreach (Player player in PlayersID.Select(GetPlayer))
        {
            player.RoundKills = 0;
            if (player.PlayerTeam != LosingTeam) continue;
            switch (player.PlayerTeam)
            {
                case Team.Red when !BombPlanted && !player.IsDead:
                case Team.Blue when BombState == BombState.Exploded && !player.IsDead:
                    player.CmdChangeMoney(1000);
                    break;
                default:
                {
                    player.CmdChangeMoney(1900);
                    switch (LossStreak)
                    {
                        case 2:
                            player.CmdChangeMoney(500);
                            break;
                        case >= 3:
                            player.CmdChangeMoney(1000);
                            break;
                    }
                    break;
                }
            }
        }
    }
    
    [Server]
    void ServerSpawnBomb()
    {
        GameObject bombInstance = Instantiate(BombPrefab);
        // bombInstance.transform.SetParent(gameObject.transform);
        bombInstance.transform.position = bombSpawnLocation.position;
        NetworkServer.Spawn(bombInstance);
        RpcSpawnBomb(bombInstance);
    }

    [ClientRpc]
    void RpcSpawnBomb(GameObject bomb)
    {
        Bomb = bomb;
    }
    
    [Server]
    void giveDefaultGun()
    {
        foreach (PlayerInventoryManager playerInventory in PlayersID.Select(GetPlayer).Select(player => player.GetComponent<PlayerInventoryManager>()))
        {
            playerInventory.CmdGiveGun(gunManager.gunList[0].GunID);
            playerInventory.CmdSwitchItem(Item.Knife);
            playerInventory.CmdSwitchItem(Item.Secondary);
            playerInventory.CmdSwitchItem(Item.Knife);
        }
    }
    
    [Server]
    void spawnPlayers()
    {
        RpcSpawnPlayers();
        int b = 0;
        int r = 0;
        foreach (Player player in PlayersID.Select(GetPlayer))
        {
            switch (player.PlayerTeam)
            {
                case Team.Blue:
                    player.RpcRespawnPlayer(new Vector3(blueSpawnPositions[b].position.x, blueSpawnPositions[b].position.y + 1, blueSpawnPositions[b].position.z), blueSpawnPositions[b].transform.eulerAngles);
                    //playerStateManger.setPlayerColor(player, Color.blue);
                    b++;
                    break;
                case Team.Red:
                    player.RpcRespawnPlayer(new Vector3(redSpawnPositions[r].position.x, redSpawnPositions[r].position.y + 1, redSpawnPositions[r].position.z), redSpawnPositions[r].transform.eulerAngles);
                    //playerStateManger.setPlayerColor(player, Color.red);
                    r++;
                    break;
            }
        }
    }

    [ClientRpc]
    void RpcSpawnPlayers()
    {
        mainCamera.transform.parent.GetComponent<CameraMove>().CanMove = true;
        mainCamera.GetComponent<CameraRotate>().CanRotate = true;
    }
    
    [Server]
    public void ServerSetPlantedState()
    {
        if (GameState is GameState.PostRound or GameState.EndGame) return;
        
        BombPlanted = true;
        GameTime = BombDetonationTime;
        BombState = BombState.Planted;
    }

    [Server]
    public void ServerSetDefusedState()
    {
        BombState = BombState.Defused;
        GameTime = PostRoundLength;
        GameState = GameState.PostRound;
        // is null??
        BombManager bombManager = Bomb.GetComponent<BombManager>();
        bombManager.canBoom = false;
        bombManager.StopAllCoroutines();
        bombManager.StopExplosion();
        playerStateManger.RpcFuckOfBoom();
    }


    #endregion
}
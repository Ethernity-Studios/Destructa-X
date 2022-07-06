using Mirror;
using System;
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

    public float StartGameLenght = 20; //45s
    public float EndgameLenght = 5; //10s

    public float PreRoundLenght = 20; //30s
    public float RoundLenght = 10; //1m 40s
    public float PostRoundlenght = 5; //5s

    public float BombPlantTime = 5;
    public float BombHalfDefuseTime = 5;
    public float BombDefuseTime = 10;
    public float BombDetonationTime = 15; //40

    [SyncVar]
    public float GameTime;

    [SyncVar]
    public GameState GameState;
    [SyncVar]
    public BombState BombState;

    [SerializeField] TMP_Text roundTimer;

    public Transform BlueUIAgents, RedUIAgents;

    readonly public SyncList<Player> Players = new();

    readonly public SyncList<Player> BlueTeam = new();
    readonly public SyncList<Player> RedTeam = new();

    [SyncVar]
    public int AliveBluePlayers;
    [SyncVar]
    public int AliveRedPlayers;

    public Slider PlantProgressSlider;
    public Slider DefuseProgressSlider;

    public GameObject ShopUI;
    [SerializeField] GameObject MOTD;

    [SerializeField] Transform[] blueSpawnPositions, redSpawnPositions;

    [SerializeField] GameObject[] dropdownWalls;

    public GameObject BombPrefab;
    public GameObject GunHolder;
    [SerializeField] Transform bombSpawnLocation;

    public GameObject Bomb;

    [SerializeField] GunManager gunManager;
    public ShopManager shopManager;

    public GameObject BulletHolder;

    [SyncVar(hook = nameof(updateBlueTeamScore))]
    public int BlueTeamScore;
    [SyncVar(hook = nameof(updateRedTeamScore))]
    public int RedTeamScore;

    [SerializeField] TMP_Text BlueTeamScoreText;
    [SerializeField] TMP_Text RedTeamScoreText;

    [SyncVar]
    public bool BombPlanted;

    [SyncVar]
    public Team LosingTeam;
    [SyncVar]
    public int LossStreak;

    private void Start()
    {
        ShopUI.SetActive(false);
        BombState = BombState.NotPlanted;
        PlantProgressSlider.gameObject.SetActive(false);
        DefuseProgressSlider.gameObject.SetActive(false);
        GameTime = StartGameLenght;


        if (!isServer) return;
        Invoke("spawnBomb", 1f);
        Invoke("spawnPlayers", 1f);
        Invoke("giveDefaultGun", 1.5f);
        StartRound(GameState.StartGame);
    }
    void spawnPlayers()
    {
        int b = 0;
        int r = 0;
        foreach (var player in Players)
        {
            if (player.PlayerTeam == Team.Blue)
            {
                player.RpcRespawnPlayer(blueSpawnPositions[b].position);
                b++;
            }
            else if (player.PlayerTeam == Team.Red)
            {
                player.RpcRespawnPlayer(redSpawnPositions[r].position);
                r++;
            }

        }
    }

    private void Update()
    {
        updateRoundTimer();
        if (isServer) updateGameState();

        if (GameTime > 0) GameTime -= Time.deltaTime;
    }

    void giveDefaultGun()
    {
        foreach (var player in Players)
        {
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            playerInventory.CmdGiveGun(gunManager.gunList[0].GunID);
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
            RpcDropWalls();
            CloseLocalPlayerShopUI();
            CmdSetGameTime(RoundLenght);
            CmdChangeGameState(GameState.Round);
            RpcToggleMOTD(false);
        }
        else if (GameState == GameState.PreRound && GameTime <= 0)
        {
            //Buy phase
            RpcDropWalls();
            CloseLocalPlayerShopUI();
            CmdChangeGameState(GameState.Round);
            CmdSetGameTime(RoundLenght);
            RpcToggleMOTD(false);
        }
        else if (BombState == BombState.Planted && GameTime <= 0)
        {
            //Bomb explosion
            BombManager bombManager = FindObjectOfType<BombManager>();
            bombManager.CmdDetonateBomb();
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
        else if(GameState == GameState.Round && AliveBluePlayers <= 0 && BlueTeam.Count > 0 && Players.Count > 1)
        {
            //All blue players dead
            startNewRound();
        }
        else if(GameState == GameState.Round && BombState == BombState.NotPlanted && AliveRedPlayers <= 0 && RedTeam.Count > 0 && Players.Count > 1)
        {
            //Bomb not planted and all red players dead
            startNewRound();
        }
        else if (GameState == GameState.PostRound && GameTime <= 0)
        {
            //time's up
            startNewRound();
        }
    }

    void startNewRound()
    {
        AliveBluePlayers = BlueTeam.Count;
        AliveRedPlayers = RedTeam.Count;
        giveMoney();
        CmdSetGameTime(PreRoundLenght);
        CmdChangeGameState(GameState.PreRound);
        CmdChangeBombState(BombState.NotPlanted);
        Round++;

        RpcToggleMOTD(true);

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
        if(BombState == BombState.Exploded ||BombState == BombState.Defused) NetworkServer.Destroy(gameObject.transform.GetChild(0).gameObject);

        spawnPlayers();
        foreach (var player in Players)
        {
            player.PreviousRoundShield = player.Shield;
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            if (playerInventory.Bomb != null) NetworkServer.Destroy(playerInventory.Bomb);

            if (playerInventory.SecondaryGun == null)
            {
                playerInventory.CmdGiveGun(gunManager.gunList[0].GunID);
                playerInventory.CmdSwitchItem(Item.Secondary);
                playerInventory.CmdSwitchItem(Item.Knife);
            }

            if (playerInventory.PrimaryGun != null)
            {
                playerInventory.CmdSwitchItem(Item.Primary);
            }
            else if (playerInventory.SecondaryGun != null && playerInventory.PrimaryGun == null)
            {
                playerInventory.CmdSwitchItem(Item.Secondary);
            }


            PlayerSpectateManager playerSpectateManager = player.GetComponent<PlayerSpectateManager>();
            playerSpectateManager.playerBody.transform.localEulerAngles = new Vector3(0, 0, 0);
            playerSpectateManager.playerBody.transform.localPosition = new Vector3(0, 0, 0);
            playerSpectateManager.playerBody.GetComponent<CapsuleCollider>().enabled = true;
            playerSpectateManager.playerBody.transform.parent.GetComponent<CharacterController>().enabled = true;
            playerSpectateManager.itemHolder.SetActive(true);
            playerSpectateManager.playerHead.transform.localPosition = new Vector3(0, .6f, 0);
            playerSpectateManager.playerHead.transform.localEulerAngles = new Vector3(0, 0, 0);
            playerSpectateManager.playerHands.GetComponent<Renderer>().enabled = true;
            player.IsDead = false;
            player.Health = 100;
            player.PlayerState = PlayerState.Idle;
        }

        BombPlanted = false;
    }

    void giveMoney()
    {
        //KILLS
        foreach (var player in Players)
        {
            player.CmdChangeMoney(player.RoundKills * 200);
        }
        //RED TEAM BOMB PLANT
        if (BombPlanted)
        {
            foreach (var player in RedTeam)
            {
                player.CmdChangeMoney(300);
            }
        }

        //WINNING TEAM
        if (BombState == BombState.Defused)
        {
            Debug.Log("1");
            BlueTeamScore++;
            foreach (var player in BlueTeam)
            {
                if (player.IsDead) player.CmdChangeMoney(3000);
                else player.CmdChangeMoney(1000);
            }
            if (LosingTeam == Team.Blue) LossStreak = 0;
            LosingTeam = Team.Red;
            LossStreak++;
        }
        else if (BombState == BombState.Exploded)
        {
            Debug.Log("2");
            RedTeamScore++;
            foreach (var player in RedTeam)
            {
                if (player.IsDead) player.CmdChangeMoney(3000);
                else player.CmdChangeMoney(1000);
            }
            if (LosingTeam == Team.Red) LossStreak = 0;
            LosingTeam = Team.Blue;
            LossStreak++;
        }
        else if (BombState == BombState.NotPlanted)
        {
            Debug.Log("3");
            BlueTeamScore++;
            foreach (var player in BlueTeam)
            {
                if (player.IsDead) player.CmdChangeMoney(3000);
                else player.CmdChangeMoney(1000);
            }
            if (LosingTeam == Team.Blue) LossStreak = 0;
            LosingTeam = Team.Red;
            LossStreak++;
        }

        //LOSING TEAM
        foreach (var player in Players)
        {
            player.RoundKills = 0;
            if (player.PlayerTeam == LosingTeam)
            {
                if (player.PlayerTeam == Team.Red && !BombPlanted) player.CmdChangeMoney(1000);
                else if (player.PlayerTeam == Team.Blue && BombState == BombState.Exploded) player.CmdChangeMoney(1000);
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
    void RpcDropWalls()
    {
        foreach (var wall in dropdownWalls)
        {
            wall.SetActive(false);
        }
    }

    [ClientRpc]
    void RpcToggleMOTD(bool statement)
    {
        MOTD.SetActive(statement);
    }

    void CloseLocalPlayerShopUI()
    {
        foreach (var player in Players)
        {
            player.gameObject.GetComponent<PlayerEconomyManager>().CloseShopUI();
        }
    }
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

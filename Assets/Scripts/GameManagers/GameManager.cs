using Mirror;
using System;
using System.Collections.Generic;
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
    [SyncVar]
    public int BlueTeamSize;
    [SyncVar]
    public int RedTeamSize;

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
    public GameObject BulletHolder;

    private void Start()
    {
        ShopUI.SetActive(false);
        BombState = BombState.NotPlanted;
        PlantProgressSlider.gameObject.SetActive(false);
        DefuseProgressSlider.gameObject.SetActive(false);
        GameTime = StartGameLenght;


        if (!isServer) return;
        Invoke("spawnBomb",1f);
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
        else if (GameState == GameState.PostRound && GameTime <= 0)
        {
            //start new round
            startNewRound();
        }
    }

    void startNewRound()
    {
        CmdSetGameTime(PreRoundLenght);
        CmdChangeGameState(GameState.PreRound);
        CmdChangeBombState(BombState.NotPlanted);
        Round++;

        RpcToggleMOTD(true);

        NetworkServer.Destroy(Bomb);
        spawnBomb();
        if(GunHolder.transform.childCount > 0)
        {
            foreach (Transform gun in GunHolder.transform)
            {
                Debug.Log("Destroyingh gun: " + gun.name);
                NetworkServer.Destroy(gun.gameObject);
            }
        }

        spawnPlayers();
        foreach (var player in Players)
        {
            Debug.Log("PLayja " + player.name);
            PlayerInventoryManager playerInventory = player.GetComponent<PlayerInventoryManager>();
            if(playerInventory.Bomb != null) NetworkServer.Destroy(playerInventory.Bomb);

            if (playerInventory.SecondaryGun == null)
            {
                Debug.Log("1");
                playerInventory.CmdGiveGun(gunManager.gunList[0].GunID);
                playerInventory.CmdSwitchItem(Item.Secondary);
                playerInventory.CmdSwitchItem(Item.Knife);
            }

            if(playerInventory.PrimaryGun != null)
            {
                Debug.Log("2");
                playerInventory.CmdSwitchItem(Item.Primary);
            }
            else if(playerInventory.SecondaryGun != null && playerInventory.PrimaryGun == null)
            {
                Debug.Log("3");
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

    #endregion
}

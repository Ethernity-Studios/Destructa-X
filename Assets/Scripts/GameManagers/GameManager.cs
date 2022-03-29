using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    public float StartGameLenght = 20; //40s
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

    readonly public SyncList<PlayerManager> Players = new();

    public Slider PlantProgressSlider;
    public Slider DefuseProgressSlider;

    public GameObject ShopUI;

    [SerializeField] Transform[] blueSpawnPositions, redSpawnPositions;

    private void Start()
    {
        ShopUI.SetActive(false);
        BombState = BombState.NotPlanted;
        PlantProgressSlider.gameObject.SetActive(false);
        DefuseProgressSlider.gameObject.SetActive(false);
        GameTime = StartGameLenght;

        if (!isServer) return;
        Invoke("spawnPlayers", .3f);
        CmdStartRound(GameState.StartGame);
    }

    void spawnPlayers()
    {
        int b = 0;
        int r = 0;
        foreach (var player in Players)
        {
            if (player.PlayerTeam == Team.Blue)
            {
                Debug.Log("spawning at blue" + blueSpawnPositions[b].position);
                player.RespawnPlayer(blueSpawnPositions[b].position);
                b++;
            }
            else if (player.PlayerTeam == Team.Red)
            {
                Debug.Log("spawning at red " + redSpawnPositions[r].position);
                player.RespawnPlayer(redSpawnPositions[r].position);
                r++;
            }
        }
    }

    private void Update()
    {
        updateRoundTimer();
        updateGameState();

        if (GameTime > 0) GameTime -= Time.deltaTime;
    }

    void updateRoundTimer()
    {
        var sec = Convert.ToInt32(GameTime % 60).ToString("00");
        var min = (Mathf.Floor(GameTime / 60) % 60).ToString("00");
        roundTimer.text = min + ":" + sec;
        if (GameTime <= 0) roundTimer.text = "00:00";
    }

    void updateGameState()
    {
        if(BombState == BombState.Planted && GameTime <= 0)
        {
            BombManager bombManager = FindObjectOfType<BombManager>();
            bombManager.CmdDetonateBomb();
            CmdChangeBombState(BombState.Exploded);
            CmdSetGameTime(PostRoundlenght);
            CmdChangeGameState(GameState.PostRound);
        } 
        if(GameState == GameState.PostRound && GameTime <= 0)
        {
            //start new round :)
        }
        if(GameState == GameState.PreRound && GameTime <= 0)
        {
            CloseLocalPlayerShopUI();
            CmdChangeGameState(GameState.Round);
            CmdSetGameTime(RoundLenght);
        }
        if(GameState == GameState.Round && GameTime <= 0)
        {
            CmdChangeGameState(GameState.PostRound);
            CmdSetGameTime(PostRoundlenght);
        }
        if(GameState == GameState.StartGame && GameTime <= 0)
        {
            CloseLocalPlayerShopUI();
            CmdSetGameTime(RoundLenght);
            CmdChangeGameState(GameState.Round);
        }
    }

    void CloseLocalPlayerShopUI()
    {
        foreach (var player in Players)
        {
            player.gameObject.GetComponent<PlayerEconomyManager>().CloseShopUI();
        }
    }

    #region RoundManagement

    [Command(requiresAuthority = false)]
    public void CmdStartRound(GameState gameState)
    {
        Round++;
        this.GameState = gameState;
    }

    [Command(requiresAuthority = false)]
    public void CmdSetGameTime(float time) => GameTime = time;

    [Command(requiresAuthority = false)]
    public void CmdChangeGameState(GameState gameState) => GameState = gameState;

    [Command(requiresAuthority = false)]
    public void CmdChangeBombState(BombState bombState) => BombState = bombState;

    #endregion
}

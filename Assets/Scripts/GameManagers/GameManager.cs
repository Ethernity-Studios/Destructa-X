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

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public int Round = 0;

    public int RoundsPerHalf = 13;

    public float StartGameLenght = 5; //40s
    public float EndgameLenght = 5; //10s

    public float PreRoundLenght = 40; //30s
    public float RoundLenght = 10; //1m 40s
    public float PostRoundlenght = 5; //5s

    public float BombPlantTime = 5;
    public float BombHalfDefuseTime = 5;
    public float BombDefuseTime = 10;
    public float BombDetonationTime = 15; //40

    [SyncVar]
    public float GameTime;

    [SyncVar]
    public GameState gameState;

    [SerializeField] TMP_Text roundTimer;

    public Transform BlueUIAgents, RedUIAgents;

    readonly public SyncList<PlayerManager> Players = new();

    public Slider PlantProgressSlider;

    public Slider DefuseProgressSlider;

    private void Start()
    {
        PlantProgressSlider.gameObject.SetActive(false);
        DefuseProgressSlider.gameObject.SetActive(false);
        GameTime = 20;

        if (!isServer) return;
        CmdStartRound(GameState.StartGame);
    }

    private void Update()
    {
        updateRoundTimer();

        if (GameTime > 0) GameTime -= Time.deltaTime;
    }

    void updateRoundTimer()
    {
        var sec = Convert.ToInt32(GameTime % 60).ToString("00");
        var min = (Mathf.Floor(GameTime / 60) % 60).ToString("00");
        roundTimer.text = min + ":" + sec;
        if (GameTime <= 0) roundTimer.text = "00:00";
    }

    #region RoundManagement

    [Command(requiresAuthority = false)]
    public void CmdStartRound(GameState gameState)
    {
        Round++;
        this.gameState = gameState;
    }

    [Command(requiresAuthority = false)]
    public void CmdSetGameTime(float time)
    {
        GameTime = time;
    }

    [Command(requiresAuthority = false)]
    public void ChangeGameState(GameState gameState)
    {
        this.gameState = gameState;
    }

    #endregion
}

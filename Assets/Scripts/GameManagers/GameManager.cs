using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using System.IO;
using objects;

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
    GameState gameState;

    [SerializeField]
    TMP_Text roundTimer;

    bool canChangeTime;

    private void Start()
    {
        CmdStartRound(GameState.StartGame);
    }

    private void Update()
    {
        if (canChangeTime)
        {
            CmdDecreaseGameTime(4);
        }
    }

    #region RoundManagement

    [Command]
    public void CmdStartRound(GameState gameState)
    {
        Round++;
        this.gameState = gameState;
    }

    [Command]
    void CmdDecreaseGameTime(float time)
    {//gametime 10 time 15
        if(time < GameTime)
        GameTime -= Time.deltaTime;
    }

    #endregion
}

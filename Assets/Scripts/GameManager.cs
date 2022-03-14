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
    public int Round = 0;

    public int StartGameLenght; //40s
    public int EndgameLenght; //10s

    public int PreRoundLenght; //30s
    public int RoundLenght; //1m 40s
    public int PostRoundlenght; //5s

    public int BombPlantTime;
    public int BombDefuseTime;
    public int BombDetonationTime;

    //public readonly SyncList<PlayerController> Players = new();

    private void Start()
    {

    }

    #region RoundManagement



    #endregion
}

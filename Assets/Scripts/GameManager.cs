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
    public int StartGameLenght; //40s
    public int EndgameLenght;

    public int PreRoundLenght;
    public int RoundLenght;
    public int PostRoundlenght;

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

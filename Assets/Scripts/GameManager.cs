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
    public readonly SyncList<PlayerController> Players = new();

    private void Start()
    {

    }
}

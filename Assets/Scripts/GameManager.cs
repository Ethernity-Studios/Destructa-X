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

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Name " + NicknameManager.DisplayName + " Team " + RoomManager.PTeam + " Agent " + RoomManager.PAgent);
    }
}

using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameManagers
{
    public class GameStateUi: NetworkBehaviour
    {
        private GameManager gameManager;

        // global ui
        public TMP_Text roundTimer;
        public TMP_Text BlueTeamScoreText;
        public TMP_Text RedTeamScoreText;
        [SyncVar(hook = nameof(DrawRoundTimer))] private float GameTime;
        [SyncVar(hook = nameof(DrawRedScore))] private int redScore;
        [SyncVar(hook = nameof(DrawBlueScore))] private int blueScore;
        // player local ui
        public Slider PlantProgressSlider;
        public Slider DefuseProgressSlider;


        private void Start()
        {
            Debug.Log("gooooooooooooofy ahhhhhhhhhhhhhhhhhhhh");
            if (isServer)
            {
                gameManager = FindObjectOfType<GameManager>();
                Debug.Log($"game manger {gameManager}");
                // player = gameManager.getLocalPlayer();
            }
        }

        private void Update()
        {
            if (isServer) ServerUpdate();
        }

        [Server]
        void ServerUpdate()
        {
            if (gameManager == null) return;
            GameTime = gameManager.GameTime;
            blueScore = gameManager.BlueTeamScore;
            redScore = gameManager.RedTeamScore;
        }

        void DrawRedScore(int _, int newValue)
        {
            RedTeamScoreText.text = newValue.ToString();
        }

        void DrawBlueScore(int _, int newValue)
        {
            BlueTeamScoreText.text = newValue.ToString();
        }
        
        void DrawRoundTimer(float _, float newValue)
        {
            var sec = Convert.ToInt32(newValue % 60).ToString("00");
            var min = (Mathf.Floor(newValue / 60) % 60).ToString("00");
            roundTimer.text = min + ":" + sec;
            if (newValue <= 0) roundTimer.text = "00:00";
        }
    }
}
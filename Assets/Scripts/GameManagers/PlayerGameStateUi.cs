using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameManagers
{
    public class PlayerGameStateUi: NetworkBehaviour
    {
        private GameManager gameManager;
        private Player player;
        
        public Slider PlantProgressSlider;
        public Slider DefuseProgressSlider;
        [SerializeField] TMP_Text roundTimer;
        [SerializeField] TMP_Text BlueTeamScoreText;
        [SerializeField] TMP_Text RedTeamScoreText;

        [SyncVar(hook = nameof(DrawPlant))] private float PlantTimeLeft;
        [SyncVar(hook = nameof(DrawDefuse))] private float DefuseTimeLeft;
        [SyncVar(hook = nameof(DrawDefuseSlider))] private bool ShowDefuseSlider;
        [SyncVar(hook = nameof(DrawPlantSlider))] private bool ShowPlantSlider;
        [SyncVar(hook = nameof(DrawRoundTimer))] private float GameTime;
        [SyncVar(hook = nameof(DrawRedScore))] private int redScore;
        [SyncVar(hook = nameof(DrawBlueScore))] private int blueScore;


        private void Start()
        {
            if (isServer)
            {
                StartKysNigr();
                // Invoke("StartKysNigr", 3);
            }
        }

        void StartKysNigr()
        {
            gameManager = FindObjectOfType<GameManager>();
            Debug.Log($"game manger {gameManager}");
            // player = gameManager.getLocalPlayer();
            player = NetworkServer.localConnection.identity.GetComponent<Player>();
            if (player == null) throw new Exception("docela sus ngl");
            Debug.Log($"player after {player.netId}");
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
            if (player.PlayerTeam == Team.Red)
            {
                PlantTimeLeft = (gameManager.PlantTimeLeft / gameManager.BombPlantTime) * 100;
                ShowPlantSlider = gameManager.isPlanting;
            }
            else if (player.PlayerTeam == Team.Blue)
            {
                DefuseTimeLeft = (gameManager.DefuseTimeLeft / gameManager.BombDefuseTime) * 100;
                ShowDefuseSlider = gameManager.isDefusing;
            }
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

        void DrawPlantSlider(bool _, bool newValue)
        {
            PlantProgressSlider.gameObject.SetActive(newValue);
        }
        
        void DrawDefuseSlider(bool _, bool newValue)
        {
            DefuseProgressSlider.gameObject.SetActive(newValue);
        }

        void DrawPlant(float _, float newValue)
        {
            PlantProgressSlider.value = PlantTimeLeft;   
        }

        void DrawDefuse(float _, float newValue)
        {
            DefuseProgressSlider.value = DefuseTimeLeft;   
        }
    }
}
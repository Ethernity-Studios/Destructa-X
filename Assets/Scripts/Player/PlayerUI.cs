using System;
using GameManagers;
using Mirror;
using UnityEngine;

namespace player
{
    public class PlayerUI: NetworkBehaviour
    {
        private GameManager gameManager;
        private Player player;
        private GameStateUi gameStateUi;
        [SyncVar(hook = nameof(DrawPlant))]
        private float PlantTimeLeft = 0;
        [SyncVar(hook = nameof(DrawDefuse))]
        private float DefuseTimeLeft = 0;
        [SyncVar(hook = nameof(DrawDefuseSlider))] 
        private bool ShowDefuseSlider = false;
        [SyncVar(hook = nameof(DrawPlantSlider))] 
        private bool ShowPlantSlider = false;
        
        private void Start()
        {
            gameStateUi = FindObjectOfType<GameStateUi>();
            gameStateUi.DefuseProgressSlider.transform.parent.gameObject.SetActive(false);
            gameStateUi.PlantProgressSlider.transform.parent.gameObject.SetActive(false);
            if (!isServer) return;
            gameStateUi = FindObjectOfType<GameStateUi>();
            gameManager = FindObjectOfType<GameManager>();
            player = NetworkServer.localConnection.identity.GetComponent<Player>();
            if (player == null) throw new Exception("docela sus ngl");
        }

        private void Update()
        {
            if (!isServer) return;
            switch (player.PlayerTeam)
            {
                case Team.Red:
                    PlantTimeLeft = (gameManager.PlantTimeLeft / gameManager.BombPlantTime) * 100;
                    ShowPlantSlider = gameManager.isPlanting;
                    break;
                case Team.Blue:
                    DefuseTimeLeft = (gameManager.DefuseTimeLeft / gameManager.BombDefuseTime) * 100;
                    ShowDefuseSlider = gameManager.isDefusing;
                    break;
            }
        }
        
        void DrawPlantSlider(bool _, bool newValue)
        {
            Debug.Log("DrawPlantSlider");
            gameStateUi.PlantProgressSlider.transform.parent.gameObject.SetActive(newValue);
            // gameStateUi.PlantProgressSlider.gameObject.SetActive(newValue);
        }
        
        void DrawDefuseSlider(bool _, bool newValue)
        {
            Debug.Log("DrawDefuseSlider");
            gameStateUi.DefuseProgressSlider.transform.parent.gameObject.SetActive(newValue);
            // gameStateUi.DefuseProgressSlider.gameObject.SetActive(newValue);
        }

        void DrawPlant(float _, float newValue)
        {
            Debug.Log("DrawPlant");
            gameStateUi.PlantProgressSlider.value = newValue;
        }

        void DrawDefuse(float _, float newValue)
        {
            Debug.Log("DrawDefuse");
            gameStateUi.DefuseProgressSlider.value = newValue;
        }
    }
}
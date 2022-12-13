using System;
using GameManagers;
using Mirror;

namespace player
{
    public class PlayerGameStateSpecificUi: NetworkBehaviour
    {
        private GameManager gameManager;
        private Player player;
        private GameStateUi gameStateUi;
        [SyncVar(hook = nameof(DrawPlant))] private float PlantTimeLeft;
        [SyncVar(hook = nameof(DrawDefuse))] private float DefuseTimeLeft;
        [SyncVar(hook = nameof(DrawDefuseSlider))] private bool ShowDefuseSlider;
        [SyncVar(hook = nameof(DrawPlantSlider))] private bool ShowPlantSlider;
        
        private void Start()
        {
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
            gameStateUi.PlantProgressSlider.gameObject.SetActive(newValue);
        }
        
        void DrawDefuseSlider(bool _, bool newValue)
        {
            gameStateUi.DefuseProgressSlider.gameObject.SetActive(newValue);
        }

        void DrawPlant(float _, float newValue)
        {
            gameStateUi.PlantProgressSlider.value = newValue;
        }

        void DrawDefuse(float _, float newValue)
        {
            gameStateUi.DefuseProgressSlider.value = newValue;   
        }
    }
}
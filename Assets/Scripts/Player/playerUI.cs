using System;
using System.Collections.Generic;
using System.Linq;
using GameManagers;
using Mirror;
using objects;
using UnityEngine;
using UnityEngine.UIElements;

namespace player
{
    public class PlayerUI : NetworkBehaviour
    {
        private GameManager gameManager;
        private Player player;
        private GameStateUi gameStateUi;
        [SyncVar(hook = nameof(DrawPlant))] private float PlantTimeLeft = 0;
        [SyncVar(hook = nameof(DrawDefuse))] private float DefuseTimeLeft = 0;

        [SyncVar(hook = nameof(DrawDefuseSlider))]
        private bool ShowDefuseSlider = false;

        [SyncVar(hook = nameof(DrawPlantSlider))]
        private bool ShowPlantSlider = false;

        private UIManager uiManager;

        [SyncVar] public GameObject PlayerHeader;

        public PlayerScoreboard ScoreboardPlayer;

        private void Start()
        {
            player = GetComponent<Player>();
            gameManager = FindObjectOfType<GameManager>();
            uiManager = gameManager.UIManager;
            gameStateUi = FindObjectOfType<GameStateUi>();
            gameStateUi.DefuseProgressSlider.transform.parent.gameObject.SetActive(false);
            gameStateUi.PlantProgressSlider.transform.parent.gameObject.SetActive(false);
            gameStateUi = FindObjectOfType<GameStateUi>();
            if (!isLocalPlayer) return;
            Invoke(nameof(CmdAddPlayersToShop), 1.5f);
            Invoke(nameof(spawnPlayerScoreboard),1f);
        }


        private void Update()
        {
            if (isLocalPlayer)
            {
                string ping = Math.Round(NetworkTime.rtt, 2) + " ms";
                uiManager.Latency.text = ping;     
            }
            
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

        [Server]
        public void AddPlayerToHeader()
        {
            GameObject playerHeader;
            switch (player.PlayerTeam)
            {
                case Team.Blue:
                    playerHeader = Instantiate(uiManager.HeaderGreenPlayer);
                    NetworkServer.Spawn(playerHeader);
                    RpcAddPlayerToHeader(playerHeader);
                    break;
                case Team.Red:
                    playerHeader = Instantiate(uiManager.HeaderRedPlayer);
                    NetworkServer.Spawn(playerHeader);
                    RpcAddPlayerToHeader(playerHeader);
                    break;
            }

            foreach (Player p in gameManager.PlayersID.Select(gameManager.GetPlayer))
            {
                if (p.PlayerTeam != player.PlayerTeam) RpcHidePlayerHeaderHealthBar(p.netIdentity.connectionToClient);
            }
        }

        [TargetRpc]
        void RpcHidePlayerHeaderHealthBar(NetworkConnection conn)
        {
            PlayerHeader.GetComponent<HeaderPlayer>().Health.transform.parent.gameObject.SetActive(false);
        }

        [ClientRpc]
        public void RpcAddPlayerToHeader(GameObject playerHeader /*, AgentScriptableObject agent*/)
        {
            if (player.PlayerTeam == Team.Blue) playerHeader.transform.SetParent(uiManager.HeaderGreenTeam.transform);
            else if (player.PlayerTeam == Team.Red) playerHeader.transform.SetParent(uiManager.HeaderRedTeam.transform);
            HeaderPlayer playerH = playerHeader.GetComponent<HeaderPlayer>();
            //if (!isLocalPlayer) return;
            PlayerHeader = playerHeader;
            //playerH.Agent.sprite = agent.Meta.Icon; // TODO set player agent icon
        }

        [Command(requiresAuthority = false)]
        void CmdAddPlayersToShop()
        {
            foreach (Player p in gameManager.PlayersID.Select(gameManager.GetPlayer))
            {
                RpcAddPlayersToShop(p);
            }
        }

        [ClientRpc]
        void RpcAddPlayersToShop(Player p)
        {
            if (!isLocalPlayer) return;

            if (p.PlayerTeam != player.PlayerTeam) return;
            if (p.isLocalPlayer) return;

            GameObject shopPlayer = Instantiate(uiManager.ShopPlayer, uiManager.ShopTeam.transform, true);

            shopPlayer.transform.localScale = Vector3.one;
            ShopPlayer sPlayer = shopPlayer.GetComponent<ShopPlayer>();
            sPlayer.PlayerId =
                (int)p.netId; //p.netIdentity.connectionToServer.connectionId; null for some reason :shrug: ¯\_(ツ)_/¯
            sPlayer.Name.text = p.PlayerName;
            //sPlayer.AgentIcon = player.AgentIcon; TODO agent icon
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

        [Command]
        void spawnPlayerScoreboard()
        {
            GameObject playerScoreboard = Instantiate(uiManager.PlayerScoreboard);
            NetworkServer.Spawn(playerScoreboard);

            foreach (var p in gameManager.PlayersID.Select(gameManager.GetPlayer))
            {
                if (p.PlayerTeam == player.PlayerTeam) RpcSpawnTeamPlayerScoreboard(p.netIdentity.connectionToClient, playerScoreboard);
                else RpcSpawnEnemyPlayerScoreboard(p.netIdentity.connectionToClient, playerScoreboard);
                
            }
        }

        [TargetRpc]
        void RpcSpawnTeamPlayerScoreboard(NetworkConnection conn, GameObject playerScoreboard)
        {
            ScoreboardPlayer = playerScoreboard.GetComponent<PlayerScoreboard>();
            ScoreboardPlayer.Owner = player;
            ScoreboardPlayer.Name.text = player.PlayerName;
            //ScoreboardPlayer.AgentIcon.sprite = 

            if (isLocalPlayer) ScoreboardPlayer.Border.color = UIManager.YellowColor;
            playerScoreboard.transform.SetParent(uiManager.FriendlyTeamScoreboard.transform);
            playerScoreboard.transform.localScale = Vector3.one;
        }

        [TargetRpc]
        void RpcSpawnEnemyPlayerScoreboard(NetworkConnection conn, GameObject playerScoreboard)
        {
            ScoreboardPlayer = playerScoreboard.GetComponent<PlayerScoreboard>();
            ScoreboardPlayer.Owner = player;
            ScoreboardPlayer.Name.text = player.PlayerName;
            //ScoreboardPlayer.AgentIcon.sprite = 
            
            playerScoreboard.transform.SetParent(uiManager.EnemyTeamScoreboard.transform);
            ScoreboardPlayer.Border.color = UIManager.RedColor;
            ScoreboardPlayer.Background.sprite = uiManager.RedTeamBackgroundScoreboard;
            playerScoreboard.transform.localScale = Vector3.one;
        }

        [TargetRpc]
        public void RpcUpdatePlayerTeamMoney()
        {
            ScoreboardPlayer.Money.text = player.PlayerMoney.ToString();
        }

        [ClientRpc]
        public void RpcUpdatePlayerMoney()
        {
            ScoreboardPlayer.Money.text = player.EnemyPlayerMoney.ToString();
        }

        [TargetRpc]
        public void RpcUpdatePlayerGun()
        {
            PlayerInventoryManager playerInventoryManager = player.GetComponent<PlayerInventoryManager>();
            if (playerInventoryManager.PrimaryGun != null)
                ScoreboardPlayer.GunIcon.sprite = playerInventoryManager.PrimaryGun.Icon;
            else if (playerInventoryManager.SecondaryGun != null && playerInventoryManager.PrimaryGun == null)
                ScoreboardPlayer.GunIcon.sprite = playerInventoryManager.SecondaryGun.Icon;
            else if(playerInventoryManager.SecondaryGun == null && playerInventoryManager.PrimaryGun == null)
                ScoreboardPlayer.GunIcon.sprite = uiManager.Knife;
        }

        [TargetRpc]
        public void RpcUpdatePlayerUltimatePoints()
        {
            //TODO 
        }

        [ClientRpc]
        public void RpcUpdatePlayerScore()
        {
            ScoreboardPlayer.KDA.text = $"{player.PlayerKills} {player.PlayerDeaths} {player.PlayerAssists}";
        }
    }
}
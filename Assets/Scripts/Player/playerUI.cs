using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using GameManagers;
using Mirror;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

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

        public PlayerHeader HeaderPlayer;
        public PlayerScoreboard ScoreboardPlayer;
        public PlayerShop ShopPlayer;

        private PlayerInput playerInput;

        private void Awake()
        {
            playerInput = new PlayerInput();
            playerInput.PlayerUI.Enable();

            playerInput.PlayerUI.Scoreboard.performed += openScoreboard;
            playerInput.PlayerUI.Scoreboard.canceled += closeScoreboard;
        }

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
            //Invoke(nameof(CmdAddPlayersToShop), 1.5f);
            Invoke(nameof(CmdSpawnPlayerScoreboard),1f);
            Invoke(nameof(CmdSpawnPlayerHeader),1f);
            Invoke(nameof(CmdSpawnPlayerShop),1f);
        }

        private void OnEnable() => playerInput.PlayerInventory.Enable();

        private void OnDisable() => playerInput.PlayerInventory.Disable();

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

        void openScoreboard(CallbackContext context)
        {
            if (!isLocalPlayer) return;
            uiManager.Scoreboard.SetActive(true);
        }

        void closeScoreboard(CallbackContext context)
        {
            if (!isLocalPlayer) return;
            uiManager.Scoreboard.SetActive(false);
        }
        
        [TargetRpc]
        public void RpcToggleHeaderBomb(NetworkConnection conn, bool state)
        {
            HeaderPlayer.GetComponent<PlayerHeader>().Bomb.gameObject.SetActive(state);
        }
        
        /*[Command(requiresAuthority = false)]
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
            PlayerShop s = shopPlayer.GetComponent<PlayerShop>();
            s.Owner = player;
                ; //p.netIdentity.connectionToServer.connectionId; null for some reason :shrug: ¯\_(ツ)_/¯
            s.Name.text = p.PlayerName;
            //sPlayer.AgentIcon = player.AgentIcon; TODO agent icon
        }*/

        [Command]
        void CmdSpawnPlayerShop()
        {
            GameObject playerShop = Instantiate(uiManager.ShopPlayer);
            NetworkServer.Spawn(playerShop);
            
            foreach (var p in gameManager.PlayersID.Select(gameManager.GetPlayer))
            {
                if (p.PlayerTeam == player.PlayerTeam) RpcSpawnTeamPlayerShop(p.netIdentity.connectionToClient, playerShop);
                else RpcSpawnEnemyPlayerShop(p.netIdentity.connectionToClient, playerShop);
            }
        }

        [TargetRpc]
        void RpcSpawnTeamPlayerShop(NetworkConnection conn, GameObject playerShop)
        {
            playerShop.transform.SetParent(uiManager.ShopTeam.transform);
            playerShop.transform.localScale = Vector3.one;

            ShopPlayer = playerShop.GetComponent<PlayerShop>();

            ShopPlayer.Owner = player;
            //shopPlayer.AgentIcon.sprite = TODO
            ShopPlayer.Name.text = player.PlayerName;
            
            if(isLocalPlayer) playerShop.SetActive(false);
        }

        [TargetRpc]
        void RpcSpawnEnemyPlayerShop(NetworkConnection conn, GameObject playerShop)
        {
            playerShop.SetActive(false);
        }

        [Command]
        void CmdSpawnPlayerHeader()
        {
            GameObject playerHeader = Instantiate(uiManager.HeaderPlayer);
            NetworkServer.Spawn(playerHeader);
            
            foreach (var p in gameManager.PlayersID.Select(gameManager.GetPlayer))
            {
                if (p.PlayerTeam == player.PlayerTeam) RpcSpawnTeamPlayerHeader(p.netIdentity.connectionToClient, playerHeader);
                else RpcSpawnEnemyPlayerHeader(p.netIdentity.connectionToClient, playerHeader);
            }
        }

        [TargetRpc]
        void RpcSpawnTeamPlayerHeader(NetworkConnection conn, GameObject playerHeader)
        {
            HeaderPlayer = playerHeader.GetComponent<PlayerHeader>();
            playerHeader.transform.SetParent(uiManager.FriendlyTeamHeader.transform);
            //HeaderPlayer.Agent.sprite =  TODO
            playerHeader.transform.localScale = Vector3.one/10;

        }

        [TargetRpc]
        void RpcSpawnEnemyPlayerHeader(NetworkConnection conn, GameObject playerHeader)
        {
            HeaderPlayer = playerHeader.GetComponent<PlayerHeader>();
            playerHeader.transform.SetParent(uiManager.EnemyTeamHeader.transform);
            HeaderPlayer.Health.transform.parent.gameObject.SetActive(false);
            HeaderPlayer.Background.sprite = uiManager.RedTeamBackgroundHeader;
            //HeaderPlayer.Agent.sprite =  TODO
            playerHeader.transform.localScale = Vector3.one/10;
        }

        [Command]
        public void CmdDestroyPlayerHeader()
        {
            NetworkServer.Destroy(HeaderPlayer.gameObject);
        }

        [Command]
        void CmdSpawnPlayerScoreboard()
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
        public void RpcUpdatePlayerTeamMoney(NetworkConnection conn, int money)
        {
            ShopPlayer.Money.text = money.ToString();
            ScoreboardPlayer.Money.text = money.ToString();
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
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBombManager : NetworkBehaviour
{
    [SyncVar]
    bool isInPlantAbleArea;
    [SyncVar]
    bool isInBombArea;
    [SyncVar] 
    private GameState gameState;
    [SyncVar] 
    private BombState bombState;

    GameManager gameManager;
    Player player;
    PlayerInventoryManager playerInventoryManager;

    [SerializeField] GameObject bombPrefab;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = new();

        playerInput.PlayerBomb.Planting.performed += startPlanting;
        playerInput.PlayerBomb.Planting.canceled += stopPlanting;
        
        playerInput.PlayerBomb.Defuse.performed += startDefusing;
        playerInput.PlayerBomb.Defuse.canceled += stopDefusing;
    }

    private void Start()
    {
        if (!isLocalPlayer) return;
        player = GetComponent<Player>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        playerInput.PlayerBomb.Enable();
    }

    private void OnDisable()
    {
        playerInput.PlayerBomb.Disable();
    }

    void Update()
    {
        if (isServer) ServerUpdate();
    }

    bool canPlant()
    {
        /*
        return isInPlantableArea
               && playerInventoryManager.Bomb != null
               && player.PlayerTeam == Team.Red 
               && !player.IsDead 
               && (gameState == GameState.Round || gameState == GameState.PostRound);
        */

        if (!isInPlantAbleArea)
        {
            Debug.Log("not in plant able area");
            return false;
        }
        if (playerInventoryManager.Bomb == null)
        {
            Debug.Log("player inventory manager bomb null");
            return false;
        }
        if (player.PlayerTeam != Team.Red)
        {
            Debug.Log("player is dead");
            return false;
        }

        if (gameState is GameState.Round or GameState.PostRound or GameState.PostRound or GameState.EndGame) return true;
        Debug.Log("invalid game state");
        return false;

    }
    
    bool canDefuse()
    {
        return isInBombArea && player.PlayerTeam == Team.Blue && !player.IsDead && bombState == BombState.Planted;
    }

    private void OnTriggerEnter(Collider other)
    {
        // FIXME would this work?
        if (isServer) checkCollisionEnterServer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isServer) checkCollisionStayServer(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (isServer) checkCollisionExitServer(other);
    }

    #region server

    [Server]
    private void checkCollisionEnterServer(Collider other)
    {
        if (other.gameObject.CompareTag("PlantArea")) isInPlantAbleArea = true;

        if (other.gameObject.CompareTag("Bomb")) isInBombArea = true;
    }

    [Server]
    private void checkCollisionExitServer(Collider other)
    {
        if (other.gameObject.CompareTag("PlantArea")) isInPlantAbleArea = false;

        if (other.gameObject.CompareTag("Bomb")) isInBombArea = false;
    }
    
    [Server]
    private void checkCollisionStayServer(Collider other)
    {
        if (other.gameObject.CompareTag("PlantArea")) isInPlantAbleArea = true;
        
        if (other.gameObject.CompareTag("Bomb")) isInBombArea = true;
    }

    [Server]
    void ServerUpdate()
    {
        gameManager = FindObjectOfType<GameManager>();
        gameState = gameManager.GameState;
        bombState = gameManager.BombState;
        if (gameManager.isDefusing) ServerHandleDefusing();
        else if (gameManager.isPlanting) ServerHandlePlanting();
    }

    [Server]
    void ServerHandleDefusing()
    {
        if (!canDefuse())
        {
            ServerCancelDefusing();
            return;
        }
        player.RpcSetState(PlayerState.Defusing);
        if (gameManager.DefuseTimeLeft < gameManager.BombDefuseTime)
        {
            gameManager.DefuseTimeLeft += Time.deltaTime;
        }
        else
        {
            ServerDefuseBomb();
        }
    }

    [Server]
    void ServerDefuseBomb()
    {
        Debug.Log("finished defusing");
        gameManager.ServerSetDefusedState();
        ServerCancelDefusing();
    }

    [Server]
    void ServerHandlePlanting()
    {
        if (!canPlant())
        {
            ServerCancelPlanting();
            return;
        }
        player.RpcSetState(PlayerState.Planting);
        if (gameManager.PlantTimeLeft < gameManager.BombPlantTime)
        {
            gameManager.PlantTimeLeft += Time.deltaTime;
        }
        else if (gameManager.PlantTimeLeft >= gameManager.BombPlantTime)
        {
            ServerPlantBomb();
        }
    }

    [Server]
    void ServerPlantBomb()
    {
        Debug.Log("finished planting!");
        
        // fixme not sure about sequential ordering
        gameManager.ServerSetPlantedState();
        
        ServerCancelPlanting();
        playerInventoryManager.Bomb = null;
        playerInventoryManager.RpcSwitchItem(playerInventoryManager.PreviousEquippedItem);
        
        GameObject bomb = Instantiate(bombPrefab);
        NetworkServer.Spawn(bomb);
        
        RpcSpawnBomb(bomb, transform.position, transform.rotation);
    }

    [Server]
    void ServerCancelPlanting()
    {
        Debug.Log("ServerCancelPlanting");
        player.RpcSetState(PlayerState.Idle);
        player.PlayerState = PlayerState.Idle;
        gameManager.isPlanting = false;
        
        gameManager.PlantTimeLeft = 0;
        ServerSetPlantSlider(false);
    }

    [Server]
    void ServerCancelDefusing()
    {
        Debug.Log("ServerCancelDefusing");
        player.RpcSetState(PlayerState.Idle);
        gameManager.isDefusing = false;
        
        if (gameManager.DefuseTimeLeft >= gameManager.BombHalfDefuseTime)
        {
            gameManager.DefuseTimeLeft = gameManager.BombHalfDefuseTime;
        }
        else
        {
            gameManager.DefuseTimeLeft = 0;
        }
        
        ServerSetDefuseSlider(false);
    }
    
    [Server]
    void ServerSetPlantSlider(bool enable)
    {
        foreach (var playerID in gameManager.PlayersID)
        {
            var player = gameManager.getPlayer(playerID);
            if (player.PlayerTeam == Team.Red)
            {
                // RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, enable);
            }
        }
    }

    [Server]
    void ServerSetDefuseSlider(bool enable)
    {
        foreach (var playerID in gameManager.BlueTeamPlayersIDs)
        {
            Player player = gameManager.getPlayer(playerID);
            // RpcDefuseSlider((NetworkConnectionToClient)player.connectionToClient, enable);
        }
    }

    #endregion

    #region rpcs

    /*
    [TargetRpc]
    void RpcDefuseSlider(NetworkConnection conn, bool enable)
    {
        // gameManager.DefuseProgressSlider.gameObject.SetActive(enable);
        // gameManager.DefuseProgressSlider.transform.parent.GetChild(1).gameObject.SetActive(enable);
    }
    */

    [ClientRpc]
    void RpcSpawnBomb(GameObject bomb, Vector3 position, Quaternion rotation)
    {
        bomb.transform.SetParent(gameManager.transform);
        bomb.transform.position = position + new Vector3(0,-1,0);
        bomb.transform.rotation = rotation;
    }
    
    /*
    [TargetRpc]
    void RpcPlantSlider(NetworkConnection conn, bool enable)
    {
        // gameManager.PlantProgressSlider.gameObject.SetActive(enable);
    }
    */

    #endregion

    #region commands

    void startPlanting(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (player.PlayerTeam == Team.Blue) return;
        Debug.Log(context.control.path);
        if (context.control.path == "/Keyboard/f" && playerInventoryManager.Bomb != null && canPlant()) CmdStartPlanting();
        else if(playerInventoryManager.Bomb != null && canPlant() && playerInventoryManager.EquippedItem == Item.Bomb) CmdStartPlanting();

        
    }

    void stopPlanting(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (player.PlayerTeam == Team.Blue) return;
        if(playerInventoryManager.EquippedItem == Item.Bomb) CmdStopPlanting();
    }

    void startDefusing(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (player.PlayerTeam == Team.Red) return;
        if(!canDefuse()) return;
        CmdStartDefusing();
    }

    void stopDefusing(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (player.PlayerTeam == Team.Red) return;
        if(!canDefuse()) return;
        CmdStopDefusing();
    }

    [Command]
    void CmdStartPlanting()
    {
        // this is still unsafe bcs playerInventoryManager.Bomb is controled by client 
        if (!canPlant() || gameManager.isPlanting) return;
        Debug.Log("CmdStartPlanting");
        player.PlayerState = PlayerState.Planting;
        gameManager.isPlanting = true;
        ServerSetPlantSlider(true);
    }

    [Command]
    void CmdStopPlanting() {
        Debug.Log("CmdStopPlanting");
        ServerCancelPlanting();
    }

    [Command]
    void CmdStartDefusing()
    {
        if (!canDefuse() || gameManager.isDefusing) return;
        Debug.Log("CmdStartDefusing");
        player.PlayerState = PlayerState.Defusing;
        gameManager.isDefusing = true;

        ServerSetDefuseSlider(true);
    }

    [Command]
    void CmdStopDefusing() => ServerCancelDefusing();

    #endregion
}
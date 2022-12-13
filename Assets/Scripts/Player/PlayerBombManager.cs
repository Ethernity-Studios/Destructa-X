using System;
using Mirror;
using UnityEngine;

public class PlayerBombManager : NetworkBehaviour
{
    [SyncVar]
    bool isInPlantableArea;
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
    [SerializeField] float bombPlantOffset;

    private void Awake()
    {
        player = GetComponent<Player>();
        // if (isServer) gameManager = FindObjectOfType<GameManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
    }

    private void Start()
    {
        if (isServer) gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (isServer) ServerUpdate();
        if (!isLocalPlayer) return;

        switch (player.PlayerTeam)
        {
            case Team.Red:
                ClientHandlePlanting();
                break;
            case Team.Blue:
                ClientHandleDefusing();
                break;
            case Team.None:
                throw new Exception("FUCK ME");
        }
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

        if (!isInPlantableArea)
        {
            Debug.Log("not in planable area");
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
        if (gameState != GameState.Round && gameState != GameState.PostRound)
        {
            Debug.Log("invalid game state");
            return false;
        }

        return true;
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
        if (other.gameObject.CompareTag("PlantArea")) isInPlantableArea = true;

        if (other.gameObject.CompareTag("Bomb")) isInBombArea = true;
    }

    [Server]
    private void checkCollisionExitServer(Collider other)
    {
        if (other.gameObject.CompareTag("PlantArea")) isInPlantableArea = false;

        if (other.gameObject.CompareTag("Bomb")) isInBombArea = false;
    }
    
    [Server]
    private void checkCollisionStayServer(Collider other)
    {
        if (other.gameObject.CompareTag("PlantArea")) isInPlantableArea = true;
        
        if (other.gameObject.CompareTag("Bomb")) isInBombArea = true;
    }

    [Server]
    void ServerUpdate()
    {
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
        playerInventoryManager.RpcSwitchItem(playerInventoryManager.PreviousEqupiedItem);
        
        GameObject bomb = Instantiate(bombPrefab);
        NetworkServer.Spawn(bomb);
        
        RpcSpawnBomb(bomb, transform.position, transform.rotation);
    }

    [Server]
    void ServerCancelPlanting()
    {
        Debug.Log("ServerCancelPlanting");
        player.PlayerState = PlayerState.Idle;
        gameManager.isPlanting = false;
        
        gameManager.PlantTimeLeft = 0;
        ServerSetPlantSlider(false);
    }

    [Server]
    void ServerCancelDefusing()
    {
        Debug.Log("ServerCancelDefusing");
        player.PlayerState = PlayerState.Idle;
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
            var player = NetworkServer.spawned[playerID].GetComponent<Player>();
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
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            // RpcDefuseSlider((NetworkConnectionToClient)player.connectionToClient, enable);
        }
    }

    #endregion

    #region client

    [Client]
    void ClientHandlePlanting()
    {
        if ((Input.GetKeyDown(KeyCode.F) ||
             playerInventoryManager.EqupiedItem == Item.Bomb && Input.GetMouseButtonDown(0)) && canPlant())
        {
            CmdStartPlanting();
        }
        if (Input.GetKeyUp(KeyCode.F) || playerInventoryManager.EqupiedItem == Item.Bomb && Input.GetMouseButtonUp(0))
        {
            CmdStopPlanting();
        }
    }

    [Client]
    void ClientHandleDefusing()
    {
        if(!canDefuse()) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        { 
            CmdStartDefusing();
        }
        if (Input.GetKeyUp(KeyCode.F))
        { 
            CmdStopDefusing();
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
        bomb.transform.position = position;
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
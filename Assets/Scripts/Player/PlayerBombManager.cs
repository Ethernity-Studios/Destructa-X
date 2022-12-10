using Mirror;
using UnityEngine;

public class PlayerBombManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(sanityMinus))]
    public bool isInPlantableArea;
    [SyncVar]
    public bool isInBombArea;

    GameManager gameManager;
    Player player;
    PlayerInventoryManager playerInventoryManager;

    [SerializeField] GameObject bombPrefab;
    [SerializeField] float bombPlantOffset;
    
    [SyncVar]
    public float PlantTimeLeft;
    [SyncVar]
    public float DefuseTimeLeft;

    [SyncVar] public bool isPlanting;
    [SyncVar] public bool isDefusing;

    void sanityMinus(bool _, bool newValue)
    {
        Debug.Log($"idk state {newValue}");
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        gameManager = FindObjectOfType<GameManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
    }
    void Update()
    {
        
        if (isServer) ServerUpdate();
        if (!isLocalPlayer) return;
        
        gameManager.PlantProgressSlider.value = (PlantTimeLeft / gameManager.BombPlantTime) * 100;
        gameManager.DefuseProgressSlider.value = (DefuseTimeLeft / gameManager.BombDefuseTime) * 100;

        switch (player.PlayerTeam)
        {
            case Team.Red:
                ClientHandlePlanting();
                break;
            case Team.Blue:
                ClientHandleDefusing();
                break;
        }
    }

    #region Planting

    [Client]
    void ClientHandlePlanting()
    {
        if (!canPlant()) return;

        if (Input.GetKey(KeyCode.F) || playerInventoryManager.EqupiedItem == Item.Bomb && Input.GetMouseButton(0))
        {
            if (!isPlanting) CmdStartPlanting();
        }
        else
        {
            if (isPlanting) CmdStopPlanting();
        }
    }

    [Client]
    void ClientHandleDefusing()
    {
        if(!canDefuse()) return;
        
        if (Input.GetKey(KeyCode.F))
        {
            if (!isDefusing) CmdStartDefusing();
        }
        else
        {
            if (isDefusing) CmdStopDefusing();
        }
    }

    #endregion

    #region Defusing

    bool canPlant()
    {
        return isInPlantableArea && playerInventoryManager.Bomb != null && player.PlayerTeam == Team.Red && !player.IsDead &&
               (gameManager.GameState == GameState.Round || gameManager.GameState == GameState.PostRound);
    }
    
    bool canDefuse()
    {
        return isInBombArea && player.PlayerTeam == Team.Blue && !player.IsDead && gameManager.BombState == BombState.Planted;
    }
    

    #endregion
    
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
        if (isDefusing) ServerHandleDefusing();
        else if (isPlanting) ServerHandlePlanting();
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
        if (DefuseTimeLeft < gameManager.BombDefuseTime)
        {
            DefuseTimeLeft += Time.deltaTime;
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
        if (PlantTimeLeft < gameManager.BombPlantTime)
        {
            PlantTimeLeft += Time.deltaTime;
        }
        else if (PlantTimeLeft >= gameManager.BombPlantTime)
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
        isPlanting = false;
        
        PlantTimeLeft = 0;
        ServerSetPlantSlider(false);
    }

    [Server]
    void ServerCancelDefusing()
    {
        Debug.Log("ServerCancelDefusing");
        player.PlayerState = PlayerState.Idle;
        isDefusing = false;
        
        if (DefuseTimeLeft >= gameManager.BombHalfDefuseTime)
        {
            DefuseTimeLeft = gameManager.BombHalfDefuseTime;
        }
        else
        {
            DefuseTimeLeft = 0;
        }
        
        ServerSetDefuseSlider(false);
    }

    #endregion

    #region client

    

    #endregion

    #region rpcs

    [ClientRpc]
    void RpcChangeDefuseSliderValue()
    {
        gameManager.DefuseProgressSlider.value = (DefuseTimeLeft / gameManager.BombDefuseTime) * 100;
    }
    
    [TargetRpc]
    void RpcDefuseSlider(NetworkConnection conn, bool enable)
    {
        gameManager.DefuseProgressSlider.gameObject.SetActive(enable);
        gameManager.DefuseProgressSlider.transform.parent.GetChild(1).gameObject.SetActive(enable);
    }

    [ClientRpc]
    void RpcSpawnBomb(GameObject bomb, Vector3 position, Quaternion rotation)
    {
        bomb.transform.SetParent(gameManager.transform);
        bomb.transform.position = position;
        bomb.transform.rotation = rotation;
    }
    
    [TargetRpc]
    void RpcPlantSlider(NetworkConnection conn, bool enable)
    {
        gameManager.PlantProgressSlider.gameObject.SetActive(enable);
    }
    
    [ClientRpc]
    void RpcChangePlantSliderValue()
    {
        gameManager.PlantProgressSlider.value = (PlantTimeLeft / gameManager.BombPlantTime) * 100;
    }

    #endregion

    #region syncCallbacks

    #endregion

    #region commands

    [Command]
    void CmdPlantSlider(bool enable)
    {
        foreach (var playerID in gameManager.PlayersID)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            if (player.PlayerTeam == Team.Red)
            {
                RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, enable);
                //GameObject plantProgressSlider = gameManager.PlantProgressSlider.gameObject;
                /*
                if (plantProgressSlider.activeInHierarchy)
                {
                    RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, enable);
                }
                else
                {
                    RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, enable);
                }
                */
            }
        }
    }

    [Server]
    void ServerSetPlantSlider(bool enable)
    {
        foreach (var playerID in gameManager.PlayersID)
        {
            var player = NetworkServer.spawned[playerID].GetComponent<Player>();
            if (player.PlayerTeam == Team.Red)
            {
                RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, enable);
            }
        }
    }

    [Server]
    void ServerSetDefuseSlider(bool enable)
    {
        foreach (var playerID in gameManager.BlueTeamPlayersIDs)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            RpcDefuseSlider((NetworkConnectionToClient)player.connectionToClient, enable);
        }
    }

    [Command]
    void CmdSetPlantTimeLeft(float time) => PlantTimeLeft = time;

    [Command]
    void CmdChangePlantSliderValue() => RpcChangePlantSliderValue();

    [Command]
    void CmdStartPlanting()
    {
        // this is still unsafe bcs playerInventoryManager.Bomb is controled by client 
        if (!canPlant()) return;
        Debug.Log("CmdStartPlanting");
        player.PlayerState = PlayerState.Planting;
        isPlanting = true;
        ServerSetPlantSlider(true);
    }

    [Command]
    void CmdStopPlanting() => ServerCancelPlanting();

    [Command]
    void CmdStartDefusing()
    {
        if (!canDefuse()) return;
        Debug.Log("CmdStartDefusing");
        player.PlayerState = PlayerState.Defusing;
        isDefusing = true;

        ServerSetDefuseSlider(true);
    }

    [Command]
    void CmdStopDefusing() => ServerCancelDefusing();

    #endregion

}
using Mirror;
using UnityEngine;

public class PlayerBombManager : NetworkBehaviour
{
    public bool isInPlantableArea;

    public bool isInBombArea;

    GameManager gameManager;
    Player player;
    PlayerInventoryManager playerInventoryManager;

    [SerializeField] GameObject bombPrefab;
    
    [SerializeField] float bombPlantOffset;
    
    [SyncVar]
    public float PlantTimeLeft = 0;
    [SyncVar]
    public float DefuseTimeLeft = 0;

    private void Awake()
    {
        player = GetComponent<Player>();
        gameManager = FindObjectOfType<GameManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
    }
    void Update()
    {
        if (!isLocalPlayer) return;

        if (player.PlayerTeam == Team.Red) plantBomb();
        if (player.PlayerTeam == Team.Blue) defuseBomb();
    }

    #region Planting

    void plantBomb()
    {
        if (isInPlantableArea && playerInventoryManager.Bomb != null)
        {
            if (player.PlayerState != PlayerState.Planting) startPlanting();
            if (Input.GetKey(KeyCode.F) || Input.GetMouseButton(0) && player.PlayerState == PlayerState.Planting)
            {
                if (PlantTimeLeft < gameManager.BombPlantTime)
                {
                    CmdIncreasePlantTimeLeft();
                    CmdChangePlantSliderValue();
                }
            }
        }

        if (player.PlayerState == PlayerState.Planting && PlantTimeLeft >= gameManager.BombPlantTime)
        {
            finishPlanting();
        }
        else if (player.PlayerState == PlayerState.Planting && Input.GetKeyUp(KeyCode.F) || Input.GetMouseButtonUp(0) && player.PlayerState == PlayerState.Planting)
        {
            stopPlanting();
        }
    }
    void startPlanting()
    {
        if (Input.GetKey(KeyCode.F) || playerInventoryManager.EqupiedItem == Item.Bomb && Input.GetMouseButton(0))
        {
            Debug.Log("started planting");
            CmdSetPlantTimeLeft(0);
            player.PlayerState = PlayerState.Planting;
            foreach (var playerID in gameManager.RedTeamPlayersIDs)
            {
                CmdPlantSlider(true);
            }
        }
    }

    void stopPlanting()
    {
        Debug.Log("stopped planting");
        player.PlayerState = PlayerState.Idle;
        CmdSetPlantTimeLeft(0);
        gameManager.PlantProgressSlider.value = 0;
        CmdChangePlantSliderValue();
        foreach (var playerID in gameManager.RedTeamPlayersIDs)
        {
            CmdPlantSlider(false);
        }
    }

    void finishPlanting()
    {
        Debug.Log("finished planting!");
        if (gameManager.GameState != GameState.PostRound || gameManager.GameState == GameState.EndGame)
        {
            gameManager.CmdSetGameTime(gameManager.BombDetonationTime);
            gameManager.CmdChangeBombState(BombState.Planted);
            gameManager.CmdSetBombPlanted();
        }
        stopPlanting();
        playerInventoryManager.Bomb = null;
        playerInventoryManager.CmdSwitchItem(playerInventoryManager.PreviousEqupiedItem);
        CmdInstantiateBomb();
    }

    #endregion

    #region Defusing

    void defuseBomb()
    {
        if (isInBombArea && player.PlayerTeam == Team.Blue && gameManager.GameState != GameState.PostRound && gameManager.GameState != GameState.EndGame)
        {
            if (player.PlayerState != PlayerState.Defusing) startDefusing();
            if (Input.GetKey(KeyCode.F) && player.PlayerState == PlayerState.Defusing)
            {
                if (DefuseTimeLeft < gameManager.BombDefuseTime)
                {
                    IncreaseDefuseTimeLeft();
                    CmdChangeDefuseSliderValue();
                }
            }
        }

        if (player.PlayerState == PlayerState.Defusing && DefuseTimeLeft >= gameManager.BombDefuseTime)
        {
            finishDefusing();
        }
        else if (player.PlayerState == PlayerState.Defusing && Input.GetKeyUp(KeyCode.F))
        {
            stopDefusing();
        }
    }

    void startDefusing()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("started defusing");
            if (DefuseTimeLeft >= gameManager.BombDefuseTime / 2) CmdSetDefuseTimeLeft(gameManager.BombDefuseTime / 2);
            else CmdSetDefuseTimeLeft(0);

            player.PlayerState = PlayerState.Defusing;
            foreach (var playerID in gameManager.BlueTeamPlayersIDs)
            {
                CmdDefuseSlider(true);
            }
        }
    }

    void stopDefusing()
    {
        Debug.Log("stopped defusing");
        player.PlayerState = PlayerState.Idle;
        if (DefuseTimeLeft >= gameManager.BombDefuseTime / 2)
        {
            CmdSetDefuseTimeLeft(gameManager.BombDefuseTime / 2);
            gameManager.DefuseProgressSlider.value = 50;
        }
        else if (DefuseTimeLeft < gameManager.BombDefuseTime / 2)
        {
            CmdSetDefuseTimeLeft(0);
            gameManager.DefuseProgressSlider.value = 0;
        }

        CmdChangeDefuseSliderValue();
        foreach (var playerID in gameManager.BlueTeamPlayersIDs)
        {
            CmdDefuseSlider(false);
        }
    }

    void finishDefusing()
    {
        Debug.Log("finished defusing");
        gameManager.CmdChangeBombState(BombState.Defused);
        // SUS
        gameManager.CmdSetGameTime(gameManager.PostRoundlenght);
        gameManager.CmdChangeGameState(GameState.PostRound);
        stopDefusing();
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlantArea")) isInPlantableArea = true;

        if (other.gameObject.CompareTag("Bomb")) isInBombArea = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("PlantArea")) isInPlantableArea = true;

        if (other.gameObject.CompareTag("Bomb")) isInBombArea = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("PlantArea")) isInPlantableArea = false;

        if (other.gameObject.CompareTag("Bomb")) isInBombArea = false;
    }

    #region server

    

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
    void RpcSetupBomb(GameObject bomb)
    {
        bomb.transform.SetParent(gameManager.transform);
        bomb.transform.position = new Vector3(transform.position.x, transform.position.y - bombPlantOffset, transform.position.z);
        bomb.transform.rotation = transform.rotation;
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
    void CmdDefuseSlider(bool enable)
    {
        foreach (var playerID in gameManager.BlueTeamPlayersIDs)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            RpcDefuseSlider((NetworkConnectionToClient)player.connectionToClient, enable);

        }
    }
    
    [Command]
    void CmdChangeDefuseSliderValue() => RpcChangeDefuseSliderValue();
    
    [Command]
    void CmdSetDefuseTimeLeft(float time) => DefuseTimeLeft = time;

    [Command]
    void IncreaseDefuseTimeLeft() => DefuseTimeLeft += Time.deltaTime;
    
    [Command]
    void CmdInstantiateBomb()
    {
        GameObject bomb = Instantiate(bombPrefab);
        NetworkServer.Spawn(bomb);
        RpcSetupBomb(bomb);
    }
    
    [Command]
    void CmdPlantSlider(bool enable)
    {
        foreach (var playerID in gameManager.PlayersID)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            if (player.PlayerTeam == Team.Red)
            {
                GameObject plantProgressSlider = gameManager.PlantProgressSlider.gameObject;
                if (plantProgressSlider.activeInHierarchy)
                {
                    RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, enable);
                }
                else
                {
                    RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, enable);
                }
            }
        }
    }
    
    [Command]
    void CmdIncreasePlantTimeLeft() => PlantTimeLeft += Time.deltaTime;

    [Command]
    void CmdSetPlantTimeLeft(float time) => PlantTimeLeft = time;

    [Command]
    void CmdChangePlantSliderValue() => RpcChangePlantSliderValue();

    #endregion

}

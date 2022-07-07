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

    [SyncVar]
    public float plantTimeLeft = 0;
    [SyncVar]
    public float defuseTimeLeft = 0;

    #region Planting

    void plantBomb()
    {
        if (isInPlantableArea && playerInventoryManager.Bomb != null)
        {
            if (player.PlayerState != PlayerState.Planting) startPlanting();
            if (Input.GetKey(KeyCode.F) || Input.GetMouseButton(0) && player.PlayerState == PlayerState.Planting)
            {
                if (plantTimeLeft < gameManager.BombPlantTime)
                {
                    CmdIncreasePlantTimeLeft();
                    CmdChangePlantSliderValue();
                }
            }
        }

        if (player.PlayerState == PlayerState.Planting && plantTimeLeft >= gameManager.BombPlantTime)
        {
            finishPlanting();
        }
        else if (player.PlayerState == PlayerState.Planting && Input.GetKeyUp(KeyCode.F) || Input.GetMouseButtonUp(0) && player.PlayerState == PlayerState.Planting)
        {
            stopPlanting();
        }
    }
    [Command]
    void CmdIncreasePlantTimeLeft() => plantTimeLeft += Time.deltaTime;

    [Command]
    void CmdSetPlantTimeLeft(float time) => plantTimeLeft = time;

    [Command]
    void CmdChangePlantSliderValue() => RpcChangePlantSliderValue();

    [ClientRpc]
    void RpcChangePlantSliderValue()
    {
        gameManager.PlantProgressSlider.value = (plantTimeLeft / gameManager.BombPlantTime) * 100;
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

    [TargetRpc]
    void RpcPlantSlider(NetworkConnection conn, bool enable)
    {
        gameManager.PlantProgressSlider.gameObject.SetActive(enable);
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

    [Command]
    void CmdInstantiateBomb()
    {
        GameObject bomb = Instantiate(bombPrefab);
        NetworkServer.Spawn(bomb);
        RpcSetupBomb(bomb);
    }
    [ClientRpc]
    void RpcSetupBomb(GameObject bomb)
    {
        bomb.transform.SetParent(gameManager.transform);
        bomb.transform.position = new Vector3(transform.position.x, transform.position.y - bombPlantOffset, transform.position.z);
        bomb.transform.rotation = transform.rotation;
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
                if (defuseTimeLeft < gameManager.BombDefuseTime)
                {
                    IncreaseDefuseTimeLeft();
                    CmdChangeDefuseSliderValue();
                }
            }
        }

        if (player.PlayerState == PlayerState.Defusing && defuseTimeLeft >= gameManager.BombDefuseTime)
        {
            finishDefusing();
        }
        else if (player.PlayerState == PlayerState.Defusing && Input.GetKeyUp(KeyCode.F))
        {
            stopDefusing();
        }
    }

    [Command]
    void CmdChangeDefuseSliderValue() => RpcChangeDefuseSliderValue();

    [ClientRpc]
    void RpcChangeDefuseSliderValue()
    {
        gameManager.DefuseProgressSlider.value = (defuseTimeLeft / gameManager.BombDefuseTime) * 100;
    }

    void startDefusing()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("started defusing");
            if (defuseTimeLeft >= gameManager.BombDefuseTime / 2) CmdSetDefuseTimeLeft(gameManager.BombDefuseTime / 2);
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
        if (defuseTimeLeft >= gameManager.BombDefuseTime / 2)
        {
            CmdSetDefuseTimeLeft(gameManager.BombDefuseTime / 2);
            gameManager.DefuseProgressSlider.value = 50;
        }
        else if (defuseTimeLeft < gameManager.BombDefuseTime / 2)
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
        gameManager.CmdSetGameTime(gameManager.PostRoundlenght);
        gameManager.CmdChangeGameState(GameState.PostRound);
        stopDefusing();
    }

    [Command]
    void CmdSetDefuseTimeLeft(float time) => defuseTimeLeft = time;

    [Command]
    void IncreaseDefuseTimeLeft() => defuseTimeLeft += Time.deltaTime;

    [Command]
    void CmdDefuseSlider(bool enable)
    {
        foreach (var playerID in gameManager.BlueTeamPlayersIDs)
        {
            Player player = NetworkServer.spawned[playerID].GetComponent<Player>();
            RpcDefuseSlider((NetworkConnectionToClient)player.connectionToClient, enable);

        }
    }

    [TargetRpc]
    void RpcDefuseSlider(NetworkConnection conn, bool enable)
    {
        gameManager.DefuseProgressSlider.gameObject.SetActive(enable);
        gameManager.DefuseProgressSlider.transform.parent.GetChild(1).gameObject.SetActive(enable);
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

}

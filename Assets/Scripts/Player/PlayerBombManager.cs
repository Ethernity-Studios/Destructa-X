using Mirror;
using System;
using UnityEngine;

public class PlayerBombManager : NetworkBehaviour
{
    public bool isInPlantableArea;

    public bool isInBombArea;

    GameManager gameManager;
    PlayerManager playerManager;

    [SerializeField] GameObject bombPrefab;

    [SerializeField] float bombPlantOffset;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        gameManager = FindObjectOfType<GameManager>();
    }
    void Update()
    {
        if (!isLocalPlayer) return;

        if (playerManager.PlayerTeam == Team.Red) plantBomb();
        if (playerManager.PlayerTeam == Team.Blue) defuseBomb();
    }

    [SyncVar]
    public float plantTimeLeft = 0;
    [SyncVar]
    public float defuseTimeLeft = 0;

    #region Planting

    void plantBomb()
    {
        if (isInPlantableArea)
        {
            if (playerManager.PlayerState != PlayerState.Planting) startPlanting();
            if (Input.GetKey(KeyCode.F) && playerManager.PlayerState == PlayerState.Planting)
            {
                if (plantTimeLeft < gameManager.BombPlantTime)
                {
                    CmdIncreasePlantTimeLeft();
                    CmdChangePlantSliderValue();
                }
            }
        }

        if (playerManager.PlayerState == PlayerState.Planting && plantTimeLeft >= gameManager.BombPlantTime)
        {
            finishPlanting();
        }
        else if (playerManager.PlayerState == PlayerState.Planting && Input.GetKeyUp(KeyCode.F))
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
        foreach (var player in gameManager.Players)
        {
            if(player.PlayerTeam == Team.Red)
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("started planting");
            CmdSetPlantTimeLeft(0);
            playerManager.PlayerState = PlayerState.Planting;
            foreach (var player in gameManager.Players)
            {
                if (player.PlayerTeam == Team.Red)
                {
                    CmdPlantSlider(true);
                } 
            }
        }
    }

    void stopPlanting()
    {
        Debug.Log("stopped planting");
        playerManager.PlayerState = PlayerState.Idle;
        CmdSetPlantTimeLeft(0);
        gameManager.PlantProgressSlider.value = 0;
        CmdChangePlantSliderValue();
        foreach (var player in gameManager.Players)
        {
            if (player.PlayerTeam == Team.Red) 
            {
                CmdPlantSlider(false);
            }
        }
    }

    void finishPlanting()
    {
        Debug.Log("finished planting!");
        gameManager.CmdSetGameTime(gameManager.BombDetonationTime);
        gameManager.CmdChangeBombState(BombState.Planted);
        stopPlanting();
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
        bomb.transform.SetParent(GameObject.Find("World").transform);
        bomb.transform.position = new Vector3(transform.position.x, transform.position.y - bombPlantOffset, transform.position.z);
        bomb.transform.rotation = transform.rotation;
    }

    #endregion

    #region Defusing

    void defuseBomb()
    {
        if (isInBombArea && playerManager.PlayerTeam == Team.Blue)
        {
            if (playerManager.PlayerState != PlayerState.Defusing) startDefusing();
            if (Input.GetKey(KeyCode.F) && playerManager.PlayerState == PlayerState.Defusing)
            {
                if (defuseTimeLeft < gameManager.BombDefuseTime)
                {
                    IncreaseDefuseTimeLeft();
                    CmdChangeDefuseSliderValue();
                }
            }
        }

        if (playerManager.PlayerState == PlayerState.Defusing && defuseTimeLeft >= gameManager.BombDefuseTime)
        {
            finishDefusing();
        }
        else if (playerManager.PlayerState == PlayerState.Defusing && Input.GetKeyUp(KeyCode.F))
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

            playerManager.PlayerState = PlayerState.Defusing;
            foreach (var player in gameManager.Players)
            {
                if (player.PlayerTeam == Team.Blue)
                {
                    CmdDefuseSlider(true);
                }
            }
        }
    }

    void stopDefusing()
    {
        Debug.Log("stopped defusing");
        playerManager.PlayerState = PlayerState.Idle;
        if (defuseTimeLeft >= gameManager.BombDefuseTime / 2)
        {
            CmdSetDefuseTimeLeft(gameManager.BombDefuseTime / 2);
            gameManager.DefuseProgressSlider.value = 50;
        }
        else if (defuseTimeLeft < gameManager.BombDefuseTime/2)
        {
            CmdSetDefuseTimeLeft(0);
            gameManager.DefuseProgressSlider.value = 0;
        }

        CmdChangeDefuseSliderValue();
        foreach (var player in gameManager.Players)
        {
            if (player.PlayerTeam == Team.Blue)
            {
                CmdDefuseSlider(false);
            }
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
        foreach (var player in gameManager.Players)
        {
            if (player.PlayerTeam == Team.Blue)
            {
                RpcDefuseSlider((NetworkConnectionToClient)player.connectionToClient, enable);
            }
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

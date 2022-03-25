using Mirror;
using System;
using UnityEngine;

public class PlayerBombManager : NetworkBehaviour
{
    public bool isInPlantableArea;

    GameManager gameManager;
    PlayerManager playerManager;

    [SerializeField] GameObject bombPrefab;

    [SerializeField] float bombPlantOffset;

    BombManager bomb;
    private void Awake()
    {
        bomb = FindObjectOfType<BombManager>();
        playerManager = GetComponent<PlayerManager>();
        gameManager = FindObjectOfType<GameManager>();
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        plantBomb();
        defuseBomb();
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
            if (Input.GetKey(KeyCode.E) && playerManager.PlayerState == PlayerState.Planting)
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
        else if (playerManager.PlayerState == PlayerState.Planting && Input.GetKeyUp(KeyCode.E))
        {
            stopPlanting();
        }
    }
    [Command]
    void CmdIncreasePlantTimeLeft()
    {
        plantTimeLeft += Time.deltaTime;
    }
    [Command]
    void CmdSetPlantTimeLeft(float time)
    {
        plantTimeLeft = time;
    }

    [Command]
    void CmdChangePlantSliderValue() => RpcChangePlantSliderValue();

    [ClientRpc]
    void RpcChangePlantSliderValue()
    {
        gameManager.PlantProgressSlider.value = (plantTimeLeft / gameManager.BombPlantTime) * 100;
    }
    [Command]
    void CmdPlantSlider() 
    {
        foreach (var player in gameManager.Players)
        {
            if(player.PlayerTeam == Team.Red)
            {
                GameObject plantProgressSlider = gameManager.PlantProgressSlider.gameObject;
                if (plantProgressSlider.activeInHierarchy)
                {
                    RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, false);
                }
                else
                {
                    RpcPlantSlider((NetworkConnectionToClient)player.connectionToClient, true);
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("started planting");
            CmdSetPlantTimeLeft(0);
            playerManager.PlayerState = PlayerState.Planting;
            foreach (var player in gameManager.Players)
            {
                if (player.PlayerTeam == Team.Red)
                {
                    CmdPlantSlider();
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
            Debug.Log(player.connectionToClient + "cpn to client");
            if (player.PlayerTeam == Team.Red) 
            {
                CmdPlantSlider();
            }

        }
    }

    void finishPlanting()
    {
        Debug.Log("finished planting!");
        gameManager.CmdSetGameTime(gameManager.BombDetonationTime);
        stopPlanting();
        CmdInstantiateBomb();

        bomb = FindObjectOfType<BombManager>();
        bomb.Invoke("CmdDetonateBomb", 2f);
    }

    [Command]
    void CmdInstantiateBomb()
    {
        GameObject bomb = Instantiate(bombPrefab, GameObject.Find("World").transform);
        bomb.transform.position = new Vector3(transform.position.x, transform.position.y - bombPlantOffset, transform.position.z);
        bomb.transform.rotation = transform.rotation;
        NetworkServer.Spawn(bomb);
    }

    #endregion

    #region Defusing

    void defuseBomb()
    {

    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlantableArea")) isInPlantableArea = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("PlantableArea")) isInPlantableArea = false;
    }

}

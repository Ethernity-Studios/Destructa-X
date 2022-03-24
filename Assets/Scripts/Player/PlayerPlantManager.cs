using Mirror;
using System;
using UnityEngine;

public class PlayerPlantManager : NetworkBehaviour
{
    public bool isInPlantableArea;

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
        plant();
    }
    [SyncVar]
    public float timeLeft = 0;
    void plant()
    {
        if (isInPlantableArea)
        {
            if (playerManager.PlayerState != PlayerState.Planting) startPlanting();
            if (Input.GetKey(KeyCode.E) && playerManager.PlayerState == PlayerState.Planting)
            {
                if (timeLeft < gameManager.BombPlantTime)
                {
                    CmdIncreaseTimeLeft();
                    CmdChangeSliderValue();
                }
            }
        }

        if (playerManager.PlayerState == PlayerState.Planting && timeLeft >= gameManager.BombPlantTime)
        {
            finishPlanting();
        }
        else if (playerManager.PlayerState == PlayerState.Planting && Input.GetKeyUp(KeyCode.E))
        {
            stopPlanting();
        }
    }
    [Command]
    void CmdIncreaseTimeLeft()
    {
        timeLeft += Time.deltaTime;
    }
    [Command]
    void CmdSetTimeLeft(float time)
    {
        timeLeft = time;
    }

    [Command]
    void CmdChangeSliderValue() => RpcChangeSliderValue();

    [ClientRpc]
    void RpcChangeSliderValue()
    {
        gameManager.PlantProgressSlider.value = (timeLeft / gameManager.BombPlantTime) * 100;
    }
    [Command]
    void CmdSlider() 
    {
        foreach (var player in gameManager.Players)
        {
            if(player.PlayerTeam == Team.Red)
            {
                GameObject plantProgressSlider = gameManager.PlantProgressSlider.gameObject;
                if (plantProgressSlider.activeInHierarchy)
                {
                    RpcSlider((NetworkConnectionToClient)player.connectionToClient, false);
                }
                else
                {
                    RpcSlider((NetworkConnectionToClient)player.connectionToClient, true);
                }
            }
        }
    } 

    [TargetRpc]
    void RpcSlider(NetworkConnection conn, bool enable)
    {
        gameManager.PlantProgressSlider.gameObject.SetActive(enable);
    }
    void startPlanting()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("started planting");
            CmdSetTimeLeft(0);
            playerManager.PlayerState = PlayerState.Planting;
            foreach (var player in gameManager.Players)
            {
                Debug.Log(player.connectionToClient + "cpn to client");
                if (player.PlayerTeam == Team.Red)
                {
                    CmdSlider();
                } 
            }
        }
    }

    void stopPlanting()
    {
        Debug.Log("stopped planting");
        playerManager.PlayerState = PlayerState.Idle;
        CmdSetTimeLeft(0);
        gameManager.PlantProgressSlider.value = 0;
        CmdChangeSliderValue();
        foreach (var player in gameManager.Players)
        {
            Debug.Log(player.connectionToClient + "cpn to client");
            if (player.PlayerTeam == Team.Red) 
            {
                CmdSlider();
            }

        }
    }
    GameObject bomb;
    void finishPlanting()
    {
        Debug.Log("finished planting!");
        gameManager.CmdAddGameTime(gameManager.BombDetonationTime);
        stopPlanting();
        CmdInstantiateBomb();
    }

    [Command]
    void CmdInstantiateBomb()
    {
        bomb = Instantiate(bombPrefab, GameObject.Find("World").transform);
        bomb.transform.position = new Vector3(transform.position.x, transform.position.y - bombPlantOffset, transform.position.z);
        bomb.transform.rotation = transform.rotation;
        NetworkServer.Spawn(bomb);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlantableArea")) isInPlantableArea = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("PlantableArea")) isInPlantableArea = false;
    }

}

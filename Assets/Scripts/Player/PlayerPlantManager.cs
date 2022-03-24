using Mirror;
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
                    //Debug.Log("planting!");
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
        RpcSlider();
    } 
    [ClientRpc]
    void RpcSlider()
    {
        if(gameManager.PlantProgressSlider.gameObject.activeInHierarchy)
        {
            gameManager.PlantProgressSlider.gameObject.SetActive(false);
        }
        else
        {
            gameManager.PlantProgressSlider.gameObject.SetActive(true);
        }
    }
    void startPlanting()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("started planting");
            CmdSetTimeLeft(0);
            playerManager.PlayerState = PlayerState.Planting;
            CmdSlider();
            //gameManager.PlantProgressSlider.gameObject.SetActive(true);
        }
    }
    void stopPlanting()
    {
        Debug.Log("stopped planting");
        playerManager.PlayerState = PlayerState.Idle;
        CmdSetTimeLeft(0);
        gameManager.PlantProgressSlider.value = 0;
        CmdChangeSliderValue();
        CmdSlider();
        //gameManager.PlantProgressSlider.gameObject.SetActive(false);
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

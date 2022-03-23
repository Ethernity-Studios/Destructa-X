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
                    Debug.Log("planting!");
                    timeLeft += Time.deltaTime;
                    gameManager.PlantProgressSlider.value = (timeLeft / gameManager.BombPlantTime) * 100;
                }
            }
        }

        if(playerManager.PlayerState == PlayerState.Planting && timeLeft >= gameManager.BombPlantTime)
        {
            finishPlanting();
        }
        else if(playerManager.PlayerState == PlayerState.Planting && Input.GetKeyUp(KeyCode.E))
        {
            stopPlanting();
        }
    }
    void startPlanting()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("started planting");
            timeLeft = 0;
            playerManager.PlayerState = PlayerState.Planting;
            gameManager.PlantProgressSlider.gameObject.SetActive(true);
        }
    }
    void stopPlanting()
    {
            Debug.Log("stopped planting");
            playerManager.PlayerState = PlayerState.Idle;
            timeLeft = 0;
            gameManager.PlantProgressSlider.value = 0;
            gameManager.PlantProgressSlider.gameObject.SetActive(false);
    }

    void finishPlanting()
    {
        Debug.Log("finished planting!");
        stopPlanting();
        GameObject bomb = Instantiate(bombPrefab,GameObject.Find("World").transform);
        bomb.transform.position = new Vector3(transform.position.x, transform.position.y - bombPlantOffset,transform.position.z);
        bomb.transform.rotation = transform.rotation;
        NetworkServer.Spawn(bomb);
        //CmdInstantiateBomb(bomb);
    }

    [Command]
    void CmdInstantiateBomb(GameObject bomb)
    {
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

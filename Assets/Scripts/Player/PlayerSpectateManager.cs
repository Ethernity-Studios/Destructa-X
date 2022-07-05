using System.Collections;
using UnityEngine;

public class PlayerSpectateManager : MonoBehaviour
{
    Player player;
    GameManager gameManager;
    UIManager uiManager;

    [SerializeField] float deathScreenTime;

    public GameObject playerBody;
    public GameObject itemHolder;
    public GameObject playerHands;
    public GameObject playerHead;

    public Camera PlayerCamera;
    public Camera ItemCamera;

    [SerializeField] Player currentlySpectating = null;

    bool isSpectating;
    private void Start()
    {
        player = GetComponent<Player>();
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
    }

    private void Update()
    {
        if (!player.IsDead) return;
        if (!isSpectating) return;
        if (Input.GetMouseButtonDown(0))
        {
            spectate();
        }
    }

    public void PlayerDeath()
    {
        playerBody.transform.localEulerAngles = new Vector3(90, 0, 0);
        playerBody.transform.localPosition = new Vector3(0, -1.5f, 0);
        playerBody.GetComponent<CapsuleCollider>().enabled = false;
        playerBody.transform.parent.GetComponent<CharacterController>().enabled = false;
        itemHolder.SetActive(false);
        playerHead.transform.localPosition = new Vector3(0, 2, 0);
        playerHead.transform.localEulerAngles = new Vector3(90, 0, 0);
        playerHands.GetComponent<Renderer>().enabled = false;
    }

    public IEnumerator PlayerDeathCoroutine()
    {
        itemHolder.SetActive(false);
        playerHands.SetActive(false);
        playerBody.transform.localEulerAngles = new Vector3(90, 0, 0);
        playerBody.transform.localPosition = new Vector3(0,-1.5f,0);
        playerHead.transform.localPosition = new Vector3(0,2,0);
        playerHead.transform.localEulerAngles = new Vector3(90,0,0);
        yield return new WaitForSeconds(deathScreenTime);
        if (player.PlayerTeam == Team.Blue && gameManager.BlueTeamSize > 1) spectate();
        else if (player.PlayerTeam == Team.Red && gameManager.RedTeamSize > 1) spectate();
        else Debug.Log("No players to spectate");
    }

    void spectate()
    {
        isSpectating = true;
        PlayerCamera.enabled = false;
        ItemCamera.enabled = false;
        foreach (var player in gameManager.Players)
        {
            if (player.isLocalPlayer) continue;
            if (player.PlayerTeam != this.player.PlayerTeam) continue;
            if(player == currentlySpectating) continue;
            
            PlayerSpectateManager playerSpectateManager = player.GetComponent<PlayerSpectateManager>();
            playerSpectateManager.PlayerCamera.enabled = true;
            playerSpectateManager.ItemCamera.enabled = true;
            currentlySpectating = player;
            uiManager.SpectatingUI.SetActive(true);
            uiManager.SpectatingPlayerName.text = player.PlayerName;
            Debug.Log("Currently spectating: " + currentlySpectating);
        }
    }
}

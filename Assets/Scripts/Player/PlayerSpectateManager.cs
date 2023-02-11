using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

public class PlayerSpectateManager : NetworkBehaviour
{
    Player player;
    GameManager gameManager;
    UIManager uiManager;

    [SerializeField] float deathScreenTime;

    public GameObject playerBody;
    public GameObject itemHolder;
    public GameObject playerHands;
    public GameObject playerHead;

    //public Camera PlayerCamera;
    //public Camera ItemCamera;

    [SerializeField] Player currentlySpectating = null;

    bool isSpectating;

    GameObject mainCamera;
    private void Start()
    {
        player = GetComponent<Player>();
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        if (!isLocalPlayer) return;
        mainCamera = Camera.main!.gameObject;
        //PlayerCamera.enabled = true;
        //ItemCamera.enabled = true;
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
        mainCamera.GetComponent<CameraRotate>().CanRotate = false;
        mainCamera.transform.parent.GetComponent<CameraMove>().CanMove = false;
        playerBody.transform.localEulerAngles = new Vector3(90, 0, 0);
        playerBody.transform.localPosition = new Vector3(0, -1.5f, 0);
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
        playerBody.transform.localPosition = new Vector3(0, -1.5f, 0);
        playerHead.transform.localPosition = new Vector3(0, 2, 0);
        playerHead.transform.localEulerAngles = new Vector3(90, 0, 0);
        yield return new WaitForSeconds(deathScreenTime);
        switch (player.PlayerTeam)
        {
            case Team.Blue:
            {
                if (gameManager.AliveBluePlayers > 1) spectate();
                break;
            }
            case Team.Red:
            {
                if(gameManager.AliveRedPlayers > 1) spectate();
                //else if(gameManager.AliveRedPlayers == 0 && gameManager.BombPlanted) spectateBomb(); /// REWORK SPECTATE BOMB
                break;
            }
            default:
                Debug.Log("No players to spectate");
                break;
        }
    }

    void spectate()
    {
        isSpectating = true;
        //PlayerCamera.enabled = false;
        //ItemCamera.enabled = false;
        CmdGetPlayers();
        foreach (Player player in from player in players where !player.isLocalPlayer where player.PlayerTeam == this.player.PlayerTeam where player != currentlySpectating let playerSpectateManager = player.GetComponent<PlayerSpectateManager>() select player)
        {
            //playerSpectateManager.PlayerCamera.enabled = true;
            //playerSpectateManager.ItemCamera.enabled = true;
            currentlySpectating = player;
            uiManager.SpectatingUI.SetActive(true);
            uiManager.SpectatingPlayerName.text = player.PlayerName;
            Debug.Log("Currently spectating: " + currentlySpectating);
        }
    }

    readonly List<Player> players = new();
    [Command]
    void CmdGetPlayers()
    {
        foreach (int playerID in gameManager.PlayersID)
        {
            RpcGetPlayers(gameManager.GetPlayer(playerID));
        }
    }
    [ClientRpc]
    void RpcGetPlayers(Player player)
    {
        players.Add(player);
    }

    void spectateBomb()
    {
        isSpectating = true;
       //PlayerCamera.enabled = false;
        //ItemCamera.enabled = false;
        Debug.Log("Currently spectating bomb");
    }

    [ClientRpc]
    public void SetPlayerTransform()
    {
        playerBody.transform.localEulerAngles = new Vector3(0, 0, 0);
        playerBody.transform.localPosition = new Vector3(0, 0, 0);
        itemHolder.SetActive(true);
        playerHead.transform.localPosition = new Vector3(0, .6f, 0);
        playerHead.transform.localEulerAngles = new Vector3(0, 0, 0);
        playerHands.GetComponent<Renderer>().enabled = true;
        Debug.Log("");
    }
}



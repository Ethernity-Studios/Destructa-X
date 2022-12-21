using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

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

    public Camera PlayerCamera;
    public Camera ItemCamera;

    [SerializeField] Player currentlySpectating = null;

    bool isSpectating;
    private void Start()
    {
        player = GetComponent<Player>();
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        if (!isLocalPlayer) return;
        PlayerCamera.enabled = true;
        ItemCamera.enabled = true;
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
        if (player.PlayerTeam == Team.Blue)
        {
            if (gameManager.AliveBluePlayers > 1) spectate();
        }
        else if (player.PlayerTeam == Team.Red) 
        {
            if(gameManager.AliveRedPlayers > 1) spectate();
            //else if(gameManager.AliveRedPlayers == 0 && gameManager.BombPlanted) spectateBomb(); /// REWORK SPECTATE BOMB
        }
        else Debug.Log("No players to spectate");
    }

    void spectate()
    {
        isSpectating = true;
        PlayerCamera.enabled = false;
        ItemCamera.enabled = false;
        CmdGetPlayers();
        foreach (var player in players)
        {
            if (player.isLocalPlayer) continue;
            if (player.PlayerTeam != this.player.PlayerTeam) continue;
            if (player == currentlySpectating) continue;
            PlayerSpectateManager playerSpectateManager = player.GetComponent<PlayerSpectateManager>();
            playerSpectateManager.PlayerCamera.enabled = true;
            playerSpectateManager.ItemCamera.enabled = true;
            currentlySpectating = player;
            uiManager.SpectatingUI.SetActive(true);
            uiManager.SpectatingPlayerName.text = player.PlayerName;
            Debug.Log("Currently spectating: " + currentlySpectating);
        }
    }

    List<Player> players = new();
    [Command]
    void CmdGetPlayers()
    {
        foreach (var playerID in gameManager.PlayersID)
        {
            RpcGetPlayers(gameManager.getPlayer(playerID));
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
        PlayerCamera.enabled = false;
        ItemCamera.enabled = false;
        Debug.Log("Currently spectating bomb");
    }

    [ClientRpc]
    public void SetPlayerTransform()
    {
        playerBody.transform.localEulerAngles = new Vector3(0, 0, 0);
        playerBody.transform.localPosition = new Vector3(0, 0, 0);
        playerBody.GetComponent<CapsuleCollider>().enabled = true;
        itemHolder.SetActive(true);
        playerHead.transform.localPosition = new Vector3(0, .6f, 0);
        playerHead.transform.localEulerAngles = new Vector3(0, 0, 0);
        playerHands.GetComponent<Renderer>().enabled = true;
    }
}

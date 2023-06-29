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

    //public Camera PlayerCamera;
    //public Camera ItemCamera;

    [SerializeField] Player currentlySpectating = null;

    public bool IsSpectating = false;

    GameObject mainCamera;

    private void Start()
    {
        player = GetComponent<Player>();
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        if (!isLocalPlayer) return;
        CmdGetPlayers();
        mainCamera = Camera.main!.gameObject;
        //PlayerCamera.enabled = true;
        //ItemCamera.enabled = true;
    }

    private void Update()
    {
        if (!player.IsDead) return;
        if (!IsSpectating) return;
        if (Input.GetMouseButtonDown(0))
        {
            Spectate();
        }
    }

    public void PlayerDeath()
    {
        if (!isLocalPlayer) return;
        mainCamera = Camera.main!.gameObject;
        mainCamera.GetComponent<CameraRotate>().CanRotate = false;
        mainCamera.transform.parent.GetComponent<CameraMove>().CanMove = false;
        //playerBody.transform.localEulerAngles = new Vector3(90, 0, 0);
        //playerBody.transform.localPosition = new Vector3(0, -1.5f, 0);
        itemHolder.SetActive(false);
        playerHead.transform.localPosition = new Vector3(0, 2, 0);
        playerHead.transform.localEulerAngles = new Vector3(90, 0, 0);
        //playerHands.GetComponent<Renderer>().enabled = false;
    }

    public IEnumerator PlayerDeathCoroutine()
    {
        itemHolder.SetActive(false);
        //TODO SET ANIMATIONS

        //playerHands.SetActive(false);
        //playerBody.transform.localEulerAngles = new Vector3(90, 0, 0);
        //playerBody.transform.localPosition = new Vector3(0, -1.5f, 0);
        //playerHead.transform.localPosition = new Vector3(0, 2, 0);
        //playerHead.transform.localEulerAngles = new Vector3(90, 0, 0);
        yield return new WaitForSeconds(deathScreenTime);
        Spectate();
    }

    public void Spectate()
    {
        switch (player.PlayerTeam)
        {
            case Team.Blue:
            {
                if (gameManager.AliveBluePlayers >= 1)
                {
                    spec();
                }
                else if(gameManager.AliveBluePlayers == 0 && gameManager.BombPlanted) spectateBomb();

                break;
            }
            case Team.Red:
            {
                if (gameManager.AliveRedPlayers >= 1) spec();
                else if (gameManager.AliveRedPlayers == 0 && gameManager.BombPlanted) spectateBomb();
                break;
            }
            default:
                Debug.Log("No players to spectate");
                break;
        }
    }

    void spec()
    {
        IsSpectating = true;

        foreach (var p in players)
        {
            if (p.PlayerTeam == player.PlayerTeam && !p.IsDead && currentlySpectating != p)
            {
                currentlySpectating = p;
                uiManager.SpectatingUI.SetActive(true);
                uiManager.SpectatingPlayerName.text = player.PlayerName;
                mainCamera.transform.parent.GetComponent<CameraMove>().cameraPosition = p.GetComponent<PlayerSpectateManager>().playerHead.transform;
                Debug.Log("Currently spectating: " + currentlySpectating);
            }
        }
    }

    public void ResetSpectate()
    {
        uiManager.SpectatingUI.SetActive(false);
        IsSpectating = false;
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
        mainCamera.transform.parent.GetComponent<CameraMove>().cameraPosition = gameManager.Bomb.transform;
        mainCamera.transform.parent.GetComponent<CameraMove>().cameraPosition.position += new Vector3(0, 2, 0);
        IsSpectating = true;
        //PlayerCamera.enabled = false;
        //ItemCamera.enabled = false;
        Debug.Log("Currently spectating bomb");
    }

    [ClientRpc]
    public void SetPlayerTransform()
    {
        FindObjectOfType<CameraMove>().cameraPosition = GetComponent<PlayerMovement>().playerHead;
        playerBody.transform.localEulerAngles = new Vector3(0, 0, 0);
        playerBody.transform.localPosition = new Vector3(0, 0, 0);
        itemHolder.SetActive(true);
        playerHead.transform.localPosition = new Vector3(0, 1.6f, 0);
        playerHead.transform.localEulerAngles = new Vector3(0, 0, 0);
        //playerHands.GetComponent<Renderer>().enabled = true;
    }
}
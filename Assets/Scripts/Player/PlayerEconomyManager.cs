using UnityEngine;
using Mirror;

public class PlayerEconomyManager : NetworkBehaviour
{
    GameManager gameManager;
    [SyncVar] 
    bool CanBeOpen;
    [HideInInspector]public bool IsShopOpen;
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    [Server]
    void ServerUpdate()
    {
        CanBeOpen = gameManager.GameState == GameState.PreRound || gameManager.GameState == GameState.StartGame;
    }

    private void Update()
    {
        if (isServer) ServerUpdate();
        if (!isLocalPlayer) return;
        if (CanBeOpen)
        {
            if (!Input.GetKeyDown(KeyCode.B)) return;
            if (IsShopOpen)
            {
                CloseShopUI();
            }
            else
            {
                IsShopOpen = true;
                gameManager.ShopUI.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else if (IsShopOpen)
        {
            CloseShopUI();
        }
    }

    public void CloseShopUI()
    {
        IsShopOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        gameManager.ShopUI.SetActive(false);
    }
}
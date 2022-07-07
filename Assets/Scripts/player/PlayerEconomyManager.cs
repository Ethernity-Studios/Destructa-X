using UnityEngine;
using Mirror;

public class PlayerEconomyManager : NetworkBehaviour
{
    GameManager gameManager;

    [HideInInspector]public bool IsShopOpen;
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (!isLocalPlayer) return;
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (gameManager.GameState == GameState.PreRound || gameManager.GameState == GameState.StartGame)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {

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
        }
    }

    public void CloseShopUI()
    {
        IsShopOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        gameManager.ShopUI.SetActive(false);
    }
}
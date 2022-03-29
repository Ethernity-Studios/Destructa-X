using UnityEngine;
using Mirror;

public class PlayerEconomyManager : NetworkBehaviour
{
    GameManager gameManager;

    [HideInInspector]public bool isShopOpen;
    private void Start()
    {
        if (!isLocalPlayer) return;
        gameManager = FindObjectOfType<GameManager>();
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (gameManager.GameState == GameState.PreRound || gameManager.GameState == GameState.StartGame)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {

                if (gameManager.ShopUI.activeInHierarchy)
                {
                    isShopOpen = false;
                    CloseShopUI();
                }
                else
                {
                    isShopOpen = true;
                    gameManager.ShopUI.gameObject.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                } 
            }
        }
    }

    public void CloseShopUI()
    {
        isShopOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        gameManager = FindObjectOfType<GameManager>();
        gameManager.ShopUI.SetActive(false);
    }
}
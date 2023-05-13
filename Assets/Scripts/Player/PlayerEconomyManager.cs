using UnityEngine;
using Mirror;

public class PlayerEconomyManager : NetworkBehaviour
{
    GameManager gameManager;
    [SyncVar] 
    bool CanBeOpen;
    [HideInInspector]public bool IsShopOpen;

    public SettingsMenu SettingsMenu;
    private PlayerInput input;

    private void Awake()
    {
        input = new PlayerInput();
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        SettingsMenu = FindObjectOfType<SettingsMenu>();
        SettingsMenu.playerEconomyManager = this;
    }

    private void OnEnable()
    {
        input.PlayerUI.Enable();
    }

    private void OnDisable()
    {
        input.PlayerUI.Disable();
    }

    [Server]
    void ServerUpdate()
    {
        CanBeOpen = gameManager.GameState is GameState.PreRound or GameState.StartGame;
    }

    private void Update()
    {
        if (isServer) ServerUpdate();
        if (!isLocalPlayer) return;
        if (CanBeOpen && !SettingsMenu.IsOpened)
        {
            if (!input.PlayerUI.Shop.triggered) return;
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
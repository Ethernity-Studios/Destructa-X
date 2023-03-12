using System;
using Mirror;
using player;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShop : NetworkBehaviour
{
    [Header("Player")] 
    public Player Owner;
    public Image AgentIcon;
    public TMP_Text Money;
    public TMP_Text Name;

    [Header("Inventory")]
    public GameObject Inventory;
    public Image ShieldIcon;
    public Image PrimaryGunIcon;
    public Image SecondaryGunIcon;
    
    [Header("Request")]
    public GameObject Request;
    public Image RequestedGunIcon;
    public TMP_Text RequestedGunPrice;

    private GameManager gameManager;
    private ShopManager shopManager;
    private Player localPlayer;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        shopManager = FindObjectOfType<ShopManager>();
        CmdGetLocalPlayer();
    }

    private void OnEnable()
    {
        gameManager = FindObjectOfType<GameManager>();
        shopManager = FindObjectOfType<ShopManager>();
    }

    [Command(requiresAuthority = false)]
    void CmdGetLocalPlayer()
    {
        foreach (int playerID in gameManager.PlayersID)
        {
            RpcGetLocalPlayer(gameManager.GetPlayer(playerID).netIdentity); 
        }
    }
    [ClientRpc]
    void RpcGetLocalPlayer(NetworkIdentity player)
    {
        if (!player.isLocalPlayer) return;
        localPlayer = player.GetComponent<Player>();
    }
    
    public void BuyRequestedGun()
    {
        Debug.Log("BuyRequested Gun hell yeah");

        CmdGetLocalPlayer();
        PlayerInventoryManager playerInventory = Owner.GetComponent<PlayerInventoryManager>();
        Gun requestedGun = Owner.GetComponent<PlayerInventoryManager>().RequestedGun;
        
        if (localPlayer.PlayerMoney >= requestedGun.Price)
        {
            Debug.Log("buying requested gun with money");
            if (playerInventory.PrimaryGun == null && requestedGun.Type == GunType.Primary)
            {
                localPlayer.CmdChangeMoney(-requestedGun.Price);
                playerInventory.CmdGiveGun(requestedGun.GunID);
            }
            else if (requestedGun.Type == GunType.Secondary)
            {
                localPlayer.CmdChangeMoney(-requestedGun.Price);
                playerInventory.CmdGiveGun(requestedGun.GunID);
                if (playerInventory.PrimaryGun == null) playerInventory.CmdSwitchItem(Item.Secondary);
            }
        }
        else if (playerInventory.PrimaryGun != null && requestedGun.Type == GunType.Primary)
        {
            Debug.Log("trying to buy requested primary gun with gun and money!");
            if (localPlayer.PlayerMoney + playerInventory.PrimaryGun.Price < requestedGun.Price) return;
            shopManager.SellGun(playerInventory.PrimaryGun);
            if (playerInventory.PrimaryGun != null || requestedGun.Type != GunType.Primary) return;
            localPlayer.CmdChangeMoney(-requestedGun.Price);
            playerInventory.CmdGiveGun(requestedGun.GunID);
        }
        else if (playerInventory.SecondaryGun != null && requestedGun.Type == GunType.Secondary)
        {
            Debug.Log("trying to buy requested secondary gun with gun and money!");
            if (localPlayer.PlayerMoney + playerInventory.SecondaryGun.Price < requestedGun.Price) return;
            shopManager.SellGun(playerInventory.SecondaryGun);
            if (playerInventory.SecondaryGun != null || requestedGun.Type != GunType.Secondary) return;
            localPlayer.CmdChangeMoney(-requestedGun.Price);
            playerInventory.CmdGiveGun(requestedGun.GunID);
        }
        
        //Owner.GetComponent<PlayerInventoryManager>().CmdGiveGun(playerInventory.RequestedGun.GunID);
        //playerInventory.GetComponent<PlayerUI>().CmdUpdateShopPlayer();
        CmdSetGunOwner(localPlayer.netIdentity.netId);
    }

    [Command(requiresAuthority = false)]
    void CmdSetGunOwner(uint playerId)
    {
        Player player = NetworkServer.spawned[playerId].gameObject.GetComponent<Player>();
        RpcSetGunOwner(player);
    }    
    
    [ClientRpc]
    void RpcSetGunOwner(Player player)
    {
        Debug.Log("RpcsetGunOwner bruh");

        PlayerInventoryManager playerInventory = Owner.GetComponent<PlayerInventoryManager>();
        Gun requestedGun = Owner.GetComponent<PlayerInventoryManager>().RequestedGun;
        Debug.Log("requested gun type: " + requestedGun.Type);

        switch (requestedGun.Type)
        {
            case GunType.Primary:
                playerInventory.PrimaryGunInstance.GetComponent<GunInstance>().GunOwner = player;
                break;
            case GunType.Secondary:
                playerInventory.SecondaryGunInstance.GetComponent<GunInstance>().GunOwner = player;
                break;
        }

        playerInventory.RequestedGun = null;

    }
}

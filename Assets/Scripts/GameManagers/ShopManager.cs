using Mirror;
using TMPro;
using UnityEngine;

public enum ShieldType
{
    None, Light, Heavy
}
public class ShopManager : NetworkBehaviour
{
    [Header("Guns")]
    [SerializeField] GameObject gunInfo;

    [SerializeField] TMP_Text gunName;
    [SerializeField] TMP_Text gunCategory;
    [SerializeField] TMP_Text gunType;

    [SerializeField] TMP_Text gunFireRate;
    [SerializeField] TMP_Text gunEquipSpeed;
    [SerializeField] TMP_Text gunReloadSpeed;
    [SerializeField] TMP_Text gunMagazine;

    //[SerializeField] TMP_Text gunHeadDamageShort, gunHeadDamageLong;
    //[SerializeField] TMP_Text gunBodyDamageShort, gunBodyDamageLong;
    //[SerializeField] TMP_Text gunLegsDamageShort, gunLegsDamageLong;

    [SerializeField] Gun[] guns;

    [Header("Shields")]
    [SerializeField] GameObject shieldInfo;

    [SerializeField] TMP_Text shieldName;
    [SerializeField] TMP_Text shieldDescription;
    [SerializeField] Sprite shieldImage;

    public PlayerInventoryManager playerInventory;

    [SerializeField] GameManager gameManager;

    private void Start()
    {
        Invoke(nameof(CmdGetLocalPlayer), 1f);
        //CmdGetLocalPlayer();
    }

    [Command(requiresAuthority = false)]
    void CmdGetLocalPlayer()
    {
        foreach (int playerID in gameManager.PlayersID)
        {
            RpcGetLocalPlayer(gameManager.getPlayer(playerID).netIdentity);
        }
    }
    [ClientRpc]
    void RpcGetLocalPlayer(NetworkIdentity player)
    {
        if (player.isLocalPlayer) playerInventory = player.GetComponent<PlayerInventoryManager>();
    }

    public void ShowGunInfo(Gun gun)
    {
        shieldInfo.SetActive(false);
        gunInfo.SetActive(true);
        gunName.text = gun.name;
        gunCategory.text = ConvertGunCategoryToString(gun.Category);
        gunType.text = ConvertGunTypeToString(gun.Type);

        gunFireRate.text = (gun.Stats.FireRate).ToString();
        gunEquipSpeed.text = gun.EquipTime.ToString();
        gunReloadSpeed.text = gun.ReloadTime.ToString();
        gunMagazine.text = gun.Ammo.ToString();

        //gunHeadDamageShort.text = gun.Stats.HeadDamageShort.ToString();
        //gunHeadDamageLong.text = gun.Stats.HeadDamageLong.ToString();
        //gunBodyDamageShort.text = gun.Stats.BodyDamageShort.ToString();
        //gunBodyDamageLong.text = gun.Stats.BodyDamageLong.ToString();
        //gunLegsDamageShort.text = gun.Stats.LegsDamageShort.ToString();
        //gunLegsDamageLong.text = gun.Stats.LegsDamageLong.ToString();
    }

    public void ShowShieldInfo(string shieldType)
    {
        gunInfo.SetActive(false);
        shieldInfo.SetActive(true);
        switch (shieldType)
        {
            case "light":

                break;
            case "heavy":

                break;
        }
    }

    public string ConvertGunCategoryToString(GunCategory cat)
    {
        return cat switch
        {
            GunCategory.MachineGun => "Machine gun",
            GunCategory.Rifle => "Rifle",
            GunCategory.Shotgun => "Shotgun",
            GunCategory.Sidearm => "Sidearm",
            GunCategory.SMG => "SMG",
            GunCategory.SniperRifle => "Sniper rifle",
            _ => ""
        };
    }

    public string ConvertGunTypeToString(GunType type)
    {
        return type switch
        {
            GunType.Primary => "Primary Fire",
            GunType.Secondary => "Secondary Fire",
            _ => ""
        };
    }

    public void BuyGun(Gun gun)
    {
        // FIXME sus?
        if (gun.GunModel == null) return;
        Player localPlayer = playerInventory.GetComponent<Player>();
        if (localPlayer.PlayerMoney >= gun.Price)
        {
            Debug.Log("buying gun with money");
            if (playerInventory.PrimaryGun == null && gun.Type == GunType.Primary)
            {
                localPlayer.CmdChangeMoney(-gun.Price);
                playerInventory.CmdGiveGun(gun.GunID);
            }
            else if (gun.Type == GunType.Secondary)
            {
                localPlayer.CmdChangeMoney(-gun.Price);
                playerInventory.CmdGiveGun(gun.GunID);
                if (playerInventory.PrimaryGun == null) playerInventory.CmdSwitchItem(Item.Secondary);
            }
        }
        else if (playerInventory.PrimaryGun != null && gun.Type == GunType.Primary)
        {
            Debug.Log("trying tzo buy primary gun with gun and money!");
            if (localPlayer.PlayerMoney + playerInventory.PrimaryGun.Price < gun.Price) return;
            Debug.Log(playerInventory.PrimaryGun.Price + "Gun price");
            Debug.Log("buying primary gun with money and primary gun");
            SellGun(playerInventory.PrimaryGun);
            if (playerInventory.PrimaryGun != null || gun.Type != GunType.Primary) return;
            localPlayer.CmdChangeMoney(-gun.Price);
            playerInventory.CmdGiveGun(gun.GunID);
        }
        else if (playerInventory.SecondaryGun != null && gun.Type == GunType.Secondary)
        {
            Debug.Log("trying tzo buy secondary gun with gun and money!");
            if (localPlayer.PlayerMoney + playerInventory.SecondaryGun.Price < gun.Price) return;
            Debug.Log(playerInventory.SecondaryGun.Price + "Gun price");
            Debug.Log("buying secondary gun with money and primary gun");
            SellGun(playerInventory.SecondaryGun);
            if (playerInventory.SecondaryGun != null || gun.Type != GunType.Secondary) return;
            localPlayer.CmdChangeMoney(-gun.Price);
            playerInventory.CmdGiveGun(gun.GunID);
        }
    }

    public void SellGun(Gun gun)
    {
        Player localPlayer = playerInventory.GetComponent<Player>();
        switch (gun.Type)
        {
            case GunType.Primary when playerInventory.PrimaryGun == gun && playerInventory.PrimaryGunInstance.GetComponent<GunInstance>().CanBeSold:
                Debug.Log("selling primary gun");
                localPlayer.CmdChangeMoney(gun.Price);
                playerInventory.CmdSellGun(playerInventory.PrimaryGunInstance.GetComponent<NetworkIdentity>().netId, gun);
                break;
            case GunType.Secondary when playerInventory.SecondaryGun == gun && playerInventory.SecondaryGunInstance.GetComponent<GunInstance>().CanBeSold:
                Debug.Log("selling secondary gun");
                localPlayer.CmdChangeMoney(gun.Price);
                playerInventory.CmdSellGun(playerInventory.SecondaryGunInstance.GetComponent<NetworkIdentity>().netId, gun);
                break;
        }
    }

    public void BuyShield(string shieldType)
    {
        Player localPlayer = playerInventory.GetComponent<Player>();
        switch (shieldType)
        {
            case "light":
            {
                if (localPlayer.Shield != 25 && localPlayer.PreviousRoundShield > 25 && localPlayer.ShieldType != ShieldType.Light)
                {
                    if (localPlayer.ShieldType == ShieldType.Heavy) SellShield("heavy");
                    if (localPlayer.PlayerMoney > 400)
                    {
                        Debug.Log("Buying light");
                        localPlayer.CmdSetShield(25);
                        localPlayer.CmdSetShieldType(ShieldType.Light);
                    }
                }

                break;
            }
            case "heavy":
            {
                if (localPlayer.Shield != 50 && localPlayer.PreviousRoundShield != 50 && localPlayer.ShieldType != ShieldType.Heavy)
                {
                    if (localPlayer.ShieldType == ShieldType.Light) SellShield("light");
                    if (localPlayer.PlayerMoney > 1000)
                    {
                        Debug.Log("Buying heavy");
                        localPlayer.CmdSetShield(50);
                        localPlayer.CmdSetShieldType(ShieldType.Heavy);
                    }
                }

                break;
            }
        }
    }

    public void SellShield(string shieldType)
    {
        Player localPlayer = playerInventory.GetComponent<Player>();
        localPlayer.CmdSetShield(localPlayer.PreviousRoundShield);
        localPlayer.CmdSetShieldType(ShieldType.None);
        Debug.Log("Selling shield: " + shieldType);
        switch (shieldType)
        {
            case "light":
                localPlayer.CmdChangeMoney(400);
                break;
            case "heavy":
                localPlayer.CmdChangeMoney(1000);
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.V)) AddMoney(100);
    }

    public void AddMoney(int money)
    {
        Player localPlayer = playerInventory.GetComponent<Player>();
        localPlayer.CmdChangeMoney(money);
    }
}

using TMPro;
using UnityEngine;
using Mirror;
public class ShopManager : MonoBehaviour
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

    [SerializeField] TMP_Text gunHeadDamageShort, gunHeadDamageLong;
    [SerializeField] TMP_Text gunBodyDamageShort, gunBodyDamageLong;
    [SerializeField] TMP_Text gunLegsDamageShort, gunLegsDamageLong;

    [SerializeField] Gun[] guns;

    [Header("Shields")]
    [SerializeField] GameObject shieldInfo;

    [SerializeField] TMP_Text shieldName;
    [SerializeField] TMP_Text shieldDescription;
    [SerializeField] Sprite shieldImage;

    PlayerInventoryManager playerInventory;

    [SerializeField] GameManager gameManager;

    public TMP_Text PlayerMoneyText;

    private void Start()
    {
        Invoke("getLocalPlayer", .3f);
    }

    void getLocalPlayer()
    {
        foreach (var player in gameManager.Players)
        {
            if (!player.isLocalPlayer) continue;
            playerInventory = player.GetComponent<PlayerInventoryManager>();
            break;
        }
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
        gunMagazine.text = gun.MaxAmmo.ToString();

        gunHeadDamageShort.text = gun.Stats.HeadDamageShort.ToString();
        gunHeadDamageLong.text = gun.Stats.HeadDamageLong.ToString();
        gunBodyDamageShort.text = gun.Stats.BodyDamageShort.ToString();
        gunBodyDamageLong.text = gun.Stats.BodyDamageLong.ToString();
        gunLegsDamageShort.text = gun.Stats.LegsDamageShort.ToString();
        gunLegsDamageLong.text = gun.Stats.LegsDamageLong.ToString();
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
        switch (cat)
        {
            case GunCategory.MachineGun:
                return "Machine gun";
            case GunCategory.Rifle:
                return "Rifle";
            case GunCategory.Shotgun:
                return "Shotgun";
            case GunCategory.Sidearm:
                return "Sidearm";
            case GunCategory.SMG:
                return "SMG";
            case GunCategory.SniperRifle:
                return "Sniper rifle";
        }
        return "";
    }

    public string ConvertGunTypeToString(GunType type)
    {
        if (type == GunType.Primary) return "Primary Fire";
        else if (type == GunType.Secondary) return "Secundary Fire";
        return "";
    }

    public void BuyGun(Gun gun)
    {
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
            return;
        }
        else if(playerInventory.PrimaryGun != null && gun.Type == GunType.Primary)
        {
            Debug.Log("trying tzo buy primary gun with gun and money!");
            if (localPlayer.PlayerMoney + playerInventory.PrimaryGun.Price < gun.Price) return;
            Debug.Log(playerInventory.PrimaryGun.Price + "Gun price");
            Debug.Log("buying primary gun with moeny and primary gun");
            SellGun(playerInventory.PrimaryGun);
            if (playerInventory.PrimaryGun == null && gun.Type == GunType.Primary)
            {
                localPlayer.CmdChangeMoney(-gun.Price);
                playerInventory.CmdGiveGun(gun.GunID);
            }
            return;
        }
        else if(playerInventory.SecondaryGun != null &&gun.Type == GunType.Secondary)
        {
            Debug.Log("trying tzo buy secondary gun with gun and money!");
            if (localPlayer.PlayerMoney + playerInventory.SecondaryGun.Price < gun.Price) return;
            Debug.Log(playerInventory.SecondaryGun.Price + "Gun price");
            Debug.Log("buying secondary gun with moeny and primary gun");
            SellGun(playerInventory.SecondaryGun);
            if (playerInventory.SecondaryGun == null && gun.Type == GunType.Secondary)
            {
                localPlayer.CmdChangeMoney(-gun.Price);
                playerInventory.CmdGiveGun(gun.GunID);
            }
            return;
        }
    }

    public void SellGun(Gun gun)
    {
        Player localPlayer = playerInventory.GetComponent<Player>();
        if (gun.Type == GunType.Primary && playerInventory.PrimaryGun == gun && playerInventory.PrimaryGunInstance.GetComponent<GunInstance>().CanBeSelled)
        {
            Debug.Log("selling primary gun");
            localPlayer.CmdChangeMoney(gun.Price);
            playerInventory.CmdSellGun(playerInventory.PrimaryGunInstance.GetComponent<NetworkIdentity>().netId, gun);
        }
        else if (gun.Type == GunType.Secondary && playerInventory.SecondaryGun == gun && playerInventory.SecondaryGunInstance.GetComponent<GunInstance>().CanBeSelled)
        {
            Debug.Log("selling secodnary gun");
            localPlayer.CmdChangeMoney(gun.Price);
            playerInventory.CmdSellGun(playerInventory.SecondaryGunInstance.GetComponent<NetworkIdentity>().netId, gun);
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

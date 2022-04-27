using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
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

    PlayerInventoryManager PlayerInventory;

    [SerializeField] GameManager gameManager;
    private void Start() 
    {
        Invoke("getLocalPlayer", .3f);
    } 

    void getLocalPlayer()
    {
        foreach (var player in gameManager.Players)
        {
            if (!player.isLocalPlayer) continue;
            PlayerInventory = player.GetComponent<PlayerInventoryManager>();
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
        Player localPlayer = PlayerInventory.GetComponent<Player>();
        if (localPlayer.PlayerGhostMoney < gun.Price) return;
        if (PlayerInventory.PrimaryGun == null || PlayerInventory.SecondaryGun == null) return;
        localPlayer.CmdAddMoney(-gun.Price);
        PlayerInventory.CmdGiveGun(gun.GunID);
    }

    public void SellGun(Gun gun)
    {
        Player localPlayer = PlayerInventory.GetComponent<Player>();
        if(gun.Type == GunType.Primary && PlayerInventory.PrimaryGun != null)
        {
            localPlayer.CmdAddMoney(gun.Price);
            PlayerInventory.DestroyGun(PlayerInventory.PrimaryWeaponHolder.transform.GetChild(0).gameObject);
        }
        else if(gun.Type == GunType.Secondary && PlayerInventory.SecondaryGun != null)
        {
            localPlayer.CmdAddMoney(gun.Price);
        }
    }
}

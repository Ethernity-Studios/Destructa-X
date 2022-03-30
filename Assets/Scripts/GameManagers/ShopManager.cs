using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
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

    [SerializeField] GameObject gunInfo;
    [SerializeField] GameObject shieldInfo;

    public void ShowGunInfo(Gun gun)
    {
        shieldInfo.SetActive(false);
        gunInfo.SetActive(true);
        gunName.text = gun.name;
        gunCategory.text = ConvertGunCategoryToString(gun.Category);
        gunType.text = ConvertGunTypeToString(gun.Type);

        gunFireRate.text = (gun.LMB.FireDelay * 4).ToString();
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

    public void ShowShieldInfo()
    {
        gunInfo.SetActive(false);
        shieldInfo.SetActive(true);
    }

    private void Start()
    {
        ShowGunInfo(guns[0]);
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
}

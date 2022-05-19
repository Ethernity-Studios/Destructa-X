using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Gun : ScriptableObject
{
    public int GunID;
    public string Name;
    public int Price;
    public int Ammo;
    public int MaxAmmo;
    public float ReloadTime;
    public float EquipTime;
    public GunType Type;
    public GunCategory Category;
    public float BulletPenetration;

    public GameObject GunModel;

    public LMB LMB;
    public RMB RMB;

    public GunStats Stats;

    public GunTransform GunTransform;
}
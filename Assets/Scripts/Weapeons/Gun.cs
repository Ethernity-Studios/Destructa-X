using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Gun : ScriptableObject
{
    public string Name;
    public int Ammo;
    public int MaxAmmo;
    public float ReloadTime;
    public float EquipTime;
    public GunType Type;
    public GunCategory Category;

    public GameObject GunModel;
    public Image GunIcon;

    public LMB LMB;
    public RMB RMB;

    public GunStats Stats;
}
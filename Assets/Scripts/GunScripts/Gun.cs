using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


[CreateAssetMenu]
public class Gun : ScriptableObject
{
    public int GunID;
    public string Name;
    public GameObject GunModel;
    public Sprite Icon;
    public int Price;
    public int MagazineAmmo;
    public int Ammo;
    public float ReloadTime;
    public float EquipTime;
    public GunType Type;
    public GunCategory Category;
    public float BulletPenetration;

    public PrimaryFire PrimaryFire;
    public bool HasSecondaryFire;
    public SecondaryFire SecondaryFire;

    
    //public float Bloom;
    //public float Recoil;

    public List<Damages> Damages;


    public GunStats Stats;

    //public Zoom Scope;

    public GunRecoil GunRecoil;
    public GunTransform GunTransform;
}

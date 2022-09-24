using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[CreateAssetMenu]
public class Gun : ScriptableObject
{
    public int GunID;
    public string Name;
    public int Price;
    public int MagazineAmmo;
    public int Ammo;
    public float ReloadTime;
    public float EquipTime;
    public GunType Type;
    public GunCategory Category;
    public float BulletPenetration;

    public PrimaryFire PrimaryFire;
    public SecondaryFire SecondaryFire;


    public float Bloom;
    public float Recoil;

    public List<Damages> Damages;

    public GameObject GunModel;

    public GunStats Stats;

    public Zoom Scope;

    public GunTransform GunTransform;
}
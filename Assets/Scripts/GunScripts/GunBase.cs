using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PrimaryFire
{
    public FireMode FireMode;
    public FireType FireType;

    public float FireDelay;
    public float BurstDelay;

    public float BulletsPerFire;
}

[Serializable]
public class SecondaryFire
{
    public FireMode FireMode;
    public FireType FireType;
    
    public ZoomType ZoomType;
    public Zoom Zoom; 

    public float FireDelay;
    public float BurstDelay;

    public float BulletsPerFire;

}

/// <summary>
/// GunSettings
/// </summary>

public enum FireType
{
    Single,
    Burst,
    Multiple
}

public enum FireMode
{
    Automatic,
    Manual
}

public enum ZoomType
{
    None, Semi, Full
}

[Serializable]
public class Zoom
{
    public int Amount;
    public int AmountMultiplier;
}

[Serializable]
public struct Damages
{
    public int MinDistance;
    public int MaxDistance;
    [Space]
    public int HeadDamage;
    public int BodyDamage;
    public int LegsDamage;
}

/// <summary>
/// other gun settings
/// </summary>

[Serializable]
public class GunTransform
{
    public Vector3 FirstPersonGunPosition;
    public Vector3 FirstPersonGunRotation;

    public Vector3 ThirdPersonGunPosition;
    public Vector3 ThirdPersonGunRotation;
}

public enum GunType
{
    Primary,
    Secondary
}

public enum GunCategory
{
    MachineGun, Rifle, Shotgun, Sidearm, SMG, SniperRifle
}

[Serializable]
public class GunStats
{
    public float FireRate;
}
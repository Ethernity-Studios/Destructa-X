using System;
using UnityEngine;

[Serializable]
public class PrimaryFire
{
    public FireMode FireMode;
    public FireType FireType;

    public float FireDelay;
    public float BurstDelay;

    public int BulletsPerFire;
    public int RemoveBulletsPerFire;
    public float Bloom;
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

    public int BulletsPerFire;
    public int RemoveBulletsPerFire;
    public float Bloom;

}

/// <summary>
/// GunSettings
/// </summary>

public enum FireType
{
    None,
    Single,
    Burst,
    Multiple
}

public enum FireMode
{
    None,
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
    public int FirstZoomFOV;
    public int SecondZoomFOV;

    public Sprite ScopeImg;
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
    [Space(10)]
    public Vector3 ThirdPersonGunPosition;
    public Vector3 ThirdPersonGunRotation;
    [Space(10)]
    public Vector3 RightHandTargetPosition;
    public Vector3 RightHandTargetRotation;
    public Vector3 RightHandHintPosition;
    public Vector3 RightHandHintRotation;
    [Space(10)]
    public Vector3 LeftHandTargetPosition;
    public Vector3 LeftHandTargetRotation;
    public Vector3 LeftHandHintPosition;
    public Vector3 LeftHandHintRotation;
}

[Serializable]
public class GunRecoil
{
    public float RecoilX;
    public float RecoilY;
    public float RecoilZ;
    [Space(10)]
    public float AimRecoilX;
    public float AimRecoilY;
    public float AimRecoilZ;
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
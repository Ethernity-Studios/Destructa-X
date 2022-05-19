using System;
using UnityEngine;

public enum FireType
{
    Burst,
    Shotgun,
    Default
}

public enum FireMode
{
    Automatic,
    SemiAutomatic,
    LimitedAutomatic
}

public enum GunMode
{
    Scope,
    Fire
}

public enum ScopeType
{
    Sniper,
    RedDot
}

[Serializable]
public class LMB
{
    [Header("Type")] public FireType FireType;
    public FireMode FireMode;
    public float FireDelay;

    public int AmmoLoss;

    [Header("Damages")] public float LegDamage;
    public float BodyDamage;
    public float HeadDamage;
    public float DamageDrop;

    [Header("AimDiff")] public float Recoil;
    public float Bloom;
}

public enum RMBType
{
    None,
    Scope,
    ScopeFire,
    Fire
}

[Serializable]
public class RMB
{
    public RMBType Type;
    public Scope Scope;
    public LMB Fire;
}

[Serializable]
public class Scope
{
    public float Magnification;
    public ScopeType ScopeType;

    public float MoveSpeedModifier;
    public float BloomModifier;
    public float RecoilModifier;
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

    public int HeadDamageShort, HeadDamageLong;
    public int BodyDamageShort, BodyDamageLong;
    public int LegsDamageShort, LegsDamageLong;
}

[Serializable]
public class GunTransform
{
    public Vector3 FirstPersonGunPosition;
    public Vector3 FirstPersonGunRotation;
}
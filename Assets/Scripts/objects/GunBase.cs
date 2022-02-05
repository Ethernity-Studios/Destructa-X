using System;
using UnityEngine;

public enum FireType
{
    Burst = 3,
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
public class Fire
{
    [Header("Type")] public FireType FireType;
    public FireMode FireMode;
    public float FireDelay;

    public int AmmoLoss;

    [Header("Damges")] public float LegDamage;
    public float BodyDamage;
    public float HeadDamage;
    public float DamageDrop;

    [Header("AimDiff")] public float Recoil;
    public float Bloom;
}

public enum RMBType
{
    Scope,
    ScopeFire,
    Fire
}

[Serializable]
public class RMB
{
    public RMBType Type;
    public Scope Scope;
    public Fire Fire;
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
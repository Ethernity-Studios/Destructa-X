using System;
using UnityEngine;

namespace objects
{
    public enum DefaultAbility
    {
        Q,
        E,
        C
    }

    public enum AbilityRefresh
    {
        None,
        Time,
        Kills
    }

    [Serializable]
    public class AgentAblities
    {
        public DefaultAbility DefaultAbility;
        public QAbility QAbility;
        public EAbility EAbility;
        public CAbility CAbility;
        public XAbility XAbility;
    }

    [Serializable]
    public class QAbility
    {
        public int Cose;
        public int Amount;
        public AbilityRefresh Refresh;
        public int RefreshValue;
        public GameObject Object;
    }

    [Serializable]
    public class EAbility
    {
        public int Cose;
        public int Amount;
        public AbilityRefresh Refresh;
        public int RefreshValue;
        public GameObject Object;
    }

    [Serializable]
    public class CAbility
    {
        public int Cose;
        public int Amount;
        public AbilityRefresh Refresh;
        public int RefreshValue;
        public GameObject Object;
    }

    [Serializable]
    public class XAbility
    {
        public int Points;
        public GameObject Object;
    }
}
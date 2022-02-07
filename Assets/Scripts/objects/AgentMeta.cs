using System;
using UnityEngine;

namespace objects
{
    public enum AgentClass
    {
        Sentine,
        Duelist
    }

    [Serializable]
    public class AgentMeta
    {
        public GameObject Object;
        public Sprite Icon;
        public AgentClass AClass;
    }
}
using System;
using UnityEngine;

namespace objects
{
    public enum AgentClass
    {
        Sentinel,
        Duelist,
        Initiator,
        Controller
    }

    [Serializable]
    public class AgentMeta
    {
        public GameObject Object;
        public Sprite Icon;
        public AgentClass AClass;
    }
}
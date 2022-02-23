using System;
using UnityEngine;

namespace objects
{
    public enum AgentClass
    {
        Sentinel,
        Duelis,
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
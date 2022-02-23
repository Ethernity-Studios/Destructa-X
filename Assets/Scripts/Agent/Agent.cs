using UnityEngine;

namespace objects
{
    [CreateAssetMenu]
    public class Agent : ScriptableObject
    {
        public string Name;

        public AgentMeta Meta;

        public AgentAblities Ablities;

    }
}
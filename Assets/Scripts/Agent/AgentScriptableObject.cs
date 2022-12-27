using UnityEngine;

namespace objects
{
    [CreateAssetMenu]
    public class AgentScriptableObject : ScriptableObject
    {
        public string Name;

        public AgentMeta Meta;

        public AgentAbilities Abilities;
    }
}
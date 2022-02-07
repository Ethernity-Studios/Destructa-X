using UnityEngine;

namespace objects
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class Agent : ScriptableObject
    {
        public string Name;

        public AgentMeta Meta;

        public AgentAblities Ablities;
    }
}
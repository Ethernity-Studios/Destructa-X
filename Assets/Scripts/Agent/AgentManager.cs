using objects;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Agent
{
    None, Astra, Breach, Brimstone, Chamber, Cypher, Jett, Kayo, Killjoy, Neon, Omen,
    Phoenix, Raze, Reyna, Sage, Skye, Sova, Viper, Yoru
}

public class AgentManager : MonoBehaviour
{
    Dictionary<string, Agent> agents = new();

    public List<AgentScriptableObject> agentScriptableObjects = new();
    Dictionary<Agent, AgentScriptableObject> agentsMeta = new();

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
        int _temp = 0;
        foreach (Agent item in (Agent[])Enum.GetValues(typeof(Agent)))
        {
            agents.Add(item.ToString(), item);

            agentsMeta.Add(item, agentScriptableObjects[_temp]);

            _temp++;
        }
    }

    public Agent GetAgentByName(string agentName)
    {
        return agents.GetValueOrDefault(agentName);
    }

    public AgentScriptableObject GetAgentMeta(Agent agentName)
    {
        return agentsMeta.GetValueOrDefault(agentName);
    }
}

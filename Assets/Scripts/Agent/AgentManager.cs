using UnityEngine;
using System.Collections.Generic;
using System;

public enum Agent {None, Astra, Breach, Brimstone, Chamber, Cypher, Jett, Kayo, Killjoy, Neon, Omen,
Phoenix, Raze, Reyna, Sage, Skye, Sova, Viper, Yoru}

public class AgentManager : MonoBehaviour
{
    Dictionary<string, Agent> agents = new();
    public Agent agent;
    public void Start()
    {
        DontDestroyOnLoad(gameObject);

        foreach (Agent item in (Agent[])Enum.GetValues(typeof(Agent)))
        {
            agents.Add(item.ToString(),item);
        }
    }

    public Agent PickAgent(string agentName)
    {
        return agents.GetValueOrDefault(agentName);
    }
}

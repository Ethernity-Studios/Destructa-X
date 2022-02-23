using UnityEngine;
using System.Collections.Generic;
using objects;
using System;

public enum Agent {None, Astra, Breach, Brimstone, Chamber, Cypher, Jett, Kayo, Killjoy, Neon, Omen,
Phoenix, Raze, Reyna, Sage, Skye, Sova, Viper, Yoru}

public class AgentManager : MonoBehaviour
{
    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    Dictionary<string, Agent> Agents = new Dictionary<string, Agent>();

    public Agent PickAgent(string agent)
    {
        return Agent.Astra;
    }
}

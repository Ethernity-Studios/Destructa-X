using System;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public enum Team
{
    None, Blue, Red
}

// FIXME just fucking fix me :)
public class RoomManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(CmdUpdateTeamSizeUI))]
    public int BlueTeamSize;
    [SyncVar(hook = nameof(CmdUpdateTeamSizeUI))]
    public int RedTeamSize;
    [SyncVar]
    public string SelectedMap;
    
    SyncDictionary<int, Agent> agentMapping = new();
    SyncList<int> bluePlayers = new();
    SyncList<int> redPlayers = new();

    [SerializeField] GameObject TeamSelectUI;
    [SerializeField] GameObject AgentSelectUI;

    // UI
    [SerializeField] Transform BlueTeamHolder;
    [SerializeField] Transform RedTeamHolder;
    [SerializeField] TMP_Text BlueTeamSizeText;
    [SerializeField] TMP_Text RedTeamSizeText;
    [SerializeField] GameObject PreselectedAgentGO;
    [SerializeField] Image PreselectedAgentImg;
    [SerializeField] GameObject LoadingUI;
    [SerializeField] GameObject AgentsUI;
    public Button[] agentButtons;

    
    AgentManager agentManager;

    [SerializeField] GameObject mapSelect;
    [SerializeField] TMP_Dropdown mapDropdown;
    [SerializeField] TMP_Text mapText;

    [SerializeField] Image backgroundImage;

    public Map[] map;
    public Dictionary<string, Map> maps = new();

    public static Agent PAgent;
    public static Team PTeam;

    NetworkManagerRoom room;
    NetworkManagerRoom Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerRoom;
        }
    }

    private void Start()
    {
        mapText.text = SelectedMap;
        if (isServer)
        {
            mapSelect.SetActive(true);
        }
        
        foreach (var item in map)
        {
            maps.Add(item.MapName, item);
            mapDropdown.options.Add(new TMP_Dropdown.OptionData(item.MapName));
        }
    }

    [ClientRpc]
    public void RpcCountdown(int time)
    {
        StartCoroutine(Countdown(time));
    }

    public IEnumerator Countdown(int time)
    {
        AgentsUI.SetActive(false);
        LoadingUI.SetActive(true);
        backgroundImage.color = Color.white;
        for (int i = time; i >= 0; i--)
        {
            yield return new WaitForSeconds(1);
        }
        Room.SelectedMap = maps[SelectedMap].SceneName;
        Room.StartGame(Room.SelectedMap);
    }

    private void Update()
    {
        mapText.text = SelectedMap;
        backgroundImage.sprite = GetMapMeta(SelectedMap).LobbyMapBackground;
    }

    #region MapManagement
    public Map GetMapMeta(string map)
    {
        return maps.GetValueOrDefault(map);
    }

    [Server]
    public void SelectMap()
    {
        SelectedMap = mapDropdown.options[mapDropdown.value].text;
        mapText.text = SelectedMap;
        RpcSelectMap(SelectedMap);
    }

    [ClientRpc]
    public void RpcSelectMap(string map)
    {
        mapText.text = map;
    }

    #endregion

    #region TeamManagement
    public void JoinTeam(int teamIndex)
    {
        foreach (var player in Room.roomSlots)
        {
            if (!player.isLocalPlayer) continue;
            var localPlayer = player.GetComponent<LobbyPlayer>();
            if (teamIndex == 1 && BlueTeamSize < 5)
            {
                localPlayer.CmdJoinTeam(Team.Blue);
                PTeam = Team.Blue;
                CmdUpdateTeamSize(1, 0);
                TeamSelectUI.SetActive(false);
                AgentSelectUI.SetActive(true);
            }
            else if (teamIndex == 2 && RedTeamSize < 5)
            {
                PTeam = Team.Red;
                localPlayer.CmdJoinTeam(Team.Red);
                CmdUpdateTeamSize(0, 1);
                TeamSelectUI.SetActive(false);
                AgentSelectUI.SetActive(true);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateTeamSize(int b, int r)
    {
        BlueTeamSize += b;
        RedTeamSize += r;
    }

    [Command(requiresAuthority = false)]
    void CmdUpdateTeamSizeUI(int _, int newValue)
    {
        RpcUpdateTeamSizeUI();
    }
    [ClientRpc]
    public void RpcUpdateTeamSizeUI()
    {
        BlueTeamSizeText.text = BlueTeamSize.ToString();
        RedTeamSizeText.text = RedTeamSize.ToString();
    }
    #endregion

    #region AgentManagement

    public void PreselectAgent(string agentName)
    {
        agentManager = FindObjectOfType<AgentManager>();
        foreach (var player in Room.roomSlots)
        {
            if (player.isLocalPlayer)
            {
                LobbyPlayer localPlayer = player.GetComponent<LobbyPlayer>();
                localPlayer.CmdPreselectAgent(agentManager.GetAgentByName(agentName));
                PreselectedAgentImg.sprite = agentManager.GetAgentMeta(agentManager.GetAgentByName(agentName)).Meta.Icon;
                PreselectedAgentImg.color = Color.white;
            }
        }
    }

    public void SelectAgent()
    {
        agentManager = FindObjectOfType<AgentManager>();
        foreach (var player in Room.roomSlots)
        {
            if (player.isLocalPlayer)
            {
                LobbyPlayer localPlayer = player.GetComponent<LobbyPlayer>();
                if (localPlayer.PlayerPreselectedAgent != Agent.None)
                {
                    PAgent = localPlayer.PlayerPreselectedAgent;
                    // null exception
                    localPlayer.CmdSelectAgent(localPlayer.PlayerPreselectedAgent);
                    localPlayer.transform.GetChild(2).GetComponent<TMP_Text>().text = agentManager.GetAgentMeta(localPlayer.PlayerSelectedAgent).Name;
                    foreach (var agentBtn in agentButtons)
                    {
                        agentBtn.interactable = false;
                    }
                }
            }
        }
    }

    [Command]
    public void CmdSelectTeam(Team team)
    {
        if (team == Team.None) return;

        var player = connectionToClient.identity.GetComponent<LobbyPlayer>();
        if (player.PlayerTeam != Team.None) return;

        switch (team)
        {
            case Team.Blue:
                if (bluePlayers.Count < 5)
                {
                    player.PlayerTeam = Team.Blue;
                    bluePlayers.Add(connectionToClient.connectionId);
                }
                break;
            case Team.Red:
                if (redPlayers.Count < 5)
                {
                    player.PlayerTeam = Team.Red;
                    redPlayers.Add(connectionToClient.connectionId);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(team), team, null);
        }
        // TODO update state ui
    }

    [Command]
    public void CmdPreSelectAgent(Agent agent)
    {
        var player = connectionToClient.identity.GetComponent<LobbyPlayer>();
        if (!canSelectAgent(agent, player.PlayerTeam)) return;

        player.PlayerPreselectedAgent = agent;
        // TODO update state ui
    }

    [Command]
    public void CmdSelectAgent()
    {
        var player = connectionToClient.identity.GetComponent<LobbyPlayer>();
        // TODO maybe set his agent to null to indicate its already picked
        if (player.PlayerPreselectedAgent == Agent.None || !canSelectAgent(player.PlayerPreselectedAgent, player.PlayerTeam)) return;

        player.PlayerSelectedAgent = player.PlayerPreselectedAgent;
        agentMapping[connectionToClient.connectionId] = player.PlayerSelectedAgent;
        AgentSelectedState();
        // TODO update state ui
    }

    bool canSelectAgent(Agent agent, Team team)
    {
        if (team == Team.None) return false;
        
        foreach (var player in room.roomSlots)
        {
            var lp = player.GetComponent<LobbyPlayer>();
            if (lp.PlayerTeam == team && lp.PlayerSelectedAgent == agent)
            {
                return false;
            }
        }

        return true;
    }

    [TargetRpc]
    public void AgentSelectedState()
    {
        var localPlayer = connectionToServer.identity.GetComponent<LobbyPlayer>();
        localPlayer.transform.GetChild(2).GetComponent<TMP_Text>().text = agentManager.GetAgentMeta(localPlayer.PlayerSelectedAgent).Name;
        foreach (var agentBtn in agentButtons)
        {
            agentBtn.interactable = false;
        }
    }

    #endregion

    [Server]
    public void connectionDisconnect(NetworkConnection connection)
    {
        // reclaim agent + team slot
        agentMapping.Remove(connection.connectionId);
        var team = connection.identity.GetComponent<LobbyPlayer>().PlayerTeam;
        switch (team)
        {
            case Team.None:
                break;
            case Team.Blue:
                bluePlayers.Remove(connection.connectionId);
                break;
            case Team.Red:
                redPlayers.Remove(connection.connectionId);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

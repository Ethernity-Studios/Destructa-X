using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum Team
{
    None, Blue, Red
}

// FIXME just fucking fix me :)
public class RoomManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncBlueTeamSizeUI))]
    public int BlueTeamSize;
    [SyncVar(hook = nameof(SyncRedTeamSizeUI))]
    public int RedTeamSize;
    [SyncVar]
    public string SelectedMap;
    
    public Dictionary<int, Agent> agentMapping = new();
    public Dictionary<int, string> playerNameMapping = new();
    public List<int> bluePlayers = new();
    public List<int> redPlayers = new();

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

    void OnRedTeamSize(int _, int newValue)
    {
        
    }
    
    void OnBlueTeamSize(int _, int newValue)
    {
        
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
        BlueTeamHolder = GameObject.Find("BlueTeam").transform;
        RedTeamHolder = GameObject.Find("RedTeam").transform;
        
        agentManager = FindObjectOfType<AgentManager>();
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

        SelectedMap = map[0].MapName;
    }
    
    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0) return;
        mapText.text = SelectedMap;
        backgroundImage.sprite = GetMapMeta(SelectedMap).LobbyMapBackground;
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
        var team = teamIndex == 1 ? Team.Blue : Team.Red;
        CmdSelectTeam(team, NicknameManager.DisplayName);
        return;
        /*
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
        */
    }

    [TargetRpc]
    void SwitchToChampSelectState(NetworkConnection conn)
    {
        TeamSelectUI.SetActive(false);
        AgentSelectUI.SetActive(true);
    }
    /*
    [Command(requiresAuthority = false)]
    public void CmdUpdateTeamSize(int b, int r)
    {
        BlueTeamSize += b;
        RedTeamSize += r;
    }
    */

    [Command(requiresAuthority = false)]
    void CmdUpdateTeamSizeUI(int _, int newValue)
    {
        RpcUpdateTeamSizeUI();
    }
    
    void SyncRedTeamSizeUI(int _, int newValue)
    {
        RedTeamSizeText.text = newValue.ToString();
    }
    
    void SyncBlueTeamSizeUI(int _, int newValue)
    {
        BlueTeamSizeText.text = newValue.ToString();
    }
    
    [ClientRpc]
    public void RpcUpdateTeamSizeUI()
    {
        BlueTeamSizeText.text = BlueTeamSize.ToString();
        RedTeamSizeText.text = RedTeamSize.ToString();
    }
    
    [ClientRpc]
    public void RpcUpdateTeamSizeUINew()
    {
        BlueTeamSizeText.text = bluePlayers.Count.ToString();
        RedTeamSizeText.text = redPlayers.Count.ToString();
    }
    
    #endregion

    #region AgentManagement

    public void PreselectAgent(string agentName)
    {
        CmdPreSelectAgent(agentManager.GetAgentByName(agentName));
        /*return;
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
        }*/
    }

    public void SelectAgent()
    {
        // agentManager = FindObjectOfType<AgentManager>();
        CmdSelectAgent();
        /*return;
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
        }*/
    }

    [Command(requiresAuthority = false)]
    void CmdSelectTeam(Team team, string playerName, NetworkConnectionToClient sender = null)
    {
        if (team == Team.None) return;
        
        var player = sender.identity.GetComponent<LobbyPlayer>();
        if (player.PlayerTeam != Team.None)
        {
            Debug.Log("client already selected team");
            return;
        }

        switch (team)
        {
            case Team.Blue:
                if (bluePlayers.Count < 5)
                {
                    player.transform.SetParent(BlueTeamHolder);
                    player.PlayerTeam = Team.Blue;
                    player.PlayerName = playerName;
                    bluePlayers.Add(sender.connectionId);
                    playerNameMapping[sender.connectionId] = playerName;
                    BlueTeamSize++;
                }
                break;
            case Team.Red:
                if (redPlayers.Count < 5)
                {
                    player.transform.SetParent(RedTeamHolder);
                    player.PlayerTeam = Team.Red;
                    player.PlayerName = playerName;
                    redPlayers.Add(sender.connectionId);
                    playerNameMapping[sender.connectionId] = playerName;
                    RedTeamSize++;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(team), team, null);
        }
        
        // RpcUpdateTeamSizeUINew();
        // player.RpcSetTeamUI(team);
        SwitchToChampSelectState(sender);
        // ServerSyncAllUI();
        // TODO update state ui
    }

    [Command(requiresAuthority = false)]
    public void CmdPreSelectAgent(Agent agent, NetworkConnectionToClient sender = null)
    {
        var player = sender.identity.GetComponent<LobbyPlayer>();
        if (!canSelectAgent(agent, player.PlayerTeam) || player.PlayerSelectedAgent != Agent.None) return;
        player.PlayerPreselectedAgent = agent;
        // ServerSyncAllUI();
        // player.preSelectChampUI();
        // TODO update state ui
    }

    [Command(requiresAuthority = false)]
    public void CmdSelectAgent(NetworkConnectionToClient sender = null)
    {
        var player = sender.identity.GetComponent<LobbyPlayer>();
        // TODO maybe set his agent to null to indicate its already picked
        if (player.PlayerPreselectedAgent == Agent.None || !canSelectAgent(player.PlayerPreselectedAgent, player.PlayerTeam) || player.PlayerSelectedAgent != Agent.None) return;
        player.PlayerSelectedAgent = player.PlayerPreselectedAgent;
        agentMapping[sender.connectionId] = player.PlayerSelectedAgent;
        player.CmdChangeReadyState(true);
        // player.readyToBegin = true;
        // room.ReadyStatusChanged();
        // ServerSyncAllUI();
        // AgentSelectedState();
        // player.selectChampUI();
        // TODO update state ui
    }

    [Server]
    void ServerSyncAllUI()
    {
        foreach (var con in Room.roomSlots)
        {
            var player = con.GetComponent<LobbyPlayer>();
            player.syncUI();
        }
    }

    bool canSelectAgent(Agent agent, Team team)
    {
        if (team == Team.None) return false;

        if (team == Team.Blue)
        {
            foreach (var p in bluePlayers)
            {
                if (!agentMapping.ContainsKey(p)) continue;
                if (agentMapping[p] == agent)
                {
                    return false;
                }
            }
        }
        else
        {
            foreach (var p in redPlayers)
            {
                if (!agentMapping.ContainsKey(p)) continue;
                if (agentMapping[p] == agent)
                {
                    return false;
                }
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
    public void PlayerDisconnect(NetworkConnection connection)
    {
        Debug.Log("nekdo se disconnectnul");
        Debug.Log($"blue {BlueTeamSize} red {RedTeamSize}");
        // reclaim agent + team slot
        agentMapping.Remove(connection.connectionId);
        playerNameMapping.Remove(connection.connectionId);
        var team = connection.identity.GetComponent<LobbyPlayer>().PlayerTeam;
        switch (team)
        {
            case Team.None:
                break;
            case Team.Blue:
                bluePlayers.Remove(connection.connectionId);
                BlueTeamSize--;
                break;
            case Team.Red:
                redPlayers.Remove(connection.connectionId);
                RedTeamSize--;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Debug.Log($"blue {BlueTeamSize} red {RedTeamSize}");
    }
}

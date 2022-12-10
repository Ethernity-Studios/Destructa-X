using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Team
{
    None, Blue, Red
}

public class RoomManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(CmdUpdateTeamSizeUI))]
    public int BlueTeamSize;
    [SyncVar(hook = nameof(CmdUpdateTeamSizeUI))]
    public int RedTeamSize;

    NetworkManagerRoom room;
    NetworkManagerRoom Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerRoom;
        }
    }

    [SerializeField] GameObject TeamSelectUI;
    [SerializeField] GameObject AgentSelectUI;

    [SerializeField] Transform BlueTeamHolder;
    [SerializeField] Transform RedTeamHolder;

    [SerializeField] TMP_Text BlueTeamSizeText;
    [SerializeField] TMP_Text RedTeamSizeText;

    [SerializeField] GameObject PreselectedAgentGO;
    [SerializeField] Image PreselectedAgentImg;

    public Button[] agentButtons;

    AgentManager agentManager;

    [SerializeField] GameObject mapSelect;
    [SerializeField] TMP_Dropdown mapDropdown;
    [SerializeField] TMP_Text mapText;

    [SerializeField] Image backgroundImage;

    [SyncVar]
    public string SelectedMap;

    public Map[] map;
    public Dictionary<string, Map> maps = new();

    public static Agent PAgent;
    public static Team PTeam;

    [SerializeField] GameObject AgentsUI;
    [SerializeField] GameObject LoadingUI;

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
            if (player.isLocalPlayer)
            {
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

    #endregion
}

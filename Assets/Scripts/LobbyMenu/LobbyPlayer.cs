using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : NetworkRoomPlayer
{
    RoomManager roomManager;
    AgentManager agentManager;

    NetworkManagerRoom room;
    NetworkManagerRoom Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerRoom;
        }
    }
    [SyncVar(hook = nameof(SyncPlayerName))]
    public string PlayerName;
    [SyncVar(hook = nameof(SyncTeamUI))]
    public Team PlayerTeam;
    [SyncVar(hook = nameof(SyncPreselectAgentUI))]
    public Agent PlayerPreselectedAgent = Agent.None;
    [SyncVar(hook = nameof(SyncSelectedAgentUI))]
    public Agent PlayerSelectedAgent = Agent.None;

    public Image AgentIcon;
    public TMP_Text agentText;

    Transform BlueTeamHolder;
    Transform RedTeamHolder;

    private void Awake()
    {
        BlueTeamHolder = GameObject.Find("BlueTeam").transform;
        RedTeamHolder = GameObject.Find("RedTeam").transform;
        agentManager = FindObjectOfType<AgentManager>();
    }

    /*public override void OnClientEnterRoom()
    {
        return;
        BlueTeamHolder = GameObject.Find("BlueTeam").transform;
        RedTeamHolder = GameObject.Find("RedTeam").transform;
        agentManager = FindObjectOfType<AgentManager>();

        foreach (var player in Room.roomSlots)
        {
            var localPlayer = player.GetComponent<LobbyPlayer>();
            var localPlayerTeam = localPlayer.PlayerTeam;
            switch (localPlayerTeam)
            {
                case Team.None:
                    break;
                case Team.Blue:
                    localPlayer.transform.SetParent(BlueTeamHolder);
                    localPlayer.GetComponent<Image>().color = new Color(0f / 255f, 203f / 255f, 255f / 255f, 1f);

                    break;
                case Team.Red:
                    localPlayer.transform.SetParent(RedTeamHolder);
                    localPlayer.GetComponent<Image>().color = new Color(195f / 255f, 63f / 255f, 63f / 255f, 1f);
                    break;
            }
            localPlayer.GetComponent<RectTransform>().localScale = Vector3.one;
            localPlayer.transform.GetChild(0).GetComponent<TMP_Text>().text = localPlayer.PlayerName;
            Image localPlayerImage = localPlayer.transform.GetChild(1).GetComponent<Image>();
            if (localPlayer.PlayerSelectedAgent == Agent.None)
            {
                localPlayerImage.sprite = null;
                if (localPlayer.PlayerPreselectedAgent == Agent.None)
                {
                    localPlayerImage.sprite = null;
                }
                else
                {
                    localPlayerImage.sprite = agentManager.GetAgentMeta(localPlayer.PlayerPreselectedAgent).Meta.Icon;
                }
            }
            else
            {
                localPlayerImage.sprite = agentManager.GetAgentMeta(localPlayer.PlayerSelectedAgent).Meta.Icon;
                localPlayerImage.color = Color.white;
                localPlayer.transform.GetChild(2).GetComponent<TMP_Text>().text = agentManager.GetAgentMeta(localPlayer.PlayerSelectedAgent).Name;
            }
        }
        base.OnClientEnterRoom();
    }*/

    public override void OnClientExitRoom()
    {
        /*
        roomManager = FindObjectOfType<RoomManager>();
        if (SceneManager.GetActiveScene().name == "RoomScene" && isServer && roomManager != null)
        {
            int tempB = 0, tempR = 0;
            foreach (var player in Room.roomSlots)
            {
                LobbyPlayer localPlayer = player.GetComponent<LobbyPlayer>();
                switch (localPlayer.PlayerTeam)
                {
                    case Team.None:
                        break;
                    case Team.Blue:
                        tempB++;
                        break;
                    case Team.Red:
                        tempR++;
                        break;
                }
                if(SceneManager.GetActiveScene().name == "RoomScene" && isServer && roomManager != null)
                {
                    roomManager.BlueTeamSize = tempB;
                    roomManager.RedTeamSize = tempR;
                }
            }
        }
        */
    }

    #region Command Sync

    [Command]
    public void CmdJoinTeam(Team team)
    {
        PlayerTeam = team;
        RpcSetTeamUI(team);
    }

    [Command]
    public void CmdSetNickname(string name)
    {
        PlayerName = name;
    }

    [Command]
    public void CmdPreselectAgent(Agent agent)
    {
        PlayerPreselectedAgent = agent;
        RpcPreselectAgent(agent);
    }

    [Command]
    public void CmdSelectAgent(Agent agent)
    {
        CmdChangeReadyState(true);
        PlayerSelectedAgent = agent;
        PlayerPreselectedAgent = Agent.None;
        RpcSelectAgent(agent);
    }


    #endregion

    #region Rpc Sync

    [ClientRpc]
    public void RpcSetTeamUI(Team team)
    {
        if (team == Team.Blue)
        {
            transform.SetParent(BlueTeamHolder);
            GetComponent<Image>().color = new Color(72f / 255f, 221f / 255f, 111f / 255f, 1f);
        }
        else if (team == Team.Red)
        {
            transform.SetParent(RedTeamHolder);
            GetComponent<Image>().color = new Color(233f / 255f, 89f / 255f, 87f / 255f, 1f);
        }
        GetComponent<RectTransform>().localScale = Vector3.one;
        transform.GetChild(0).GetComponent<TMP_Text>().text = PlayerName;
    }

    [ClientRpc]
    public void RpcPreselectAgent(Agent agent)
    {
        if (PlayerTeam == Team.Blue)
        {
            transform.SetParent(BlueTeamHolder);
            GetComponent<Image>().color = new Color(72f / 255f, 221f / 255f, 111f / 255f, 1f);
        }
        else if (PlayerTeam == Team.Red)
        {
            transform.SetParent(RedTeamHolder);
            GetComponent<Image>().color = new Color(233f / 255f, 89f / 255f, 87f / 255f, 1f);
        }
        GetComponent<RectTransform>().localScale = Vector3.one;
        transform.GetChild(0).GetComponent<TMP_Text>().text = PlayerName;
        
        AgentIcon.color = Color.gray;
        agentManager = FindObjectOfType<AgentManager>();
        AgentIcon.sprite = agentManager.GetAgentMeta(agent).Meta.Icon;
    }

    [ClientRpc]
    public void RpcSelectAgent(Agent agent)
    {
        if (PlayerTeam == Team.Blue)
        {
            transform.SetParent(BlueTeamHolder);
            GetComponent<Image>().color = new Color(72f / 255f, 221f / 255f, 111f / 255f, 1f);
        }
        else if (PlayerTeam == Team.Red)
        {
            transform.SetParent(RedTeamHolder);
            GetComponent<Image>().color = new Color(233f / 255f, 89f / 255f, 87f / 255f, 1f);
        }
        GetComponent<RectTransform>().localScale = Vector3.one;
        transform.GetChild(0).GetComponent<TMP_Text>().text = PlayerName;
        
        
        AgentIcon.color = Color.white;
        agentManager = FindObjectOfType<AgentManager>();
        agentText.text = agentManager.GetAgentMeta(agent).Name;
    }

    #endregion


    [ClientRpc]
    public void setupUI()
    {
        switch (PlayerTeam)
        {
            case Team.None:
                break;
            case Team.Blue:
                transform.SetParent(BlueTeamHolder);
                GetComponent<Image>().color = new Color(72f / 255f, 221f / 255f, 111f / 255f, 1f);
                break;
            case Team.Red:
                transform.SetParent(RedTeamHolder);
                GetComponent<Image>().color = new Color(233f / 255f, 89f / 255f, 87f / 255f, 1f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
            
        }
        GetComponent<RectTransform>().localScale = Vector3.one;
        transform.GetChild(0).GetComponent<TMP_Text>().text = PlayerName;
    }

    [ClientRpc]
    public void preSelectChampUI()
    {
        var localPlayerImage = transform.GetChild(1).GetComponent<Image>();
        if (PlayerPreselectedAgent == Agent.None) return;
        localPlayerImage.sprite = agentManager.GetAgentMeta(PlayerPreselectedAgent).Meta.Icon;
        localPlayerImage.color = Color.gray;
        transform.GetChild(2).GetComponent<TMP_Text>().text = agentManager.GetAgentMeta(PlayerPreselectedAgent).Name;
    }

    [ClientRpc]
    public void selectChampUI()
    {
        var localPlayerImage = transform.GetChild(1).GetComponent<Image>();
        if (PlayerSelectedAgent == Agent.None) return;
        localPlayerImage.sprite = agentManager.GetAgentMeta(PlayerSelectedAgent).Meta.Icon;
        localPlayerImage.color = Color.white;
        transform.GetChild(2).GetComponent<TMP_Text>().text = agentManager.GetAgentMeta(PlayerSelectedAgent).Name;
    }

    void SyncTeamUI(Team _, Team newValue)
    {

        switch (newValue)
        {
            case Team.None:
                throw new Exception("SyncTeamUI team None");
                //break;
            case Team.Blue:
                transform.SetParent(BlueTeamHolder);
                GetComponent<Image>().color = new Color(72f / 255f, 221f / 255f, 111f / 255f, 1f);
                break;
            case Team.Red:
                transform.SetParent(RedTeamHolder);
                GetComponent<Image>().color = new Color(233f / 255f, 89f / 255f, 87f / 255f, 1f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
            
        }
        GetComponent<RectTransform>().localScale = Vector3.one;
    }
    
    void SyncPreselectAgentUI(Agent _, Agent newValue)
    {
        // if (PlayerSelectedAgent != Agent.None) return;
        var localPlayerImage = transform.GetChild(1).GetComponent<Image>();
        var agentName = transform.GetChild(2).GetComponent<TMP_Text>();

        localPlayerImage.sprite = agentManager.GetAgentMeta(newValue).Meta.Icon;
        // localPlayerImage.color = Color.gray;
        agentName.text = agentManager.GetAgentMeta(newValue).Name;
    }
    
    void SyncSelectedAgentUI(Agent _, Agent newValue)
    {
        var localPlayerImage = transform.GetChild(1).GetComponent<Image>();
        var agentName = transform.GetChild(2).GetComponent<TMP_Text>();

        localPlayerImage.sprite = agentManager.GetAgentMeta(newValue).Meta.Icon;
        localPlayerImage.color = Color.white;
        agentName.text = agentManager.GetAgentMeta(newValue).Name;
    }

    void SyncPlayerName(string _, string newValue)
    {
        BlueTeamHolder = GameObject.Find("BlueTeam").transform;
        RedTeamHolder = GameObject.Find("RedTeam").transform;
        transform.GetChild(0).GetComponent<TMP_Text>().text = newValue;
    }
    
    void SyncAgentUI(Agent _, Agent newValue)
    {
        LobbyPlayer player = this;
        switch (player.PlayerTeam)
        {
            case Team.None:
                break;
            case Team.Blue:
                player.transform.SetParent(BlueTeamHolder);
                player.GetComponent<Image>().color = new Color(72f / 255f, 221f / 255f, 111f / 255f, 1f);
                break;
            case Team.Red:
                player.transform.SetParent(RedTeamHolder);
                player.GetComponent<Image>().color = new Color(233f / 255f, 89f / 255f, 87f / 255f, 1f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
            
        }
        player.GetComponent<RectTransform>().localScale = Vector3.one;
        player.transform.GetChild(0).GetComponent<TMP_Text>().text = PlayerName;
        var localPlayerImage = player.transform.GetChild(1).GetComponent<Image>();
        var agentName = player.transform.GetChild(2).GetComponent<TMP_Text>();

        if (player.PlayerSelectedAgent != Agent.None)
        {
            localPlayerImage.sprite = agentManager.GetAgentMeta(PlayerSelectedAgent).Meta.Icon;
            localPlayerImage.color = Color.white;
            agentName.text = agentManager.GetAgentMeta(PlayerSelectedAgent).Name;
            return;
        }
        if (player.PlayerPreselectedAgent == Agent.None) return;
            
        localPlayerImage.sprite = agentManager.GetAgentMeta(PlayerPreselectedAgent).Meta.Icon;
        localPlayerImage.color = Color.gray;
        agentName.text = agentManager.GetAgentMeta(player.PlayerPreselectedAgent).Name;
    }


    [ClientRpc]
    public void syncUI()
    {
        var player = this;
        switch (player.PlayerTeam)
        {
            case Team.None:
                break;
            case Team.Blue:
                player.transform.SetParent(BlueTeamHolder);
                player.GetComponent<Image>().color = new Color(72f / 255f, 221f / 255f, 111f / 255f, 1f);
                break;
            case Team.Red:
                player.transform.SetParent(RedTeamHolder);
                player.GetComponent<Image>().color = new Color(233f / 255f, 89f / 255f, 87f / 255f, 1f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
            
        }
        player.GetComponent<RectTransform>().localScale = Vector3.one;
        player.transform.GetChild(0).GetComponent<TMP_Text>().text = PlayerName;
        var localPlayerImage = player.transform.GetChild(1).GetComponent<Image>();
        var agentName = player.transform.GetChild(2).GetComponent<TMP_Text>();

        if (player.PlayerSelectedAgent != Agent.None)
        {
            localPlayerImage.sprite = agentManager.GetAgentMeta(PlayerSelectedAgent).Meta.Icon;
            localPlayerImage.color = Color.white;
            agentName.text = agentManager.GetAgentMeta(PlayerSelectedAgent).Name;
            return;
        }
        if (player.PlayerPreselectedAgent == Agent.None) return;
            
        localPlayerImage.sprite = agentManager.GetAgentMeta(PlayerPreselectedAgent).Meta.Icon;
        localPlayerImage.color = Color.gray;
        agentName.text = agentManager.GetAgentMeta(player.PlayerPreselectedAgent).Name;
    }
}

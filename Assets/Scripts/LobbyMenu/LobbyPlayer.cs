using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SyncVar]
    public string PlayerName;
    [SyncVar]
    public Team PlayerTeam;
    [SyncVar]
    public Agent PlayerPreselectedAgent = Agent.None;
    [SyncVar]
    public Agent PlayerSelectedAgent = Agent.None;

    public Image AgentIcon;
    public TMP_Text agentText;

    Transform BlueTeamHolder;
    Transform RedTeamHolder;

    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;
        PlayerPreselectedAgent = Agent.None;
        PlayerSelectedAgent = Agent.None;
        CmdSetNickname(NicknameManager.DisplayName);

        base.OnStartClient();
    }

    public override void OnClientEnterRoom()
    {
        BlueTeamHolder = GameObject.Find("BlueTeam").transform;
        RedTeamHolder = GameObject.Find("RedTeam").transform;
        agentManager = FindObjectOfType<AgentManager>();

        foreach (var player in Room.roomSlots)
        {
            LobbyPlayer localPlayer = player.GetComponent<LobbyPlayer>();
            Team localPlayerTeam = localPlayer.PlayerTeam;
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
    }

    public override void OnClientExitRoom()
    {
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
                roomManager.BlueTeamSize = tempB;
                roomManager.RedTeamSize = tempR;
            }
            Debug.Log(tempB + " W " + tempR);
        }
        base.OnClientExitRoom();
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
        BlueTeamHolder = GameObject.Find("BlueTeam").transform;
        RedTeamHolder = GameObject.Find("RedTeam").transform;
        if (team == Team.Blue)
        {
            transform.SetParent(BlueTeamHolder);
            GetComponent<Image>().color = new Color(0f / 255f, 203f / 255f, 255f / 255f, 1f);
        }
        else if (team == Team.Red)
        {
            transform.SetParent(RedTeamHolder);
            GetComponent<Image>().color = new Color(195f / 255f, 63f / 255f, 63f / 255f, 1f);
        }
        GetComponent<RectTransform>().localScale = Vector3.one;
        transform.GetChild(0).GetComponent<TMP_Text>().text = PlayerName;
    }

    [ClientRpc]
    public void RpcPreselectAgent(Agent agent)
    {
        agentManager = FindObjectOfType<AgentManager>();
        AgentIcon.sprite = agentManager.GetAgentMeta(agent).Meta.Icon;
    }

    [ClientRpc]
    public void RpcSelectAgent(Agent agent)
    {
        AgentIcon.color = Color.white;
        agentText.text = agentManager.GetAgentMeta(agent).Name;
    }

    #endregion
}

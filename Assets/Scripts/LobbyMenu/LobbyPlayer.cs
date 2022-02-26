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
    [SyncVar]
    public string PlayerName;
    [SyncVar]
    public Team PlayerTeam;
    [SyncVar]
    public Agent PlayerPreselectedAgent;
    [SyncVar]
    public Agent PlayerSelectedAgent;

    Transform BlueTeamHolder;
    Transform RedTeamHolder;

    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;
        roomManager = FindObjectOfType<RoomManager>();
        agentManager = FindObjectOfType<AgentManager>();

        CmdSetNickname(NicknameManager.DisplayName);

        base.OnStartClient();
    }

    public override void OnClientEnterRoom()
    {
        BlueTeamHolder = GameObject.Find("BlueTeam").transform;
        RedTeamHolder = GameObject.Find("RedTeam").transform;
        foreach (var player in Room.roomSlots)
        {
            Debug.Log("Tak to je mrdka");
            Debug.Log(player.index);
            LobbyPlayer localPlayer = player.GetComponent<LobbyPlayer>();
            Team localPlayerTeam = localPlayer.PlayerTeam;
            string localPlayerName = localPlayer.PlayerName;
            Debug.Log("Local PLAYER TEAM:" + localPlayer.PlayerTeam);
            switch (localPlayerTeam)
            {
                case Team.None:
                    break;
                case Team.Blue:
                    localPlayer.transform.SetParent(BlueTeamHolder);
                    localPlayer.GetComponent<Image>().color = new Color(0f / 255f, 203f / 255f, 255f / 255f, 1f);
                    localPlayer.GetComponent<RectTransform>().localScale = Vector3.one;
                    localPlayer.transform.GetChild(0).GetComponent<TMP_Text>().text =  localPlayer.PlayerName;
                    break;
                case Team.Red:
                    localPlayer.transform.SetParent(RedTeamHolder);
                    localPlayer.GetComponent<Image>().color = new Color(195f / 255f, 63f / 255f, 63f / 255f, 1f);
                    localPlayer.GetComponent<RectTransform>().localScale = Vector3.one;
                    localPlayer.transform.GetChild(0).GetComponent<TMP_Text>().text = localPlayer.PlayerName;
                    break;
            }

        }
        base.OnClientEnterRoom();
    }

    public override void OnClientExitRoom()
    {
        roomManager = FindObjectOfType<RoomManager>();
        int tempB = 0, tempR = 0;
        foreach (var player in Room.roomSlots)
        {
            if (player.isLocalPlayer)
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
        }

        base.OnClientExitRoom();
    }

    #region Command synchronization

    [Command]
    public void CmdJoinTeam(Team team)
    {
        PlayerTeam = team;
        RpcSetTeamUI(team);
    }

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

    [Command]
    public void CmdSetNickname(string name)
    {
        PlayerName = name;
    }

    [Command]
    public void CmdPreselectAgent(Agent agent)
    {
        PlayerPreselectedAgent = agent;
    }

    [Command]
    public void CmdSelectAgent(Agent agent)
    {
        PlayerSelectedAgent = agent;
    }

    #endregion
}

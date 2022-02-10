using UnityEngine;
using Mirror;

public class NetworkPlayerRoom : NetworkRoomPlayer
{
    public string NameTest;

    LobbyMenuManager lobbyMenuManager;
    LobbyPlayer localPlayer;

    new void Start()
    {
        if (!isLocalPlayer) return;
        lobbyMenuManager = FindObjectOfType<LobbyMenuManager>();
        localPlayer = gameObject.GetComponent<LobbyPlayer>();
        base.Start();
    }

    public override void OnClientExitRoom()
    {
        /*if (localPlayer.SelectedTeam == Team.Blue)
        {
            lobbyMenuManager.CmdUpdateTeamSize(-1,0);
        }
        else if(localPlayer.SelectedTeam == Team.Red)
        {
            lobbyMenuManager.CmdUpdateTeamSize(0, -1);
        }*/
        TestCommand();
        base.OnClientExitRoom();
    }

    [Command]
    void TestCommand()
    {
        Debug.Log("TestOCMCOMDOAWMDOAWDMAODMAWODMAWOMDOMDODAMS");
    }
}

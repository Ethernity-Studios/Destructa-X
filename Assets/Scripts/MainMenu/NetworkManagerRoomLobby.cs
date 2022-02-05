using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class NetworkManagerRoomLobby : NetworkManager
{
    

    public void StartGame()
    {
        NetworkClient.Ready();
    }
}

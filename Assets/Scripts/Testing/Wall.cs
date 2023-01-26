using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Wall : NetworkBehaviour
{
    public List<GameObject> Walls;

    bool wallDown;

    void Update()
    {
        if (!isServer) return;
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            if (wallDown) RpcShowWalls();
            else RpcHideWalls();
        }*/
    }
    [ClientRpc]
    void RpcShowWalls()
    {
        wallDown = false;
        foreach (var wall in Walls)
        {
            wall.SetActive(true);
        }
    }
    [ClientRpc]
    void RpcHideWalls()
    {
        wallDown = true;
        foreach (var wall in Walls)
        {
            wall.SetActive(false);
        }
    }
}

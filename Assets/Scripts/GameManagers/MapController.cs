using Mirror;
using UnityEngine;

public class MapController : NetworkBehaviour
{
    [SerializeField] GameObject[] dropdownWalls;

    [ClientRpc]
    void RpcDropWalls()
    {
        foreach (GameObject wall in dropdownWalls)
        {
            if (wall == null) continue;
            wall.SetActive(false);
        }
    }

    [ClientRpc]
    void RpcResetWalls()
    {
        foreach (GameObject wall in dropdownWalls)
        {
            if (wall == null) continue;
            wall.SetActive(true);
        }
    }

    [Server]
    public void DropWalls()
    {
        RpcDropWalls();
    }
    
    [Server]
    public void ResetWalls()
    {
        RpcResetWalls();
    }
}

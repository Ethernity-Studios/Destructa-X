using UnityEngine;
using Mirror;

public class MapController : NetworkBehaviour
{
    [SerializeField] GameObject[] dropdownWalls;

    [ClientRpc]
    public void RpcDropWalls()
    {
        foreach (var wall in dropdownWalls)
        {
            wall.SetActive(false);
        }
    }

    [ClientRpc]
    public void RpcResetWalls()
    {
        foreach (var wall in dropdownWalls)
        {
            wall.SetActive(true);
        }
    }
}

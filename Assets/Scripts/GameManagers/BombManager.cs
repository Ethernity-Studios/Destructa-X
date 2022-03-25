using UnityEngine;
using Mirror;

public class BombManager : NetworkBehaviour
{
    [SerializeField] GameObject bombExplosion;

    [SerializeField] bool detonating;

    [Command(requiresAuthority = false)]
    public void CmdDetonateBomb()
    {
        if (!detonating)
        {
            detonating = true;
            GameObject explosion = Instantiate(bombExplosion);
            NetworkServer.Spawn(explosion);
            RpcSetupExplosion(explosion);
        }
    }
    [ClientRpc]
    void RpcSetupExplosion(GameObject explosion)
    {
        explosion.transform.SetParent(transform);
        explosion.transform.localScale = new Vector3(.1f, .1f, .1f);
        explosion.transform.position = transform.position;
    }
}

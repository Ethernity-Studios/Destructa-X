using UnityEngine;
using Mirror;
using System.Collections;

public class BombManager : NetworkBehaviour
{
    [SerializeField] GameObject bombExplosion;

    [SerializeField] bool detonating;
    [SyncVar] public bool canBoom = true;

    [Command(requiresAuthority = false)]
    public void CmdDetonateBomb()
    {
        if (!canBoom) return;
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
    private void Start()
    {
        StartCoroutine(startBombTimer());
    }

    IEnumerator startBombTimer()
    {
        yield return new WaitForSeconds(40);
        CmdDetonateBomb();
    }

    [ClientRpc]
    public void noBoomPwease()
    {
        StopAllCoroutines();
    }
}

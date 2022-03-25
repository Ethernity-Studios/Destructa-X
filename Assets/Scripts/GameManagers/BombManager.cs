using UnityEngine;
using Mirror;

public class BombManager : NetworkBehaviour
{
    [SerializeField] GameObject bombExplosion;

    [SerializeField] bool detonating;

    GameObject explosion;

    public float ExplosionSize;
    public float IncreaseSize;

    [Command(requiresAuthority = false)]
    public void CmdDetonateBomb()
    {
        if (!detonating)
        {
            detonating = true;
            explosion = Instantiate(bombExplosion, transform);
            explosion.transform.position = transform.position;
            NetworkServer.Spawn(explosion);
            Debug.Log("Explosionen!");
        }
    }
}

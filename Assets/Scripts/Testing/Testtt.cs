using UnityEngine;
using Mirror;

public class Testtt : NetworkBehaviour
{
    [SerializeField] GameObject Bullet;
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.I))
        {
            CmdSpawn();
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSpawn()
    {
        GameObject bl = Instantiate(Bullet,transform);
        NetworkServer.Spawn(bl);
    }
}

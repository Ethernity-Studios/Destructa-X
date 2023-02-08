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

        trol t = new(){Name = "Pepa"};


        bool canGo = t.Name == "Pepa" ? true : false;
    }
}

class trol
{
    public string Name;
}

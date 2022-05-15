using Mirror;
using UnityEngine;

public class PlayerShootingManager : NetworkBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] PlayerInventoryManager playerInventory;
    [SerializeField] Transform cameraHolder;

    GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (playerInventory.EqupiedGun != null)
                Shoot();
        }



        RaycastHit hit;
        Ray ray = cameraHolder.GetComponent<Camera>().ViewportPointToRay(new Vector3(.5f, .5f, 0));
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, mask))
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.magenta, .1f);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.white);
        }
    }

    public void Shoot()
    {
        CmdSpawnBullet();
    }

    [Command]
    void CmdSpawnBullet()
    {
        GameObject bulletInstance = Instantiate(bullet);
        NetworkServer.Spawn(bulletInstance);
        RpcSpawnBullet(bulletInstance);
    }
    [SerializeField] LayerMask mask;

    void RpcSpawnBullet(GameObject bulletInstance)
    {
        bulletInstance.transform.SetParent(gameManager.BulletHolder.transform);

        RaycastHit hit;
        Ray ray = cameraHolder.GetComponent<Camera>().ViewportPointToRay(new Vector3(.5f,.5f,0));
        if (Physics.Raycast(ray.origin, ray.direction, out hit,Mathf.Infinity, mask))
        {
            if(Vector3.Distance(ray.origin,hit.point) > 2)
            {
                bulletInstance.transform.localPosition = playerInventory.EqupiedGunInstance.transform.GetChild(2).transform.position;
            }
            else
            {
                bulletInstance.transform.localPosition = ray.origin;
                Bullet bullet = bulletInstance.GetComponent<Bullet>();
                bullet.GetComponent<MeshRenderer>().enabled = false;
                bullet.GetComponent<TrailRenderer>().enabled = false;
            }
            bulletInstance.transform.LookAt(hit.point);
        }
        else
        {
            bulletInstance.transform.localPosition = playerInventory.EqupiedGunInstance.transform.GetChild(2).transform.position;
            bulletInstance.transform.LookAt(new Vector3(ray.GetPoint(10).x,ray.GetPoint(10).y-.2f, ray.GetPoint(10).z));
        }
    }
}

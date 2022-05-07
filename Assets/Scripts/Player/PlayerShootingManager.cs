using Mirror;
using UnityEngine;

public class PlayerShootingManager : NetworkBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] PlayerInventoryManager playerInventory;
    [SerializeField] Transform cameraHolder;
    void Update()
    {
        Ray ray = new Ray(cameraHolder.transform.position, cameraHolder.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, mask))
        {
            Debug.DrawRay(ray.origin, hit.point, Color.magenta,2f,false);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.yellow, 2f, false);
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (playerInventory.EqupiedGun != null)
                Shoot();
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
    [ClientRpc]
    void RpcSpawnBullet(GameObject bulletInstance)
    {
        Debug.Log("Camera pos: " + cameraHolder.transform.position);
        bulletInstance.transform.localPosition = playerInventory.EqupiedGunInstance.transform.GetChild(2).transform.position;
        //Ray ray = new Ray(transform.position, cameraHolder.transform.forward);
        RaycastHit hit;
        //Ray ray = transform.GetChild(0).GetComponent<Camera>().ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z-5));
        if(Physics.Raycast(transform.position, cameraHolder.transform.forward, hitInfo: out hit,mask))
        {
            if(hit.transform != transform && hit.transform.gameObject.layer !=6 && hit.transform.gameObject.layer != 7)
            {
                bulletInstance.transform.LookAt(hit.point);
            }
        }
        else
        {
            Debug.Log("shooting at air");
            //bulletInstance.transform.LookAt(ray.GetPoint(50));
        }

        //bulletInstance.transform.localEulerAngles = cameraHolder.transform.localEulerAngles + transform.localEulerAngles + transform.forward;
    }
}

using Mirror;
using System.Collections;
using UnityEngine;

public class PlayerShootingManager : NetworkBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] PlayerInventoryManager playerInventory;
    [SerializeField] UIManager uiManager;

    [SerializeField] Transform cameraHolder;

    GameManager gameManager;

    [SerializeField] bool canShoot = true;
    public bool Reloading;

    public GunInstance gunInstance;
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        if (!isLocalPlayer) return;
        cameraHolder.GetComponent<Camera>().enabled = true;
        cameraHolder.GetChild(0).GetComponent<Camera>().enabled = true;
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        if (gunInstance == null) return;

        if (playerInventory.EqupiedGun != null && playerInventory.gunEqupied && canShoot && gunInstance.Magazine > 0 && !Reloading)
        {
            if (playerInventory.EqupiedGun.LMB.FireMode == FireMode.Manual && Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            else if (playerInventory.EqupiedGun.LMB.FireMode == FireMode.Automatic && Input.GetMouseButton(0))
            {
                Shoot();
            }
        }
        if (playerInventory.EqupiedGun == null) return;
        if (gunInstance.Magazine == 0 && gunInstance.Ammo > 0 && !Reloading)
        {
            StartCoroutine(Reload());
        }
        if (gunInstance.Magazine != playerInventory.EqupiedGun.MagazineAmmo && Input.GetKeyDown(KeyCode.R) && gunInstance.Ammo > 0 && !Reloading)
        {
            StartCoroutine(Reload());
        }
        /*RaycastHit hit;
        Ray ray = cameraHolder.GetComponent<Camera>().ViewportPointToRay(new Vector3(.5f, .5f, 0));
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, mask))
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.magenta, .1f);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.white);
        }*/
    }

    public IEnumerator DelayFire()
    {
        yield return new WaitForSeconds(playerInventory.EqupiedGun.LMB.FireDelay);
        canShoot = true;
    }

    public void Shoot()
    {
        canShoot = false;
        StartCoroutine(DelayFire());
        gunInstance.Magazine--;
        CmdSpawnBullet();
        UpdateUIAmmo();
    }

    public IEnumerator Reload()
    {
        Debug.Log("Starting reloiad");
        Reloading = true;
        yield return new WaitForSeconds(playerInventory.EqupiedGun.ReloadTime);
        Debug.Log("Stopped reloading");
        Reloading = false;
        if (gunInstance.Ammo >= playerInventory.EqupiedGun.MagazineAmmo)
        {
            gunInstance.Ammo -= playerInventory.EqupiedGun.MagazineAmmo - gunInstance.Magazine;
            gunInstance.Magazine = playerInventory.EqupiedGun.MagazineAmmo;
        }
        else
        {
            gunInstance.Magazine = gunInstance.Ammo;
            gunInstance.Ammo = 0;
        }
        UpdateUIAmmo();
    }

    public void UpdateUIAmmo()
    {
        uiManager.MaxAmmoText.text = gunInstance.Ammo.ToString();
        uiManager.MagazineText.text = gunInstance.Magazine.ToString();
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
        Bullet bullet = bulletInstance.GetComponent<Bullet>();
        bullet.PenetrationAmount = playerInventory.EqupiedGun.BulletPenetration;
        bulletInstance.transform.SetParent(gameManager.BulletHolder.transform);

        RaycastHit hit;

        Ray ray = cameraHolder.GetComponent<Camera>().ViewportPointToRay(new Vector3(.5f, .5f, 0));
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, mask))
        {
            if (Vector3.Distance(ray.origin, hit.point) > .9f)
            {
                bulletInstance.transform.localPosition = ray.origin;
                bulletInstance.transform.LookAt(hit.point);
                bullet.CheckPenetration();
                bulletInstance.transform.localPosition = playerInventory.EqupiedGunInstance.transform.GetChild(2).transform.position;
            }
            else
            {
                Debug.Log("ELSE");
                bulletInstance.GetComponent<Renderer>().enabled = false;
                bulletInstance.GetComponent<TrailRenderer>().enabled = false;
                bulletInstance.transform.localPosition = ray.origin;
                bulletInstance.transform.LookAt(hit.point);
                bullet.CheckPenetration();
            }
            bulletInstance.transform.LookAt(hit.point);
        }
        else
        {
            bulletInstance.transform.localPosition = playerInventory.EqupiedGunInstance.transform.GetChild(2).transform.position;
            bulletInstance.transform.LookAt(new Vector3(ray.GetPoint(10).x, ray.GetPoint(10).y - .15f, ray.GetPoint(10).z));
        }
        bullet.CameraPosition = cameraHolder.position;
        bullet.BulletDirection = new Vector3(cameraHolder.eulerAngles.x, transform.eulerAngles.y);
        bullet.BulletOwner = GetComponent<Player>();
    }
}

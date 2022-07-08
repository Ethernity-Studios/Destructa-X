using Mirror;
using System.Collections;
using UnityEngine;

public class PlayerShootingManager : NetworkBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] PlayerInventoryManager playerInventory;
    Player player;
    [SerializeField] UIManager uiManager;

    [SerializeField] Transform cameraHolder;

    GameManager gameManager;
    PlayerEconomyManager playerEconomyManager;

    public bool CanShoot = true;
    public bool Reloading;

    public GunInstance GunInstance;
    private void Start()
    {
        player = GetComponent<Player>();
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        playerEconomyManager = GetComponent<PlayerEconomyManager>();
        if (!isLocalPlayer) return;
        cameraHolder.GetComponent<Camera>().enabled = true;
        cameraHolder.GetChild(0).GetComponent<Camera>().enabled = true;
    }
    void Update()
    {
        Debug.DrawRay(cameraHolder.position, cameraHolder.forward, Color.red);
        if (player.IsDead) return;
        if (!isLocalPlayer) return;
        if (GunInstance == null) return;
        if (playerEconomyManager.IsShopOpen) return;

        if (playerInventory.EqupiedGun != null && playerInventory.GunEqupied && CanShoot && GunInstance.Magazine > 0 && !Reloading)
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
        if (GunInstance.Magazine == 0 && GunInstance.Ammo > 0 && !Reloading)
        {
            StartCoroutine(Reload());
        }
        if (GunInstance.Magazine != playerInventory.EqupiedGun.MagazineAmmo && Input.GetKeyDown(KeyCode.R) && GunInstance.Ammo > 0 && !Reloading)
        {
            StartCoroutine(Reload());
        }
    }

    public IEnumerator DelayFire()
    {
        yield return new WaitForSeconds(playerInventory.EqupiedGun.LMB.FireDelay);
        CanShoot = true;
    }

    public IEnumerator Reload()
    {
        Reloading = true;
        yield return new WaitForSeconds(playerInventory.EqupiedGun.ReloadTime);
        Reloading = false;
        if (GunInstance.Ammo >= playerInventory.EqupiedGun.MagazineAmmo)
        {
            GunInstance.Ammo -= playerInventory.EqupiedGun.MagazineAmmo - GunInstance.Magazine;
            GunInstance.Magazine = playerInventory.EqupiedGun.MagazineAmmo;
        }
        else
        {
            GunInstance.Magazine = GunInstance.Ammo;
            GunInstance.Ammo = 0;
        }
        UpdateUIAmmo();
    }

    public void UpdateUIAmmo()
    {
        if (!isLocalPlayer) return;
        uiManager.MaxAmmoText.text = GunInstance.Ammo.ToString();
        uiManager.MagazineText.text = GunInstance.Magazine.ToString();
    }
    public void Shoot()
    {
        Debug.Log("Shooting!");
        CanShoot = false;
        StartCoroutine(DelayFire());
        GunInstance.Magazine--;
        UpdateUIAmmo();
        penetrationAmount = playerInventory.EqupiedGun.BulletPenetration;
        CheckPenetration();
    }

    [SerializeField] LayerMask mask;
    int BulletDamage;
    Vector3 endPoint;
    Vector3? penetrationPoint;
    Vector3? impactPoint;
    bool canPenetrate;
    float penetrationAmount;

    public void CheckPenetration()
    {
        Ray ray = new Ray(transform.position + new Vector3(0, 0, transform.localScale.z), transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(cameraHolder.position, cameraHolder.forward, out hit, Mathf.Infinity, mask))
        {
            if (hit.transform.parent.gameObject.TryGetComponent(out IDamageable entity))
            {
                Player hittedPlayer = hit.transform.parent.gameObject.GetComponent<Player>();
                ///// Cannot hit dummy -- need rework
                if (hittedPlayer.PlayerTeam != player.PlayerTeam && !hittedPlayer.IsDead)
                    if (entity.TakeDamage(CalculateDamage(hit.point))) 
                    {
                        player.CmdAddKill();
                        player.CmdAddRoundKill();
                    } 
            }
            impactPoint = hit.point;
            Ray penRay = new Ray(hit.point + ray.direction * penetrationAmount, -ray.direction);
            RaycastHit penHit;
            if (hit.collider.Raycast(penRay, out penHit, penetrationAmount))
            {
                penetrationPoint = penHit.point;
                endPoint = transform.position + transform.forward * 1000;

                if (hit.transform.TryGetComponent(out MaterialToughness materialToughness))
                {
                    penetrationAmount -= Vector3.Distance((Vector3)penetrationPoint, hit.point);
                    penetrationAmount -= materialToughness.ToughnessAmount;
                    CheckPenetration();
                }
                else
                {
                    return;
                }
            }
            else
            {
                endPoint = impactPoint.Value + ray.direction * penetrationAmount;
                penetrationPoint = endPoint;
                return;
            }
        }
        else
        {
            endPoint = transform.position + transform.forward * 1000;
            penetrationPoint = null;
            impactPoint = null;
        }
    }


    int CalculateDamage(Vector3 entityPosition)
    {
        Gun gun = playerInventory.EqupiedGun;
        float distance = Vector3.Distance(entityPosition, cameraHolder.position);
        if (gun.Damages.Count == 1)
        {
            BulletDamage = gun.Damages[0].BodyDamage;
        }
        else if (gun.Damages.Count == 2)
        {
            if (distance <= gun.Damages[0].MaxDistance) BulletDamage = gun.Damages[0].BodyDamage;
            else if (distance >= gun.Damages[1].MinDistance) BulletDamage = gun.Damages[1].BodyDamage;
        }
        else if (gun.Damages.Count == 3)
        {
            if (distance <= gun.Damages[0].MaxDistance) BulletDamage = gun.Damages[0].BodyDamage;
            else if (distance >= gun.Damages[1].MinDistance && distance <= gun.Damages[1].MaxDistance) BulletDamage = gun.Damages[1].BodyDamage;
            else if (distance >= gun.Damages[2].MinDistance) BulletDamage = gun.Damages[2].BodyDamage;
        }
        return BulletDamage;
    }

    /*[Command]
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
        bullet.Gun = playerInventory.EqupiedGun;
    }*/
}

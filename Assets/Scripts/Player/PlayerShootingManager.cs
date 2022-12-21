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
    [SerializeField] GameObject BulletImpactDecalPenetrable, BulletImpactDecalNotPenetrable;
    private void Awake()
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
        Debug.DrawRay(cameraHolder.position, cameraHolder.forward * 2, Color.green);
        if (player.IsDead) return;
        if (!isLocalPlayer) return;
        if (GunInstance == null) return;
        if (playerEconomyManager.IsShopOpen) return;

        if (playerInventory.EqupiedGun != null && playerInventory.GunEqupied && CanShoot && GunInstance.Magazine > 0 && !Reloading)
        {
            if (playerInventory.EqupiedGun.PrimaryFire.FireMode == FireMode.Manual && Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            else if (playerInventory.EqupiedGun.PrimaryFire.FireMode == FireMode.Automatic && Input.GetMouseButton(0))
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
        yield return new WaitForSeconds(playerInventory.EqupiedGun.PrimaryFire.FireDelay);
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
        CanShoot = false;
        StartCoroutine(DelayFire());
        GunInstance.Magazine--;
        UpdateUIAmmo();
        penetrationAmount = playerInventory.EqupiedGun.BulletPenetration;
        CheckPenetration(cameraHolder.position);
    }

    [SerializeField] LayerMask mask;
    int BulletDamage;
    Vector3 endPoint;
    Vector3? penetrationPoint;
    Vector3? impactPoint;
    bool canPenetrate;
    float penetrationAmount;

    public void CheckPenetration(Vector3 originPosition)
    {
        Ray ray = new Ray(originPosition, transform.forward + cameraHolder.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(originPosition, cameraHolder.forward, out hit, Mathf.Infinity, layerMask: mask))
        {
            if (hit.collider.transform.parent != null)
            {
                if (hit.collider.transform.parent.TryGetComponent(out IDamageable entity))
                {
                    Player hittedPlayer = hit.collider.transform.parent.gameObject.GetComponent<Player>();
                    if (hittedPlayer.PlayerTeam != player.PlayerTeam && !hittedPlayer.IsDead)
                        if (entity.TakeDamage(calculateDamage(hit.point)))
                        {
                            player.CmdAddKill();
                            player.CmdAddRoundKill();
                        }
                }
            }

            impactPoint = hit.point;
            Ray penRay = new Ray(hit.point + ray.direction * penetrationAmount, -ray.direction);
            RaycastHit penHit;
            if (hit.collider.Raycast(penRay, out penHit, penetrationAmount))
            {
                penetrationPoint = penHit.point;
                endPoint = transform.position + transform.forward * 1000;
                if (hit.collider.transform.TryGetComponent(out MaterialToughness materialToughness))
                {

                    CmdInstantiateImpactDecal(true, hit.point, hit.normal); // first point
                    CmdInstantiateImpactDecal(true, penHit.point, penHit.normal); //second point


                    penetrationAmount -= Vector3.Distance((Vector3)penetrationPoint, hit.point);
                    penetrationAmount -= materialToughness.ToughnessAmount;
                    CheckPenetration(hit.point);
                }
                else
                {
                    CmdInstantiateImpactDecal(false, hit.point, hit.normal);

                    return;
                }
            }
            else
            {
                CmdInstantiateImpactDecal(false, hit.point, hit.normal);
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

    int calculateDamage(Vector3 entityPosition)
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

    [Command]
    void CmdInstantiateImpactDecal(bool canPenetrate, Vector3 position, Vector3 rotation)
    {
        GameObject bulletImpact;
        if (canPenetrate) bulletImpact = Instantiate(BulletImpactDecalPenetrable);
        else bulletImpact = Instantiate(BulletImpactDecalNotPenetrable);
        NetworkServer.Spawn(bulletImpact);
        RpcInstantiateImpactDecal(bulletImpact, position, rotation);
        StartCoroutine(destroyDecal(bulletImpact));
    }

    IEnumerator destroyDecal(GameObject bulletImpact)
    {
        yield return new WaitForSeconds(7);
        NetworkServer.Destroy(bulletImpact);
    }

    [ClientRpc]
    void RpcInstantiateImpactDecal(GameObject bulletImpact, Vector3 position, Vector3 rotation)
    {
        bulletImpact.transform.position = position;
        bulletImpact.transform.rotation = Quaternion.LookRotation(rotation);
    }
}

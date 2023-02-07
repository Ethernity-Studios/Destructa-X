using Mirror;
using System.Collections;
using UnityEngine;

public class PlayerShootingManager : NetworkBehaviour
{
    [SerializeField] PlayerInventoryManager playerInventory;
    Player player;
    [SerializeField] UIManager uiManager;

    [SerializeField] Transform cameraHolder;

    PlayerEconomyManager playerEconomyManager;

    public bool CanShoot = true;
    public bool Reloading;

    public GunInstance GunInstance;
    [SerializeField] GameObject BulletImpactDecalPenetrable, BulletImpactDecalNotPenetrable;

    private PlayerInput playerInput;
    private void Awake()
    {
        

        player = GetComponent<Player>();
        FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        playerEconomyManager = GetComponent<PlayerEconomyManager>();
        if (!isLocalPlayer) return;
        
        playerInput = new PlayerInput();
        
        cameraHolder.GetComponent<Camera>().enabled = true;
        cameraHolder.GetChild(0).GetComponent<Camera>().enabled = true;
    }

    private void OnEnable()
    {
        if (!isLocalPlayer) return;
        playerInput.PlayerShoot.Enable();
    }

    private void OnDisable()
    {
        if (!isLocalPlayer) return;
        playerInput.PlayerShoot.Disable();
    }
    

    void Update()
    {
        Debug.DrawRay(cameraHolder.position, cameraHolder.forward * 2, Color.green);
        if (player.IsDead) return;
        if (!isLocalPlayer) return;
        if (GunInstance == null) return;
        if (playerEconomyManager.IsShopOpen) return;

        if (playerInventory.EquippedGun != null && playerInventory.GunEquipped && CanShoot && GunInstance.Magazine > 0 && !Reloading)
        {
            if (playerInventory.EquippedGun.PrimaryFire.FireMode == FireMode.Manual && playerInput.PlayerShoot.Primary.triggered)
            {
                Shoot();
            }
            else if (playerInventory.EquippedGun.PrimaryFire.FireMode == FireMode.Automatic && playerInput.PlayerShoot.Primary.IsPressed())
            {
                Shoot();
            }
        }
        if (playerInventory.EquippedGun == null) return;
        if (GunInstance.Magazine == 0 && GunInstance.Ammo > 0 && !Reloading)
        {
            StartCoroutine(Reload());
        }
        if (GunInstance.Magazine != playerInventory.EquippedGun.MagazineAmmo && playerInput.PlayerShoot.Reload.triggered && GunInstance.Ammo > 0 && !Reloading)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator DelayFire()
    {
        yield return new WaitForSeconds(playerInventory.EquippedGun.PrimaryFire.FireDelay);
        CanShoot = true;
    }

    private IEnumerator Reload()
    {
        Reloading = true;
        yield return new WaitForSeconds(playerInventory.EquippedGun.ReloadTime);
        Reloading = false;
        if (GunInstance.Ammo >= playerInventory.EquippedGun.MagazineAmmo)
        {
            GunInstance.Ammo -= playerInventory.EquippedGun.MagazineAmmo - GunInstance.Magazine;
            GunInstance.Magazine = playerInventory.EquippedGun.MagazineAmmo;
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

    private void Shoot()
    {
        CanShoot = false;
        StartCoroutine(DelayFire());
        GunInstance.Magazine--;
        UpdateUIAmmo();
        penetrationAmount = playerInventory.EquippedGun.BulletPenetration;
        CheckPenetration(cameraHolder.position);
    }

    [SerializeField] LayerMask mask;
    int BulletDamage;
    Vector3 endPoint;
    Vector3? penetrationPoint;
    Vector3? impactPoint;
    bool canPenetrate;
    float penetrationAmount;

    private void CheckPenetration(Vector3 originPosition)
    {
        while (true)
        {
            Ray ray = new Ray(originPosition, transform.forward + cameraHolder.transform.forward);
            if (Physics.Raycast(originPosition, cameraHolder.forward, out RaycastHit hit, Mathf.Infinity, layerMask: mask))
            {
                if (hit.collider.transform.parent != null)
                {
                    if (hit.collider.transform.parent.TryGetComponent(out IDamageable entity))
                    {
                        Player hitPlayer = hit.collider.transform.parent.gameObject.GetComponent<Player>();
                        if (hitPlayer.PlayerTeam != player.PlayerTeam && !hitPlayer.IsDead)
                            if (entity.TakeDamage(calculateDamage(hit.point)))
                            {
                                player.CmdAddKill();
                                player.CmdAddRoundKill();
                            }
                    }
                }

                impactPoint = hit.point;
                Ray penRay = new Ray(hit.point + ray.direction * penetrationAmount, -ray.direction);
                if (hit.collider.Raycast(penRay, out RaycastHit penHit, penetrationAmount))
                {
                    penetrationPoint = penHit.point;
                    endPoint = transform.position + transform.forward * 1000;
                    if (hit.collider.transform.TryGetComponent(out MaterialToughness materialToughness))
                    {
                        CmdInstantiateImpactDecal(true, hit.point, hit.normal); // first point
                        CmdInstantiateImpactDecal(true, penHit.point, penHit.normal); //second point


                        penetrationAmount -= Vector3.Distance((Vector3)penetrationPoint, hit.point);
                        penetrationAmount -= materialToughness.ToughnessAmount;
                        originPosition = hit.point;
                        continue;
                    }
                    else
                    {
                        CmdInstantiateImpactDecal(false, hit.point, hit.normal);
                    }
                }
                else
                {
                    CmdInstantiateImpactDecal(false, hit.point, hit.normal);
                    endPoint = impactPoint.Value + ray.direction * penetrationAmount;
                    penetrationPoint = endPoint;
                }
            }
            else
            {
                endPoint = transform.position + transform.forward * 1000;
                penetrationPoint = null;
                impactPoint = null;
            }

            break;
        }
    }

    int calculateDamage(Vector3 entityPosition)
    {
        Gun gun = playerInventory.EquippedGun;
        float distance = Vector3.Distance(entityPosition, cameraHolder.position);
        switch (gun.Damages.Count)
        {
            case 1:
            case 2 when distance <= gun.Damages[0].MaxDistance:
                BulletDamage = gun.Damages[0].BodyDamage;
                break;
            case 2:
            {
                if (distance >= gun.Damages[1].MinDistance) BulletDamage = gun.Damages[1].BodyDamage;
                break;
            }
            case 3 when distance <= gun.Damages[0].MaxDistance:
                BulletDamage = gun.Damages[0].BodyDamage;
                break;
            case 3 when distance >= gun.Damages[1].MinDistance && distance <= gun.Damages[1].MaxDistance:
                BulletDamage = gun.Damages[1].BodyDamage;
                break;
            case 3:
            {
                if (distance >= gun.Damages[2].MinDistance) BulletDamage = gun.Damages[2].BodyDamage;
                break;
            }
        }
        return BulletDamage;
    }

    [Command]
    void CmdInstantiateImpactDecal(bool canPenetrate, Vector3 position, Vector3 rotation)
    {
        GameObject bulletImpact = Instantiate(canPenetrate ? BulletImpactDecalPenetrable : BulletImpactDecalNotPenetrable);
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

using Mirror;
using UnityEngine;

public enum Item
{
    Primary,
    Secondary,
    Knife,
    Bomb
}
public class PlayerInventoryManager : NetworkBehaviour
{
    public GameObject KnifeHolder;
    public GameObject BombHolder;
    public GameObject PrimaryWeaponHolder;
    public GameObject SecondaryWeaponHolder;
    
    public Item EqupiedItem;
    public Item PreviousEqupiedItem;


    public Gun PrimaryGun;
    public Gun SecondaryGun;

    public GameObject PrimaryGunInstance;
    public GameObject SecondaryGunInstance;

    public GameObject Bomb;

    GameManager gameManager;
    GunManager gunManager;
    Player player;

    bool canPickBomb;
    private void Start()
    {
        player = GetComponent<Player>();
        gunManager = FindObjectOfType<GunManager>();
        gameManager = FindObjectOfType<GameManager>();

        if (!isLocalPlayer) return;

        CmdSwitchItem(Item.Secondary);
        setLayerMask(KnifeHolder.transform.GetChild(0).gameObject, 6);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.Alpha1) && PrimaryGun != null) CmdSwitchItem(Item.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2) && SecondaryGun != null) CmdSwitchItem(Item.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CmdSwitchItem(Item.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && Bomb != null) CmdSwitchItem(Item.Bomb);

        if (Input.GetKeyDown(KeyCode.G) && EqupiedItem == Item.Primary) CmdDropGun((int)PrimaryGunInstance.GetComponent<NetworkIdentity>().netId);
        else if(Input.GetKeyDown(KeyCode.G) && EqupiedItem == Item.Secondary) CmdDropGun((int)SecondaryGunInstance.GetComponent<NetworkIdentity>().netId);
    }
    void setLayerMask(GameObject gameObject, int layerMask)
    {
        foreach (Transform c in gameObject.transform.GetComponentsInChildren<Transform>())
        {
            c.gameObject.layer = layerMask;
        }
    }

    [Command]
    public void CmdSwitchItem(Item item) => RpcSwitchItem(item);

    [ClientRpc]
    void RpcSwitchItem(Item item)
    {
        if (item == EqupiedItem) return;
        PreviousEqupiedItem = EqupiedItem;
        EqupiedItem = item;
        switch (item)
        {
            case Item.Primary:
                PrimaryWeaponHolder.SetActive(true);
                SecondaryWeaponHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Secondary:
                SecondaryWeaponHolder.SetActive(true);
                PrimaryWeaponHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Knife:
                KnifeHolder.SetActive(true);
                PrimaryWeaponHolder.SetActive(false);
                SecondaryWeaponHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Bomb:
                BombHolder.SetActive(true);
                PrimaryWeaponHolder.SetActive(false);
                SecondaryWeaponHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;
        gameManager = FindObjectOfType<GameManager>();
        if (other.gameObject == gameManager.Bomb.transform.GetChild(0).gameObject && player.PlayerTeam == Team.Red) CmdPickBomb();
        if (other.gameObject.TryGetComponent(out GunInstance instance)) if (instance.IsDropped) CmdPickGun();
    }

    [Command]
    void CmdPickBomb() => RpcPickBomb();

    [ClientRpc]
    void RpcPickBomb()
    {
        Bomb = gameManager.Bomb;
        Bomb.transform.SetParent(BombHolder.transform);
        Bomb.transform.localEulerAngles = Vector3.zero;
        Bomb.transform.localPosition = new Vector3(0, 0, .5f);
        Bomb.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Bomb.GetComponent<BoxCollider>().enabled = false;
        if (isLocalPlayer) CmdSwitchItem(EqupiedItem);
    }

    [Command]
    void CmdPickGun() => RpcPickGun();

    [ClientRpc]
    void RpcPickGun()
    {

    }

    [Command]
    public void CmdDropBomb() => RpcDropBomb();


    [ClientRpc]
    void RpcDropBomb()
    {

    }

    [Command]
    public void CmdDropGun(int gunID) => RpcDropGun(gunID);

    [ClientRpc]
    void RpcDropGun(int gunID)
    {
        
    }

    [Command]
    public void CmdGiveGun(int gunID)
    {
        GunType type = gunManager.GetGunByID(gunID).Type;

        GameObject gunInstance = Instantiate(gunManager.GetGunByID(gunID).GunModel);
        if (type == GunType.Primary) PrimaryGunInstance = gunInstance;
        else if (type == GunType.Secondary) SecondaryGunInstance = gunInstance;
        NetworkServer.Spawn(gunInstance);
        RpcGiveGun(gunID, gunInstance.GetComponent<NetworkIdentity>());
    }

    [ClientRpc]
    public void RpcGiveGun(int gunID, NetworkIdentity gunNetworkIdentity)
    {

        GameObject gunInstance = gunNetworkIdentity.gameObject;
        Gun gun = gunManager.GetGunByID(gunID);
        gunInstance.AddComponent<GunInstance>();
        GunInstance spawnedGun = gunInstance.GetComponent<GunInstance>();
        spawnedGun.GunOwner = player;
        if (gun.Type == GunType.Primary)
        {
            PrimaryGun = gun;
            gunInstance.transform.SetParent(PrimaryWeaponHolder.transform);
            if (hasAuthority) CmdSwitchItem(Item.Primary);

        }
        else if (gun.Type == GunType.Secondary)
        {
            SecondaryGun = gun;
            gunInstance.transform.SetParent(SecondaryWeaponHolder.transform);
            if (hasAuthority) CmdSwitchItem(Item.Secondary);
        }
        //REMAKE

        gunInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        gunInstance.transform.localPosition = gun.GunTransform.FirstPersonGunPosition;
        gunInstance.transform.localEulerAngles = gun.GunTransform.FirstPersonGunRotation;
        if(isLocalPlayer) setLayerMask(gunInstance, 6);
    }

    [Command]
    public void DestroyGun(GameObject gun)
    {
        NetworkServer.Destroy(gun);
    }







    /*public Collider coll;
    public GameObject Knife;

    public CurrentWeapon CurrentWeapon;
    public GameObject CurrenRenderWeapon;
    public Gun DefaultGun;

    private Camera camyr;

    public GunInstance? Primary;
    public GunInstance? Secondary;

    private void Start()
    {
        if (!isLocalPlayer) return;

        CurrentWeapon = CurrentWeapon.Secondary;
        Secondary = new GunInstance {Gun = DefaultGun, Ammo = DefaultGun.MaxAmmo, Magazine = DefaultGun.MagazineSize};
        CurrenRenderWeapon = Instantiate(DefaultGun.GunModel, transform.position, Quaternion.identity, transform);

        camyr = Camera.main;
        coll = GetComponent<Collider>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        control();
        ChnageWeapon();
        HandleDropWeapon();
    }
#nullable enable
    public void OnCollisionEnter(Collision collision)
    {
        GunScript? gun = null;
        collision.collider.TryGetComponent(out gun);

        if (gun == null) return;
        switch (gun.Gun.Gun.Type)
        {
            case GunType.Primary:
            {
                if (Primary != null)
                {
                    Primary = gun.Gun;
                    CurrentWeapon = CurrentWeapon.Primary;
                    RenderWeapon();
                }

                break;
            }
            case GunType.Secondary:
            {
                if (Secondary != null)
                {
                    Secondary = gun.Gun;
                    CurrentWeapon = CurrentWeapon.Primary;
                    RenderWeapon();
                }

                break;
            }
        }

        Destroy(collision.collider.transform);
    }
#nullable disable
    public void HandlePickup()
    {
    }

    public void Shoot()
    {
        // todo
    }

    public void Reload()
    {
        // todo
    }

    private void ChnageWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && Primary != null)
        {
            CurrentWeapon = CurrentWeapon.Primary;
            RenderWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && Secondary != null)
        {
            CurrentWeapon = CurrentWeapon.Secondary;
            RenderWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CurrentWeapon = CurrentWeapon.Knife;
            RenderWeapon();
        }
    }


    private void RenderWeapon()
    {
        Destroy(CurrenRenderWeapon);

        switch (CurrentWeapon)
        {
            case CurrentWeapon.Primary:
            {
                var cur = Primary;
                CurrenRenderWeapon = Instantiate(cur?.Gun.GunModel, transform.position, Quaternion.identity, transform);
                break;
            }
            case CurrentWeapon.Secondary:
            {
                var cur = Secondary;
                CurrenRenderWeapon = Instantiate(cur?.Gun.GunModel, transform.position, Quaternion.identity, transform);
                break;
            }
            case CurrentWeapon.Knife:
            {
                CurrenRenderWeapon = Instantiate(Knife, transform.position, Quaternion.identity, transform);
                break;
            }
        }
    }

    public void control()
    {
        // todo
        // switch weapons
        // shoot
        // reload
        // drop weapon
        // pickup weapon
    }

    // Debug.DrawRay(pos.position, pos.TransformDirection(Vector3.forward) * 10, Color.cyan);
    public BulletPath BulletPenetration(Transform pos, int maxpen = 5) // FIXME
    {
        var direction = pos.TransformDirection(Vector3.forward) * 10;

        BulletImpact[] impacts = null;
        RaycastHit? hitus = null;
        var damagemod = 0f;

        var origin = pos;

        for (var i = 0; i <= maxpen; i++)
        {
            var ray = new Ray(origin.position, direction);

            if (Physics.Raycast(ray, out var hit) && hit.collider.GetComponent<EntityBase>())
            {
                hitus = hit;
            }
            else if (hit.collider.GetComponent<Penetration>())
            {
                var pen = hit.collider.GetComponent<Penetration>();

                if (coll.Raycast(new Ray(hit.transform.position + Vector3.one * 20, direction * -1), out var hitb,
                        50.0f))
                {
                    var len = (hitb.transform.position - hit.transform.position).magnitude;
                    origin = hitb.transform; // todo some small offset

                    damagemod += pen.value * len;
                    if (damagemod >= 100)
                    {
                        impacts.Append(new BulletImpact {Location = hit, Penetrated = true});
                        impacts.Append(new BulletImpact {Location = hitb, Penetrated = false});
                        return new BulletPath {Impacts = impacts, hit = hitus, DamageModifier = damagemod};
                    }

                    impacts.Append(new BulletImpact {Location = hit, Penetrated = true});
                    impacts.Append(new BulletImpact {Location = hitb, Penetrated = true});
                }
                else
                {
                    impacts.Append(new BulletImpact {Location = hit, Penetrated = false});
                    return new BulletPath {Impacts = impacts, hit = hitus, DamageModifier = damagemod};
                }
            }
        }

        return new BulletPath {Impacts = impacts, hit = hitus, DamageModifier = damagemod};
    }

    public void Refresh()
    {
        // TODO replanish ammo after respawn
    }

    public void PickupWeapon()
    {
        // TODO
    }

    public void HandleDropWeapon()
    {
        if (Input.GetKeyDown(KeyCode.G)) DropCurrentWeapon();
    }


    public void DropCurrentWeapon()
    {
        switch (CurrentWeapon)
        {
            case CurrentWeapon.Primary:
            {
                var model = Primary?.Gun.GunModel;
                var obj = Instantiate(model, transform.position, Quaternion.identity);
                //var bodyr = obj.AddComponent<Rigidbody>();
                obj.AddComponent<GunScript>().Init(Primary.Value);
                Primary = null;
                //bodyr.AddForce(transform.position.normalized*2*Time.deltaTime);
                if (Secondary != null)
                {
                    CurrentWeapon = CurrentWeapon.Secondary;
                    RenderWeapon();
                }
                else
                {
                    CurrentWeapon = CurrentWeapon.Knife;
                    RenderWeapon();
                }

                break;
            }
            case CurrentWeapon.Secondary:
            {
                var model = Secondary?.Gun.GunModel;
                var obj = Instantiate(model, transform.position, Quaternion.identity);
                //obj.AddComponent<Rigidbody>();
                obj.AddComponent<GunScript>().Init(Secondary.Value);
                Secondary = null;
                //obj.GetComponent<Rigidbody>().AddForce(transform.position.normalized*2*Time.deltaTime);

                if (Primary != null)
                {
                    CurrentWeapon = CurrentWeapon.Secondary;
                    RenderWeapon();
                }
                else
                {
                    CurrentWeapon = CurrentWeapon.Knife;
                    RenderWeapon();
                }

                break;
            }
        }
    }


    public struct BulletImpact
    {
        public bool Penetrated;
        public RaycastHit Location;
    }

    public struct BulletPath
    {
        public BulletImpact[] Impacts;
        public RaycastHit? hit;
        public float? DamageModifier;
    }*/
}
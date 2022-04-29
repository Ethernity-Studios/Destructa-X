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
    public GameObject PrimaryGunHolder;
    public GameObject SecondaryGunHolder;

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

        if (Input.GetKeyDown(KeyCode.G)) dropItem();
    }
    void setLayerMask(GameObject gameObject, int layerMask)
    {
        foreach (Transform c in gameObject.transform.GetComponentsInChildren<Transform>())
        {
            c.gameObject.layer = layerMask;
        }
    }

    void dropItem()
    {
        if(EqupiedItem == Item.Primary && PrimaryGun != null) CmdDropGun(GunType.Primary);
        if(EqupiedItem == Item.Secondary && SecondaryGun != null) CmdDropGun(GunType.Secondary);
        if (EqupiedItem == Item.Bomb && Bomb != null) CmdDropBomb();
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
                PrimaryGunHolder.SetActive(true);
                SecondaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Secondary:
                SecondaryGunHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Knife:
                KnifeHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                SecondaryGunHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Bomb:
                BombHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                SecondaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                break;
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;
        gameManager = FindObjectOfType<GameManager>();
        if (other.gameObject == gameManager.Bomb.transform.GetChild(0).gameObject && player.PlayerTeam == Team.Red) CmdPickBomb();

        if (other.gameObject.TryGetComponent(out GunInstance instance)) if (instance.CanBePicked) CmdPickGun(instance.GetComponent<NetworkIdentity>().netId);
    }*/

    private void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer) return;
        gameManager = FindObjectOfType<GameManager>();
        if (other.gameObject == gameManager.Bomb.transform.GetChild(0).gameObject && other.gameObject.layer != 6 && player.PlayerTeam == Team.Red) CmdPickBomb();

        if (other.gameObject.TryGetComponent(out GunInstance instance)) if (instance.CanBePicked) CmdPickGun(instance.GetComponent<NetworkIdentity>().netId);
    }

    [Command]
    void CmdPickBomb() => RpcPickBomb();

    [ClientRpc]
    void RpcPickBomb()
    {
        Bomb = gameManager.Bomb;
        Bomb.transform.GetChild(0).gameObject.layer = 6;
        //gameManager.Bomb = null;
        Bomb.transform.SetParent(BombHolder.transform);
        Bomb.transform.localEulerAngles = Vector3.zero;
        Bomb.transform.localPosition = new Vector3(0, 0, .5f);
        Bomb.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Bomb.GetComponent<BoxCollider>().enabled = false;
        if (isLocalPlayer) CmdSwitchItem(EqupiedItem);
    }

    [Command]
    public void CmdDropBomb() => RpcDropBomb();


    [ClientRpc]
    void RpcDropBomb()
    {
        Bomb.transform.localPosition = new Vector3(0, .6f, .5f);
        Bomb.transform.SetParent(gameManager.gameObject.transform);
        Invoke("setBombLayer", .5f);
        Rigidbody rb = Bomb.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        Bomb.GetComponent<BoxCollider>().enabled = true;
        rb.AddForce(transform.GetChild(0).transform.TransformDirection(new Vector3(0, 0, 400)));
        //gameManager.Bomb = Bomb;
        Bomb = null;
    }

    void setBombLayer() => gameManager.Bomb.transform.GetChild(0).gameObject.layer = 0;

    [Command]
    void CmdPickGun(uint GunID) => RpcPickGun(NetworkServer.spawned[GunID].gameObject);

    [ClientRpc]
    void RpcPickGun(GameObject gunInstance)
    {
        GunInstance instance = gunInstance.GetComponent<GunInstance>();
        if (!instance.CanBePicked) return;

        instance.CanBePicked = false;
        instance.IsDropped = false;
        Gun gun = instance.Gun;
        if(gun.Type == GunType.Primary)
        {
            if (PrimaryGun != null) return;
            gunInstance.transform.SetParent(PrimaryGunHolder.transform);
            PrimaryGunInstance = gunInstance;
            PrimaryGun = gun;
        }
        else if(gun.Type == GunType.Secondary)
        {
            if(SecondaryGun != null) return;
            gunInstance.transform.SetParent(SecondaryGunHolder.transform);
            SecondaryGunInstance = gunInstance;
            SecondaryGun = gun;
        }
        setGunTransform(gunInstance, gun);
        Rigidbody rb = gunInstance.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        gunInstance.gameObject.transform.GetChild(0).gameObject.layer = 6;
        setLayerMask(gunInstance.transform.GetChild(1).gameObject, 6);

    }

    [Command]
    public void CmdDropGun(GunType gunType) => RpcDropGun(gunType);

    [ClientRpc]
    void RpcDropGun(GunType gunType)
    {
        GameObject gunInstance = null;
        if (gunType == GunType.Primary)
        {
            gunInstance = SecondaryGunInstance;
            gunInstance.GetComponent<Rigidbody>();
            PrimaryGunInstance = null;
            PrimaryGun = null;
        }
        else if(gunType == GunType.Secondary)
        {
            gunInstance = SecondaryGunInstance;
            gunInstance.GetComponent<Rigidbody>();
            SecondaryGunInstance = null;
            SecondaryGun = null;
        }
        gunInstance.transform.localPosition = new Vector3(0, .6f, .5f);
        gunInstance.transform.localEulerAngles += new Vector3(30,0,0);
        gunInstance.transform.SetParent(gameManager.GunHolder.transform);
        gunInstance.transform.GetChild(0).GetComponent<BoxCollider>().enabled = true;
        GunInstance instance = gunInstance.GetComponent<GunInstance>();
        instance.CanBeSelled = false;
        instance.IsDropped = true;
        instance.Invoke("SetPickStatus", .5f);
        gunInstance.gameObject.transform.GetChild(0).gameObject.layer = 8;
        setLayerMask(gunInstance.transform.GetChild(1).gameObject, 8);
        Rigidbody rb = gunInstance.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        //rb.AddForce(Camera.main.transform.TransformDirection(new Vector3(0, 0, 400)));
        rb.AddForce(transform.GetChild(0).transform.TransformDirection(new Vector3(0, 0, 400))); // Camera
    }

    [Command]
    public void CmdGiveGun(int gunID)
    {
        GameObject gunInstance = Instantiate(gunManager.GetGunByID(gunID).GunModel);
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
        spawnedGun.CanBeSelled = true;
        spawnedGun.Gun = gun;
        GunType type = gunManager.GetGunByID(gunID).Type;
        if (type == GunType.Primary) PrimaryGunInstance = gunInstance;
        else if (type == GunType.Secondary) SecondaryGunInstance = gunInstance;
        if (gun.Type == GunType.Primary)
        {
            PrimaryGun = gun;
            gunInstance.transform.SetParent(PrimaryGunHolder.transform);
            if (hasAuthority) CmdSwitchItem(Item.Primary);

        }
        else if (gun.Type == GunType.Secondary)
        {
            SecondaryGun = gun;
            gunInstance.transform.SetParent(SecondaryGunHolder.transform);
            if (hasAuthority) CmdSwitchItem(Item.Secondary);
        }
        //REMAKE
        setGunTransform(gunInstance, gun);

        if (isLocalPlayer) setLayerMask(gunInstance, 6);
    }

    void setGunTransform(GameObject gunInstance, Gun gun)
    {
        gunInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        gunInstance.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
        gunInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        gunInstance.transform.localPosition = gun.GunTransform.FirstPersonGunPosition;
        gunInstance.transform.localEulerAngles = gun.GunTransform.FirstPersonGunRotation;
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
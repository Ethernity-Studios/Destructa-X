using Mirror;
using UnityEngine;

public struct GunInstance
{
    public Gun Gun;
    public int Ammo;
    public int Magazine;
}

public enum Item
{
    Primary,
    Secondary,
    Knife,
    Bomb
}
public class PlayerInventoryManager : NetworkBehaviour
{
    public Item EqupiedItem;
    public Item PreviousEqupiedItem;

    public GameObject KnifeHolder;
    public GameObject BombHolder;
    public GameObject PrimaryWeaponHolder;
    public GameObject SecondaryWeaponHolder;

    public Gun PrimaryGun;
    public Gun SecondaryGun;
    public GameObject Bomb;

    GameManager gameManager;

    [SerializeField] Gun gun_Classic;   
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (!isLocalPlayer) return;
        Invoke("giveDefaultGun",.3f);
        CmdSwitchItem(Item.Secondary);
        PreviousEqupiedItem = EqupiedItem;
        setLayerMask(KnifeHolder, 6);
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.Alpha1) && PrimaryGun != null) CmdSwitchItem(Item.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2) && SecondaryGun != null) CmdSwitchItem(Item.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CmdSwitchItem(Item.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && Bomb != null) CmdSwitchItem(Item.Bomb);
    }
    void giveDefaultGun()
    {
        GiveGun(gun_Classic);
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
        switch (item)
        {
            case Item.Primary:
                PreviousEqupiedItem = EqupiedItem;
                PrimaryWeaponHolder.SetActive(true);
                EqupiedItem = Item.Primary;
                SecondaryWeaponHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Secondary:
                PreviousEqupiedItem = EqupiedItem;
                SecondaryWeaponHolder.SetActive(true);
                EqupiedItem = Item.Secondary;
                PrimaryWeaponHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Knife:
                PreviousEqupiedItem = EqupiedItem;
                KnifeHolder.SetActive(true);
                EqupiedItem = Item.Knife;
                PrimaryWeaponHolder.SetActive(false);
                SecondaryWeaponHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Bomb:
                PreviousEqupiedItem = EqupiedItem;
                BombHolder.SetActive(true);
                EqupiedItem = Item.Bomb;
                PrimaryWeaponHolder.SetActive(false);
                SecondaryWeaponHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;
        /*if (other.gameObject == gameManager.Bomb.transform.GetChild(0).gameObject) CmdPickBomb();
        else if (other.gameObject.transform.CompareTag("PickableGun")) CmdPickGun();*/
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
    public void CmdDropGun() => RpcDropGun();

    [ClientRpc]
    void RpcDropGun()
    {

    }

    Gun gun;
    public void GiveGun(Gun gun)
    {
        Debug.Log("Give Gun " + gun);
        this.gun = gun;
        CmdGiveGun();
    }
    GameObject gunInstance;
    [Command]
    public void CmdGiveGun()
    {
        if (gun == null) return;
        Debug.Log("CmdGiveGun " + gun + gun.GunModel + "< model");
        gunInstance = Instantiate(gun.GunModel);
        NetworkServer.Spawn(gunInstance);
        RpcGiveGun();
    }

    [ClientRpc]
    void RpcGiveGun()
    {
        if (gun == null) return;
        if (gun.Type == GunType.Primary)
        {
            PrimaryGun = gun;
            gunInstance.transform.SetParent(PrimaryWeaponHolder.transform);
            CmdSwitchItem(Item.Primary);
        }
        else if (gun.Type == GunType.Secondary)
        {
            SecondaryGun = gun;
            gunInstance.transform.SetParent(SecondaryWeaponHolder.transform);
        }
        setLayerMask(gunInstance, 6);
        gunInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        gunInstance.transform.localPosition = gun.GunTransform.FirstPersonGunPosition;
        gunInstance.transform.localEulerAngles = gun.GunTransform.FirstPersonGunRotation;
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
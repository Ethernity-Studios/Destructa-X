using Mirror;
using System;
using System.Collections.Generic;
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

    [SerializeField] GameObject knifeHolder;
    [SerializeField] GameObject bombHolder;
    [SerializeField] GameObject primaryWeaponHolder;
    [SerializeField] GameObject secondaryWeaponHolder;

    public Gun PrimaryGun;
    public Gun SecondaryGun;

    public GameObject Bomb;
    private void Start()
    {
        if (!isLocalPlayer) return;
        EqupiedItem = Item.Secondary;
        PreviousEqupiedItem = EqupiedItem;
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.Alpha1) && PrimaryGun != null) CmdSwitchItem(Item.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2) && SecondaryGun != null) CmdSwitchItem(Item.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CmdSwitchItem(Item.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && Bomb != null) CmdSwitchItem(Item.Bomb);
    }
    [Command]
    public void CmdSwitchItem(Item item)
    {
        RpcSwitchItem(item);
    }

    [ClientRpc]
    void RpcSwitchItem(Item item)
    {
        switch (item)
        {
            case Item.Primary:
                primaryWeaponHolder.SetActive(true);
                EqupiedItem = Item.Primary;
                secondaryWeaponHolder.SetActive(false);
                knifeHolder.SetActive(false);
                bombHolder.SetActive(false);
                break;
            case Item.Secondary:
                secondaryWeaponHolder.SetActive(true);
                EqupiedItem = Item.Secondary;
                primaryWeaponHolder.SetActive(false);
                knifeHolder.SetActive(false);
                bombHolder.SetActive(false);
                break;
            case Item.Knife:
                knifeHolder.SetActive(true);
                EqupiedItem = Item.Knife;
                primaryWeaponHolder.SetActive(false);
                secondaryWeaponHolder.SetActive(false);
                bombHolder.SetActive(false);
                break;
            case Item.Bomb:
                bombHolder.SetActive(true);
                EqupiedItem = Item.Bomb;
                primaryWeaponHolder.SetActive(false);
                secondaryWeaponHolder.SetActive(false);
                knifeHolder.SetActive(false);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;
        if(GetComponent<PlayerManager>().PlayerTeam == Team.Red && Bomb == null && other.transform.name == "BombTrigger") CmdPickBomb();
    }

    [Command(requiresAuthority = false)]
    void CmdPickBomb()
    {
        RpcPickBomb();
    }

    [ClientRpc]
    void RpcPickBomb()
    {
        GameObject bomb = GameObject.Find("Bomb");
        Bomb = bomb;
        bomb.transform.SetParent(bombHolder.transform);
        bomb.transform.localPosition = new Vector3(0, 0, .5f);
        bomb.transform.localEulerAngles = new Vector3(0, 0, 0);
        bomb.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        bomb.GetComponent<BoxCollider>().enabled = false;
        CmdSwitchItem(EqupiedItem);
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
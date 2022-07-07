using Mirror;
using System.Collections;
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

    public Item EqupiedItem = Item.Secondary;
    public Item PreviousEqupiedItem = Item.Knife;
    public Gun EqupiedGun;
    public GameObject EqupiedGunInstance;

    public Gun PrimaryGun;
    public Gun SecondaryGun;

    public GameObject PrimaryGunInstance;
    public GameObject SecondaryGunInstance;

    public GameObject Bomb;

    GameManager gameManager;
    GunManager gunManager;
    PlayerShootingManager playerShootingManager;
    Player player;

    public bool gunEqupied = true;

    private void Start()
    {
        player = GetComponent<Player>();
        gunManager = FindObjectOfType<GunManager>();
        gameManager = FindObjectOfType<GameManager>();
        playerShootingManager = GetComponent<PlayerShootingManager>();

        if (!isLocalPlayer) return;
        setLayerMask(KnifeHolder.transform.GetChild(0).gameObject, 6);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.Alpha1) && PrimaryGun != null && EqupiedItem != Item.Primary) SwitchItem(Item.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2) && SecondaryGun != null && EqupiedItem != Item.Secondary) SwitchItem(Item.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3) && EqupiedItem != Item.Knife) SwitchItem(Item.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && Bomb != null && EqupiedItem != Item.Bomb) SwitchItem(Item.Bomb);

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
        if (EqupiedItem == Item.Primary && PrimaryGun != null) CmdDropGun(GunType.Primary);
        if (EqupiedItem == Item.Secondary && SecondaryGun != null) CmdDropGun(GunType.Secondary);
        if (EqupiedItem == Item.Bomb && Bomb != null) CmdDropBomb();
    }

    void SwitchItem(Item item)
    {
        if (player.PlayerState == PlayerState.Planting || player.PlayerState == PlayerState.Defusing || player.PlayerState == PlayerState.Dead) return;
        StopCoroutine(toggleEqupiedGun());
        gunEqupied = false;
        playerShootingManager.StopAllCoroutines();
        playerShootingManager.Reloading = false;
        CmdSwitchItem(item);
    }

    IEnumerator toggleEqupiedGun()
    {
        yield return new WaitForSeconds(EqupiedGun.EquipTime);
        gunEqupied = true;
    }

    [Command(requiresAuthority = false)]
    public void CmdSwitchItem(Item item) => RpcSwitchItem(item);

    [ClientRpc]
    void RpcSwitchItem(Item item)
    {
        PreviousEqupiedItem = EqupiedItem;
        EqupiedItem = item;
        Debug.Log("Switching item: " + item);
        switch (item)
        {
            case Item.Primary:
                EqupiedGunInstance = PrimaryGunInstance;
                playerShootingManager.gunInstance = EqupiedGunInstance.GetComponent<GunInstance>();
                EqupiedGun = PrimaryGun;
                PrimaryGunHolder.SetActive(true);
                SecondaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                StartCoroutine(toggleEqupiedGun());
                playerShootingManager.UpdateUIAmmo();
                break;
            case Item.Secondary:
                EqupiedGunInstance = SecondaryGunInstance;
                playerShootingManager.gunInstance = EqupiedGunInstance.GetComponent<GunInstance>();
                EqupiedGun = SecondaryGun;
                SecondaryGunHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                StartCoroutine(toggleEqupiedGun());
                playerShootingManager.UpdateUIAmmo();
                break;
            case Item.Knife:
                EqupiedGunInstance = null;
                EqupiedGun = null;
                KnifeHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                SecondaryGunHolder.SetActive(false);
                BombHolder.SetActive(false);
                break;
            case Item.Bomb:
                EqupiedGunInstance = null;
                EqupiedGun = null;
                BombHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                SecondaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        gameManager = FindObjectOfType<GameManager>();
        if (!isLocalPlayer) return;
        if (gameManager.Bomb == null) return;
        if (other.gameObject == gameManager.Bomb.transform.GetChild(0).gameObject && other.gameObject.layer != 6 && player.PlayerTeam == Team.Red) CmdPickBomb();

        if (other.gameObject.TryGetComponent(out GunInstance instance)) if (instance.CanBePicked && instance.IsDropped && !player.IsDead) CmdPickGun(instance.GetComponent<NetworkIdentity>().netId);
    }

    [Command]
    void CmdPickBomb() => RpcPickBomb();

    [ClientRpc]
    void RpcPickBomb()
    {
        Debug.Log("RPc pick bomb");
        Bomb = gameManager.Bomb;
        Bomb.transform.GetChild(0).gameObject.layer = 6;
        Bomb.transform.SetParent(BombHolder.transform);
        Bomb.transform.localEulerAngles = Vector3.zero;
        Bomb.transform.localPosition = new Vector3(0, -.5f, .7f);
        Bomb.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Bomb.GetComponent<BoxCollider>().enabled = false;
    }

    [Command]
    public void CmdDropBomb() => RpcDropBomb();


    [ClientRpc]
    void RpcDropBomb()
    {
        if (isLocalPlayer) CmdSwitchItem(PreviousEqupiedItem);
        Bomb.transform.localPosition = new Vector3(0, .6f, .5f);
        Bomb.transform.SetParent(gameManager.gameObject.transform);
        Invoke("setBombLayer", .5f);
        Rigidbody rb = Bomb.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        Bomb.GetComponent<BoxCollider>().enabled = true;
        rb.AddForce(transform.GetChild(0).transform.TransformDirection(new Vector3(0, 0, 400)));
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

        Gun gun = instance.Gun;

        if (gun.Type == GunType.Primary)
        {
            if (PrimaryGun != null) return;
            instance.StopAllCoroutines();
            gunInstance.transform.SetParent(PrimaryGunHolder.transform);
            PrimaryGunInstance = gunInstance;
            PrimaryGun = gun;
        }
        else if (gun.Type == GunType.Secondary)
        {
            if (SecondaryGun != null) return;
            instance.StopAllCoroutines();
            gunInstance.transform.SetParent(SecondaryGunHolder.transform);
            SecondaryGunInstance = gunInstance;
            SecondaryGun = gun;
        }
        instance.IsDropped = false;
        instance.CanBePicked = false;
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
        playerShootingManager.StopAllCoroutines();
        playerShootingManager.Reloading = false;
        GameObject gunInstance = null;
        if (gunType == GunType.Primary)
        {
            if(SecondaryGun != null) SwitchItem(Item.Secondary);
            else SwitchItem(Item.Knife);

            gunInstance = PrimaryGunInstance;
            gunInstance.GetComponent<Rigidbody>();
            PrimaryGunInstance = null;
            PrimaryGun = null;
        }
        else if (gunType == GunType.Secondary)
        {
            if (PrimaryGun != null) SwitchItem(Item.Primary);
            else SwitchItem(Item.Knife);

            gunInstance = SecondaryGunInstance;
            gunInstance.GetComponent<Rigidbody>();
            SecondaryGunInstance = null;
            SecondaryGun = null;
        }

        gunInstance.transform.localPosition = new Vector3(0, .5f, .5f);
        gunInstance.transform.localEulerAngles += new Vector3(30, 0, 0);
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
        rb.AddForce(transform.GetChild(0).transform.TransformDirection(new Vector3(0, 0, 400))); // Camera
    }

    [Command(requiresAuthority = false)]
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
        spawnedGun.Ammo = gun.Ammo;
        spawnedGun.Magazine = gun.MagazineAmmo;
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
            if (PrimaryGun == null && hasAuthority) CmdSwitchItem(Item.Secondary);
        }
        setGunTransform(gunInstance, gun);

        setLayerMask(gunInstance, 6);
    }

    void setGunTransform(GameObject gunInstance, Gun gun)
    {
        gunInstance.transform.localPosition = Vector3.zero;
        gunInstance.transform.localEulerAngles = Vector3.zero;
        gunInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        gunInstance.GetComponent<Rigidbody>().velocity = Vector3.zero;
        gunInstance.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
        gunInstance.transform.localScale = new Vector3(1f, 1f, 1.5f);
        gunInstance.transform.localPosition = gun.GunTransform.FirstPersonGunPosition;
        gunInstance.transform.localEulerAngles = gun.GunTransform.FirstPersonGunRotation;
    }

    [Command]
    public void CmdSellGun(uint gunID, Gun gun)
    {
        if (gun.Type == GunType.Primary)
        {
            PrimaryGun = null;
            PrimaryGunInstance = null;
        }
        else if (gun.Type == GunType.Secondary)
        {
            SecondaryGun = null;
            SecondaryGunInstance = null;
        }
        CmdSwitchItem(PreviousEqupiedItem);
        NetworkServer.Destroy(NetworkServer.spawned[gunID].gameObject);
    }

    [Command]
    public void CmdDestroyGun(GunType type)
    {
        if(type == GunType.Primary && PrimaryGun != null)
        {
            Debug.Log("Destroying primary gun");
            PrimaryGun = null;
            PrimaryGunHolder.SetActive(true);
            NetworkServer.Destroy(NetworkServer.spawned[PrimaryGunInstance.GetComponent<NetworkIdentity>().netId].gameObject);
            PrimaryGunHolder.SetActive(false);
            CmdSwitchItem(Item.Knife);
        }
        else if(type == GunType.Secondary && SecondaryGun != null)
        {
            Debug.Log("Destroying secondary gun");
            SecondaryGun = null;
            SecondaryGunHolder.SetActive(false);
            NetworkServer.Destroy(NetworkServer.spawned[SecondaryGunInstance.GetComponent<NetworkIdentity>().netId].gameObject);
            SecondaryGunHolder.SetActive(true);
            CmdSwitchItem(Item.Knife);
        }
    }
}
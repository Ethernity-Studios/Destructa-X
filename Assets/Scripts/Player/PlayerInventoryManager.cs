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

    [SyncVar]
    public bool GunEqupied = true;

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
        if (Input.GetKeyDown(KeyCode.Alpha1) && PrimaryGun != null && EqupiedItem != Item.Primary) switchItem(Item.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2) && SecondaryGun != null && EqupiedItem != Item.Secondary) switchItem(Item.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3) && EqupiedItem != Item.Knife) switchItem(Item.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && Bomb != null && EqupiedItem != Item.Bomb) switchItem(Item.Bomb);

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
        if (player.IsDead) return;
        if (EqupiedItem == Item.Primary && PrimaryGun != null) CmdDropGun(GunType.Primary);
        if (EqupiedItem == Item.Secondary && SecondaryGun != null) CmdDropGun(GunType.Secondary);
        if (EqupiedItem == Item.Bomb && Bomb != null) CmdDropBomb();
    }

    void switchItem(Item item)
    {
        if (player.PlayerState == PlayerState.Planting || player.PlayerState == PlayerState.Defusing || player.PlayerState == PlayerState.Dead) return;
        StopCoroutine("toggleEqupiedGun");
        if(isLocalPlayer) CmdToggleEqupiedGun(false);
        playerShootingManager.StopAllCoroutines();
        playerShootingManager.Reloading = false;
        CmdSwitchItem(item);
    }

    IEnumerator toggleEqupiedGun()
    {
        yield return new WaitForSeconds(EqupiedGun.EquipTime);
        CmdToggleEqupiedGun(true);
    }
    [Command]
    void CmdToggleEqupiedGun(bool state) => GunEqupied = state;

    [Command(requiresAuthority = false)]
    public void CmdSwitchItem(Item item) 
    {
        RpcSwitchItem(item);
    } 

    [ClientRpc]
    void RpcSwitchItem(Item item)
    {
        PreviousEqupiedItem = EqupiedItem;
        EqupiedItem = item;
        playerShootingManager.CanShoot = true;
        playerShootingManager.Reloading = false;
        switch (item)
        {
            case Item.Primary:
                if (PrimaryGunInstance == null) return;
                EqupiedGunInstance = PrimaryGunInstance;
                playerShootingManager.GunInstance = EqupiedGunInstance.GetComponent<GunInstance>();
                EqupiedGun = PrimaryGun;
                PrimaryGunHolder.SetActive(true);
                SecondaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);

                if (!isLocalPlayer) return;
                StartCoroutine(toggleEqupiedGun());
                playerShootingManager.UpdateUIAmmo();
                break;
            case Item.Secondary:
                if(SecondaryGunInstance == null) return;
                EqupiedGunInstance = SecondaryGunInstance;
                playerShootingManager.GunInstance = EqupiedGunInstance.GetComponent<GunInstance>();
                EqupiedGun = SecondaryGun;
                SecondaryGunHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);

                if (!isLocalPlayer) return;
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
                if (Bomb == null) return;
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
        if (other.gameObject == gameManager.Bomb.transform.GetChild(0).gameObject && other.gameObject.layer != 6 && player.PlayerTeam == Team.Red && !player.IsDead) CmdPickBomb();

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
    void CmdPickGun(uint GunID) 
    {
        Debug.Log("CmdPickGun id: " + GunID);
        Debug.Log("CmdPickGun gameobject: " + NetworkServer.spawned[GunID].gameObject);
        RpcPickGun(NetworkServer.spawned[GunID].gameObject);
    }

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
        if(isLocalPlayer)
        setLayerMask(gunInstance, 6);
    }

    [Command]
    public void CmdDropGun(GunType gunType) => RpcDropGun(gunType);

    [ClientRpc]
    void RpcDropGun(GunType gunType)
    {
        GunEqupied = false;
        playerShootingManager.StopAllCoroutines();
        playerShootingManager.Reloading = false;
        playerShootingManager.CanShoot = true;
        GameObject gunInstance = null;
        if (gunType == GunType.Primary)
        {
            if (PrimaryGun == null || PrimaryGunInstance == null) return;
            if(SecondaryGun != null) switchItem(Item.Secondary);
            else switchItem(Item.Knife);

            gunInstance = PrimaryGunInstance;
            PrimaryGunInstance = null;
            PrimaryGun = null;
        }
        else if (gunType == GunType.Secondary)
        {
            if (SecondaryGun == null || SecondaryGunInstance == null) return;
            if (PrimaryGun != null) switchItem(Item.Primary);
            else switchItem(Item.Knife);

            gunInstance = SecondaryGunInstance;
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
            if (isLocalPlayer) CmdSwitchItem(Item.Primary);
        }
        else if (gun.Type == GunType.Secondary)
        {
            SecondaryGun = gun;
            gunInstance.transform.SetParent(SecondaryGunHolder.transform);
            if (PrimaryGun == null && isLocalPlayer) CmdSwitchItem(Item.Secondary);
        }
        setGunTransform(gunInstance, gun);
        if(isLocalPlayer)
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
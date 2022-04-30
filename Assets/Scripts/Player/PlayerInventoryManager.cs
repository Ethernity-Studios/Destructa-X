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

    public Item EqupiedItem = Item.Secondary;
    public Item PreviousEqupiedItem = Item.Knife;


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
        CmdSwitchItem(Item.Knife);
        setLayerMask(KnifeHolder.transform.GetChild(0).gameObject, 6);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.Alpha1) && PrimaryGun != null && EqupiedItem != Item.Primary) CmdSwitchItem(Item.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2) && SecondaryGun != null && EqupiedItem != Item.Secondary) CmdSwitchItem(Item.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3) && EqupiedItem != Item.Knife) CmdSwitchItem(Item.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && Bomb != null && EqupiedItem != Item.Bomb) CmdSwitchItem(Item.Bomb);

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
        if(EqupiedItem == Item.Bomb && Bomb != null) CmdDropBomb();
    }

    [Command]
    public void CmdSwitchItem(Item item) => RpcSwitchItem(item);

    [ClientRpc]
    void RpcSwitchItem(Item item)
    { 
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

    private void OnTriggerStay(Collider other)
    {
        gameManager = FindObjectOfType<GameManager>();
        if (!isLocalPlayer) return;
        if (other.gameObject == gameManager.Bomb.transform.GetChild(0).gameObject && other.gameObject.layer != 6 && player.PlayerTeam == Team.Red) CmdPickBomb();

        if (other.gameObject.TryGetComponent(out GunInstance instance)) if (instance.CanBePicked && instance.IsDropped) CmdPickGun(instance.GetComponent<NetworkIdentity>().netId);
    }

    [Command]
    void CmdPickBomb() => RpcPickBomb();

    [ClientRpc]
    void RpcPickBomb()
    {
        Bomb = gameManager.Bomb;
        Bomb.transform.GetChild(0).gameObject.layer = 6;
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
        CmdSwitchItem(PreviousEqupiedItem);
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
        else if(gun.Type == GunType.Secondary)
        {
            if(SecondaryGun != null) return;
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
        GameObject gunInstance = null;
        if (gunType == GunType.Primary)
        {
            gunInstance = PrimaryGunInstance;
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
        CmdSwitchItem(PreviousEqupiedItem);
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
            CmdSwitchItem(Item.Primary);
        }
        else if (gun.Type == GunType.Secondary)
        {
            SecondaryGun = gun;
            gunInstance.transform.SetParent(SecondaryGunHolder.transform);
            if (PrimaryGun == null) CmdSwitchItem(Item.Secondary);
        }
        setGunTransform(gunInstance, gun);

        if (isLocalPlayer) setLayerMask(gunInstance, 6);
    }

    void setGunTransform(GameObject gunInstance, Gun gun)
    {
        gunInstance.transform.localPosition = Vector3.zero;
        gunInstance.transform.localEulerAngles = Vector3.zero;
        gunInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        gunInstance.GetComponent<Rigidbody>().velocity = Vector3.zero;
        gunInstance.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
        gunInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
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
}
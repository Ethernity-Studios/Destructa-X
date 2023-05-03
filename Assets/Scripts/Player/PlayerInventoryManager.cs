using Mirror;
using System.Collections;
using System.Linq;
using player;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Item
{
    Primary,
    Secondary,
    Knife,
    Bomb,
    None
}

public class PlayerInventoryManager : NetworkBehaviour
{
    public GameObject KnifeHolder;
    public GameObject BombHolder;
    public GameObject PrimaryGunHolder;
    public GameObject SecondaryGunHolder;

    public Item EquippedItem = Item.Secondary;
    public Item PreviousEquippedItem = Item.Knife;
    public Gun EquippedGun;
    public GameObject EquippedGunInstance;

    public Gun PrimaryGun;
    public Gun SecondaryGun;

    public GameObject PrimaryGunInstance;
    public GameObject SecondaryGunInstance;

    public Gun RequestedGun;

    [SyncVar] public GameObject Bomb;

    GameManager gameManager;
    GunManager gunManager;
    PlayerShootingManager playerShootingManager;
    private UIManager uiManager;
    Player player;
    private PlayerUI playerUI;

    [SyncVar] public bool GunEquipped = true;

    private PlayerInput playerInput;
    
    [SerializeField] private Transform RightHandHint;
    [SerializeField] private Transform LeftHandTarget;
    [SerializeField] private Transform RightHandTarget;
    [SerializeField] private Transform LeftHandHint;

    [SerializeField] private Vector3 KnifeLeftHandTargetPosition;
    [SerializeField] private Vector3 KnifeLeftHandTargetRotation;
    [SerializeField] private Vector3 KnifeLeftHandHintPosition;
    [SerializeField] private Vector3 KnifeRightHandTargetPosition;
    [SerializeField] private Vector3 KnifeRightHandTargetRotation;
    [SerializeField] private Vector3 KnifeRightHandHintPositon;

    private void Awake()
    {
        playerInput = new PlayerInput();

        playerInput.PlayerInventory.Drop.performed += DropItem;
        playerInput.PlayerInventory.SwitchPrimaryItem.performed += switchPrimaryGun;
        playerInput.PlayerInventory.SwitchSecondaryItem.performed += switchSecondaryGun;
        playerInput.PlayerInventory.SwitchMelee.performed += switchKnife;
        playerInput.PlayerInventory.SwitchBomb.performed += switchBomb;
    }

    private void OnEnable() => playerInput.PlayerInventory.Enable();

    private void OnDisable() => playerInput.PlayerInventory.Disable();

    private void Start()
    {
        player = GetComponent<Player>();
        gunManager = FindObjectOfType<GunManager>();
        playerShootingManager = GetComponent<PlayerShootingManager>();
        gameManager = FindObjectOfType<GameManager>();
        uiManager = gameManager.UIManager;
        playerUI = player.GetComponent<PlayerUI>();

        if (!isLocalPlayer) return;
        setLayerMask(KnifeHolder.transform.GetChild(0).gameObject, 6);
    }

    void setLayerMask(GameObject gameObject, int layerMask)
    {
        foreach (Transform c in gameObject.transform.GetComponentsInChildren<Transform>())
        {
            c.gameObject.layer = layerMask;
        }
    }

    public void DropItem(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (player.IsDead) return;
        if (EquippedItem == Item.Primary && PrimaryGun != null) CmdDropGun(GunType.Primary);
        if (EquippedItem == Item.Secondary && SecondaryGun != null) CmdDropGun(GunType.Secondary);
        if (EquippedItem == Item.Bomb && Bomb != null) CmdDropBomb();
    }

    void switchPrimaryGun(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (PrimaryGun != null && EquippedItem != Item.Primary) switchItem(Item.Primary);
    }

    void switchSecondaryGun(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (SecondaryGun != null && EquippedItem != Item.Secondary) switchItem(Item.Secondary);
    }

    void switchKnife(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (EquippedItem != Item.Knife) switchItem(Item.Knife);
    }

    void switchBomb(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (Bomb != null && EquippedItem != Item.Bomb) switchItem(Item.Bomb);
    }


    void switchItem(Item item)
    {
        if (!isLocalPlayer) return;
        if (player.PlayerState is PlayerState.Planting or PlayerState.Defusing or PlayerState.Dead) return;
        StopAllCoroutines();
        CmdToggleEquippedGun(false);
        playerShootingManager.StopAllCoroutines();
        playerShootingManager.Reloading = false;
        CmdSwitchItem(item);
    }

    IEnumerator toggleEquippedGun()
    {
        yield return new WaitForSeconds(EquippedGun.EquipTime);
        CmdToggleEquippedGun(true);
    }

    [Command]
    void CmdToggleEquippedGun(bool state) => GunEquipped = state;

    [Command(requiresAuthority = false)]
    public void CmdSwitchItem(Item item)
    {
        RpcSwitchItem(item);
    }

    [ClientRpc]
    public void RpcSwitchItem(Item item)
    {
        if (EquippedItem != Item.None)
        {
            PreviousEquippedItem = EquippedItem;
        }
        EquippedItem = item;
        playerShootingManager = GetComponent<PlayerShootingManager>();
        playerShootingManager.StopAllCoroutines();
        playerShootingManager.CanShoot = true;
        playerShootingManager.Reloading = false;
        switch (item)
        {
            case Item.Primary:
                if (PrimaryGunInstance == null) return;
                EquippedGunInstance = PrimaryGunInstance;
                playerShootingManager.GunInstance = EquippedGunInstance.GetComponent<GunInstance>();
                EquippedGun = PrimaryGun;
                PrimaryGunHolder.SetActive(true);
                SecondaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                playerUI.ToggleAmmoUI(true);
                setHandTransform(EquippedGun);

                if (!isLocalPlayer) return;
                StartCoroutine(toggleEquippedGun());
                playerShootingManager.UpdateUIAmmo();
                break;
            case Item.Secondary:
                if (SecondaryGunInstance == null) return;
                EquippedGunInstance = SecondaryGunInstance;
                playerShootingManager.GunInstance = EquippedGunInstance.GetComponent<GunInstance>();
                EquippedGun = SecondaryGun;
                SecondaryGunHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                BombHolder.SetActive(false);
                playerUI.ToggleAmmoUI(true);
                setHandTransform(EquippedGun);

                if (!isLocalPlayer) return;
                StartCoroutine(toggleEquippedGun());
                playerShootingManager.UpdateUIAmmo();
                break;
            case Item.Knife:
                EquippedGunInstance = null;
                EquippedGun = null;
                KnifeHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                SecondaryGunHolder.SetActive(false);
                BombHolder.SetActive(false);
                playerUI.ToggleAmmoUI(false);
                setKnifeHandTransform();
                break;
            case Item.Bomb:
                if (Bomb == null) return;
                EquippedGunInstance = null;
                EquippedGun = null;
                BombHolder.SetActive(true);
                PrimaryGunHolder.SetActive(false);
                SecondaryGunHolder.SetActive(false);
                KnifeHolder.SetActive(false);
                playerUI.ToggleAmmoUI(false);
                break;
        }

    }



    private void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer) return;
        
        if (gameManager.Bomb == null) return;
        if (other.gameObject == gameManager.Bomb.transform.GetChild(0).gameObject && other.gameObject.layer == 8 &&
            player.PlayerTeam == Team.Red && !player.IsDead) CmdPickBomb();
        
        if (!other.gameObject.TryGetComponent(out GunInstance instance)) return;
        if (instance.CanBePicked && instance.IsDropped && !player.IsDead)
            CmdPickGun(instance.GetComponent<NetworkIdentity>().netId);
    }


    [Command]
    void CmdPickBomb()
    {
        RpcPickBomb();
        foreach (var p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            if(p.PlayerTeam == player.PlayerTeam) playerUI.RpcToggleHeaderBomb(p.netIdentity.connectionToClient, true);
        }
    }

    [ClientRpc]
    void RpcPickBomb()
    {
        Bomb = gameManager.Bomb;
        Bomb.transform.GetChild(0).gameObject.layer = 6;
        if (isLocalPlayer)
        {
            setLayerMask(Bomb, 6);
            uiManager.Bomb.SetActive(true);
        }
        Bomb.transform.SetParent(BombHolder.transform);
        Bomb.transform.localEulerAngles = Vector3.zero;
        Bomb.transform.localPosition = new Vector3(0, -.5f, .7f);
        Bomb.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Bomb.GetComponent<BoxCollider>().enabled = false;
    }

    [Command]
    public void CmdDropBomb()
    {
        RpcDropBomb();
        foreach (var p in gameManager.PlayersID.Select(gameManager.GetPlayer))
        {
            if(p.PlayerTeam == player.PlayerTeam) playerUI.RpcToggleHeaderBomb(p.netIdentity.connectionToClient, false);
        }
    }
    

    [ClientRpc]
    void RpcDropBomb()
    {
        if (isLocalPlayer)
        {
            CmdSwitchItem(PreviousEquippedItem);
            uiManager.Bomb.SetActive(false);
        }
        Bomb.transform.localPosition = new Vector3(0, .6f, .5f);
        Bomb.transform.SetParent(gameManager.gameObject.transform);
        setLayerMask(Bomb, 0);
        Invoke(nameof(setBombLayer), .5f);
        Rigidbody rb = Bomb.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        Bomb.GetComponent<BoxCollider>().enabled = true;
        rb.AddForce(transform.GetChild(0).transform.TransformDirection(new Vector3(0, 0, 400)));
        Bomb = null;
    }



    void setBombLayer() => gameManager.Bomb.transform.GetChild(0).gameObject.layer = 8;

    [Command]
    void CmdPickGun(uint GunID)
    {
        RpcPickGun(NetworkServer.spawned[GunID].gameObject);
    }

    [ClientRpc]
    void RpcPickGun(GameObject gunInstance)
    {
        GunInstance instance = gunInstance.GetComponent<GunInstance>();
        if (!instance.CanBePicked) return;
        instance.IsDropped = false;
        instance.CanBePicked = false;

        Gun gun = instance.Gun;

        switch (gun.Type)
        {
            case GunType.Primary when PrimaryGun != null:
                return;
            case GunType.Primary:
                instance.StopAllCoroutines();
                gunInstance.transform.SetParent(PrimaryGunHolder.transform);
                PrimaryGunInstance = gunInstance;
                PrimaryGun = gun;
                break;
            case GunType.Secondary when SecondaryGun != null:
                return;
            case GunType.Secondary:
                instance.StopAllCoroutines();
                gunInstance.transform.SetParent(SecondaryGunHolder.transform);
                SecondaryGunInstance = gunInstance;
                SecondaryGun = gun;
                break;
        }

        setGunTransform(gunInstance, gun);
        Rigidbody rb = gunInstance.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        gunInstance.gameObject.transform.GetChild(0).gameObject.layer = 6;
        
        if (!isLocalPlayer) return;
        setLayerMask(gunInstance, 6);
        if(playerUI.ShopPlayer != null) playerUI.CmdUpdateShopPlayer();
        playerUI.UpdateLocalPlayerShopPlayer();
        playerUI.UpdateEquippedItem();
        if(playerUI.ScoreboardPlayer != null)playerUI.CmdUpdateScoreboardPlayer();
    }

    [Command(requiresAuthority =  false)]
    public void CmdDropGun(GunType gunType) => RpcDropGun(gunType);

    [ClientRpc]
    void RpcDropGun(GunType gunType)
    {
        GunEquipped = false;
        playerShootingManager.StopAllCoroutines();
        playerShootingManager.Reloading = false;
        playerShootingManager.CanShoot = true;
        GameObject gunInstance = null;
        switch (gunType)
        {
            case GunType.Primary when PrimaryGun == null || PrimaryGunInstance == null:
                return;
            case GunType.Primary:
            {
                switchItem(SecondaryGun != null ? Item.Secondary : Item.Knife);

                gunInstance = PrimaryGunInstance;
                PrimaryGunInstance = null;
                PrimaryGun = null;
                break;
            }
            case GunType.Secondary when SecondaryGun == null || SecondaryGunInstance == null:
                return;
            case GunType.Secondary:
            {
                switchItem(PrimaryGun != null ? Item.Primary : Item.Knife);

                gunInstance = SecondaryGunInstance;
                SecondaryGunInstance = null;
                SecondaryGun = null;
                break;
            }
        }

        if (gunInstance == null) return;
        gunInstance.transform.localPosition = new Vector3(0, .5f, .5f);
        gunInstance.transform.localEulerAngles += new Vector3(30, 0, 0);
        gunInstance.transform.SetParent(gameManager.GunHolder.transform);
        gunInstance.transform.GetChild(0).GetComponent<BoxCollider>().enabled = true;
        gunInstance.transform.GetComponent<SphereCollider>().enabled = true;
        GunInstance instance = gunInstance.GetComponent<GunInstance>();
        instance.CanBeSold = false;
        instance.IsDropped = true;
        instance.Invoke(nameof(GunInstance.SetPickStatus), .5f);
        //instance.SetPickStatus();
        setLayerMask(gunInstance, 0);
        gunInstance.gameObject.transform.GetChild(0).gameObject.layer = 8;
        Rigidbody rb = gunInstance.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(transform.GetChild(0).transform.TransformDirection(new Vector3(0, 0, 400))); // Camera

        if (!isLocalPlayer) return;
        CmdUpdateUI();
        playerUI.UpdateEquippedItem();
        if(playerUI.ScoreboardPlayer != null)playerUI.CmdUpdateScoreboardPlayer();
    }


    [Command(requiresAuthority = false)]
    public void CmdGiveGun(int gunID)
    {
        GameObject gunInstance = Instantiate(gunManager.GetGunByID(gunID).GunModel);
        NetworkServer.Spawn(gunInstance);
        RpcGiveGun(gunID, gunInstance.GetComponent<NetworkIdentity>());
    }
    
    [ClientRpc]
    private void RpcGiveGun(int gunID, NetworkIdentity gunNetworkIdentity)
    {
        GameObject gunInstance = gunNetworkIdentity.gameObject;
        gunManager = FindObjectOfType<GunManager>();
        Gun gun = gunManager.GetGunByID(gunID);
        gunInstance.AddComponent<GunInstance>();
        GunInstance spawnedGun = gunInstance.GetComponent<GunInstance>();
        spawnedGun.CanBeSold = true;
        spawnedGun.Gun = gun;
        spawnedGun.Ammo = gun.Ammo;
        spawnedGun.Magazine = gun.MagazineAmmo;
        GunType type = gunManager.GetGunByID(gunID).Type;
        spawnedGun.GunOwner = player;
        switch (type)
        {
            case GunType.Primary:
                PrimaryGunInstance = gunInstance;
                break;
            case GunType.Secondary:
                SecondaryGunInstance = gunInstance;
                break;
        }

        if (gun.Type != GunType.Primary)
        {
            if (gun.Type == GunType.Secondary)
            {
                SecondaryGun = gun;
                gunInstance.transform.SetParent(SecondaryGunHolder.transform);
                if (PrimaryGun == null && isLocalPlayer) CmdSwitchItem(Item.Secondary);
            }
        }
        else
        {
            PrimaryGun = gun;
            gunInstance.transform.SetParent(PrimaryGunHolder.transform);
            if (isLocalPlayer) CmdSwitchItem(Item.Primary);
        }

        setGunTransform(gunInstance, gun);
        
        if (!isLocalPlayer) return;
        setLayerMask(gunInstance, 6);
        if(playerUI.ShopPlayer != null) playerUI.CmdUpdateShopPlayer();
        playerUI.UpdateLocalPlayerShopPlayer();
        playerUI.UpdateEquippedItem();
        if(playerUI.ScoreboardPlayer != null)playerUI.CmdUpdateScoreboardPlayer();
    }

    private void setGunTransform(GameObject gunInstance, Gun gun)
    {
        gunInstance.transform.localPosition = Vector3.zero;
        gunInstance.transform.localEulerAngles = Vector3.zero;
        Rigidbody rb = gunInstance.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        gunInstance.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
        gunInstance.transform.GetComponent<SphereCollider>().enabled = false;
        gunInstance.transform.localScale = new Vector3(1f, 1f, 1f);
        //gunInstance.transform.localPosition = gun.GunTransform.FirstPersonGunPosition;
        //gunInstance.transform.localEulerAngles = gun.GunTransform.FirstPersonGunRotation;
        //if (isLocalPlayer) return;
        gunInstance.transform.localPosition = gun.GunTransform.ThirdPersonGunPosition;
        gunInstance.transform.localEulerAngles = gun.GunTransform.ThirdPersonGunRotation;
    }

    private void setHandTransform(Gun gun)
    {
        RightHandTarget.localPosition = gun.GunTransform.RightHandTargetPosition;
        RightHandTarget.localEulerAngles = gun.GunTransform.RightHandTargetRotation;
        RightHandHint.localPosition = gun.GunTransform.RightHandHintPosition;
        RightHandHint.localEulerAngles = gun.GunTransform.RightHandHintRotation;

        LeftHandTarget.localPosition = gun.GunTransform.LeftHandTargetPosition;
        LeftHandTarget.localEulerAngles = gun.GunTransform.LeftHandTargetRotation;
        LeftHandHint.localPosition = gun.GunTransform.LeftHandHintPosition;
        LeftHandHint.localEulerAngles = gun.GunTransform.LeftHandHintRotation;
    }

    private void setKnifeHandTransform()
    {
        RightHandTarget.localPosition = KnifeRightHandTargetPosition;
        RightHandTarget.localEulerAngles = KnifeRightHandTargetRotation;
        RightHandHint.localPosition = KnifeRightHandHintPositon;

        LeftHandTarget.localPosition = KnifeLeftHandTargetPosition;
        LeftHandTarget.localEulerAngles = KnifeLeftHandTargetRotation;
        LeftHandHint.localPosition = KnifeLeftHandHintPosition;

    }

    [Command(requiresAuthority =  false)]
    public void CmdSellGun(uint gunID, GunType gunType)
    {
        switch (gunType)
        {
            case GunType.Primary:
                PrimaryGun = null;
                PrimaryGunInstance = null;
                break;
            case GunType.Secondary:
                SecondaryGun = null;
                SecondaryGunInstance = null;
                break;
        }

        CmdSwitchItem(PreviousEquippedItem);
        NetworkServer.Destroy(NetworkServer.spawned[gunID].gameObject);
        RpcUpdateUI();
    }

    [Command(requiresAuthority =  false)]
    public void CmdDestroyGun(GunType type)
    {
        switch (type)
        {
            case GunType.Primary when PrimaryGun != null:
                Debug.Log("Destroying primary gun");
                PrimaryGun = null;
                PrimaryGunHolder.SetActive(true);
                NetworkServer.Destroy(NetworkServer.spawned[PrimaryGunInstance.GetComponent<NetworkIdentity>().netId]
                    .gameObject);
                PrimaryGunHolder.SetActive(false);
                CmdSwitchItem(Item.Knife);
                break;
            case GunType.Secondary when SecondaryGun != null:
                Debug.Log("Destroying secondary gun");
                SecondaryGun = null;
                SecondaryGunHolder.SetActive(false);
                NetworkServer.Destroy(NetworkServer.spawned[SecondaryGunInstance.GetComponent<NetworkIdentity>().netId]
                    .gameObject);
                SecondaryGunHolder.SetActive(true);
                CmdSwitchItem(Item.Knife);
                break;
        }
        RpcUpdateUI();
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateUI() => RpcUpdateUI();
    
    [ClientRpc]
    public void RpcUpdateUI()
    {
        if (!isLocalPlayer) return;
        if(playerUI.ShopPlayer != null) playerUI.CmdUpdateShopPlayer();
        playerUI.UpdateLocalPlayerShopPlayer();
    }
}
using Mirror;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerShootingManager : NetworkBehaviour
{
    [SerializeField] PlayerInventoryManager playerInventory;
    Player player;
    [SerializeField] UIManager uiManager;

    [SerializeField] Transform cameraHolder;

    PlayerEconomyManager playerEconomyManager;
    private PlayerCombatReport playerCombatReport;

    public bool CanShoot = true;
    public bool Reloading;

    public GunInstance GunInstance;
    [SerializeField] GameObject BulletImpactDecalPenetrable, BulletImpactDecalNotPenetrable;

    private PlayerInput playerInput;

    [SerializeField] private bool isAiming;
    [SerializeField] private bool isShooting;

    public int GunHeath;
    float bloomAmount;
    public float BloomModifier;
    [SerializeField] private float heathDecreaseTime;

    public IEnumerator BurstShootCoroutine;
    public IEnumerator SetGunScopeCoroutine;

    [SerializeField] private float targetFOV = 80;
    [SerializeField] private float targetZoomLayerWeight;
    private float zoomLayerWeight;
    [SerializeField] private float zoomLayerWeightSmoothness;
    [SerializeField] private float FOVsmoothness;

    [SerializeField] public int ZoomState;

    private Camera mainCamera;
    [SerializeField] private Animator anim;

    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.PlayerShoot.Enable();

        player = GetComponent<Player>();
        uiManager = FindObjectOfType<UIManager>();
        playerEconomyManager = GetComponent<PlayerEconomyManager>();
        playerCombatReport = GetComponent<PlayerCombatReport>();
        
        if (!isLocalPlayer) return;


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

    private void Start()
    {
        if (!isLocalPlayer) return;
        mainCamera = Camera.main; 
        cameraRecoil = mainCamera!.transform.gameObject;
    }

    void Update()
    {
        Debug.DrawRay(cameraHolder.position, cameraHolder.forward * 2, Color.green);
        Debug.DrawRay(cameraHolder.position, transform.forward + cameraHolder.transform.forward, Color.magenta);
        if (player.IsDead) return;
        if (!isLocalPlayer) return;
        Gun gun = playerInventory.EquippedGun;
        recoil();
        decreaseGunHeath();
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, FOVsmoothness);
        zoomLayerWeight = Mathf.Lerp(zoomLayerWeight, targetZoomLayerWeight, zoomLayerWeightSmoothness);
        anim.SetLayerWeight(1, zoomLayerWeight);
        
        if (GunInstance == null) return;
        handleZoom(gun);
        if (playerEconomyManager.IsShopOpen) return;


        if (gun != null && playerInventory.GunEquipped && CanShoot && GunInstance.Magazine > 0 && !Reloading)
        {
            if (gun.PrimaryFire.FireMode == FireMode.Manual && playerInput.PlayerShoot.Primary.triggered)
            {
                Shoot(Side.Left);
            }
            else if (gun.PrimaryFire.FireMode == FireMode.Automatic && playerInput.PlayerShoot.Primary.IsPressed())
            {
                Shoot(Side.Left);
                isShooting = true;
            }
            else if (gun.HasSecondaryFire && gun.SecondaryFire.FireMode == FireMode.Manual && playerInput.PlayerShoot.Secondary.triggered)
            {
                Shoot(Side.Right);
            }
            else if (gun.HasSecondaryFire && gun.SecondaryFire.FireMode == FireMode.Automatic && playerInput.PlayerShoot.Secondary.IsPressed() && !isAiming)
            {
                Shoot(Side.Right);
                isShooting = true;
            }
            else isShooting = false;
        }

        if (gun == null) return;
        if (GunInstance.Magazine == 0 && GunInstance.Ammo > 0 && !Reloading)
        {
            StartCoroutine(Reload());
        }

        if (GunInstance.Magazine != gun.MagazineAmmo && playerInput.PlayerShoot.Reload.triggered && GunInstance.Ammo > 0 && !Reloading)
        {
            StartCoroutine(Reload());
        }
        
    }

    void handleZoom(Gun gun)
    {
        if (!gun.HasSecondaryFire) return;

        if (ZoomState == 0)
        {
            isAiming = false;
            targetFOV = 80;
            targetZoomLayerWeight = 0;
        }
        
        switch (gun.SecondaryFire.ZoomType)
        {
            case ZoomType.None:
                break;
            case ZoomType.Semi when playerInput.PlayerShoot.Secondary.IsPressed():
                isAiming = true;
                targetFOV = gun.SecondaryFire.Zoom.FirstZoomFOV;
                targetZoomLayerWeight = 1;
                break;
            case ZoomType.Full when playerInput.PlayerShoot.Secondary.triggered:
            {
                isAiming = true;
                if (gun.SecondaryFire.Zoom.SecondZoomFOV == 0)
                {
                    if (ZoomState == 0)
                    {
                        ZoomState = 1;
                        targetFOV = gun.SecondaryFire.Zoom.FirstZoomFOV;
                        targetZoomLayerWeight = 1;
                    }
                }
                else if (gun.SecondaryFire.Zoom.SecondZoomFOV != 0)
                {
                    SetGunScopeCoroutine = SetGunScope(gun);
                    StartCoroutine(SetGunScopeCoroutine);
                }

                break;
            }
        }
    }

    IEnumerator SetGunScope(Gun gun)
    {
        switch (ZoomState)
        {
            case 0:
                targetFOV = gun.SecondaryFire.Zoom.FirstZoomFOV;
                ZoomState = 1;
                yield return new WaitForSeconds(.25f);
                uiManager.Scope.sprite = gun.SecondaryFire.Zoom.ScopeImg;
                break;
            case 1:
                mainCamera.fieldOfView = gun.SecondaryFire.Zoom.SecondZoomFOV;
                targetFOV = gun.SecondaryFire.Zoom.SecondZoomFOV;
                ZoomState = 2;
                break;
            case 2:
                ZoomState = 0;
                targetFOV = 80;
                uiManager.Scope.sprite = uiManager.TransparentImage;
                break;
        }
    }

    private IEnumerator DelayFire(float time)
    {
        yield return new WaitForSeconds(time);
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

    private void Shoot(Side buttonPressed)
    {
        CanShoot = false;
        Debug.Log(buttonPressed);

        penetrationAmount = playerInventory.EquippedGun.BulletPenetration;
        recoilFire(playerInventory.EquippedGun);
        if (GunHeath < 30) GunHeath += 2;
        Gun gun = playerInventory.EquippedGun;

        switch (buttonPressed)
        {
            case Side.Left:
            {
                if (!isAiming)
                {
                    Debug.Log("FIRE");

                    switch (gun.PrimaryFire.FireType)
                    {
                        case FireType.Single:
                            CheckPenetration(gun.PrimaryFire.Bloom);
                            GunInstance.Magazine--;
                            UpdateUIAmmo();
                            break;
                        case FireType.Burst:
                            BurstShootCoroutine = BurstShoot(gun.PrimaryFire.BulletsPerFire, gun.PrimaryFire.BurstDelay, gun.PrimaryFire.Bloom, gun.PrimaryFire.RemoveBulletsPerFire);
                            StartCoroutine(BurstShootCoroutine);
                            break;
                        case FireType.Multiple:
                            for (int i = 0; i < gun.PrimaryFire.BulletsPerFire; i++)
                            {
                                CheckPenetration(gun.PrimaryFire.Bloom);
                                GunInstance.Magazine--;
                                UpdateUIAmmo();
                            }

                            break;
                    }
                    StartCoroutine(DelayFire(gun.PrimaryFire.FireDelay));
                }
                else
                {
                    Debug.Log("ALT FIRE");

                    switch (gun.SecondaryFire.FireType)
                    {
                        case FireType.Single:
                            CheckPenetration(gun.SecondaryFire.Bloom);
                            GunInstance.Magazine -= gun.SecondaryFire.RemoveBulletsPerFire;
                            UpdateUIAmmo();
                            break;
                        case FireType.Burst:
                            BurstShootCoroutine = BurstShoot(gun.SecondaryFire.BulletsPerFire, gun.SecondaryFire.BurstDelay, gun.SecondaryFire.Bloom, gun.SecondaryFire.RemoveBulletsPerFire);
                            StartCoroutine(BurstShootCoroutine);
                            break;
                        case FireType.Multiple:
                            for (int i = 0; i < gun.SecondaryFire.BulletsPerFire; i++)
                            {
                                CheckPenetration(gun.SecondaryFire.Bloom);
                            }

                            GunInstance.Magazine -= gun.SecondaryFire.RemoveBulletsPerFire;
                            UpdateUIAmmo();

                            break;
                    }
                    StartCoroutine(DelayFire(gun.SecondaryFire.FireDelay));
                }

                
                break;
            }
            case Side.Right when !isAiming:
            {
                Debug.Log("RIGHT CLICK");

                if (!gun.HasSecondaryFire) return;
                switch (gun.SecondaryFire.FireType)
                {
                    case FireType.Single:
                        CheckPenetration(gun.SecondaryFire.Bloom);
                        GunInstance.Magazine -= gun.SecondaryFire.RemoveBulletsPerFire;
                        UpdateUIAmmo();
                        break;
                    case FireType.Burst:
                        BurstShootCoroutine = BurstShoot(gun.SecondaryFire.BulletsPerFire, gun.SecondaryFire.BurstDelay, gun.SecondaryFire.Bloom, gun.SecondaryFire.RemoveBulletsPerFire);
                        StartCoroutine(BurstShootCoroutine);
                        break;
                    case FireType.Multiple:
                        for (int i = 0; i < gun.SecondaryFire.BulletsPerFire; i++)
                        {
                            CheckPenetration(gun.SecondaryFire.Bloom);
                        }

                        GunInstance.Magazine -= gun.SecondaryFire.RemoveBulletsPerFire;
                        UpdateUIAmmo();

                        break;
                }

                StartCoroutine(DelayFire(gun.SecondaryFire.FireDelay));
                break;
            }
        }
    }

    IEnumerator BurstShoot(int j, float burstDelay, float bloom, int bulletsToRemove)
    {
        for (int i = 0; i < j; i++)
        {
            if (GunInstance.Magazine <= 0) break;
            CheckPenetration(bloom);
            GunInstance.Magazine -= bulletsToRemove;
            UpdateUIAmmo();
            yield return new WaitForSeconds(burstDelay);
        }
    }

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    [SerializeField] private GameObject cameraRecoil;

    void recoil()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        cameraRecoil.transform.localRotation = Quaternion.Euler(currentRotation);
    }

    void recoilFire(Gun gun)
    {
        if (isAiming)
        {
            targetRotation += new Vector3(gun.GunRecoil.AimRecoilX, Random.Range(-gun.GunRecoil.AimRecoilY, gun.GunRecoil.AimRecoilY), Random.Range(-gun.GunRecoil.AimRecoilZ, gun.GunRecoil.AimRecoilZ));
        }
        else
        {
            targetRotation += new Vector3(gun.GunRecoil.RecoilX, Random.Range(-gun.GunRecoil.RecoilY, gun.GunRecoil.RecoilY), Random.Range(-gun.GunRecoil.RecoilZ, gun.GunRecoil.RecoilZ));
        }
    }

    private float t;

    public void decreaseGunHeath()
    {
        t += Time.deltaTime;
        if (!(t >= heathDecreaseTime) || GunHeath <= 0 || isShooting) return;
        t = 0;
        GunHeath-= 3;
    }

    [SerializeField] LayerMask mask;
    int BulletDamage;
    Vector3 endPoint;
    Vector3? penetrationPoint;
    Vector3? impactPoint;
    bool canPenetrate;
    float penetrationAmount;

    private void CheckPenetration(float bloom)
    {
        int penIndex = 0;
        Vector3 originPosition = cameraHolder.position;

        while (true)
        {
            Vector3 direction;
            if (penIndex == 0) //First pen check
            {
                bloomAmount = GunHeath * 3 + BloomModifier + bloom;
                if (bloomAmount < 0) bloomAmount = 0;
                penIndex++;
                Vector3 bloomDirection = cameraHolder.position + cameraHolder.forward * 1000f;
                bloomDirection += Random.Range(-bloomAmount, bloomAmount) * cameraHolder.up; //Y
                bloomDirection += Random.Range(-bloomAmount, bloomAmount) * cameraHolder.right; //X
                bloomDirection -= cameraHolder.position;
                bloomDirection.Normalize();
                direction = bloomDirection;
            }
            else //Pen check
            {
                direction = cameraHolder.forward;
            }

            penIndex++;
            Ray ray = new(originPosition, direction);
            if (Physics.Raycast(originPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask: mask))
            {
                if (hit.collider.transform.parent != null)
                {
                    if (hit.collider.transform.parent.TryGetComponent(out IDamageable entity))
                    {
                        Player hitPlayer = hit.collider.transform.parent.gameObject.GetComponent<Player>();
                        if (hitPlayer.PlayerTeam != player.PlayerTeam && !hitPlayer.IsDead)
                        {
                            Body body = GetBody(hit.collider.gameObject);
                            var damage = calculateDamage(hit.point);
                            CombatReport report = new()
                            {
                                TargetPlayerId = hitPlayer.netId,
                                OwnerPlayerId = player.netId,
                                OutComingDamage = damage,
                                GunId = playerInventory.EquippedGun.GunID,
                            };
                            if (body != Body.None) report.TargetBody.Add(body);
                            if (entity.TakeDamage(damage))
                            {
                                report.TargetState = ReportState.Killed;
                                player.CmdAddKill();
                            }

                            playerCombatReport.AddReport(report);
                        }
                    }
                }

                impactPoint = hit.point;
                Ray penRay = new(hit.point + ray.direction * penetrationAmount, -ray.direction);
                if (hit.collider.Raycast(penRay, out RaycastHit penHit, penetrationAmount))
                {
                    penetrationPoint = penHit.point;
                    endPoint = transform.position + transform.forward * 1000;
                    if (hit.collider.transform.TryGetComponent(out MaterialToughness materialToughness))
                    {
                        CmdInstantiateImpactDecal(true, hit.point, -hit.normal); // first point
                        CmdInstantiateImpactDecal(true, penHit.point, -penHit.normal); //second point


                        penetrationAmount -= Vector3.Distance((Vector3)penetrationPoint, hit.point);
                        penetrationAmount -= materialToughness.ToughnessAmount;
                        originPosition = hit.point;
                        continue;
                    }
                    else
                    {
                        CmdInstantiateImpactDecal(false, hit.point, -hit.normal);
                    }
                }
                else
                {
                    CmdInstantiateImpactDecal(false, hit.point, -hit.normal);
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


    Body GetBody(GameObject hit) =>
        hit.tag switch
        {
            "leg" => Body.Legs,
            "body" => Body.Body,
            "head" => Body.Head,
            _ => Body.None
        };

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
            case 3 when
                (distance >= gun.Damages[1].MinDistance && distance <= gun.Damages[1].MaxDistance):
                BulletDamage = gun.Damages[1].BodyDamage;
                break;
            case 3:
            {
                if (distance >= gun.Damages[2].MinDistance) BulletDamage = gun.Damages[2].BodyDamage;
                break;
            }
        }

        //Debug.Log("Bullet damage before: " + BulletDamage);
        //BulletDamage -= (int)penetrationAmount*2; //TODO need better damage calculation
        //Debug.Log("Bullet damage after: " + BulletDamage);
        return BulletDamage;
    }

    [Command]
    void CmdInstantiateImpactDecal(bool canPenetrate, Vector3 position, Vector3 rotation)
    {
        GameObject bulletImpact = Instantiate(canPenetrate ? BulletImpactDecalPenetrable : BulletImpactDecalNotPenetrable);
        NetworkServer.Spawn(bulletImpact);
        RpcInstantiateImpactDecal(bulletImpact, position, rotation);
    }


    [ClientRpc]
    void RpcInstantiateImpactDecal(GameObject bulletImpact, Vector3 position, Vector3 rotation)
    {
        bulletImpact.transform.position = position;
        bulletImpact.transform.rotation = Quaternion.LookRotation(rotation);
    }
}
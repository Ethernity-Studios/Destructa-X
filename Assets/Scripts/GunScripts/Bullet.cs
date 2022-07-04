using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float PenetrationAmount;
    [SerializeField] float bulletSpeed;

    public bool CanPenetrate;

    [HideInInspector] public Vector3 BulletDirection;
    [HideInInspector] public Vector3 CameraPosition;

    TrailRenderer trailRenderer;
    Renderer BulletRenderer;
    Rigidbody rb;

    Vector3 endPoint;
    Vector3? penetrationPoint;
    Vector3? impactPoint;

    public Player BulletOwner;
    public Gun Gun;

    GameObject NonPenetrableObject;

    public int BulletDamage;

    bool collided;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        BulletRenderer = GetComponent<Renderer>();
        Destroy(gameObject, 5f);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * bulletSpeed);
    }

    public void CheckPenetration()
    {
        Ray ray = new Ray(this.transform.position+new Vector3(0,0,transform.localScale.z), this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            impactPoint = hit.point;
            Ray penRay = new Ray(hit.point + ray.direction * PenetrationAmount, -ray.direction);
            RaycastHit penHit;
            if (hit.collider.Raycast(penRay, out penHit, PenetrationAmount))
            {
                penetrationPoint = penHit.point;
                endPoint = this.transform.position + this.transform.forward * 1000;

                if(hit.transform.TryGetComponent(out MaterialToughness materialToughness))
                {
                    CanPenetrate = true;
                    PenetrationAmount -= Vector3.Distance((Vector3)penetrationPoint, hit.point);
                    PenetrationAmount -= materialToughness.ToughnessAmount;
                }
                else
                {
                    CanPenetrate = false;
                    NonPenetrableObject = hit.transform.gameObject;
                }
            }
            else
            {
                endPoint = impactPoint.Value + ray.direction * PenetrationAmount;
                penetrationPoint = endPoint;
                CanPenetrate = false;
                NonPenetrableObject = hit.transform.gameObject;
            }
        }
        else
        {
            CanPenetrate = true;
            endPoint = this.transform.position + this.transform.forward * 1000;
            penetrationPoint = null;
            impactPoint = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (NonPenetrableObject == collision.gameObject && !CanPenetrate) Destroy(gameObject);
        CheckPenetration();
        if (!BulletRenderer.enabled) enableBulletRenderer();
        if (transform.eulerAngles != BulletDirection) transform.eulerAngles = BulletDirection;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        if (other.transform.parent == null) return;
        if (other.transform.parent.TryGetComponent(out IDamageable iDamageable))
        {
            if (other.gameObject == BulletOwner.gameObject.transform.GetChild(0).gameObject) return;
            iDamageable.TakeDamage(CalculateDamage());
            Destroy(gameObject);
        }
    }


    int CalculateDamage()
    {
        float distance = Vector3.Distance(transform.position, CameraPosition);
        if (Gun.Damages.Count == 1)
        {
            BulletDamage = Gun.Damages[0].BodyDamage;
        }
        else if(Gun.Damages.Count == 2)
        {
            if(distance <= Gun.Damages[0].MaxDistance) BulletDamage = Gun.Damages[0].BodyDamage;
            else if(distance >= Gun.Damages[1].MinDistance) BulletDamage = Gun.Damages[1].BodyDamage;
        }
        else if(Gun.Damages.Count == 3)
        {
            if (distance <= Gun.Damages[0].MaxDistance) BulletDamage = Gun.Damages[0].BodyDamage;
            else if (distance >= Gun.Damages[1].MinDistance && distance <= Gun.Damages[1].MaxDistance) BulletDamage = Gun.Damages[1].BodyDamage;
            else if (distance >= Gun.Damages[2].MinDistance) BulletDamage = Gun.Damages[2].BodyDamage; 
        }
        return BulletDamage;
    }

    void enableBulletRenderer()
    {
        BulletRenderer.enabled = true;
        trailRenderer.enabled = true;
    }
}

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
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        BulletRenderer = GetComponent<Renderer>();
        BulletRenderer.enabled = false;
        trailRenderer.enabled = false;
        Destroy(gameObject, 5f);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * bulletSpeed);
    }

    public void CheckPenetration()
    {
        Debug.Log("Checking penetration");
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
                    Debug.Log("Object has meterial toughness");
                    CanPenetrate = true;
                    PenetrationAmount -= Vector3.Distance((Vector3)penetrationPoint, hit.point);
                    PenetrationAmount -= materialToughness.ToughnessAmount;
                }
                else
                {
                    Debug.Log("Object doesnt have meterial toughness");
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
    [SerializeField] GameObject NonPenetrableObject;
    private void OnCollisionEnter(Collision collision)
    {
        if (NonPenetrableObject == collision.gameObject && !CanPenetrate) Destroy(gameObject);
        CheckPenetration();
        if (!BulletRenderer.enabled) enableBulletRenderer();
        if (transform.eulerAngles != BulletDirection) transform.eulerAngles = BulletDirection;
    }

    void enableBulletRenderer()
    {
        BulletRenderer.enabled = true;
        trailRenderer.enabled = true;
    }

    private void OnDrawGizmos()
    {
        //CheckPenetration();

        if (!penetrationPoint.HasValue || !impactPoint.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position + new Vector3(0, 0, transform.localScale.z), endPoint);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position + new Vector3(0, 0, transform.localScale.z), impactPoint.Value);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(impactPoint.Value, penetrationPoint.Value);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(penetrationPoint.Value, endPoint);
        }
    }
}

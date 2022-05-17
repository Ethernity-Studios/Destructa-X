using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float penetrationAmount;
    public Vector3 endPoint;
    public Vector3? penetrationPoint;
    public Vector3? impactPoint;

    [SerializeField] float bulletSpeed;

    TrailRenderer trailRenderer;
    Renderer BulletRenderer;
    Rigidbody rb;

    [HideInInspector]public Vector3 BulletDirection;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        BulletRenderer = GetComponent<Renderer>();
        BulletRenderer.enabled = false;
        trailRenderer.enabled = false;
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (penetrationPoint != null)
            UpdatePenetration();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * bulletSpeed);
    }

    void UpdatePenetration()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            impactPoint = hit.point;
            Ray penRay = new Ray(hit.point + ray.direction * penetrationAmount, -ray.direction);
            RaycastHit penHit;
            if (hit.collider.Raycast(penRay, out penHit, penetrationAmount))
            {
                penetrationPoint = penHit.point;
                endPoint = this.transform.position + this.transform.forward * 1000;
            }
            else
            {
                endPoint = impactPoint.Value + ray.direction * penetrationAmount;
                penetrationPoint = endPoint;
            }
        }
        else
        {
            endPoint = this.transform.position + this.transform.forward * 1000;
            penetrationPoint = null;
            impactPoint = null;
        }
    }
    bool firstCollision;
    private void OnCollisionEnter(Collision collision)
    {
        if (BulletRenderer.enabled == false) Invoke("enableBulletVisuals", .1f);
        if (firstCollision == false) setBulletDirection();
    }

    void enableBulletVisuals()
    {
        BulletRenderer.enabled = true;
        trailRenderer.enabled = true;
    }



    void setBulletDirection()
    {
        Debug.Log("setting bullet direction");
        transform.eulerAngles = BulletDirection;
        firstCollision = false;
    }

    private void OnDrawGizmos()
    {
        UpdatePenetration();

        if (!penetrationPoint.HasValue || !impactPoint.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position, endPoint);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position, impactPoint.Value);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(impactPoint.Value, penetrationPoint.Value);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(penetrationPoint.Value, endPoint);
        }
    }
}

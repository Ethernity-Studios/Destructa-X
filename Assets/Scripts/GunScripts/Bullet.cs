using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float penetrationAmount;
    public Vector3 endPoint;
    public Vector3? penetrationPoint;
    public Vector3? impactPoint;

    [SerializeField] float bulletSpeed;

    TrailRenderer trailRenderer;
    Renderer BulletRenderer;

    private void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        BulletRenderer = GetComponent<Renderer>();
        BulletRenderer.enabled = false;
        trailRenderer.enabled = false;
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
        if (penetrationPoint != null)
            UpdatePenetration();
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
        if (BulletRenderer.enabled == false) Invoke("enableBulletVisuals",.1f);
        if(firstCollision == false) setBulletDirection();
        Debug.Log("Colisionen bulleten");
    }

    void enableBulletVisuals()
    {
        BulletRenderer.enabled = true;
        trailRenderer.enabled = true;
    }

    Vector3 bulletDirection;

    [Command(requiresAuthority = false)]
    public void SetBulletDirection(Vector3 dir) => bulletDirection = dir;

    void setBulletDirection() 
    {
        transform.eulerAngles = bulletDirection;
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

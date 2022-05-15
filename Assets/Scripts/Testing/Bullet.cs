using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float penetrationAmount;
    public Vector3 endPoint;
    public Vector3? penetrationPoint;
    public Vector3? impactPoint;

    [SerializeField] float bulletSpeed;

    MeshRenderer meshRenderer;
    TrailRenderer trailRenderer;
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
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
    private void OnCollisionEnter(Collision collision)
    {
        if (meshRenderer.enabled == false) 
        {
            meshRenderer.enabled = true;
            trailRenderer.enabled = true;
        } 
        Debug.Log("Colisionen bulleten");
        Destroy(gameObject);
    }
}

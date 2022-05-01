using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float time;
    [SerializeField] float speed;

    public int Damage;
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);



        if (isInWall)
        {
            time += Time.deltaTime;
        }
        else
        {
            time = 0;
        }
    }
    public bool isInWall;

    public GameObject wallGO;

    private void OnTriggerEnter(Collider other)
    {
        if(wallGO == null)
        {
            isInWall = true;
            Debug.Log("Entered object!" + other.name);
            wallGO = other.gameObject;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == wallGO)
        {
            isInWall = false;
            Debug.Log("Leaved object!" + other.name);
            wallGO = null;
        }

    }

}

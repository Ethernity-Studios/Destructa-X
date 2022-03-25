using UnityEngine;

public class Explosion : MonoBehaviour
{
    BombManager bombManager;

    bool exploded = false;

    bool decrease = false;
    private void Start()
    {
        bombManager = FindObjectOfType<BombManager>();
    }
    private void Update()
    {
        if (transform.localScale.x < bombManager.ExplosionSize && !exploded)
        {
            transform.localScale += new Vector3(bombManager.IncreaseSize * Time.deltaTime, bombManager.IncreaseSize * Time.deltaTime /2, bombManager.IncreaseSize * Time.deltaTime);
        }

        if(transform.localScale.x >= bombManager.ExplosionSize)
        {
            exploded = true;
            Invoke("canDecrease", 5f);
        }
        if (transform.localScale.x > 0 && decrease)
        {
            transform.localScale -= new Vector3(bombManager.IncreaseSize * Time.deltaTime * 2, bombManager.IncreaseSize * Time.deltaTime, bombManager.IncreaseSize * Time.deltaTime * 2);
        }
    }
    void canDecrease()
    {
        decrease = true;
    }
}

using UnityEngine;

public class Explosion : MonoBehaviour
{
    BombManager bombManager;

    bool exploded = false;

    bool decrease = false;

    [SerializeField] float explosionSize;
    [SerializeField] float increaseSize;
    private void Start()
    {
        bombManager = FindObjectOfType<BombManager>();
    }
    private void Update()
    {
        if (transform.localScale.x < explosionSize && !exploded)
        {
            transform.localScale += new Vector3(increaseSize * Time.deltaTime, increaseSize * Time.deltaTime / 2f, increaseSize * Time.deltaTime);
        }

        if (transform.localScale.x >= explosionSize)
        {
            exploded = true;
            Invoke("canDecrease", 4f);
        }
        if (transform.localScale.x > 0 && decrease)
        {
            transform.localScale -= new Vector3(increaseSize * Time.deltaTime * 2, increaseSize * Time.deltaTime, increaseSize * Time.deltaTime * 2);
        }
    }
    void canDecrease()
    {
        decrease = true;
    }
}

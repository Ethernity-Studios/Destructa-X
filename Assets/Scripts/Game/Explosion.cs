using UnityEngine;

public class Explosion : MonoBehaviour
{
    BombManager bombManager;
    private void Start()
    {
        bombManager = FindObjectOfType<BombManager>();
    }
    private void Update()
    {
        if (transform.localScale.x < bombManager.ExplosionSize)
        {
            transform.localScale += new Vector3(bombManager.IncreaseSize * Time.deltaTime, bombManager.IncreaseSize * Time.deltaTime /2, bombManager.IncreaseSize * Time.deltaTime);
        }
    }
}

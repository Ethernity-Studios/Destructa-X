using UnityEngine;

public class Explosion : MonoBehaviour
{
    bool exploded = false;

    bool decrease = false;

    [SerializeField] float explosionSize;
    [SerializeField] float increaseSize;
    private void Update()
    {
        if (transform.localScale.x < explosionSize && !exploded)
        {
            transform.localScale += new Vector3(increaseSize * Time.deltaTime, increaseSize * Time.deltaTime / 2f, increaseSize * Time.deltaTime);
        }

        if (transform.localScale.x >= explosionSize)
        {
            exploded = true;
            Invoke(nameof(canDecrease), 4f);
            //canDecrease();
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            player.CmdKillPlayer();
        }
    }
}

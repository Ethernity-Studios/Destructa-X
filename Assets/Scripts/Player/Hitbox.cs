using UnityEngine;

public class Hitbox : MonoBehaviour, IDamageable
{
    [SerializeField] public Player Player;
    [SerializeField] public BodyType BodyType;
    private void Start()
    {
        Player = transform.GetComponentInParent<Player>();
    }

    public bool TakeDamage(int damage)
    {
        return Player.TakeDamage(damage);
    }

    public void AddHealth(int health)
    {
        throw new System.NotImplementedException();
    }
}

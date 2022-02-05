using Mirror;

public abstract class EntityBase : NetworkBehaviour
{
    public float Health;
    public float MaxHealth;
    public bool Dead;


    public abstract bool TakeDamage(float damage);

    public abstract void Heal(float damage);
}
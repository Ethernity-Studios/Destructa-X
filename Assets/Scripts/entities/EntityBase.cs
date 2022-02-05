using Mirror;

public abstract class EntityBase : NetworkBehaviour
{
    public int Health;
    public int MaxHealth;
    public bool Dead;


    public abstract bool TakeDamage(int damage);

    public abstract void Heal(int damage);
}
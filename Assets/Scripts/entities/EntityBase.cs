using Mirror;

public enum OwOner
{
    Blue,
    Red,
    Neutral
}

public abstract class EntityBase : NetworkBehaviour
{
    public float Health;
    public float MaxHealth;
    public bool Dead;
    public OwOner Type;


    public abstract bool TakeDamage(float damage);

    public abstract void Heal(float hp);
}
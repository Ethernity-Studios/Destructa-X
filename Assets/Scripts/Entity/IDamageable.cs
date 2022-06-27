public interface IDamageable
{
    public float Health { get; set; }
    public float MaxHealth { get; set; }

    public abstract bool TakeDamage(float damage);
    public abstract void AddHealth(float health);
}


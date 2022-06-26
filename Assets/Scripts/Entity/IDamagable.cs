public interface IDamagable 
{
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public bool Dead { get; set; }
    public Team Team { get; set; }


    public abstract bool TakeDamage(float damage);

    public abstract void AddHealth(float health);
}
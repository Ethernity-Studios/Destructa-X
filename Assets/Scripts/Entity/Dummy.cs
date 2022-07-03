using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable
{
    public void AddHealth(int health)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log(gameObject.name + " Taken damage: " + damage);
    }
}

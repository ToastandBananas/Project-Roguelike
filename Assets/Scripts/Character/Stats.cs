using UnityEngine;

public class Stats : MonoBehaviour
{
    public Stat maxHealth;
    public int currentHealth { get; private set; }

    [HideInInspector] public bool canTakeDamage = true;
    [HideInInspector] public bool isDeadOrDestroyed;

    void Awake()
    {
        currentHealth = maxHealth.GetValue();
    }

    public virtual void TakeDamage(int damage)
    {
        if (canTakeDamage)
        {
            if (damage <= 0)
                damage = 1;

            TextPopup.CreateDamagePopup(transform.position, damage, false);

            currentHealth -= damage;
            if (currentHealth <= 0)
                Die();
        }
    }

    public virtual void Die()
    {
        isDeadOrDestroyed = true;
    }
}

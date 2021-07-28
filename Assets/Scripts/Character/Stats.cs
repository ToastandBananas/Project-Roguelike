using UnityEngine;

public class Stats : MonoBehaviour
{
    public Stat maxHealth;
    public int currentHealth { get; private set; }

    [HideInInspector] public bool canTakeDamage = true;
    [HideInInspector] public bool isDeadOrDestroyed;

    [HideInInspector] public GameManager gm;

    void Awake()
    {
        currentHealth = maxHealth.GetValue();
    }

    public virtual void Start()
    {
        gm = GameManager.instance;
    }

    public virtual int TakeDamage(int damage)
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

        return damage;
    }

    public virtual int Heal(int healAmount)
    {
        if (currentHealth + healAmount > maxHealth.GetValue())
        {
            healAmount = maxHealth.GetValue() - currentHealth;
            currentHealth = maxHealth.GetValue();
        }
        else
            currentHealth += healAmount;

        TextPopup.CreateHealPopup(transform.position, healAmount);

        return healAmount;
    }

    public virtual void Die()
    {
        isDeadOrDestroyed = true;

        gm.flavorText.WriteLine(name + " was destroyed.");
    }
}

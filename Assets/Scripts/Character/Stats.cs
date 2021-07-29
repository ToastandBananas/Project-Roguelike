using UnityEngine;

public enum LocomotionType { Inanimate, Humanoid, Biped, Quadruped, Hexapod, Octoped }

public class Stats : MonoBehaviour
{
    public LocomotionType locomotionType = LocomotionType.Humanoid;

    [Header("Health")]
    public Stat maxHealth;
    [HideInInspector] public int currentHealth;

    [HideInInspector] public bool canTakeDamage = true;
    [HideInInspector] public bool isDeadOrDestroyed;

    [HideInInspector] public GameManager gm;

    public virtual void Awake()
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

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

    SpriteManager spriteManager;

    public virtual void Awake()
    {
        currentHealth = maxHealth.GetValue();
    }

    public virtual void Start()
    {
        gm = GameManager.instance;
        spriteManager = GetComponent<SpriteManager>();
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
        GameTiles.RemoveObject(transform.position);

        if (TryGetComponent(out Inventory inv))
        {
            if (inv.items.Count > 0)
                gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine("The " + name + " was destroyed and its contents spill out on the ground."));
            else
                gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine("The " + name + " was destroyed."));

            for (int i = 0; i < inv.items.Count; i++)
            {
                gm.dropItemController.DropItem(transform.position, inv.items[i], inv.items[i].currentStackSize, inv, null);
                inv.items[i].ReturnToObjectPool();
            }

            inv.items.Clear();
        }
        else
            gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine("The " + name + " was destroyed."));

        gameObject.SetActive(false);
    }
}

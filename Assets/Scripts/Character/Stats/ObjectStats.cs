using UnityEngine;

public class ObjectStats : Stats
{
    public IntStat maxHealth;
    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool isDestroyed;

    void Awake()
    {
        currentHealth = maxHealth.GetValue();
    }

    public override int TakeDamage(int damage)
    {
        if (canTakeDamage)
        {
            if (damage <= 0)
                damage = 1;

            TextPopup.CreateDamagePopup(spriteRenderer, transform.position, damage, false);

            currentHealth -= damage;
            if (currentHealth <= 0)
                Destroy();
        }

        return damage;
    }

    public override int Heal(int healAmount)
    {
        if (currentHealth + healAmount > maxHealth.GetValue())
        {
            healAmount = maxHealth.GetValue() - currentHealth;
            currentHealth = maxHealth.GetValue();
        }
        else
            currentHealth += healAmount;

        TextPopup.CreateHealPopup(spriteRenderer, transform.position, healAmount);

        return healAmount;
    }

    public void Destroy()
    {
        isDestroyed = true;
        GameTiles.RemoveObject(transform.position);

        if (TryGetComponent(out Inventory inv))
        {
            if (inv.items.Count > 0)
                gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine("The " + name + " was destroyed and its contents spill out on the ground."));
            else
                gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine("The " + name + " was destroyed."));

            for (int i = 0; i < inv.items.Count; i++)
            {
                gm.dropItemController.DropItem(null, transform.position, inv.items[i], inv.items[i].currentStackSize, inv, null);
                inv.items[i].ReturnToObjectPool();
            }

            inv.items.Clear();
        }
        else
            gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine("The " + name + " was destroyed."));

        gameObject.SetActive(false);
    }

    public override int GetMaxHealth()
    {
        return maxHealth.GetValue();
    }

    public override bool IsDeadOrDestroyed()
    {
        return isDestroyed;
    }
}

using UnityEngine;
using System.Collections;

public class CharacterStats : MonoBehaviour
{
    public Stat maxAP;
    public int currentAP { get; private set; }

    public Stat maxPersonalInvWeight;
    public Stat maxPersonalInvVolume;

    public Stat maxHealth;
    public int currentHealth { get; private set; }

    public Stat damage;
    public Stat defense;

    [HideInInspector] public CharacterManager characterManager;
    [HideInInspector] public BoxCollider2D hitCollider;

    [HideInInspector] public bool canTakeDamage = true;

    void Awake()
    {
        currentHealth = maxHealth.GetValue();
    }

    void Start()
    {
        characterManager = GetComponentInParent<CharacterManager>();
        hitCollider = GetComponent<BoxCollider2D>();

        currentAP = maxAP.GetValue();

        characterManager.equipmentManager.onWearableChanged += OnWearableChanged;
        characterManager.equipmentManager.onWeaponChanged += OnWeaponChanged;
    }

    public int UseAPAndGetRemainder(int amount)
    {
        int remainingAmount = amount;
        if (currentAP >= amount)
        {
            UseAP(amount);
            remainingAmount = 0;
        }
        else
        {
            remainingAmount = amount - currentAP;
            UseAP(currentAP);
        }

        return remainingAmount;
    }

    public void UseAP(int amount)
    {
        if (characterManager.remainingAPToBeUsed > 0)
        {
            characterManager.remainingAPToBeUsed -= amount;
            if (characterManager.remainingAPToBeUsed < 0)
                characterManager.remainingAPToBeUsed = 0;
        }

        currentAP -= amount;
    }

    public void ReplenishAP()
    {
        currentAP = maxAP.GetValue();
    }

    public void AddToCurrentAP(int amountToAdd)
    {
        currentAP += amountToAdd;
    }

    public void TakeDamage(int damage)
    {
        if (canTakeDamage)
        {
            damage -= defense.GetValue();

            if (damage <= 0)
                damage = 1;

            currentHealth -= damage;
            if (currentHealth <= 0)
                Die();
        }
    }

    public IEnumerator TakeDamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(0.25f);
        canTakeDamage = true;
    }

    public virtual void Die()
    {
        Debug.Log(name + " died.");
        characterManager.boxCollider.enabled = false;
        characterManager.vision.visionCollider.enabled = false;
        characterManager.vision.enabled = false;
        hitCollider.enabled = false;

        if (characterManager.stateController != null)
            characterManager.stateController.enabled = false;
    }

    public virtual void OnWearableChanged(ItemData newItemData, ItemData oldItemData)
    {
        if (newItemData != null)
        {
            defense.AddModifier(newItemData.defense);
        }

        if (oldItemData != null)
        {
            defense.RemoveModifier(oldItemData.defense);
        }
    }

    public virtual void OnWeaponChanged(ItemData newItemData, ItemData oldItemData)
    {
        if (newItemData != null)
        {
            damage.AddModifier(newItemData.damage);
        }

        if (oldItemData != null)
        {
            damage.RemoveModifier(oldItemData.damage);
        }
    }
}

using UnityEngine;

public class CharacterStats : Stats
{
    public Stat maxAP;
    public int currentAP { get; private set; }

    public Stat maxPersonalInvWeight;
    public Stat maxPersonalInvVolume;

    public Stat damage;
    public Stat defense;

    [HideInInspector] public CharacterManager characterManager;

    void Start()
    {
        characterManager = GetComponentInParent<CharacterManager>();

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

    public override void TakeDamage(int damage)
    {
        damage -= defense.GetValue();
        base.TakeDamage(damage);
    }

    public override void Die()
    {
        Debug.Log(name + " died.");
        characterManager.circleCollider.enabled = false;
        characterManager.vision.visionCollider.enabled = false;
        characterManager.vision.enabled = false;

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

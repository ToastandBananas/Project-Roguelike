using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodyPart
{
    public BodyPartType bodyPartType;

    [Header("Health")]
    public Stat maxHealth;
    public int currentHealth;
    public List<LocationalInjury> injuries = new List<LocationalInjury>();

    [Header("Buildups")]
    public float damageBuildup;
    public float healingBuildup;

    [Header("Defense")]
    public Stat naturalDefense;
    public Stat addedDefense_Armor, addedDefense_Clothing;

    [HideInInspector] public CharacterManager characterManager;

    public BodyPart(CharacterManager characterManager, BodyPartType bodyPartType, int baseMaxHealth)
    {
        this.characterManager = characterManager;
        this.bodyPartType = bodyPartType;
        maxHealth.SetBaseValue(baseMaxHealth);
        currentHealth = baseMaxHealth;
    }

    public int Damage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0)
            currentHealth = 0;

        if (characterManager.isNPC == false)
            HealthDisplay.instance.UpdateHealthText(bodyPartType);

        return currentHealth;
    }

    public int HealInstant_StaticValue(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth.GetValue())
            currentHealth = maxHealth.GetValue();

        if (characterManager.isNPC == false)
            HealthDisplay.instance.UpdateHealthText(bodyPartType);

        return currentHealth;
    }

    public int HealInstant_Percent(float healPercent)
    {
        int healAmount = Mathf.RoundToInt(maxHealth.GetValue() * healPercent);
        if (currentHealth + healAmount > maxHealth.GetValue()) // Make sure not to heal over the max health
        {
            healAmount = maxHealth.GetValue() - currentHealth;
            currentHealth += healAmount;
        }
        else
            currentHealth += healAmount;

        if (characterManager.isNPC == false)
            HealthDisplay.instance.UpdateHealthText(bodyPartType);

        return currentHealth;
    }

    public bool IsBleeding()
    {
        for (int i = 0; i < injuries.Count; i++)
        {
            if (injuries[i].bleedTimeRemaining > 0)
                return true;
        }
        return false;
    }
}

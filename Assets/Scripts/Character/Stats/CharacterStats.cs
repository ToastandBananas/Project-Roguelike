using UnityEngine;

public class CharacterStats : Stats
{
    [Header("Main Stats")]
    public IntStat agility;      // Evasion & block chance
    public IntStat constitution; // Max health
    public IntStat dexterity;    // Physical attack accuracy & critical hit chance
    public IntStat endurance;    // Max stamina
    public IntStat intelligence; // Magic power
    public IntStat speed;        // Max AP
    public IntStat strength;     // Max carrying capacity & melee damage
    public IntStat wisdom;       // Max MP & MP regen

    [Header("Personal Inventory")]
    public FloatStat maxPersonalInvVolume;

    [Header("Skills")]
    public IntStat martialArtsSkill;
    public IntStat shieldSkill;
    public IntStat swordSkill;

    // [Header("AP")]
    public int currentAP { get; private set; }
    int APLossBuildup;
    readonly int baseAP = 25;

    [HideInInspector] public float totalAgilityMods, totalConstitutionMods, totalDexterityMods, totalEnduranceMods, totalIntelligenceMods, totalSpeedMods, totalStrengthMods, totalWisdomMods;
    [HideInInspector] public CharacterManager characterManager;

    readonly float baseCarryWeight = 10;

    void Awake()
    {
        ReplenishAP();
    }

    #region AP
    public int MaxAP()
    {
        if (speed.GetValue() > 0)
            return Mathf.RoundToInt(baseAP + (speed.GetValue() * 1.5f));
        else
            return baseAP;
    }

    public int UseAPAndGetRemainder(int amount)
    {
        // Debug.Log("Current AP: " + currentAP);
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

        //Debug.Log("Remaining amount: " + remainingAmount);
        return remainingAmount;
    }

    public void UseAP(int amount)
    {
        // Debug.Log("AP used: " + amount);
        currentAP -= amount;
        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateAPText();
    }

    public void ReplenishAP()
    {
        currentAP = MaxAP();
    }

    public void AddToCurrentAP(int amountToAdd)
    {
        currentAP += amountToAdd;
    }

    public void AddToAPLossBuildup(int amount)
    {
        APLossBuildup += amount;
    }

    public void ApplyAPLossBuildup()
    {
        if (currentAP > APLossBuildup)
        {
            currentAP -= APLossBuildup;
            APLossBuildup = 0;
        }
        else
        {
            APLossBuildup -= currentAP;
            currentAP = 0;
            StartCoroutine(gm.turnManager.FinishTurn(characterManager));
        }
    }

    public void SetAPLossBuildup(int amount)
    {
        APLossBuildup = amount;
    }

    public int GetAPLossBuildup()
    {
        return APLossBuildup;
    }
    #endregion

    #region Stat Mods
    public void AdjustTotalAgilityMods(float amount)
    {
        characterManager.characterStats.agility.RemoveModifier(Mathf.RoundToInt(totalAgilityMods));
        totalAgilityMods += amount;
        characterManager.characterStats.agility.AddModifier(Mathf.RoundToInt(totalAgilityMods));
    }

    public void AdjustTotalConstitutionMods(float amount)
    {
        characterManager.characterStats.constitution.RemoveModifier(Mathf.RoundToInt(totalConstitutionMods));
        totalConstitutionMods += amount;
        characterManager.characterStats.constitution.AddModifier(Mathf.RoundToInt(totalConstitutionMods));
    }

    public void AdjustTotalDexterityMods(float amount)
    {
        characterManager.characterStats.dexterity.RemoveModifier(Mathf.RoundToInt(totalDexterityMods));
        totalDexterityMods += amount;
        characterManager.characterStats.dexterity.AddModifier(Mathf.RoundToInt(totalDexterityMods));
    }

    public void AdjustTotalEnduranceMods(float amount)
    {
        characterManager.characterStats.endurance.RemoveModifier(Mathf.RoundToInt(totalEnduranceMods));
        totalEnduranceMods += amount;
        characterManager.characterStats.endurance.AddModifier(Mathf.RoundToInt(totalEnduranceMods));
    }

    public void AdjustTotalIntelligenceMods(float amount)
    {
        characterManager.characterStats.intelligence.RemoveModifier(Mathf.RoundToInt(totalIntelligenceMods));
        totalIntelligenceMods += amount;
        characterManager.characterStats.intelligence.AddModifier(Mathf.RoundToInt(totalIntelligenceMods));
    }

    public void AdjustTotalSpeedMods(float amount)
    {
        characterManager.characterStats.speed.RemoveModifier(Mathf.RoundToInt(totalSpeedMods));
        totalSpeedMods += amount;
        characterManager.characterStats.speed.AddModifier(Mathf.RoundToInt(totalSpeedMods));
    }

    public void AdjustTotalStrengthMods(float amount)
    {
        characterManager.characterStats.strength.RemoveModifier(Mathf.RoundToInt(totalStrengthMods));
        totalStrengthMods += amount;
        characterManager.characterStats.strength.AddModifier(Mathf.RoundToInt(totalStrengthMods));
    }

    public void AdjustTotalWisdomMods(float amount)
    {
        characterManager.characterStats.wisdom.RemoveModifier(Mathf.RoundToInt(totalWisdomMods));
        totalWisdomMods += amount;
        characterManager.characterStats.wisdom.AddModifier(Mathf.RoundToInt(totalWisdomMods));
    }
    #endregion

    public float BlockSkill(Weapon weapon)
    {
        return (WeaponSkill(weapon) + agility.GetValue()) / 2f;
    }

    public float BlockSkill(Shield shield)
    {
        return (shieldSkill.GetValue() + agility.GetValue()) / 2f;
    }

    public float EvasionSkill()
    {
        return agility.GetValue();
    }

    public float CriticalChance()
    {
        return (strength.GetValue() + dexterity.GetValue()) / 10f;
    }

    public int WeaponSkill(Weapon weapon)
    {
        if (weapon.weaponType == WeaponType.Sword)
            return swordSkill.GetValue();
        return 0;
    }

    public int UnarmedDamage()
    {
        return Mathf.RoundToInt((martialArtsSkill.GetValue() + strength.GetValue()) / Random.Range(3.8f, 4.2f));
    }

    public float AttackAccuracy(Weapon weapon)
    {
        if (weapon != null)
            return (dexterity.GetValue() + WeaponSkill(weapon)) / 2f;
        else
            return (dexterity.GetValue() + martialArtsSkill.GetValue()) / 2f;
    }

    /// <summary>This is the amount that a character can carry before they become over-encumbered.</summary>
    public float MaximumWeightCapacity()
    {
        return baseCarryWeight + (strength.GetValue() * 2f);
    }

    public override bool IsDeadOrDestroyed()
    {
        return characterManager.status.isDead;
    }
}
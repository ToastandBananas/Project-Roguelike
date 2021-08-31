using UnityEngine;

public class CharacterStats : Stats
{
    [Header("Main Stats")]
    public IntStat agility;
    public IntStat constitution;
    public IntStat dexterity;
    public IntStat endurance;
    public IntStat intelligence;
    public IntStat speed;
    public IntStat strength;

    //[Header("AP")]
    public int currentAP { get; private set; }
    int APLossBuildup;
    readonly int baseAP = 25;

    [Header("Personal Inv. Volume")]
    public FloatStat maxPersonalInvVolume;

    [Header("Skills")]
    public IntStat swordSkill;

    [Header("Combat")]
    public IntStat unarmedDamage;
    public FloatStat meleeAccuracy;
    public FloatStat rangedAccuracy;
    public FloatStat evasion;
    public FloatStat shieldBlock;
    public FloatStat weaponBlock;

    [HideInInspector] public CharacterManager characterManager;

    readonly float baseCarryWeight = 10;

    void Awake()
    {
        ReplenishAP();
    }

    public int MaxAP()
    {
        return Mathf.RoundToInt(baseAP + (speed.GetValue() * 1.5f));
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

    public float GetCriticalChance()
    {
        return (strength.GetValue() + dexterity.GetValue()) / 10f;
    }

    public int GetWeaponSkill(Weapon weapon)
    {
        if (weapon.weaponType == WeaponType.Sword)
            return swordSkill.GetValue();
        return 0;
    }

    /// <summary>This is the amount that a character can carry before they become over-encumbered.</summary>
    public float GetMaximumWeightCapacity()
    {
        return baseCarryWeight + (strength.GetValue() * 2f);
    }

    public override bool IsDeadOrDestroyed()
    {
        return characterManager.status.isDead;
    }
}
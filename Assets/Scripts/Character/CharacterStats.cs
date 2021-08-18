using UnityEngine;

public class CharacterStats : Stats
{
    [Header("Main Stats")]
    public IntStat agility;
    public IntStat constitution;
    public IntStat dexterity;
    public IntStat endurance;
    public IntStat speed;
    public IntStat strength;

    [Header("AP")]
    public IntStat maxAP;
    public int currentAP { get; private set; }
    int APLossBuildup;

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

    public override void Awake()
    {
        base.Awake();

        currentAP = maxAP.GetValue();
    }

    public override void Start()
    {
        base.Start();
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
    }

    public void ReplenishAP()
    {
        currentAP = maxAP.GetValue();
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

    public int GetWeaponSkill(Weapon weapon)
    {
        if (weapon.weaponType == WeaponType.Sword)
            return swordSkill.GetValue();
        return 0;
    }

    public float GetCriticalChance()
    {
        return (strength.GetValue() + dexterity.GetValue()) / 10f;
    }
}
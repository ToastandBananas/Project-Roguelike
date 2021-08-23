using UnityEngine;

public enum MeleeAttackType { Swipe, Thrust, Overhead, Unarmed }
public enum PhysicalDamageType { Slash, Blunt, Pierce, Cleave }
public enum WeaponType { Sword, Dagger, Axe, SpikedAxe, Club, SpikedClub, Mace, SpikedMace, Hammer, SpikedHammer, Flail, SpikedFlail, Staff, Spear, Polearm, BluntPolearm,
                            Sling, Bow, Crossbow, ThrowingKnife, ThrowingAxe, ThrowingStar, ThrowingClub }

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : Equipment
{
    [Header("Weapon Stats")]
    public WeaponType weaponType;
    public MeleeAttackType defaultMeleeAttackType;
    public int attackRange = 1;
    public int strengthRequirement_OneHand;

    [Header("Swipe Attack Damage")]
    public Vector2Int bluntDamage_Swipe;
    public Vector2Int pierceDamage_Swipe;
    public Vector2Int slashDamage_Swipe;
    public Vector2Int cleaveDamage_Swipe;

    [Header("Thrust Attack Damage")]
    public Vector2Int bluntDamage_Thrust;
    public Vector2Int pierceDamage_Thrust;
    public Vector2Int slashDamage_Thrust;
    public Vector2Int cleaveDamage_Thrust;

    [Header("Overhead Attack Damage")]
    public Vector2Int bluntDamage_Overhead;
    public Vector2Int pierceDamage_Overhead;
    public Vector2Int slashDamage_Overhead;
    public Vector2Int cleaveDamage_Overhead;

    [Header("Block")]
    [Range(0f, 1.8f)] public float minBlockChanceMultiplier = 0.2f;
    [Range(0f, 1.8f)] public float maxBlockChanceMultiplier = 0.3f;

    public bool CanOneHand(CharacterManager characterManager)
    {
        if (characterManager.characterStats.strength.GetValue() >= strengthRequirement_OneHand)
            return true;
        return false;
    }

    public bool IsTwoHanded(CharacterManager characterManager)
    {
        if (CanOneHand(characterManager) == false)
            return true;
        return false;
    }

    public int StrengthRequired_TwoHand()
    {
        return Mathf.RoundToInt(strengthRequirement_OneHand / 2f);
    }

    public override bool IsWeapon()
    {
        return true;
    }
}

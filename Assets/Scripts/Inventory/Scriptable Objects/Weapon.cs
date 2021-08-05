using UnityEngine;

public enum WeaponType { Sword, Dagger, Axe, SpikedAxe, Club, SpikedClub, Mace, SpikedMace, Hammer, SpikedHammer, Flail, SpikedFlail, Staff, Spear, Polearm, BluntPolearm,
                            Sling, Bow, Crossbow, ThrowingKnife, ThrowingAxe, ThrowingStar, ThrowingClub }

public enum MeleeAttackType { Swipe, Thrust, Overhead, Unarmed }
public enum PhysicalDamageType { Slash, Blunt, Pierce, Cleave }

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : Equipment
{
    [Header("Weapon Stats")]
    public WeaponType weaponType;
    public MeleeAttackType defaultMeleeAttackType;
    public bool isTwoHanded;
    public int attackRange = 1;

    [Header("Swipe Attack Damage")]
    public int minBaseBluntDamage_Swipe = 1;
    public int maxBaseBluntDamage_Swipe = 5;
    public int minBasePierceDamage_Swipe;
    public int maxBasePierceDamage_Swipe;
    public int minBaseSlashDamage_Swipe;
    public int maxBaseSlashDamage_Swipe;
    public int minBaseCleaveDamage_Swipe;
    public int maxBaseCleaveDamage_Swipe;

    [Header("Thrust Attack Damage")]
    public int minBaseBluntDamage_Thrust;
    public int maxBaseBluntDamage_Thrust;
    public int minBasePierceDamage_Thrust = 1;
    public int maxBasePierceDamage_Thrust = 5;
    public int minBaseSlashDamage_Thrust;
    public int maxBaseSlashDamage_Thrust;
    public int minBaseCleaveDamage_Thrust;
    public int maxBaseCleaveDamage_Thrust;

    [Header("Overhead Attack Damage")]
    public int minBaseBluntDamage_Overhead = 1;
    public int maxBaseBluntDamage_Overhead = 5;
    public int minBasePierceDamage_Overhead;
    public int maxBasePierceDamage_Overhead;
    public int minBaseSlashDamage_Overhead;
    public int maxBaseSlashDamage_Overhead;
    public int minBaseCleaveDamage_Overhead;
    public int maxBaseCleaveDamage_Overhead;

    [Header("Block")]
    [Range(0f, 1.8f)] public float minBlockChanceMultiplier = 0.2f;
    [Range(0f, 1.8f)] public float maxBlockChanceMultiplier = 0.3f;

    public override bool IsWeapon()
    {
        return true;
    }
}

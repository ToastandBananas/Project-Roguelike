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
    public int minBaseDamage = 1;
    public int maxBaseDamage = 5;
    public int minBaseTreeDamage, maxBaseTreeDamage;
    [Range(0f, 1.8f)] public float minBlockChanceMultiplier = 0.2f;
    [Range(0f, 1.8f)] public float maxBlockChanceMultiplier = 0.3f;

    public override bool IsWeapon()
    {
        return true;
    }
}

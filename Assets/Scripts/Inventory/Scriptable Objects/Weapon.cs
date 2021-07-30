using UnityEngine;

public enum WeaponType { Sword, Dagger, Axe, Club, Mace, Hammer, Flail, Staff, Spear, Polearm, Sling, Bow, Crossbow, Throwing }

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : Equipment
{
    [Header("Weapon Stats")]
    public WeaponType weaponType;
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

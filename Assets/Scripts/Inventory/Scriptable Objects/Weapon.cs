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

    public override bool IsWeapon()
    {
        return true;
    }
}

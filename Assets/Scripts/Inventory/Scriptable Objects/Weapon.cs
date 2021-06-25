using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : Equipment
{
    [Header("Weapon Stats")]
    public bool isTwoHanded;
    public float attackCooldown = 0.5f;
    public int minBaseDamage = 1;
    public int maxBaseDamage = 5;
    public int minBaseTreeDamage, maxBaseTreeDamage;

    [Header("Weapon Trail")]
    public float weaponLength = 1f;
    public float leftWeaponTrailOffset = 0.15f;

    public override bool IsWeapon()
    {
        return true;
    }
}

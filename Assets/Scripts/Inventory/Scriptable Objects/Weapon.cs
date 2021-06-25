using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : Equipment
{
    [Header("Weapon Stats")]
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

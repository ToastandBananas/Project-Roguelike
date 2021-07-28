using UnityEngine;

[CreateAssetMenu(fileName = "New Shield", menuName = "Inventory/Shield")]
public class Shield : Equipment
{
    [Header("Basic Shield Data")]
    public int minShieldBashDamage = 1;
    public int maxShieldBashDamage = 5;

    public override bool IsShield()
    {
        return true;
    }
}

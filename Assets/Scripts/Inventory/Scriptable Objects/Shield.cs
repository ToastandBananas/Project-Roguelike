using UnityEngine;

public class Shield : Equipment
{
    [Header("Basic Shield Data")]
    public int minShieldBashDamage = 1;
    public int maxShieldBashDamage = 5;
    public int minBaseDefense = 1;
    public int maxBaseDefense = 5;

    public override bool IsShield()
    {
        return true;
    }
}

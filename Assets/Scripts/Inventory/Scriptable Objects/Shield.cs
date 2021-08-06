using UnityEngine;

[CreateAssetMenu(fileName = "New Shield", menuName = "Inventory/Shield")]
public class Shield : Equipment
{
    [Header("Basic Shield Data")]
    public Vector2Int shieldBashDamage;
    [Range(0.2f, 2f)] public float minBlockChanceMultiplier = 0.8f;
    [Range(0.2f, 2f)] public float maxBlockChanceMultiplier = 1.2f;

    public override bool IsShield()
    {
        return true;
    }
}

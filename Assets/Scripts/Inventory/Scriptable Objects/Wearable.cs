using UnityEngine;

[CreateAssetMenu(fileName = "New Wearable", menuName = "Inventory/Wearable")]
public class Wearable : Equipment
{
    [Header("Armor Stats")]
    public int minBaseDefense = 1;
    public int maxBaseDefense = 5;
    public int coldResistance;
    public int heatResistance;

    public override bool IsWearable()
    {
        return true;
    }
}

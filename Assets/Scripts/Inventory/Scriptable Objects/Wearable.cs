using UnityEngine;

[CreateAssetMenu(fileName = "New Wearable", menuName = "Inventory/Wearable")]
public class Wearable : Equipment
{
    [Header("Defense")]
    public Vector2Int torsoDefense;
    public Vector2Int headDefense;
    public Vector2Int armDefense;
    public Vector2Int handDefense;
    public Vector2Int legDefense;
    public Vector2Int footDefense;

    [Header("Weather Resistance")]
    public int coldResistance;
    public int heatResistance;

    public override bool IsWearable()
    {
        return true;
    }

    public bool IsMetallic()
    {
        if (mainMaterial == ItemMaterial.Copper || mainMaterial == ItemMaterial.Bronze || mainMaterial == ItemMaterial.Iron || mainMaterial == ItemMaterial.Brass
                 || mainMaterial == ItemMaterial.Steel || mainMaterial == ItemMaterial.Mithril || mainMaterial == ItemMaterial.Dragonscale)
            return true;
        return false;
    }

    public bool IsWooden()
    {
        if (mainMaterial == ItemMaterial.Wood || mainMaterial == ItemMaterial.Bark)
            return true;
        return false;
    }
}

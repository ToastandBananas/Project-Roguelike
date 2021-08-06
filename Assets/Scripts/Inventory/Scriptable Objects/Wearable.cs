using UnityEngine;

[CreateAssetMenu(fileName = "New Wearable", menuName = "Inventory/Wearable")]
public class Wearable : Equipment
{
    [Header("Defense")]
    public bool isClothing;
    public BodyPartType[] primaryBodyPartsCovered, secondaryBodyPartsCovered, tertiaryBodyPartsCovered;
    public Vector2Int primaryDefense;
    public Vector2Int secondaryDefense;
    public Vector2Int tertiaryDefense;

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

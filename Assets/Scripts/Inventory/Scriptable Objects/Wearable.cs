using UnityEngine;

[CreateAssetMenu(fileName = "New Wearable", menuName = "Inventory/Wearable")]
public class Wearable : Equipment
{
    [Header("Defense")]
    public int minBaseTorsoDefense;
    public int maxBaseTorsoDefense;
    public int minBaseHeadDefense;
    public int maxBaseHeadDefense;
    public int minBaseArmDefense;
    public int maxBaseArmDefense;
    public int minBaseHandDefense;
    public int maxBaseHandDefense;
    public int minBaseLegDefense;
    public int maxBaseLegDefense;
    public int minBaseFootDefense;
    public int maxBaseFootDefense;

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

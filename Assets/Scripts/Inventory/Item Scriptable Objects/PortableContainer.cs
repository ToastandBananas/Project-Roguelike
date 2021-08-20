using UnityEngine;

[CreateAssetMenu(fileName = "New Portable Container", menuName = "Inventory/PortableContainer")]
public class PortableContainer : Item
{
    [Header("Inventory")]
    public float maxWeight = 50f;
    public float maxVolume = 20f;
    public float singleItemVolumeLimit;

    public void SetupPortableContainerInventory(Inventory portableContainerInv)
    {
        portableContainerInv.maxWeight = maxWeight;
        portableContainerInv.maxVolume = maxVolume;
        portableContainerInv.singleItemVolumeLimit = singleItemVolumeLimit;
    }

    public override bool IsPortableContainer()
    {
        return true;
    }
}
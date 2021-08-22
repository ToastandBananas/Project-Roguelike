using UnityEngine;

[CreateAssetMenu(fileName = "New Bag", menuName = "Inventory/Bag")]
public class Bag : Equipment
{
    public Sprite sidebarSprite;

    [Header("Inventory")]
    public float maxWeight = 50f;
    public float maxVolume = 20f;
    public float singleItemVolumeLimit;

    public override void Use(CharacterManager characterManager, Inventory inventory, InventoryItem invItem, ItemData itemData, int itemCount, PartialAmount partialAmountToUse = PartialAmount.Whole, EquipmentSlot equipSlot = EquipmentSlot.Backpack)
    {
        // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
        if (itemData.IsPickup() && itemData.bagInventory == GameManager.instance.containerInvUI.activeInventory)
            GameManager.instance.containerInvUI.RemoveBagFromGround(itemData.bagInventory);

        base.Use(characterManager, inventory, invItem, itemData, itemCount, partialAmountToUse, equipSlot);
    }

    public void SetupBagInventory(Inventory bagInv)
    {
        bagInv.maxWeight = maxWeight;
        bagInv.maxVolume = maxVolume;
        bagInv.singleItemVolumeLimit = singleItemVolumeLimit;
    }

    public override bool IsBag()
    {
        return true;
    }
}

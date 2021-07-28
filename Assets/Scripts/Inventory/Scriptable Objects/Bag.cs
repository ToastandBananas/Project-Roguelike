using UnityEngine;

[CreateAssetMenu(fileName = "New Bag", menuName = "Inventory/Bag")]
public class Bag : Equipment
{
    public Sprite sidebarSprite;

    [Header("Inventory")]
    public float maxWeight = 50f;
    public float maxVolume = 20f;
    public float singleItemVolumeLimit;

    public override void Use(CharacterManager characterManager, EquipmentSlot equipSlot, Inventory inventory, InventoryItem invItem, int itemCount)
    {
        // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
        if (invItem.myInvUI == GameManager.instance.containerInvUI && invItem.itemData.bagInventory == GameManager.instance.containerInvUI.activeInventory)
            GameManager.instance.containerInvUI.RemoveBagFromGround(invItem.itemData.bagInventory);

        base.Use(characterManager, equipSlot, inventory, invItem, itemCount);
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

using UnityEngine;

public enum EquipmentSlot { Helmet, Shirt, Pants, Boots, Gloves, BodyArmor, LegArmor, RightWeapon, LeftWeapon, Ranged, Ammunition }

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    [Header("General Equipment Stats")]
    public EquipmentSlot equipmentSlot;
    public int minBaseDurability = 75;
    public int maxBaseDurability = 100;

    public override void Use(EquipmentManager equipmentManager, Inventory inventory, InventoryItem invItem, int itemCount)
    {
        base.Use(equipmentManager, inventory, invItem, itemCount);

        // Equip the item
        equipmentManager.Equip(invItem.itemData, equipmentSlot);

        // Remove it from the equipment inventory
        RemoveFromInventory(inventory, itemCount, invItem);
    }

    public override bool IsEquipment()
    {
        return true;
    }
}

using UnityEngine;

public enum EquipmentSlot { Helmet, Shirt, Pants, Boots, Gloves, BodyArmor, LegArmor, RightWeapon, LeftWeapon, Ranged, Quiver, Backpack, LeftHipPouch, RightHipPouch }

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    [Header("General Equipment Stats")]
    public EquipmentSlot equipmentSlot;
    public int minBaseDurability = 75;
    public int maxBaseDurability = 100;

    [Header("Sprites")]
    public Sprite equippedSprite;

    public override void Use(EquipmentManager equipmentManager, Inventory inventory, InventoryItem invItem, int itemCount)
    {
        // Equip the item
        if (invItem.myEquipmentManager == null)
            equipmentManager.Equip(invItem.itemData, invItem, equipmentSlot);
        else
        {
            // Unequip the item
            EquipmentSlot equipSlot = invItem.myEquipmentManager.GetEquipmentSlotFromItemData(invItem.itemData);
            equipmentManager.Unequip(equipSlot, true);
        }

        base.Use(equipmentManager, inventory, invItem, itemCount);
    }

    public override bool IsEquipment()
    {
        return true;
    }
}

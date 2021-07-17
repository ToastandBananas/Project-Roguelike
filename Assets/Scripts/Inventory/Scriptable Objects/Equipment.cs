using UnityEngine;

public enum EquipmentSlot { Helmet, Shirt, Pants, Boots, Gloves, BodyArmor, LegArmor, LeftWeapon, RightWeapon, Ranged, Quiver, Backpack, LeftHipPouch, RightHipPouch }

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    [Header("General Equipment Stats")]
    public EquipmentSlot equipmentSlot;
    public int minBaseDurability = 75;
    public int maxBaseDurability = 100;

    [Header("Sprites")]
    public Sprite primaryEquippedSprite;
    public Sprite secondaryEquippedSprite;

    public override void Use(EquipmentManager equipmentManager, EquipmentSlot equipSlot, Inventory inventory, InventoryItem invItem, int itemCount)
    {
        bool itemUsed = false;
        bool itemEquipped = false;
        Equipment newEquipment = (Equipment)invItem.itemData.item;

        // Setup temporary ItemDatas so that we can update the characters stats when they finish equipping/unequipping the item
        ItemData oldItemData = null;
        if (equipmentManager.currentEquipment[(int)equipSlot] != null)
        {
            oldItemData = GameManager.instance.objectPoolManager.GetItemDataFromPool(equipmentManager.currentEquipment[(int)equipSlot].item);
            oldItemData.gameObject.SetActive(true);
            oldItemData.TransferData(equipmentManager.currentEquipment[(int)equipSlot], oldItemData);
            Inventory playersInv = null;
            if (invItem.itemData.item.IsBag())
            {
                playersInv = PlayerInventoryUI.instance.GetInventoryFromBagEquipSlot(invItem.itemData);
                oldItemData.bagInventory.currentWeight = playersInv.currentWeight;
                oldItemData.bagInventory.currentVolume = playersInv.currentVolume;
            }
        }

        ItemData itemDataUsing = GameManager.instance.objectPoolManager.GetItemDataFromPool(invItem.itemData.item);
        itemDataUsing.gameObject.SetActive(true);
        itemDataUsing.TransferData(invItem.itemData, itemDataUsing);
        if (invItem.itemData.item.IsBag())
        {
            itemDataUsing.bagInventory.currentWeight = invItem.itemData.bagInventory.currentWeight;
            itemDataUsing.bagInventory.currentVolume = invItem.itemData.bagInventory.currentVolume;
        }

        // Equip the item
        if (invItem.myEquipmentManager == null)
        {
            itemUsed = equipmentManager.Equip(invItem.itemData, invItem, equipSlot);
            itemEquipped = true;
        }
        else
        {
            // Unequip the item
            EquipmentSlot itemDatasEquipSlot = invItem.myEquipmentManager.GetEquipmentSlotFromItemData(invItem.itemData);
            itemUsed = equipmentManager.Unequip(itemDatasEquipSlot, true, true);
        }

        if (itemUsed)
        {
            if (itemEquipped)
                equipmentManager.StartCoroutine(equipmentManager.UseAPAndSetupEquipment(newEquipment, equipSlot, itemDataUsing, oldItemData));
            else
                equipmentManager.StartCoroutine(equipmentManager.UseAPAndSetupEquipment(newEquipment, equipSlot, null, oldItemData));

            base.Use(equipmentManager, equipSlot, inventory, invItem, itemCount);
            GameManager.instance.playerInvUI.UpdateUI();
            GameManager.instance.containerInvUI.UpdateUI();
        }
    }

    public override bool IsEquipment()
    {
        return true;
    }
}

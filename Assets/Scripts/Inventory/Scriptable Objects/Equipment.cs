using UnityEngine;

public enum EquipmentSlot { Helmet, Shirt, Pants, Boots, Gloves, BodyArmor, LegArmor, LeftWeapon, RightWeapon, Ranged, Quiver, Backpack, LeftHipPouch, RightHipPouch, Cape }

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    [Header("General Equipment Stats")]
    public EquipmentSlot equipmentSlot;
    public Vector2Int baseDurability;

    [Header("Sprites")]
    public Sprite primaryEquippedSprite;
    public Sprite secondaryEquippedSprite;

    public override void Use(CharacterManager characterManager, Inventory inventory, InventoryItem invItem, ItemData itemData, int itemCount, EquipmentSlot equipSlot)
    {
        if (itemData.durability <= 0)
        {
            GameManager.instance.flavorText.WriteTryEquipBrokenItemLine(itemData, characterManager);
            return;
        }

        bool itemUsed = false;
        bool itemEquipped = false;
        Equipment newEquipment = (Equipment)itemData.item;

        // Setup temporary ItemDatas so that we can update the characters stats when they finish equipping/unequipping the item
        ItemData oldItemData = null;
        if (characterManager.equipmentManager.currentEquipment[(int)equipSlot] != null)
        {
            oldItemData = GameManager.instance.objectPoolManager.GetItemDataFromPool(characterManager.equipmentManager.currentEquipment[(int)equipSlot].item);
            oldItemData.gameObject.SetActive(true);
            oldItemData.TransferData(characterManager.equipmentManager.currentEquipment[(int)equipSlot], oldItemData);
            Inventory playersInv = null;
            if (itemData.item.IsBag())
            {
                playersInv = PlayerInventoryUI.instance.GetInventoryFromBagEquipSlot(itemData);
                oldItemData.bagInventory.currentWeight = playersInv.currentWeight;
                oldItemData.bagInventory.currentVolume = playersInv.currentVolume;
            }
        }

        ItemData itemDataUsing = GameManager.instance.objectPoolManager.GetItemDataFromPool(itemData.item);
        itemDataUsing.gameObject.SetActive(true);
        itemDataUsing.TransferData(itemData, itemDataUsing);
        if (itemData.item.IsBag())
        {
            itemDataUsing.bagInventory.currentWeight = itemData.bagInventory.currentWeight;
            itemDataUsing.bagInventory.currentVolume = itemData.bagInventory.currentVolume;
        }

        // If this item is already equipped
        if (itemData.IsEquipped())
        {
            // Unequip the item
            EquipmentSlot itemDatasEquipSlot = characterManager.equipmentManager.GetEquipmentSlotFromItemData(itemData);
            itemUsed = characterManager.equipmentManager.Unequip(itemDatasEquipSlot, true, true, false);
        }
        else
        {
            // Equip the item
            itemUsed = characterManager.equipmentManager.Equip(itemData, invItem, equipSlot);
            itemEquipped = true;
        }

        if (itemUsed)
        {
            // Remove the item from the ItemDatas dictionary if it was on the ground
            if (itemData.IsPickup())
                GameTiles.RemoveItemData(itemData, itemData.transform.position);

            float newBagInvWeight = 0;
            float oldBagInvWeight = 0;
            if (itemDataUsing != null && itemDataUsing.item.IsBag())
                newBagInvWeight += itemDataUsing.bagInventory.currentWeight;
            if (oldItemData != null && oldItemData.item.IsBag())
                oldBagInvWeight += oldItemData.bagInventory.currentWeight;

            int APCost = 0;
            if (itemDataUsing != null)
                APCost += GameManager.instance.apManager.GetEquipAPCost(newEquipment, newBagInvWeight);
            if (oldItemData != null)
                APCost += GameManager.instance.apManager.GetEquipAPCost((Equipment)oldItemData.item, oldBagInvWeight);

            if (itemEquipped)
            {
                GameManager.instance.StartCoroutine(GameManager.instance.apManager.UseAP(characterManager, APCost));
                characterManager.equipmentManager.StartCoroutine(characterManager.equipmentManager.SetUpEquipment(itemDataUsing, oldItemData, newEquipment, equipSlot, false));
            }
            else
            {
                GameManager.instance.StartCoroutine(GameManager.instance.apManager.UseAP(characterManager, APCost));
                characterManager.equipmentManager.StartCoroutine(characterManager.equipmentManager.SetUpEquipment(null, oldItemData, newEquipment, equipSlot, false));
            }

            base.Use(characterManager, inventory, invItem, itemData, itemCount, equipSlot);

            GameManager.instance.playerInvUI.UpdateUI();
            GameManager.instance.containerInvUI.UpdateUI();
        }
    }

    public override bool IsEquipment()
    {
        return true;
    }
}

using UnityEngine;

public enum ItemType { Item, Weapon, Ammo, Clothing, Armor, Food, Drink, Ingredient, Seed, Readable, Key, QuestItem, Bag, Container }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("General Item Info")]
    new public string name = "New Item";
    public ItemType itemType;
    public string description;
    public int maxStackSize = 1;
    public float weight = 0.1f;
    public float volume = 0.1f;
    public bool isUsable = true;

    [Header("Value")]
    public int minBaseValue;
    public int maxBaseValue;
    public int staticValue = 1;

    [Header("Pickup Sprite")]
    public Sprite pickupSprite;

    public virtual void Use(EquipmentManager equipmentManager, Inventory inventory, InventoryItem invItem, int itemCount)
    {
        if (invItem != null && invItem.itemData != null)
        {
            if (isUsable)
            {
                invItem.itemData.currentStackSize -= itemCount;

                // If there's none left, remove the item
                if (invItem.itemData.currentStackSize <= 0)
                {
                    if (inventory != null) // If using an item that's inside and inventory
                    {
                        // Remove it from the inventory
                        RemoveFromInventory(inventory, itemCount, invItem);
                    }
                    else if (invItem.myEquipmentManager == null) // If using an item that was on the ground
                    {
                        invItem.gm.containerInvUI.GetItemsListFromActiveDirection().Remove(invItem.itemData);
                        invItem.ClearItem();
                    }
                }
                else
                    invItem.UpdateItemNumberTexts();
            }
            else
            {
                Debug.Log("You cannot use this item");
                return;
            }
        }
    }

    public void RemoveFromInventory(Inventory inventory, int itemCount, InventoryItem invItem)
    {
        inventory.RemoveItem(invItem.itemData, itemCount, invItem);
    }

    public virtual bool IsEquipment()
    {
        return false;
    }

    public virtual bool IsWeapon()
    {
        return false;
    }

    public virtual bool IsWearable()
    {
        return false;
    }

    public virtual bool IsShield()
    {
        return false;
    }

    public virtual bool IsBag()
    {
        return false;
    }

    public virtual bool IsPortableContainer()
    {
        return false;
    }

    public virtual bool IsConsumable()
    {
        return false;
    }

    public virtual bool IsKey()
    {
        return false;
    }
}

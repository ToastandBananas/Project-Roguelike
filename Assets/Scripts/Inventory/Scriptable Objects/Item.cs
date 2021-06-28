using UnityEngine;

public enum ItemType { Item, Weapon, Ammo, Clothing, Armor, Food, Drink, Ingredient, Seed, Readable, Key, QuestItem }

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

    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite pickupSprite;

    public virtual void Use(EquipmentManager equipmentManager, Inventory inventory, InventoryItem inventorySlot, int itemCount)
    {
        if (inventorySlot != null && isUsable)
        {
            inventorySlot.itemData.currentStackSize -= itemCount;
            inventorySlot.UpdateItemTexts();
        }
    }

    public void RemoveFromInventory(Inventory inventory, int itemCount, InventoryItem inventorySlot)
    {
        inventory.Remove(inventorySlot.itemData, itemCount, inventorySlot);
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

    public virtual bool IsConsumable()
    {
        return false;
    }

    public virtual bool IsKey()
    {
        return false;
    }
}

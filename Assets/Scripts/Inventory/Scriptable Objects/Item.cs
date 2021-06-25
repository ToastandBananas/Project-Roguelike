using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("General Item Info")]
    new public string name = "New Item";
    public string description;
    public int maxStackSize = 1;
    public float pickupRadius = 1f;
    public bool isUsable = true;

    [Header("Value")]
    public int minBaseValue;
    public int maxBaseValue;
    public int staticValue = 1;

    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite pickupSprite;

    public virtual void Use(EquipmentManager equipmentManager, Inventory inventory, InventorySlot inventorySlot, int itemCount)
    {
        if (inventorySlot != null && isUsable)
        {
            inventorySlot.currentStackSize -= itemCount;
            inventorySlot.UpdateStackSizeText();
        }
    }

    public void RemoveFromInventory(Inventory inventory, int itemCount, InventorySlot inventorySlot)
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
}

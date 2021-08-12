using UnityEngine;

public enum ItemType { Item, Weapon, Ammo, Clothing, Armor, Food, Drink, Ingredient, Seed, Readable, Key, QuestItem, Bag, Container, Shield, Medical }
public enum ItemMaterial { Liquid, ViscousLiquid, Meat, Bone, Food, Fat, Bug, Leaf, Charcoal, Wood, Bark, Paper, Hair, Linen, QuiltedLinen, Cotton, Wool, QuiltedWool, Silk, Hemp, Fur,
                            UncuredHide, Rawhide, SoftLeather, HardLeather, Keratin, Chitin, Glass, Obsidian, Stone, Gemstone, Silver, Gold, Copper, Bronze, Iron, Brass, Steel, Mithril, Dragonscale }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("General Item Info")]
    new public string name = "New Item";
    public string pluralName;
    public ItemType itemType;
    public ItemMaterial mainMaterial;
    public string description;
    public int maxStackSize = 1;
    public float weight = 0.1f;
    public float volume = 0.1f;
    public bool isUsable = true;

    [Header("Value")]
    public Vector2Int value;
    public int staticValue = 1;

    [Header("Pickup Sprite")]
    public Sprite pickupSprite;

    public virtual void Use(CharacterManager characterManager, Inventory inventory, InventoryItem invItem, ItemData itemData, int itemCount, EquipmentSlot equipSlot = EquipmentSlot.Shirt)
    {
        if (itemData != null)
        {
            if (isUsable)
            {
                itemData.currentStackSize -= itemCount;

                // If there's none left, remove the item
                if (itemData.currentStackSize <= 0)
                {
                    // Remove the item from the ItemDatas dictionary if it was on the ground
                    if (invItem != null && invItem.myInventory == null)
                        GameTiles.RemoveItemData(invItem.itemData, invItem.itemData.transform.position);

                    if (inventory != null) // If using an item that's inside and inventory
                    {
                        // Remove it from the inventory
                        RemoveFromInventory(inventory, invItem, itemData, itemCount);
                    }
                    else if (invItem.myEquipmentManager == null) // If using an item that was on the ground
                    {
                        invItem.gm.containerInvUI.GetItemsListFromActiveDirection().Remove(invItem.itemData);
                        invItem.ClearItem();
                    }
                }
                else if (invItem != null)
                    invItem.UpdateInventoryWeightAndVolume();

                if (invItem != null)
                    invItem.myInvUI.UpdateUI();
            }
            else
            {
                Debug.Log("You cannot use this item");
                return;
            }
        }
    }

    public void RemoveFromInventory(Inventory inventory, InventoryItem invItem, ItemData itemData, int itemCount)
    {
        inventory.RemoveItem(itemData, itemCount, invItem);
    }

    public virtual bool IsEquipment() { return false; }

    public virtual bool IsWeapon() { return false; }

    public virtual bool IsWearable() { return false; }

    public virtual bool IsShield() { return false; }

    public virtual bool IsBag() { return false; }

    public virtual bool IsPortableContainer() { return false; }

    public virtual bool IsConsumable() { return false; }

    public virtual bool IsMedicalSupply() { return false; }

    public virtual bool IsKey() { return false; }
}

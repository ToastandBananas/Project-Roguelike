using UnityEngine;

public enum ItemSize { ExtraSmall, VerySmall, Small, Medium, Large, VeryLarge, ExtraLarge }
public enum PartialAmount { Whole, Half, Quarter, Tenth }
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
    public ItemSize itemSize;
    public string description;
    public int maxStackSize = 1;
    public float weight = 0.1f;
    public float volume = 0.1f;
    public bool isUsable = true;
    public bool canUsePartial;

    [Header("Value")]
    public Vector2Int value;
    public int staticValue = 1;

    [Header("Sprites")]
    public Sprite pickupSprite;

    public virtual void Use(CharacterManager characterManager, Inventory inventory, InventoryItem invItem, ItemData itemData, int itemCount, PartialAmount partialAmountToUse = PartialAmount.Whole, EquipmentSlot equipSlot = EquipmentSlot.Shirt)
    {
        if (itemData != null)
        {
            characterManager.RemoveCarriedItem(itemData, itemCount); // This will only run if the character is carrying this Item

            if (canUsePartial && partialAmountToUse != PartialAmount.Whole)
            {
                InventoryItem newInvItem = StackSizeSelector.instance.SplitStack(invItem, 1);
                newInvItem.itemData.UsePartial(partialAmountToUse);
                newInvItem.UpdateAllItemTexts();
            }
            else
                itemData.currentStackSize -= itemCount;

            // If there's none left, remove the item
            if (itemData.currentStackSize <= 0)
                itemData.RemoveItemData();
            else if (invItem != null)
                invItem.UpdateInventoryWeightAndVolume();
            
            if (inventory != null)
            {
                inventory.UpdateCurrentWeightAndVolume();
                if (inventory.inventoryOwner != null)
                    inventory.inventoryOwner.SetTotalCarriedWeightAndVolume();
            }

            if (invItem != null)
                invItem.myInvUI.UpdateUI();
        }
    }

    public void RemoveFromInventory(Inventory inventory, InventoryItem invItem, ItemData itemData, int itemCount)
    {
        inventory.RemoveItem(itemData, itemCount, invItem);
    }

    /// <summary>Returns partialAmount as a whole number percent.</summary>
    public int GetPartialAmountsPercentage(PartialAmount partialAmount)
    {
        switch (partialAmount)
        {
            case PartialAmount.Whole:
                return 100;
            case PartialAmount.Half:
                return 50;
            case PartialAmount.Quarter:
                return 25;
            case PartialAmount.Tenth:
                return 10;
            default:
                return 100;
        }
    }

    /// <summary>Used to determine how much a character can carry in their hands, based off of ItemSize.</summary>
    public float GetSizeFactor()
    {
        switch (itemSize)
        {
            case ItemSize.ExtraSmall:
                return 0.05f;
            case ItemSize.VerySmall:
                return 0.1f;
            case ItemSize.Small:
                return 0.25f;
            case ItemSize.Medium:
                return 0.5f;
            case ItemSize.Large:
                return 1f;
            case ItemSize.VeryLarge:
                return 1.5f;
            case ItemSize.ExtraLarge:
                return 2f;
            default:
                return 1f;
        }
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

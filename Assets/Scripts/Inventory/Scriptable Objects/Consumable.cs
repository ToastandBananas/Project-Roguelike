using UnityEngine;

public enum ConsumableType { Food, Drink }

public class Consumable : Item
{
    [HideInInspector] public int minBaseFreshness = 25;
    [HideInInspector] public int maxBaseFreshness = 100;

    [Header("Consumable Stats")]
    public ConsumableType consumableType;
    public int maxUses = 1;
    public int nourishment;
    public int thirstQuench;
    public int energyRestoration;
    public int healAmount;
    public int staminaRecoveryAmount;
    public int manaRecoveryAmount;

    public override void Use(EquipmentManager equipmentManager, Inventory inventory, InventorySlot inventorySlot, int itemCount)
    {
        base.Use(equipmentManager, inventory, inventorySlot, itemCount);
    }

    public override bool IsConsumable()
    {
        return true;
    }
}

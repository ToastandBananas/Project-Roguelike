using UnityEngine;

public enum ConsumableType { Food, Drink }

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
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

    public override void Use(CharacterManager characterManager, EquipmentSlot equipSlot, Inventory inventory, InventoryItem invItem, int itemCount)
    {
        characterManager.StartCoroutine(characterManager.characterStats.UseAPAndConsume(this));

        base.Use(characterManager, equipSlot, inventory, invItem, itemCount);
    }

    public override bool IsConsumable()
    {
        return true;
    }
}

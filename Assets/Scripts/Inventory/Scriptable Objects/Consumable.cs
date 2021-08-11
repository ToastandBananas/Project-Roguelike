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
    public int staminaRecoveryAmount;
    public int manaRecoveryAmount;

    [Header("Healing")]
    public float healthinessAdjustment;
    [Range(0f, 1f)] public float instantHealPercent;
    [Range(0f, 1f)] public float gradualHealPercent;
    public Vector3Int minGradualHealTime;
    public Vector3Int maxGradualHealTime;

    public override void Use(CharacterManager characterManager, Inventory inventory, InventoryItem invItem, int itemCount, EquipmentSlot equipSlot = EquipmentSlot.Shirt)
    {
        characterManager.StartCoroutine(characterManager.status.Consume(invItem.itemData));

        base.Use(characterManager, inventory, invItem, itemCount, equipSlot);
    }

    public override bool IsConsumable()
    {
        return true;
    }
}

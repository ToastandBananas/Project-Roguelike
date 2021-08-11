using UnityEngine;

public enum MedicalSupplyType { Bandage }

[CreateAssetMenu(fileName = "New Medical Supply", menuName = "Inventory/Medical Supply")]
public class MedicalSupply : Item
{
    [Header("Medical Stats")]
    public float quality = 0.5f;

    public override void Use(CharacterManager characterManager, Inventory inventory, InventoryItem invItem, int itemCount, EquipmentSlot equipSlot = EquipmentSlot.Shirt)
    {
        

        base.Use(characterManager, inventory, invItem, itemCount);
    }

    public override bool IsMedicalSupply() { return true; }
}

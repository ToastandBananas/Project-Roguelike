using UnityEngine;

public enum MedicalSupplyType { Bandage }

[CreateAssetMenu(fileName = "New Medical Supply", menuName = "Inventory/Medical Supply")]
public class MedicalSupply : Item
{
    [Header("Medical Stats")]
    public MedicalSupplyType medicalSupplyType;
    public float quality = 0.5f;

    public override bool IsMedicalSupply() { return true; }
}

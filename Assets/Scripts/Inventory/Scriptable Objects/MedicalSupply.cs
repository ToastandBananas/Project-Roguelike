using UnityEngine;

[CreateAssetMenu(fileName = "New Medical Supply", menuName = "Inventory/Medical Supply")]
public class MedicalSupply : Item
{
    [Header("Medical Stats")]
    public float quality = 0.5f;

    public override bool IsMedicalSupply() { return true; }
}

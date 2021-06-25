using UnityEngine;

public class PlayerEquipmentManager : EquipmentManager
{
    #region Singleton
    public static PlayerEquipmentManager instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of PlayerEquipmentManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    public override void Start()
    {
        base.Start();
    }

    public override void Equip(ItemData newItemData, EquipmentSlot equipmentSlot)
    {
        base.Equip(newItemData, equipmentSlot);
    }

    public override void Unequip(EquipmentSlot equipmentSlot, bool shouldAddToInventory)
    {
        base.Unequip(equipmentSlot, shouldAddToInventory);
    }

    public override void AssignEquipment(ItemData newItemData, EquipmentSlot equipmentSlot)
    {
        base.AssignEquipment(newItemData, equipmentSlot);
    }

    public override void UnassignEquipment(ItemData oldItemData, EquipmentSlot equipmentSlot, bool shouldAddToInventory)
    {
        base.UnassignEquipment(oldItemData, equipmentSlot, shouldAddToInventory);
    }
}

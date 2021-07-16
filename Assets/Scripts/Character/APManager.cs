using UnityEngine;

public enum ActionType { Move, Attack, Equip, Unequip }

public class APManager : MonoBehaviour
{
    #region Singleton
    public static APManager instance;

    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of APManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    readonly int baseMovementCost = 40;

    public int GetMovementAPCost()
    {
        return baseMovementCost;
    }

    public int GetEquipAPCost(Equipment equipment)
    {
        switch (equipment.equipmentSlot)
        {
            case EquipmentSlot.Helmet:
                return 60;
            case EquipmentSlot.Shirt:
                return 50;
            case EquipmentSlot.Pants:
                return 70;
            case EquipmentSlot.Boots:
                return 150;
            case EquipmentSlot.Gloves:
                return 60;
            case EquipmentSlot.BodyArmor:
                return 220;
            case EquipmentSlot.LegArmor:
                return 220;
            case EquipmentSlot.RightWeapon:
                return 50;
            case EquipmentSlot.LeftWeapon:
                return 50;
            case EquipmentSlot.Ranged:
                return 50;
            case EquipmentSlot.Quiver:
                return 40;
            case EquipmentSlot.Backpack:
                return 50;
            case EquipmentSlot.LeftHipPouch:
                return 100;
            case EquipmentSlot.RightHipPouch:
                return 100;
            default:
                return 50;
        }
    }
}

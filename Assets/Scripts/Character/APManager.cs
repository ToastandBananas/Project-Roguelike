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

    public int GetAttackAPCost(CharacterManager characterManager, Weapon weapon, AttackType attackType)
    {
        switch (attackType)
        {
            case AttackType.Unarmed:
                return 50;
            case AttackType.PrimaryWeapon:
                return CalculateMeleeAttackAPCost(characterManager, weapon);
            case AttackType.SecondaryWeapon:
                return CalculateMeleeAttackAPCost(characterManager, weapon);
            case AttackType.DualWield:
                if (characterManager.attack.dualWieldAttackCount == 0)
                    return CalculateMeleeAttackAPCost(characterManager, weapon);
                else
                    return CalculateDualWieldSecondAttackAPCost(characterManager, weapon);
            case AttackType.Ranged:
                return CalculateMeleeAttackAPCost(characterManager, weapon);
            case AttackType.Throwing:
                return CalculateThrowAPCost(characterManager, weapon);
            case AttackType.Magic:
                return 100;
            default:
                return 100;
        }
    }

    int CalculateMeleeAttackAPCost(CharacterManager characterManager, Weapon weapon)
    {
        float amount = 50 + (weapon.volume * 3) + (weapon.weight * 2);
        if (characterManager.equipmentManager.isTwoHanding)
            amount *= 1.35f;

        //Debug.Log(Mathf.RoundToInt(amount));
        return Mathf.RoundToInt(amount);
    }

    int CalculateDualWieldSecondAttackAPCost(CharacterManager characterManager, Weapon weapon)
    {
        //Debug.Log(Mathf.RoundToInt(CalculateMeleeAttackAPCost(characterManager, weapon) * 0.7f));
        return Mathf.RoundToInt(CalculateMeleeAttackAPCost(characterManager, weapon) * 0.7f);
    }

    int CalculateShootAPCost(CharacterManager characterManager, Weapon weapon)
    {
        Debug.Log(Mathf.RoundToInt(50 + (weapon.volume * 3) + (weapon.weight * 2)));
        return Mathf.RoundToInt(50 + (weapon.volume * 3) + (weapon.weight * 2));
    }

    int CalculateThrowAPCost(CharacterManager characterManager, Item item)
    {
        Debug.Log(40 + (item.volume * 2) + (item.weight * 3));
        return Mathf.RoundToInt(40 + (item.volume * 2) + (item.weight * 3));
    }

    public int GetSwapStanceAPCost(CharacterManager characterManager)
    {
        return 15;
    }

    public int GetTransferItemCost(Item item, int itemCount, float invWeight, float invVolume, bool transferringInventoryToInventory)
    {
        float cost = (item.weight + item.volume) * itemCount;
        cost += invWeight + invVolume;

        if (transferringInventoryToInventory)
            cost *= 2;

        cost = Mathf.RoundToInt(cost);

        if (cost == 0)
            cost = 1;

        // Debug.Log(cost);
        return (int)cost;
    }

    public int GetEquipAPCost(Equipment equipment, float bagInvWeight)
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
            case EquipmentSlot.LeftWeapon:
                return 50;
            case EquipmentSlot.RightWeapon:
                return 50;
            case EquipmentSlot.Ranged:
                return 50;
            case EquipmentSlot.Quiver:
                return 40 + Mathf.RoundToInt(bagInvWeight);
            case EquipmentSlot.Backpack:
                return 50 + Mathf.RoundToInt(bagInvWeight);
            case EquipmentSlot.LeftHipPouch:
                return 100 + Mathf.RoundToInt(bagInvWeight);
            case EquipmentSlot.RightHipPouch:
                return 100 + Mathf.RoundToInt(bagInvWeight);
            default:
                return 50;
        }
    }

    public int GetConsumeAPCost(Consumable consumable)
    {
        return Mathf.RoundToInt((consumable.volume * 100) + (consumable.weight * 100));
    }
}

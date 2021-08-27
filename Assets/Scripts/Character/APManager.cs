using System.Collections;
using UnityEngine;

public enum ActionType { Move, Attack, Equip, Unequip }

public class APManager : MonoBehaviour
{
    readonly int baseWeaponStickCost = 200;
    readonly int minWeaponStickAPCost = 20;

    readonly int baseBandageAPCost = 1000;

    readonly int baseMovementCost = 100;
    readonly int rotationCost = 6;

    readonly float baseOverEncumberedPenalty = 0.5f;

    GameManager gm;

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

    void Start()
    {
        gm = GameManager.instance;
    }

    public void LoseAP(CharacterManager characterManager, int APAmount)
    {
        if (characterManager.characterStats.currentAP > APAmount)
            characterManager.characterStats.UseAP(APAmount);
        else
        {
            APAmount -= characterManager.characterStats.currentAP;
            characterManager.characterStats.UseAP(characterManager.characterStats.currentAP);

            if (APAmount > 0)
                characterManager.characterStats.AddToAPLossBuildup(APAmount);

            StartCoroutine(gm.turnManager.FinishTurn(characterManager));
        }
    }

    public int GetMovementAPCost(bool diagonal)
    {
        if (diagonal)
            return Mathf.RoundToInt(baseMovementCost * 1.414214f);
        else
            return baseMovementCost;
    }

    public int GetRotateAPCost()
    {
        return rotationCost;
    }

    public int GetAttackAPCost(CharacterManager characterManager, Weapon weapon, GeneralAttackType attackType)
    {
        switch (attackType)
        {
            case GeneralAttackType.Unarmed:
                return 50;
            case GeneralAttackType.PrimaryWeapon:
                return CalculateMeleeAttackAPCost(characterManager, weapon);
            case GeneralAttackType.SecondaryWeapon:
                return CalculateMeleeAttackAPCost(characterManager, weapon);
            case GeneralAttackType.DualWield:
                if (characterManager.attack.dualWieldAttackCount == 0)
                    return CalculateMeleeAttackAPCost(characterManager, weapon);
                else
                    return CalculateDualWieldSecondAttackAPCost(characterManager, weapon);
            case GeneralAttackType.Ranged:
                return CalculateMeleeAttackAPCost(characterManager, weapon);
            case GeneralAttackType.Throwing:
                return CalculateThrowAPCost(characterManager, weapon);
            case GeneralAttackType.Magic:
                return 100;
            default:
                return 100;
        }
    }

    public int GetWeaponStickAPCost(CharacterManager characterManager, Weapon weaponUsed, PhysicalDamageType mainPhysicalDamageType, float percentDamage)
    {
        int APCost = Mathf.RoundToInt((baseWeaponStickCost * (percentDamage * 2)) - (characterManager.characterStats.strength.GetValue() / 3) - (characterManager.characterStats.swordSkill.GetValue() / 4));

        // When causing piercing damage, stabbing type weapons (or weapons with spikes), use less AP, since they should be easier to remove
        if (mainPhysicalDamageType == PhysicalDamageType.Pierce)
        {
            if (weaponUsed.weaponType == WeaponType.Spear || weaponUsed.weaponType == WeaponType.Dagger || weaponUsed.weaponType == WeaponType.Polearm)
                APCost = Mathf.RoundToInt(APCost / 2);
            else if (weaponUsed.weaponType == WeaponType.SpikedAxe || weaponUsed.weaponType == WeaponType.SpikedClub || weaponUsed.weaponType == WeaponType.SpikedHammer || weaponUsed.weaponType == WeaponType.SpikedMace)
                APCost = Mathf.RoundToInt(APCost / 1.5f);
        }

        if (APCost < minWeaponStickAPCost)
            APCost = minWeaponStickAPCost;
        
        return APCost;
    }

    public int GetStuckWithWeaponAPLoss(CharacterManager target, float percentDamage)
    {
        return Mathf.RoundToInt(baseWeaponStickCost * percentDamage * Random.Range(0.75f, 1.25f));
    }

    int CalculateMeleeAttackAPCost(CharacterManager characterManager, Weapon weapon)
    {
        float amount = 50 + (weapon.volume * 3) + (weapon.weight * 2);
        if (characterManager.equipmentManager.isTwoHanding)
            amount *= 1.35f;
        
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
        cost *= 2;

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
            case EquipmentSlot.LeftHandItem:
                return 50;
            case EquipmentSlot.RightHandItem:
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

    public int GetSheatheWeaponAPCost(EquipmentManager equipmentManager, bool sheatheLeft, bool sheatheRight)
    {
        float cost = 0;
        if (sheatheLeft && equipmentManager.LeftWeaponSheathed() == false && equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem] != null)
            cost += equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].item.weight + equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].item.volume;

        if (sheatheRight && equipmentManager.RightWeaponSheathed() == false && equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem] != null)
            cost += equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].item.weight + equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].item.volume;

        return Mathf.RoundToInt(cost);
    }

    public int GetUnheatheWeaponAPCost(EquipmentManager equipmentManager)
    {
        float cost = 0;
        if (equipmentManager.LeftHandItemEquipped() && equipmentManager.LeftWeaponSheathed())
            cost += equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].item.weight + equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].item.volume;

        if (equipmentManager.RightHandItemEquipped() && equipmentManager.RightWeaponSheathed())
            cost += equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].item.weight + equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].item.volume;

        return Mathf.RoundToInt(cost / 2);
    }

    public int GetConsumeAPCost(Consumable consumable, float itemCount, float percentUsed)
    {
        return Mathf.RoundToInt(((consumable.volume * 100) + (consumable.weight * 100)) * itemCount * percentUsed);
    }

    public int GetApplyMedicalItemAPCost(MedicalSupply medSupply)
    {
        if (medSupply.medicalSupplyType == MedicalSupplyType.Bandage)
            return baseBandageAPCost;
        return 100;
    }

    public int GetRemoveMedicalItemAPCost(MedicalSupply medSupply)
    {
        return Mathf.RoundToInt(GetApplyMedicalItemAPCost(medSupply) * 0.75f);
    }

    public int GetOverEncumberedAPPenalty(CharacterManager characterManager, int APAmount)
    {
        return Mathf.RoundToInt(APAmount * (baseOverEncumberedPenalty + ((characterManager.totalCarryWeight - characterManager.characterStats.GetMaximumWeightCapacity()) / characterManager.characterStats.GetMaximumWeightCapacity())));
    }
}

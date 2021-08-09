using System.Collections;
using UnityEngine;

public enum ActionType { Move, Attack, Equip, Unequip }

public class APManager : MonoBehaviour
{
    readonly int baseWeaponStickCost = 200;
    readonly int minWeaponStickAPCost = 20;

    readonly int baseMovementCost = 100;

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

    public IEnumerator UseAP(CharacterManager characterManager, int APAmount, bool queuingNewAction = true)
    {
        Debug.Log(characterManager.name + " is about to use AP...");
        if (queuingNewAction) characterManager.actionsQueued++;

        if (APAmount <= 0)
        {
            Debug.Log("APAmount <= 0");
            characterManager.actionsQueued--;
            characterManager.currentQueueNumber++;
            yield break;
        }

        while (characterManager.isMyTurn == false) { yield return null; }

        if (characterManager.status.isDead)
        {
            characterManager.actionsQueued--;
            characterManager.currentQueueNumber++;
            yield break;
        }

        /*if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                characterManager.actionsQueued--;
                characterManager.currentQueueNumber++;

                Debug.Log(characterManager.name + " Using AP: " + characterManager.remainingAPToBeUsed);
                characterManager.characterStats.UseAP(characterManager.remainingAPToBeUsed);

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager, false);
                else
                    characterManager.TakeTurn();
            }
            else
            {
                Debug.Log(characterManager.name + " Using AP: " + APAmount);
                int APRemainder = characterManager.characterStats.UseAPAndGetRemainder(APAmount);
                Debug.Log(characterManager.name + " AP Remainder: " + APRemainder);

                //characterManager.characterStats.UseAP(characterManager.characterStats.currentAP);
                gm.turnManager.FinishTurn(characterManager, false);
                StartCoroutine(UseAP(characterManager, APRemainder, false));
            }
        }
        else*/
        //{

        Debug.Log(characterManager.name + " Using AP: " + APAmount);
        // Try to use the full AP amount. If we can't, then use what we can and return the remainder to APRemainder
        int APRemainder = characterManager.characterStats.UseAPAndGetRemainder(APAmount);
        // If the entire amount was used
        if (APRemainder <= 0)
        {
            // Adjust our queue numbers, so that the appropriate coroutines can finish running
            characterManager.actionsQueued--;
            characterManager.currentQueueNumber++;

            // If the character has no AP remaining, end their turn
            if (characterManager.characterStats.currentAP <= 0)
                StartCoroutine(gm.turnManager.FinishTurn(characterManager));
            else // Take another action, if this is an NPC
                characterManager.TakeTurn();
        }
        else
        {
            // Add to the character's remainingAPToBeUsed, finish their turn and run this coroutine again with the remaining AP
            //characterManager.remainingAPToBeUsed += APRemainder;
            Debug.Log("Heeeeeeeeere");//characterManager.name + " Remaining AP To Be Used: " + characterManager.remainingAPToBeUsed);
            StartCoroutine(gm.turnManager.FinishTurn(characterManager));
            while (characterManager.movement.isMoving) { yield return null; }
            StartCoroutine(UseAP(characterManager, APRemainder, false));
        }
        //}
    }

    public int GetMovementAPCost()
    {
        return baseMovementCost;
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
        int amount = Mathf.RoundToInt(baseWeaponStickCost * percentDamage * Random.Range(0.75f, 1.25f));
        Debug.Log("Stuck with weapon AP cost: " + amount);
        return amount;
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

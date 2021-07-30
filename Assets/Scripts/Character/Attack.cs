using System.Collections;
using UnityEngine;

public enum AttackType { Unarmed, PrimaryWeapon, SecondaryWeapon, DualWield, Ranged, Throwing, Magic }

public class Attack : MonoBehaviour
{
    public int attackRange = 1;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;
    
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public int dualWieldAttackCount = 0;

    readonly int maxBlockChance = 85;

    public virtual void Start()
    {
        canAttack = true;

        gm = GameManager.instance;
        characterManager = GetComponent<CharacterManager>();
    }

    /// <summary>This function determines what type of attack the character should do.</summary>
    public virtual void DetermineAttack(Stats targetsStats)
    {
        // This is just meant to be overridden
    } 

    public virtual void DoAttack(Stats targetsStats, AttackType attackType)
    {
        characterManager.movement.FaceForward(targetsStats.transform.position);

        switch (attackType)
        {
            case AttackType.Unarmed:
                MeleeAttack(targetsStats, attackType);
                break;
            case AttackType.PrimaryWeapon:
                MeleeAttack(targetsStats, attackType);
                break;
            case AttackType.SecondaryWeapon:
                MeleeAttack(targetsStats, attackType);
                break;
            case AttackType.DualWield:
                DualWieldAttack(targetsStats);
                break;
            case AttackType.Ranged:
                break;
            case AttackType.Throwing:
                break;
            case AttackType.Magic:
                break;
            default:
                break;
        }

        StartCoroutine(AttackCooldown());
    }

    public void StartMeleeAttack(Stats targetsStats)
    {
        if (characterManager.equipmentManager.IsDualWielding())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetRightWeapon(), AttackType.DualWield));
        else if (characterManager.equipmentManager.RightWeaponEquipped())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetRightWeapon(), AttackType.PrimaryWeapon));
        else if (characterManager.equipmentManager.LeftWeaponEquipped())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetLeftWeapon(), AttackType.SecondaryWeapon));
        else // Punch, if no weapons equipped
            StartCoroutine(UseAPAndAttack(targetsStats, null, AttackType.Unarmed));
    }

    public void MeleeAttack(Stats targetsStats, AttackType attackType)
    {
        StartCoroutine(characterManager.movement.BlockedMovement(targetsStats.transform.position));

        CharacterStats targetCharStats = (CharacterStats)targetsStats;

        if (TryEvade(targetCharStats) == false) // Check if the target evaded the attack (or the attacker missed)
        {
            int damage = GetDamage(attackType);
            if (TryBlock(targetCharStats, damage) == false) // Check if the target blocked the attack with a shield or a weapon
            {
                if (targetsStats.locomotionType != LocomotionType.Inanimate)
                {
                    BodyPart bodyPartToHit = targetCharStats.GetBodyPartToHit();

                    targetCharStats.TakeLocationalDamage(characterManager.equipmentManager.GetRightWeaponAttackDamage(), bodyPartToHit);

                    if (damage > 0)
                        gm.flavorText.WriteAttackLine(attackType, bodyPartToHit, characterManager, targetCharStats.characterManager, damage);
                }
            }
        }
    }

    int GetDamage(AttackType attackType)
    {
        switch (attackType)
        {
            case AttackType.Unarmed:
                return characterManager.characterStats.unarmedDamage.GetValue();
            case AttackType.PrimaryWeapon:
                return characterManager.equipmentManager.GetRightWeaponAttackDamage();
            case AttackType.SecondaryWeapon:
                return characterManager.equipmentManager.GetLeftWeaponAttackDamage();
            case AttackType.Ranged:
                return 0;
            case AttackType.Throwing:
                return 0;
            case AttackType.Magic:
                return 0;
            default:
                return 0;
        }
    }

    public void DualWieldAttack(Stats targetsStats)
    {
        if (dualWieldAttackCount == 0)
        {
            MeleeAttack(targetsStats, AttackType.PrimaryWeapon);
            dualWieldAttackCount++;
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetLeftWeapon(), AttackType.DualWield));
        }
        else
        {
            MeleeAttack(targetsStats, AttackType.SecondaryWeapon);
            dualWieldAttackCount = 0;
        }
    }

    bool TryEvade(CharacterStats targetsCharStats)
    {
        // Evade/miss chance equals the attacker's accuracy plus the target's evasion divided by 2
        float evadePlusMiss = 100 - characterManager.characterStats.accuracy.GetValue() + targetsCharStats.evasion.GetValue();
        float evadeChance = evadePlusMiss / 2;
        
        if (evadeChance > 0)
        {
            // Determine if the attack hit the target or not
            float random = Random.Range(1f, 100f);
            if (random <= evadeChance)
            {
                // Determine if the attack was evaded or if the attacker just straight up missed
                float percentChanceEvade = targetsCharStats.evasion.GetValue() / evadePlusMiss;

                random = Random.Range(0f, 1f);
                if (random > percentChanceEvade)
                {
                    // Attack missed
                    TextPopup.CreateTextStringPopup(targetsCharStats.transform.position, "Missed");
                    gm.flavorText.WriteMissedAttackLine(characterManager, targetsCharStats.characterManager);
                    return true;
                }
                else
                {
                    // Attack was evaded
                    TextPopup.CreateTextStringPopup(targetsCharStats.transform.position, "Evaded");
                    gm.flavorText.WriteEvadedAttackLine(characterManager, targetsCharStats.characterManager);
                    return true;
                }
            }
        }

        return false;
    }

    bool TryBlock(CharacterStats targetsCharStats, int damage)
    {
        // A character can only block if they have a weapon and/or shield equipped
        if (targetsCharStats.characterManager.equipmentManager != null && (targetsCharStats.characterManager.equipmentManager.ShieldEquipped() || targetsCharStats.characterManager.equipmentManager.MeleeWeaponEquipped()))
        {
            // Total up the block chance based off of the character's shield and/or weapon block ability and the block chance multiplier of what they have equipped
            // (Shields generally have a higher chance to block than a weapon)
            float blockChance = 0;
            float leftBlockChance = 0;
            float rightBlockChance = 0;

            // Get the left weapon/shield's block chance
            if (targetsCharStats.characterManager.equipmentManager.LeftWeaponEquipped())
            {
                if (targetsCharStats.characterManager.equipmentManager.GetEquipment(EquipmentSlot.LeftWeapon).IsShield())
                    leftBlockChance = (targetsCharStats.shieldBlock.GetValue() / 2.5f * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftWeapon).blockChanceMultiplier);
                else
                    leftBlockChance = (targetsCharStats.weaponBlock.GetValue() / 3 * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftWeapon).blockChanceMultiplier);

                blockChance += leftBlockChance;
            }

            // Get the right weapon/shield's block chance
            if (targetsCharStats.characterManager.equipmentManager.RightWeaponEquipped())
            {
                if (targetsCharStats.characterManager.equipmentManager.GetEquipment(EquipmentSlot.RightWeapon).IsShield())
                    rightBlockChance = (targetsCharStats.shieldBlock.GetValue() / 2.5f * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightWeapon).blockChanceMultiplier);
                else
                    rightBlockChance = (targetsCharStats.weaponBlock.GetValue() / 3 * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightWeapon).blockChanceMultiplier);

                blockChance += rightBlockChance;
            }

            // Cap the block chance (we don't want anyone being untouchable)
            if (blockChance > maxBlockChance)
                blockChance = maxBlockChance;

            // Determine if the attack was blocked
            float random = Random.Range(1f, 100f);
            if (random <= blockChance)
            {
                // Attack was blocked
                // Determine which shield or weapon blocked the attack and damage its durability
                float percentChanceBlockLeft = leftBlockChance / blockChance;

                random = Random.Range(0f, 1f);
                if (leftBlockChance == 0 || random > leftBlockChance)
                {
                    gm.flavorText.WriteBlockedAttackLine(characterManager, targetsCharStats.characterManager, targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightWeapon));
                    targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightWeapon).DamageDurability(damage);
                }
                else
                {
                    gm.flavorText.WriteBlockedAttackLine(characterManager, targetsCharStats.characterManager, targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftWeapon));
                    targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftWeapon).DamageDurability(damage);
                }

                TextPopup.CreateTextStringPopup(targetsCharStats.transform.position, "Blocked");
                return true;
            }
        }

        return false;
    }

    public IEnumerator UseAPAndAttack(Stats targetsStats, Weapon weapon, AttackType attackType)
    {
        characterManager.actionQueued = true;

        while (characterManager.isMyTurn == false || characterManager.movement.isMoving || canAttack == false)
        {
            yield return null;
        }

        if (TargetInAttackRange(targetsStats.transform) == false || targetsStats.isDeadOrDestroyed)
        {
            CancelAttack();
            yield break;
        }

        if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                characterManager.actionQueued = false;
                characterManager.characterStats.UseAP(characterManager.remainingAPToBeUsed);
                DoAttack(targetsStats, attackType);
            }
            else
            {
                characterManager.characterStats.UseAP(characterManager.characterStats.currentAP);
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndAttack(targetsStats, weapon, attackType));
            }
        }
        else
        {
            int remainingAP = characterManager.characterStats.UseAPAndGetRemainder(gm.apManager.GetAttackAPCost(characterManager, weapon, attackType));
            if (remainingAP == 0)
            {
                characterManager.actionQueued = false;
                DoAttack(targetsStats, attackType);
            }
            else
            {
                characterManager.remainingAPToBeUsed = remainingAP;
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndAttack(targetsStats, weapon, attackType));
            }
        }
    }

    public bool TargetInAttackRange(Transform target)
    {
        int distX = Mathf.RoundToInt(Mathf.Abs(transform.position.x - target.position.x));
        int distY = Mathf.RoundToInt(Mathf.Abs(transform.position.y - target.position.y));

        if (distX <= attackRange && distY <= attackRange)
            return true;

        return false;
    }

    public void CancelAttack()
    {
        dualWieldAttackCount = 0;
        characterManager.remainingAPToBeUsed = 0;
        characterManager.actionQueued = false;
        if (characterManager.isNPC && characterManager.characterStats.isDeadOrDestroyed == false)
            characterManager.TakeTurn();
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.4f);
        canAttack = true;
    }

    /*public void ShowWeaponTrail(HeldWeapon heldWeapon)
    {
        WeaponTrail weaponTrail = weaponTrailObjectPool.GetPooledObject().GetComponent<WeaponTrail>();
        weaponTrail.gameObject.SetActive(true);
        
        weaponTrail.ShowChopTrail(heldWeapon, characterManager.transform);
    }*/
}

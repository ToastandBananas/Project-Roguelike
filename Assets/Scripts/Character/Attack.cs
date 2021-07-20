using System.Collections;
using UnityEngine;

public enum AttackType { Unarmed, PrimaryWeapon, SecondaryWeapon, DualWield, Ranged, Throwing, Magic }

public class Attack : MonoBehaviour
{
    public int attackRange = 1;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;
    
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool isAttacking;

    [HideInInspector] public int dualWieldAttackCount = 0;

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
                UnarmedAttack(targetsStats);
                break;
            case AttackType.PrimaryWeapon:
                PrimaryWeaponAttack(targetsStats);
                break;
            case AttackType.SecondaryWeapon:
                SecondaryWeaponAttack(targetsStats);
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
    }

    public void DoMeleeAttack(Stats targetsStats)
    {
        if (characterManager.equipmentManager.IsDualWielding())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetPrimaryWeapon(), AttackType.DualWield));
        else if (characterManager.equipmentManager.PrimaryWeaponEquipped())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetPrimaryWeapon(), AttackType.PrimaryWeapon));
        else if (characterManager.equipmentManager.SecondaryWeaponEquipped())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetSecondaryWeapon(), AttackType.SecondaryWeapon));
        else // Punch, if no weapons equipped
            StartCoroutine(UseAPAndAttack(targetsStats, null, AttackType.Unarmed));
    }

    public void UnarmedAttack(Stats targetsStats)
    {
        StartCoroutine(characterManager.movement.BlockedMovement(targetsStats.transform.position));
        targetsStats.TakeDamage(1);
    }

    public void PrimaryWeaponAttack(Stats targetsStats)
    {
        StartCoroutine(characterManager.movement.BlockedMovement(targetsStats.transform.position));
        targetsStats.TakeDamage(characterManager.equipmentManager.GetPrimaryWeaponAttackDamage());
    }

    public void SecondaryWeaponAttack(Stats targetsStats)
    {
        StartCoroutine(characterManager.movement.BlockedMovement(targetsStats.transform.position));
        targetsStats.TakeDamage(characterManager.equipmentManager.GetSecondaryWeaponAttackDamage());
    }

    public void DualWieldAttack(Stats targetsStats)
    {
        if (dualWieldAttackCount == 0)
        {
            PrimaryWeaponAttack(targetsStats);
            dualWieldAttackCount++;
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetSecondaryWeapon(), AttackType.DualWield));
        }
        else
        {
            SecondaryWeaponAttack(targetsStats);
            dualWieldAttackCount = 0;
        }
    }

    public IEnumerator UseAPAndAttack(Stats targetsStats, Weapon weapon, AttackType attackType)
    {
        characterManager.actionQueued = true;

        while (characterManager.isMyTurn == false || characterManager.movement.isMoving)
        {
            yield return null;
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

    /*public void ShowWeaponTrail(HeldWeapon heldWeapon)
    {
        WeaponTrail weaponTrail = weaponTrailObjectPool.GetPooledObject().GetComponent<WeaponTrail>();
        weaponTrail.gameObject.SetActive(true);
        
        weaponTrail.ShowChopTrail(heldWeapon, characterManager.transform);
    }*/
}

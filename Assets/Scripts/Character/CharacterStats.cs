using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartType { Torso, Head, LeftArm, RightArm, LeftLeg, RightLeg, LeftHand, RightHand, LeftFoot, RightFoot }

public class CharacterStats : Stats
{
    [Header("Main Stats")]
    public Stat agility;
    public Stat constitution;
    public Stat endurance;
    public Stat speed;
    public Stat strength;

    [Header("AP")]
    public Stat maxAP;
    public int currentAP { get; private set; }

    [Header("Weight/Volume")]
    public Stat maxPersonalInvWeight;
    public Stat maxPersonalInvVolume;

    [Header("Skills")]
    public Stat swordSkill;

    [Header("Combat")]
    public Stat unarmedDamage;
    public Stat meleeAccuracy;
    public Stat rangedAccuracy;
    public Stat evasion;
    public Stat shieldBlock;
    public Stat weaponBlock;

    [HideInInspector] public CharacterManager characterManager;

    public override void Awake()
    {
        base.Awake();

        currentAP = maxAP.GetValue();
    }

    public override void Start()
    {
        base.Start();

        characterManager = GetComponentInParent<CharacterManager>();
    }

    public int UseAPAndGetRemainder(int amount)
    {
        Debug.Log("Current AP: " + currentAP);
        int remainingAmount = amount;
        if (currentAP >= amount)
        {
            UseAP(amount);
            remainingAmount = 0;
        }
        else
        {
            remainingAmount = amount - currentAP;
            UseAP(currentAP);
        }
        Debug.Log("Remaining amount: " + remainingAmount);
        return remainingAmount;
    }

    public void UseAP(int amount)
    {
        /*if (characterManager.remainingAPToBeUsed > 0)
        {
            characterManager.remainingAPToBeUsed -= amount;
            if (characterManager.remainingAPToBeUsed < 0)
                characterManager.remainingAPToBeUsed = 0;
        }*/
        // Debug.Log("AP used: " + amount);
        currentAP -= amount;
    }

    public void ReplenishAP()
    {
        currentAP = maxAP.GetValue();
        Debug.Log("Replenished AP: " + currentAP);
    }

    public void AddToCurrentAP(int amountToAdd)
    {
        currentAP += amountToAdd;
    }

    public IEnumerator Consume(ItemData consumableItemData)
    {
        int queueNumber = characterManager.currentQueueNumber + characterManager.actionsQueued;
        while (queueNumber != characterManager.currentQueueNumber)
        {
            yield return null;
            if (characterManager.status.isDead) yield break;
        }

        Consumable consumable = (Consumable)consumableItemData.item;

        // Adjust overall bodily healthiness
        if (consumable.healthinessAdjustment != 0)
            characterManager.status.AdjustHealthiness(consumable.healthinessAdjustment);

        // Instantly heal entire body
        if (consumable.instantHealPercent > 0)
            characterManager.status.HealAllBodyParts_Percent(consumable.instantHealPercent);

        // Apply heal over time buff
        if (consumable.gradualHealPercent > 0)
            TraumaSystem.ApplyBuff(characterManager, consumable);

        // Show some flavor text
        gm.flavorText.WriteConsumeLine(consumable, characterManager);
    }

    /*public IEnumerator UseAPAndConsume(Consumable consumable)
    {
        characterManager.actionsQueued = true;

        while (characterManager.isMyTurn == false)
        {
            yield return null;
        }

        if (characterManager.status.isDead)
        {
            characterManager.actionsQueued = false;
            characterManager.remainingAPToBeUsed = 0;
            yield break;
        }

        if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                Consume(consumable);
                characterManager.actionsQueued = false;
                characterManager.characterStats.UseAP(characterManager.remainingAPToBeUsed);

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.characterStats.UseAP(characterManager.characterStats.currentAP);
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndConsume(consumable));
            }
        }
        else
        {
            int remainingAP = characterManager.characterStats.UseAPAndGetRemainder(gm.apManager.GetConsumeAPCost(consumable));
            if (remainingAP == 0)
            {
                Consume(consumable);
                characterManager.actionsQueued = false;

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.remainingAPToBeUsed += remainingAP;
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndConsume(consumable));
            }
        }
    }*/

    public int GetWeaponSkill(Weapon weapon)
    {
        if (weapon.weaponType == WeaponType.Sword)
            return swordSkill.GetValue();
        return 0;
    }
}
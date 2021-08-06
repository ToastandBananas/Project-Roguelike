using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartType { Torso, Head, LeftArm, RightArm, LeftLeg, RightLeg, LeftHand, RightHand, LeftFoot, RightFoot }

public class CharacterStats : Stats
{
    [Header("Body Parts")]
    public BodyPart torso;
    public BodyPart head, leftArm, rightArm, leftHand, rightHand, leftLeg, rightLeg, leftFoot, rightFoot;
    [HideInInspector] public List<BodyPart> bodyParts = new List<BodyPart>();

    [Header("Defense")]
    public Stat torsoDefense;
    public Stat headDefense;
    public Stat armDefense;
    public Stat handDefense;
    public Stat legDefense;
    public Stat footDefense;

    [Header("Healthiness")]
    public Stat maxBloodAmount;
    public int currentBloodAmount;
    public Stat maxFullness;
    public float currentFullness;
    public Stat maxWater;
    public float currentWater;

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

        torso.currentHealth = torso.maxHealth.GetValue();
        head.currentHealth = head.maxHealth.GetValue();
        leftArm.currentHealth = leftArm.maxHealth.GetValue();
        rightArm.currentHealth = rightArm.maxHealth.GetValue();
        leftHand.currentHealth = leftHand.maxHealth.GetValue();
        rightHand.currentHealth = rightHand.maxHealth.GetValue();
        leftLeg.currentHealth = leftLeg.maxHealth.GetValue();
        rightHand.currentHealth = rightHand.maxHealth.GetValue();
        leftFoot.currentHealth = leftFoot.maxHealth.GetValue();
        rightFoot.currentHealth = rightFoot.maxHealth.GetValue();

        bodyParts.Add(torso);
        bodyParts.Add(head);
        bodyParts.Add(leftArm);
        bodyParts.Add(rightArm);
        bodyParts.Add(leftHand);
        bodyParts.Add(rightHand);
        bodyParts.Add(leftLeg);
        bodyParts.Add(rightLeg);
        bodyParts.Add(leftFoot);
        bodyParts.Add(rightFoot);

        currentBloodAmount = maxBloodAmount.GetValue();
        currentAP = maxAP.GetValue();
    }

    public override void Start()
    {
        base.Start();

        characterManager = GetComponentInParent<CharacterManager>();

        characterManager.equipmentManager.onWearableChanged += OnWearableChanged;
        characterManager.equipmentManager.onWeaponChanged += OnWeaponChanged;
    }

    public int UseAPAndGetRemainder(int amount)
    {
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

        return remainingAmount;
    }

    public void UseAP(int amount)
    {
        if (characterManager.remainingAPToBeUsed > 0)
        {
            characterManager.remainingAPToBeUsed -= amount;
            if (characterManager.remainingAPToBeUsed < 0)
                characterManager.remainingAPToBeUsed = 0;
        }

        currentAP -= amount;
    }

    public void ReplenishAP()
    {
        currentAP = maxAP.GetValue();
    }

    public void AddToCurrentAP(int amountToAdd)
    {
        currentAP += amountToAdd;
    }

    public void Consume(Consumable consumable)
    {
        // Adjust overall bodily healthiness
        if (consumable.healthinessAdjustment != 0)
            characterManager.status.AdjustHealthiness(consumable.healthinessAdjustment);

        // Instantly heal entire body
        if (consumable.instantHealPercent > 0)
            HealAllBodyParts_Percent(consumable.instantHealPercent);

        // Apply heal over time buff
        if (consumable.gradualHealPercent > 0)
            TraumaSystem.ApplyBuff(characterManager, consumable);

        // Show some flavor text
        gm.flavorText.WriteConsumeLine(consumable, characterManager);
    }

    public IEnumerator UseAPAndConsume(Consumable consumable)
    {
        characterManager.actionQueued = true;

        while (gm.turnManager.IsPlayersTurn() == false)
        {
            yield return null;
        }

        if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                Consume(consumable);
                characterManager.actionQueued = false;
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
                characterManager.actionQueued = false;

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.remainingAPToBeUsed = remainingAP;
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndConsume(consumable));
            }
        }
    }

    public void HealAllBodyParts_Instant(int healAmount)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].HealInstant_StaticValue(healAmount);
        }
    }

    public void HealAllBodyParts_Percent(float healPercent)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].HealInstant_Percent(healPercent);
        }
    }

    int GetCurrentHealth(BodyPartType bodyPartType)
    {
        return GetBodyPart(bodyPartType).currentHealth;
    }

    public int TakeStaticLocationalDamage(int damage, BodyPartType bodyPartType)
    {
        if (canTakeDamage)
        {
            if (damage < 0)
                damage = 0;

            if (damage > 0)
                TextPopup.CreateDamagePopup(transform.position, damage, false);
            else
                TextPopup.CreateTextStringPopup(transform.position, "Absorbed");

            GetBodyPart(bodyPartType).DamageHealth(damage);

            if (torso.currentHealth <= 0 || head.currentHealth <= 0)
                Die();
        }

        return damage;
    }

    public int TakeLocationalDamage(int bluntDamage, int pierceDamage, int slashDamage, int cleaveDamage, BodyPartType bodyPartType, EquipmentManager equipmentManager, Wearable armor, Wearable clothing, bool armorPenetrated, bool clothingPenetrated)
    {
        int totalDamage = bluntDamage + pierceDamage + slashDamage + cleaveDamage;

        if (armorPenetrated == false && equipmentManager != null)
            totalDamage -= GetLocationalArmorDefense(equipmentManager, bodyPartType);

        if (clothingPenetrated == false && equipmentManager != null)
            totalDamage -= GetLocationalClothingDefense(equipmentManager, bodyPartType);

        totalDamage -= GetBodyPart(bodyPartType).naturalDefense;

        if (canTakeDamage)
        {
            if (totalDamage <= 0)
                totalDamage = 0;
            else if ((armor == null || armorPenetrated) && (clothing == null || clothingPenetrated))
            {
                // If armor & clothing was penetrated or the character isn't wearing either, 
                // cause an injury based off of damage types and amounts relative to the character's max health for the body part attacked
                Debug.Log(name + " was injured.");
                if (bluntDamage > 0)
                {

                }
                else if (pierceDamage > 0)
                {

                }
                else if (slashDamage > 0)
                {
                    TraumaSystem.ApplyInjury(characterManager, gm.traumaSystem.GetCut(characterManager, bodyPartType, slashDamage), bodyPartType);
                }
                else if (cleaveDamage > 0)
                {

                }
            }

            if (totalDamage > 0)
                TextPopup.CreateDamagePopup(transform.position, totalDamage, false);
            else
                TextPopup.CreateTextStringPopup(transform.position, "Absorbed");

            GetBodyPart(bodyPartType).DamageHealth(totalDamage);
            
            if (torso.currentHealth <= 0 || head.currentHealth <= 0)
                Die();
        }

        return totalDamage;
    }

    public virtual BodyPartType GetBodyPartToHit()
    {
        int random = Random.Range(0, 100);
        if (random < 40)
            return BodyPartType.Torso;
        else if (random < 48)
            return BodyPartType.Head;
        else if (random < 58)
            return BodyPartType.LeftArm;
        else if (random < 68)
            return BodyPartType.RightArm;
        else if (random < 78)
            return BodyPartType.LeftLeg;
        else if (random < 88)
            return BodyPartType.RightArm;
        else if (random < 92)
            return BodyPartType.LeftHand;
        else if (random < 96)
            return BodyPartType.RightHand;
        else if (random < 98)
            return BodyPartType.LeftFoot;
        else
            return BodyPartType.RightFoot;
    }

    public Stat GetBodyPartsMaxHealth(BodyPartType bodyPartType)
    {
        return GetBodyPart(bodyPartType).maxHealth;
    }

    int GetLocationalArmorDefense(EquipmentManager equipmentManager, BodyPartType bodyPartType)
    {
        switch (bodyPartType)
        {
            case BodyPartType.Torso:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].torsoDefense;
                return 0;
            case BodyPartType.Head:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet].headDefense;
                return 0;
            case BodyPartType.LeftArm:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].armDefense;
                return 0;
            case BodyPartType.RightArm:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].armDefense;
                return 0;
            case BodyPartType.LeftLeg:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].legDefense;
                return 0;
            case BodyPartType.RightLeg:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].legDefense;
                return 0;
            case BodyPartType.LeftHand:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].handDefense;
                return 0;
            case BodyPartType.RightHand:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].handDefense;
                return 0;
            case BodyPartType.LeftFoot:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].footDefense;
                return 0;
            case BodyPartType.RightFoot:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].footDefense;
                return 0;
            default:
                return 0;
        }
    }

    public int GetLocationalClothingDefense(EquipmentManager equipmentManager, BodyPartType bodyPartType)
    {
        switch (bodyPartType)
        {
            case BodyPartType.Torso:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].torsoDefense;
                return 0;
            case BodyPartType.LeftArm:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].armDefense;
                return 0;
            case BodyPartType.RightArm:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].armDefense;
                return 0;
            case BodyPartType.LeftLeg:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].legDefense;
                return 0;
            case BodyPartType.RightLeg:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].legDefense;
                return 0;
            default:
                return 0;
        }
    }

    public override void Die()
    {
        isDeadOrDestroyed = true;

        if (characterManager.isNPC) // If an NPC dies
        {
            GameTiles.RemoveNPC(transform.position);
            if (characterManager.equipmentManager != null)
            {
                for (int i = 0; i < characterManager.equipmentManager.currentEquipment.Length; i++)
                {
                    if (characterManager.equipmentManager.currentEquipment[i] != null)
                    {
                        characterManager.inventory.items.Add(characterManager.equipmentManager.currentEquipment[i]);
                        characterManager.equipmentManager.currentEquipment[i] = null;
                    }
                }
            }

            characterManager.vision.visionCollider.enabled = false;
            characterManager.vision.enabled = false;
            characterManager.attack.CancelAttack();
            characterManager.attack.enabled = false;

            gameObject.tag = "Dead Body";
            gameObject.layer = 14;

            characterManager.spriteRenderer.sortingLayerName = "Object";
            characterManager.spriteRenderer.sortingOrder = -1;

            gm.turnManager.npcs.Remove(characterManager);

            if (gm.turnManager.npcsFinishedTakingTurnCount >= gm.turnManager.npcs.Count)
                gm.turnManager.ReadyPlayersTurn();

            characterManager.stateController.enabled = false;
            characterManager.npcMovement.AIDestSetter.enabled = false;
            characterManager.npcMovement.AIPath.enabled = false;

            if (characterManager.IsNextToPlayer())
                gm.containerInvUI.GetItemsAroundPlayer();

            characterManager.movement.enabled = false;
        }
        else // If the player dies
        {
            for (int i = 0; i < characterManager.vision.enemiesInRange.Count; i++)
            {
                if (characterManager.vision.enemiesInRange[i].vision.enemiesInRange.Contains(characterManager))
                    characterManager.vision.enemiesInRange[i].vision.enemiesInRange.Remove(characterManager);

                if (characterManager.vision.enemiesInRange[i].vision.knownEnemiesInRange.Contains(characterManager))
                    characterManager.vision.enemiesInRange[i].vision.knownEnemiesInRange.Remove(characterManager);

                if (characterManager.vision.enemiesInRange[i].npcMovement.target == characterManager)
                    characterManager.vision.enemiesInRange[i].npcAttack.SwitchTarget(characterManager.vision.enemiesInRange[i].alliances.GetClosestKnownEnemy());
            }
        }

        characterManager.characterSpriteManager.SetToDeathSprite(characterManager.spriteRenderer);

        gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine(gm.flavorText.GetPronoun(characterManager, true, false) + "died."));
    }

    public virtual BodyPart GetBodyPart(BodyPartType bodyPartType)
    {
        switch (bodyPartType)
        {
            case BodyPartType.Torso:
                return torso;
            case BodyPartType.Head:
                return head;
            case BodyPartType.LeftArm:
                return leftArm;
            case BodyPartType.RightArm:
                return rightArm;
            case BodyPartType.LeftLeg:
                return leftLeg;
            case BodyPartType.RightLeg:
                return rightLeg;
            case BodyPartType.LeftHand:
                return leftHand;
            case BodyPartType.RightHand:
                return rightHand;
            case BodyPartType.LeftFoot:
                return leftFoot;
            case BodyPartType.RightFoot:
                return rightFoot;
            default:
                return null;
        }
    }

    public virtual void OnWearableChanged(ItemData newItemData, ItemData oldItemData)
    {
        if (newItemData != null)
        {
            torsoDefense.AddModifier(newItemData.torsoDefense);
            headDefense.AddModifier(newItemData.headDefense);
            armDefense.AddModifier(newItemData.armDefense);
            handDefense.AddModifier(newItemData.handDefense);
            legDefense.AddModifier(newItemData.legDefense);
            footDefense.AddModifier(newItemData.footDefense);
        }

        if (oldItemData != null)
        {
            torsoDefense.RemoveModifier(oldItemData.torsoDefense);
            headDefense.RemoveModifier(oldItemData.headDefense);
            armDefense.RemoveModifier(oldItemData.armDefense);
            handDefense.RemoveModifier(oldItemData.handDefense);
            legDefense.RemoveModifier(oldItemData.legDefense);
            footDefense.RemoveModifier(oldItemData.footDefense);
        }
    }

    public virtual void OnWeaponChanged(ItemData newItemData, ItemData oldItemData)
    {
        
    }
}

[System.Serializable]
public class BodyPart
{
    public BodyPartType bodyPartType;
    public Stat maxHealth;
    public int currentHealth;
    public int naturalDefense;

    public BodyPart(BodyPartType bodyPartType, int baseMaxHealth)
    {
        this.bodyPartType = bodyPartType;
        maxHealth.SetBaseValue(baseMaxHealth);
        currentHealth = baseMaxHealth;
    }

    public int DamageHealth(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0)
            currentHealth = 0;
        return currentHealth;
    }

    public int HealInstant_StaticValue(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth.GetValue())
            currentHealth = maxHealth.GetValue();
        return currentHealth;
    }

    public int HealInstant_Percent(float healPercent)
    {
        int healAmount = Mathf.RoundToInt(maxHealth.GetValue() * healPercent);
        if (currentHealth + healAmount > maxHealth.GetValue()) // Make sure not to heal over the max health
        {
            healAmount = maxHealth.GetValue() - currentHealth;
            currentHealth += healAmount;
        }
        else
            currentHealth += healAmount;
        return currentHealth;
    }
}
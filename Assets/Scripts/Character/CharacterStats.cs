using System.Collections;
using UnityEngine;

public enum BodyPart { Torso, Head, LeftArm, RightArm, LeftLeg, RightLeg, LeftHand, RightHand, LeftFoot, RightFoot }

public class CharacterStats : Stats
{
    public Stat maxHeadHealth;
    public int currentHeadHealth { get; private set; }
    public Stat maxLeftArmHealth;
    public int currentLeftArmHealth { get; private set; }
    public Stat maxRightArmHealth;
    public int currentRightArmHealth { get; private set; }
    public Stat maxLeftHandHealth;
    public int currentLeftHandHealth { get; private set; }
    public Stat maxRightHandHealth;
    public int currentRightHandHealth { get; private set; }
    public Stat maxLeftLegHealth;
    public int currentLeftLegHealth { get; private set; }
    public Stat maxRightLegHealth;
    public int currentRightLegHealth { get; private set; }
    public Stat maxLeftFootHealth;
    public int currentLeftFootHealth { get; private set; }
    public Stat maxRightFootHealth;
    public int currentRightFootHealth { get; private set; }

    [Header("Defense")]
    public Stat torsoDefense;
    public Stat headDefense;
    public Stat armDefense;
    public Stat handDefense;
    public Stat legDefense;
    public Stat footDefense;

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
        
        currentHeadHealth = maxHeadHealth.GetValue();
        currentLeftArmHealth = maxLeftArmHealth.GetValue();
        currentRightArmHealth = maxRightArmHealth.GetValue();
        currentLeftHandHealth = maxLeftHandHealth.GetValue();
        currentRightHandHealth = maxRightHandHealth.GetValue();
        currentLeftLegHealth = maxLeftLegHealth.GetValue();
        currentRightLegHealth = maxRightLegHealth.GetValue();
        currentLeftFootHealth = maxLeftFootHealth.GetValue();
        currentRightFootHealth = maxRightFootHealth.GetValue();
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
        HealAllBodyParts(consumable.healPercent);
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

    public void HealAllBodyParts(float healPercent)
    {
        HealBodyPart(healPercent, BodyPart.Torso);
        HealBodyPart(healPercent, BodyPart.Head);
        HealBodyPart(healPercent, BodyPart.LeftArm);
        HealBodyPart(healPercent, BodyPart.RightArm);
        HealBodyPart(healPercent, BodyPart.LeftHand);
        HealBodyPart(healPercent, BodyPart.RightHand);
        HealBodyPart(healPercent, BodyPart.LeftLeg);
        HealBodyPart(healPercent, BodyPart.RightLeg);
        HealBodyPart(healPercent, BodyPart.LeftFoot);
        HealBodyPart(healPercent, BodyPart.RightFoot);
    }

    public int HealBodyPart(float healPercent, BodyPart bodyPartToHeal)
    {
        int healAmount = 0;
        switch (bodyPartToHeal)
        {
            case BodyPart.Torso:
                healAmount = Mathf.RoundToInt(maxHealth.GetValue() * (healPercent / 100f));
                if (currentHealth + healAmount > maxHealth.GetValue())
                {
                    healAmount = maxHealth.GetValue() - currentHealth;
                    currentHealth = maxHealth.GetValue();
                }
                else
                    currentHealth += healAmount;
                break;
            case BodyPart.Head:
                healAmount = Mathf.RoundToInt(maxHeadHealth.GetValue() * (healPercent / 100f));
                if (currentHeadHealth + healAmount > maxHeadHealth.GetValue())
                {
                    healAmount = maxHeadHealth.GetValue() - currentHeadHealth;
                    currentHeadHealth = maxHeadHealth.GetValue();
                }
                else
                    currentHeadHealth += healAmount;
                break;
            case BodyPart.LeftArm:
                healAmount = Mathf.RoundToInt(maxLeftArmHealth.GetValue() * (healPercent / 100f));
                if (currentLeftArmHealth + healAmount > maxLeftArmHealth.GetValue())
                {
                    healAmount = maxLeftArmHealth.GetValue() - currentLeftArmHealth;
                    currentLeftArmHealth = maxLeftArmHealth.GetValue();
                }
                else
                    currentLeftArmHealth += healAmount;
                break;
            case BodyPart.RightArm:
                healAmount = Mathf.RoundToInt(maxRightArmHealth.GetValue() * (healPercent / 100f));
                if (currentRightArmHealth + healAmount > maxRightArmHealth.GetValue())
                {
                    healAmount = maxRightArmHealth.GetValue() - currentRightArmHealth;
                    currentRightArmHealth = maxRightArmHealth.GetValue();
                }
                else
                    currentRightArmHealth += healAmount;
                break;
            case BodyPart.LeftLeg:
                healAmount = Mathf.RoundToInt(maxLeftLegHealth.GetValue() * (healPercent / 100f));
                if (currentLeftLegHealth + healAmount > maxLeftLegHealth.GetValue())
                {
                    healAmount = maxLeftLegHealth.GetValue() - currentLeftLegHealth;
                    currentLeftLegHealth = maxLeftLegHealth.GetValue();
                }
                else
                    currentLeftLegHealth += healAmount;
                break;
            case BodyPart.RightLeg:
                healAmount = Mathf.RoundToInt(maxRightLegHealth.GetValue() * (healPercent / 100f));
                if (currentRightLegHealth + healAmount > maxRightLegHealth.GetValue())
                {
                    healAmount = maxRightLegHealth.GetValue() - currentRightLegHealth;
                    currentRightLegHealth = maxRightLegHealth.GetValue();
                }
                else
                    currentRightLegHealth += healAmount;
                break;
            case BodyPart.LeftHand:
                healAmount = Mathf.RoundToInt(maxLeftHandHealth.GetValue() * (healPercent / 100f));
                if (currentLeftHandHealth + healAmount > maxLeftHandHealth.GetValue())
                {
                    healAmount = maxLeftHandHealth.GetValue() - currentLeftHandHealth;
                    currentLeftHandHealth = maxLeftHandHealth.GetValue();
                }
                else
                    currentLeftHandHealth += healAmount;
                break;
            case BodyPart.RightHand:
                healAmount = Mathf.RoundToInt(maxRightHandHealth.GetValue() * (healPercent / 100f));
                if (currentRightHandHealth + healAmount > maxRightHandHealth.GetValue())
                {
                    healAmount = maxRightHandHealth.GetValue() - currentRightHandHealth;
                    currentRightHandHealth = maxRightHandHealth.GetValue();
                }
                else
                    currentRightHandHealth += healAmount;
                break;
            case BodyPart.LeftFoot:
                healAmount = Mathf.RoundToInt(maxLeftFootHealth.GetValue() * (healPercent / 100f));
                if (currentLeftFootHealth + healAmount > maxLeftFootHealth.GetValue())
                {
                    healAmount = maxLeftFootHealth.GetValue() - currentLeftFootHealth;
                    currentLeftFootHealth = maxLeftFootHealth.GetValue();
                }
                else
                    currentLeftFootHealth += healAmount;
                break;
            case BodyPart.RightFoot:
                healAmount = Mathf.RoundToInt(maxRightFootHealth.GetValue() * (healPercent / 100f));
                if (currentRightFootHealth + healAmount > maxRightFootHealth.GetValue())
                {
                    healAmount = maxRightFootHealth.GetValue() - currentRightFootHealth;
                    currentRightFootHealth = maxRightFootHealth.GetValue();
                }
                else
                    currentRightFootHealth += healAmount;
                break;
            default:
                break;
        }

        //TextPopup.CreateHealPopup(transform.position, healAmount);

        return healAmount;
    }

    public int TakeLocationalDamage(int damage, BodyPart bodyPart, EquipmentManager equipmentManager, bool armorPenetrated, bool clothingPenetrated)
    {
        if (armorPenetrated == false && equipmentManager != null)
            damage -= GetLocationalArmorDefense(equipmentManager, bodyPart);

        if (clothingPenetrated == false && equipmentManager != null)
            damage -= GetLocationalClothingDefense(equipmentManager, bodyPart);

        if (canTakeDamage)
        {
            if (damage < 0)
                damage = 0;

            if (damage > 0)
                TextPopup.CreateDamagePopup(transform.position, damage, false);
            else
                TextPopup.CreateTextStringPopup(transform.position, "Absorbed");

            switch (bodyPart)
            {
                case BodyPart.Torso:
                    currentHealth -= damage;
                    break;
                case BodyPart.Head:
                    currentHeadHealth -= damage;
                    break;
                case BodyPart.LeftArm:
                    currentLeftArmHealth -= damage;
                    break;
                case BodyPart.RightArm:
                    currentRightArmHealth -= damage;
                    break;
                case BodyPart.LeftLeg:
                    currentLeftLegHealth -= damage;
                    break;
                case BodyPart.RightLeg:
                    currentRightLegHealth -= damage;
                    break;
                case BodyPart.LeftHand:
                    currentLeftHandHealth -= damage;
                    break;
                case BodyPart.RightHand:
                    currentRightHandHealth -= damage;
                    break;
                case BodyPart.LeftFoot:
                    currentLeftFootHealth -= damage;
                    break;
                case BodyPart.RightFoot:
                    currentRightFootHealth -= damage;
                    break;
                default:
                    break;
            }
            
            if (currentHealth <= 0 || currentHeadHealth <= 0)
                Die();
        }

        return damage;
    }

    public BodyPart GetBodyPartToHit()
    {
        int random = Random.Range(0, 100);
        if (random < 40)
            return BodyPart.Torso;
        else if (random < 48)
            return BodyPart.Head;
        else if (random < 58)
            return BodyPart.LeftArm;
        else if (random < 68)
            return BodyPart.RightArm;
        else if (random < 78)
            return BodyPart.LeftLeg;
        else if (random < 88)
            return BodyPart.RightArm;
        else if (random < 92)
            return BodyPart.LeftHand;
        else if (random < 96)
            return BodyPart.RightHand;
        else if (random < 98)
            return BodyPart.LeftFoot;
        else
            return BodyPart.RightFoot;
    }

    int GetLocationalArmorDefense(EquipmentManager equipmentManager, BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Torso:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].torsoDefense;
                return 0;
            case BodyPart.Head:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet].headDefense;
                return 0;
            case BodyPart.LeftArm:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].armDefense;
                return 0;
            case BodyPart.RightArm:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].armDefense;
                return 0;
            case BodyPart.LeftLeg:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].legDefense;
                return 0;
            case BodyPart.RightLeg:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].legDefense;
                return 0;
            case BodyPart.LeftHand:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].handDefense;
                return 0;
            case BodyPart.RightHand:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].handDefense;
                return 0;
            case BodyPart.LeftFoot:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].footDefense;
                return 0;
            case BodyPart.RightFoot:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].footDefense;
                return 0;
            default:
                return 0;
        }
    }

    public int GetLocationalClothingDefense(EquipmentManager equipmentManager, BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Torso:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].torsoDefense;
                return 0;
            case BodyPart.LeftArm:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].armDefense;
                return 0;
            case BodyPart.RightArm:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].armDefense;
                return 0;
            case BodyPart.LeftLeg:
                if (equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                    return equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].legDefense;
                return 0;
            case BodyPart.RightLeg:
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

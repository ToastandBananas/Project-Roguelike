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

    [Header("Blood")]
    public Stat maxBloodAmount;
    public int currentBloodAmount;

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

    float healingBuildup;

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
        if (consumable.instantHealPercent > 0)
            HealAllBodyParts(consumable.instantHealPercent, true);
        if (consumable.healPercent > 0)
            HealAllBodyParts(consumable.healPercent, false);

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

    public void HealAllBodyParts(float healPercent, bool healInstantly)
    {
        if (healInstantly)
        {
            HealBodyPart(healPercent, BodyPart.Torso, true);
            HealBodyPart(healPercent, BodyPart.Head, true);
            HealBodyPart(healPercent, BodyPart.LeftArm, true);
            HealBodyPart(healPercent, BodyPart.RightArm, true);
            HealBodyPart(healPercent, BodyPart.LeftHand, true);
            HealBodyPart(healPercent, BodyPart.RightHand, true);
            HealBodyPart(healPercent, BodyPart.LeftLeg, true);
            HealBodyPart(healPercent, BodyPart.RightLeg, true);
            HealBodyPart(healPercent, BodyPart.LeftFoot, true);
            HealBodyPart(healPercent, BodyPart.RightFoot, true);
        }
        else
        {
            HealBodyPart(healPercent, BodyPart.Torso, false);
            HealBodyPart(healPercent, BodyPart.Head, false);
            HealBodyPart(healPercent, BodyPart.LeftArm, false);
            HealBodyPart(healPercent, BodyPart.RightArm, false);
            HealBodyPart(healPercent, BodyPart.LeftHand, false);
            HealBodyPart(healPercent, BodyPart.RightHand, false);
            HealBodyPart(healPercent, BodyPart.LeftLeg, false);
            HealBodyPart(healPercent, BodyPart.RightLeg, false);
            HealBodyPart(healPercent, BodyPart.LeftFoot, false);
            HealBodyPart(healPercent, BodyPart.RightFoot, false);
        }
    }

    public int HealBodyPart(float healPercent, BodyPart bodyPartToHeal, bool healInstantly)
    {
        int healAmount = 0;
        if (healInstantly)
            healAmount = HealBodyPart_Instant(healPercent, bodyPartToHeal);
        else
            healingBuildup += healPercent;

        if (healInstantly)
            TextPopup.CreateHealPopup(transform.position, healAmount);

        return healAmount;
    }

    void ApplyHealingBuildup()
    {
        if (healingBuildup > 0)
        {
            // TODO: Heal over time
        }
    }

    int HealBodyPart_Instant(float healPercent, BodyPart bodyPartToHeal)
    {
        Stat maxHealth = GetBodyPartsMaxHealth(bodyPartToHeal);
        int healAmount = Mathf.RoundToInt(maxHealth.GetValue() * healPercent);
        if (GetCurrentHealth(bodyPartToHeal) + healAmount > maxHealth.GetValue()) // Make sure not to heal over the max health
        {
            healAmount = maxHealth.GetValue() - GetCurrentHealth(bodyPartToHeal);
            AddToCurrentHealth_Instant(bodyPartToHeal, healAmount);
        }
        else
            AddToCurrentHealth_Instant(bodyPartToHeal, healAmount);

        return healAmount;
    }

    int AddToCurrentHealth_Instant(BodyPart bodyPart, int healAmount)
    {
        switch (bodyPart)
        {
            case BodyPart.Torso:
                return currentHealth += healAmount;
            case BodyPart.Head:
                return currentHeadHealth += healAmount;
            case BodyPart.LeftArm:
                return currentLeftArmHealth += healAmount;
            case BodyPart.RightArm:
                return currentRightArmHealth += healAmount;
            case BodyPart.LeftLeg:
                return currentLeftLegHealth += healAmount;
            case BodyPart.RightLeg:
                return currentRightLegHealth += healAmount;
            case BodyPart.LeftHand:
                return currentLeftHandHealth += healAmount;
            case BodyPart.RightHand:
                return currentRightHandHealth += healAmount;
            case BodyPart.LeftFoot:
                return currentLeftFootHealth += healAmount;
            case BodyPart.RightFoot:
                return currentRightFootHealth += healAmount;
            default:
                return 0;
        }
    }

    int GetCurrentHealth(BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Torso:
                return currentHealth;
            case BodyPart.Head:
                return currentHeadHealth;
            case BodyPart.LeftArm:
                return currentLeftArmHealth;
            case BodyPart.RightArm:
                return currentRightArmHealth;
            case BodyPart.LeftLeg:
                return currentLeftLegHealth;
            case BodyPart.RightLeg:
                return currentRightLegHealth;
            case BodyPart.LeftHand:
                return currentLeftHandHealth;
            case BodyPart.RightHand:
                return currentRightHandHealth;
            case BodyPart.LeftFoot:
                return currentLeftFootHealth;
            case BodyPart.RightFoot:
                return currentRightFootHealth;
            default:
                return 0;
        }
    }

    public int TakeStaticLocationalDamage(int damage, BodyPart bodyPart)
    {
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

    public int TakeLocationalDamage(int bluntDamage, int pierceDamage, int slashDamage, int cleaveDamage, BodyPart bodyPart, EquipmentManager equipmentManager, Wearable armor, Wearable clothing, bool armorPenetrated, bool clothingPenetrated)
    {
        int totalDamage = bluntDamage + pierceDamage + slashDamage + cleaveDamage;

        if (armorPenetrated == false && equipmentManager != null)
            totalDamage -= GetLocationalArmorDefense(equipmentManager, bodyPart);

        if (clothingPenetrated == false && equipmentManager != null)
            totalDamage -= GetLocationalClothingDefense(equipmentManager, bodyPart);

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
                    TraumaSystem.ApplyInjury(characterManager, gm.traumaSystem.GetCut(characterManager, bodyPart, slashDamage), bodyPart);
                }
                else if (cleaveDamage > 0)
                {

                }
            }

            if (totalDamage > 0)
                TextPopup.CreateDamagePopup(transform.position, totalDamage, false);
            else
                TextPopup.CreateTextStringPopup(transform.position, "Absorbed");

            switch (bodyPart)
            {
                case BodyPart.Torso:
                    currentHealth -= totalDamage;
                    break;
                case BodyPart.Head:
                    currentHeadHealth -= totalDamage;
                    break;
                case BodyPart.LeftArm:
                    currentLeftArmHealth -= totalDamage;
                    break;
                case BodyPart.RightArm:
                    currentRightArmHealth -= totalDamage;
                    break;
                case BodyPart.LeftLeg:
                    currentLeftLegHealth -= totalDamage;
                    break;
                case BodyPart.RightLeg:
                    currentRightLegHealth -= totalDamage;
                    break;
                case BodyPart.LeftHand:
                    currentLeftHandHealth -= totalDamage;
                    break;
                case BodyPart.RightHand:
                    currentRightHandHealth -= totalDamage;
                    break;
                case BodyPart.LeftFoot:
                    currentLeftFootHealth -= totalDamage;
                    break;
                case BodyPart.RightFoot:
                    currentRightFootHealth -= totalDamage;
                    break;
                default:
                    break;
            }
            
            if (currentHealth <= 0 || currentHeadHealth <= 0)
                Die();
        }

        return totalDamage;
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

    public Stat GetBodyPartsMaxHealth(BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Torso:
                return maxHealth;
            case BodyPart.Head:
                return maxHeadHealth;
            case BodyPart.LeftArm:
                return maxLeftArmHealth;
            case BodyPart.RightArm:
                return maxRightArmHealth;
            case BodyPart.LeftLeg:
                return maxLeftLegHealth;
            case BodyPart.RightLeg:
                return maxRightLegHealth;
            case BodyPart.LeftHand:
                return maxLeftHandHealth;
            case BodyPart.RightHand:
                return maxRightHandHealth;
            case BodyPart.LeftFoot:
                return maxLeftFootHealth;
            case BodyPart.RightFoot:
                return maxRightFootHealth;
            default:
                return null;
        }
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

using System.Collections.Generic;
using UnityEngine;

public enum LocomotionType { Inanimate, Humanoid, Biped, Quadruped, Hexapod, Octoped }
public enum BodyPartType { Torso, Head, LeftArm, RightArm, LeftLeg, RightLeg, LeftHand, RightHand, LeftFoot, RightFoot }
/*public enum TorsoSublocation_Front { LeftShoulder, RightShoulder, Sternum, LeftPectoral, RightPectoral, LeftRibCage, RightRibCage, Gut }
public enum TorsoSublocation_Back { LeftShoulder, RightShoulder, UpperLeftBack, UpperRightBack, MidLeftBack, MidRightBack, LowerLeftBack, LowerRightBack }
public enum HeadSublocation_Front { Head, Forehead, Nose, UpperLip, LowerLip, LeftCheek, RightCheek, Chin, LeftEye, RightEye, LeftNeck, RightNeck, Throat }
public enum HeadSublocation_Back { Head, LeftNeck, RightNeck, Neck }
public enum ArmSublocation_Front { Wrist, Forearm, InnerElbow, Bicep, UpperArm }
public enum ArmSublocation_Back {  }*/

public class Status : MonoBehaviour
{
    public LocomotionType locomotionType = LocomotionType.Humanoid;

    [Header("Lists")]
    public List<BodyPart> bodyParts = new List<BodyPart>();
    public List<Buff> buffs = new List<Buff>();

    [Header("Stamina")]
    public IntStat maxStamina;
    public float currentStamina;
    public FloatStat staminaRegenPercent;
    int staminaRegenTurnCooldown;

    [Header("Healthiness")]
    public IntStat maxBloodAmount;
    public float currentBloodAmount;

    [HideInInspector] public bool isDead;
    [HideInInspector] public CharacterManager characterManager;

    readonly float naturalHealingPercentPerTurn = 0.0001f; // 60 minutes (100 turns) to naturally heal entire body 1% if healthiness == 100f
    readonly float naturalBloodProductionPerTurn = 0.0164f; // 1 pint takes 48 hours IRL and 1 pint = 473 mL | (473mL / 48hr / 60min / 60sec) * 6sec/turn = 0.0164mL/turn

    // Healthiness signifies your overall bodily health, effecting your natural healing over time.
    // Eating healthy, among other things, slowly builds this up over time, but this can be easily reversed by ailments such as an infection or a concussion.
    float healthiness = 50f;
    readonly float minHealthiness = -200f;
    readonly float maxHealthiness = 200f;

    // Used whenever a critical hit happens (always happens with attacks from behind)
    readonly Vector2 criticalHitMultiplier = new Vector2(1.15f, 1.4f);

    GameManager gm;

    void Awake()
    {
        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();

        currentStamina = maxStamina.GetValue();
        currentBloodAmount = maxBloodAmount.GetValue();

        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].characterManager = characterManager;
            bodyParts[i].currentHealth = bodyParts[i].maxHealth.GetValue();
        }
    }

    void Start()
    {
        gm = GameManager.instance;
    }

    #region Healthiness
    public float GetHealthiness()
    {
        return healthiness;
    }

    public void AdjustHealthiness(float amount)
    {
        healthiness += amount;
        if (healthiness < minHealthiness)
            healthiness = minHealthiness;
        else if (healthiness > maxHealthiness)
            healthiness = maxHealthiness;
    }
    #endregion

    #region Update Buffs/Injuries
    public void UpdateBuffs(int timePassed = TimeSystem.defaultTimeTickInSeconds)
    {
        Heal_Passive(timePassed);
        ApplyHealingBuildup();

        if (buffs.Count > 0)
        {
            int buffCount = buffs.Count;
            for (int i = buffCount - 1; i >= 0; i--)
            {
                // Update buff time remaining
                buffs[i].buffTimeRemaining -= timePassed;
                if (buffs[i].buffTimeRemaining <= 0)
                    buffs.Remove(buffs[i]);
            }
        }
    }

    public void UpdateInjuries(int timePassed = TimeSystem.defaultTimeTickInSeconds)
    {
        if (isDead) return;

        Bleed(timePassed);
        ApplyDamageBuildup();

        // For each injury on each body part
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (bodyParts[i].injuries.Count > 0)
            {
                int injuryCount = bodyParts[i].injuries.Count;
                for (int j = injuryCount - 1; j >= 0; j--)
                {
                    if (bodyParts[i].injuries[j] == null)
                        continue;

                    // Update bandage soil, if one has been applied to the wound
                    if (bodyParts[i].injuries[j].bandage != null)
                        bodyParts[i].injuries[j].SoilBandage(0.1f / bodyParts[i].injuries[j].bandage.quality);

                    // Update injury time remaining
                    bodyParts[i].injuries[j].injuryTimeRemaining -= Mathf.RoundToInt(timePassed * bodyParts[i].injuries[j].injuryHealMultiplier);
                    if (bodyParts[i].injuries[j].injuryTimeRemaining <= 0 && bodyParts[i].injuries[j].bandageItemData == null)
                    {
                        bodyParts[i].injuries.Remove(bodyParts[i].injuries[j]);
                        gm.healthDisplay.UpdateHealthHeaderColor(bodyParts[i].bodyPartType, bodyParts[i]);
                    }
                    // If this is an NPC and the injury is finished healing, but it still has an applied medical item
                    else if (characterManager.isNPC && bodyParts[i].injuries[j].injuryTimeRemaining <= 0 && characterManager.stateController.currentState != State.Fight 
                        && characterManager.humanoidSpriteManager != null && characterManager.actions.Count == 0)
                    {
                        // If the applied medical item is a bandage
                        if (bodyParts[i].injuries[j].bandageItemData != null)
                            characterManager.QueueAction(bodyParts[i].injuries[j].RemoveMedicalItem(characterManager, MedicalSupplyType.Bandage), APManager.instance.GetRemoveMedicalItemAPCost(bodyParts[i].injuries[j].bandage));
                    }

                    // If this is an NPC and any of their injuries are untreated, check if they have the appropriate medical items and if so, treat the wound
                    if (characterManager.isNPC && characterManager.stateController.currentState != State.Fight && characterManager.humanoidSpriteManager != null && characterManager.actions.Count == 0 
                        && bodyParts[i].injuries[j].InjuryRemedied() == false && (bodyParts[i].injuries[j].injuryTimeRemaining > 240 || bodyParts[i].injuries[j].bleedTimeRemaining > 0))
                    {
                        // Bandage the injury?
                        if (bodyParts[i].injuries[j].injury.CanBandage() && bodyParts[i].injuries[j].bandage == null)
                        {
                            List<ItemData> bandages = characterManager.GetMedicalSupplies(MedicalSupplyType.Bandage);
                            if (bandages.Count > 0)
                                characterManager.QueueAction(bodyParts[i].injuries[j].ApplyMedicalItem(characterManager, bandages[0], bandages[0].parentInventory, null), APManager.instance.GetApplyMedicalItemAPCost((MedicalSupply)bandages[0].item));
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Bleed
    void Bleed(int timePassed)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            for (int k = 0; k < bodyParts[i].injuries.Count; k++)
            {
                // Lower blood loss per turn as the injury heals
                if (bodyParts[i].injuries[k].injury.GetBloodLossPerTurn().x > 0 && bodyParts[i].injuries[k].bloodLossPerTurn >= bodyParts[i].injuries[k].injury.GetBloodLossPerTurn().x / 2f)
                    bodyParts[i].injuries[k].bloodLossPerTurn -= bodyParts[i].injuries[k].injury.GetBloodLossPerTurn().x * 0.001f;

                // If the injury is still bleeding
                if (bodyParts[i].injuries[k].bleedTimeRemaining > 0)
                {
                    // if (characterManager.isNPC) Debug.Log(bodyParts[i].bodyPartType + " bleeds from " + bodyParts[i].injuries[k].injury.name);

                    // Add to the damage buildup
                    bodyParts[i].damageBuildup += bodyParts[i].injuries[k].damagePerTurn;

                    // Lose some blood
                    LoseBlood(bodyParts[i].injuries[k].bloodLossPerTurn);

                    // Update bandage soil, if one has been applied to the wound
                    if (bodyParts[i].injuries[k].bandage != null)
                        bodyParts[i].injuries[k].SoilBandage(bodyParts[i].injuries[k].bloodLossPerTurn / 10f / bodyParts[i].injuries[k].bandage.quality);

                    // Update bleed time remaining
                    bodyParts[i].injuries[k].bleedTimeRemaining -= Mathf.RoundToInt(timePassed * bodyParts[i].injuries[k].injuryHealMultiplier);
                    if (bodyParts[i].injuries[k].bleedTimeRemaining < 0)
                    {
                        bodyParts[i].injuries[k].bleedTimeRemaining = 0;

                        if (characterManager.isNPC == false)
                            gm.healthDisplay.UpdateHealthHeaderColor(bodyParts[i].bodyPartType, bodyParts[i]);
                    }
                }
            }

            // Show some flavor text if the injury is bleeding
            if (bodyParts[i].damageBuildup >= 1f && characterManager.isNPC == false)
                gm.flavorText.WriteLine_Bleed(characterManager, bodyParts[i].bodyPartType, Mathf.FloorToInt(bodyParts[i].damageBuildup));
        }
    }

    public void LoseBlood(float amount)
    {
        currentBloodAmount -= amount;
        // TODO: Negative effects, such as lightheadedness and fainting
        
    }

    public void AddBlood(float amount)
    {
        currentBloodAmount += amount;
        if (currentBloodAmount > maxBloodAmount.GetValue())
            currentBloodAmount = maxBloodAmount.GetValue();
    }

    public bool BodyPartIsBleeding(BodyPartType bodyPartType)
    {
        BodyPart bodyPart = GetBodyPart(bodyPartType);
        for (int i = 0; i < bodyPart.injuries.Count; i++)
        {
            if (bodyPart.injuries[i].bleedTimeRemaining > 0)
                return true;
        }
        return false;
    }
    #endregion

    #region Damage
    void ApplyDamageBuildup()
    {
        int roundedDamageAmount = 0;
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (bodyParts[i].damageBuildup >= 1f)
            {
                roundedDamageAmount = Mathf.FloorToInt(bodyParts[i].damageBuildup);
                TakeLocationalDamage_IgnoreArmor(roundedDamageAmount, bodyParts[i].bodyPartType);
                bodyParts[i].damageBuildup -= roundedDamageAmount;
            }
        }
    }

    public int TakeLocationalDamage_IgnoreArmor(int damage, BodyPartType bodyPartType)
    {
        if (damage < 0)
            damage = 0;

        if (damage > 0)
            TextPopup.CreateDamagePopup(characterManager.spriteRenderer, transform.position, damage, false);
        else
            TextPopup.CreateTextStringPopup(characterManager.spriteRenderer, transform.position, "Absorbed");

        BodyPart bodyPart = GetBodyPart(bodyPartType);
        bodyPart.Damage(damage);

        if ((bodyPart.bodyPartType == BodyPartType.Torso || bodyPart.bodyPartType == BodyPartType.Head) && bodyPart.currentHealth <= 0)
            Die();

        return damage;
    }

    public int TakeLocationalDamage(CharacterManager attacker, int bluntDamage, int pierceDamage, int slashDamage, int cleaveDamage, bool attackedFromBehind, BodyPartType bodyPartType, EquipmentManager equipmentManager, Wearable armor, Wearable clothing, bool armorPenetrated, bool clothingPenetrated)
    {
        bool criticalHit = false;
        if (attackedFromBehind || Random.Range(1f, 100f) <= attacker.characterStats.GetCriticalChance())
            criticalHit = true;

        if (criticalHit)
        {
            bluntDamage = Mathf.RoundToInt(bluntDamage * Random.Range(criticalHitMultiplier.x, criticalHitMultiplier.y));
            pierceDamage = Mathf.RoundToInt(pierceDamage * Random.Range(criticalHitMultiplier.x, criticalHitMultiplier.y));
            slashDamage = Mathf.RoundToInt(slashDamage * Random.Range(criticalHitMultiplier.x, criticalHitMultiplier.y));
            cleaveDamage = Mathf.RoundToInt(cleaveDamage * Random.Range(criticalHitMultiplier.x, criticalHitMultiplier.y));
        }

        int startTotalDamage = bluntDamage + pierceDamage + slashDamage + cleaveDamage;
        int finalTotalDamage = startTotalDamage;

        int damageTypeCount = 0;
        if (bluntDamage > 0) damageTypeCount++;
        if (pierceDamage > 0) damageTypeCount++;
        if (slashDamage > 0) damageTypeCount++;
        if (cleaveDamage > 0) damageTypeCount++;

        BodyPart bodyPart = GetBodyPart(bodyPartType);
        if (armorPenetrated == false && equipmentManager != null)
            finalTotalDamage -= bodyPart.addedDefense_Armor.GetValue();

        if (clothingPenetrated == false && equipmentManager != null)
            finalTotalDamage -= bodyPart.addedDefense_Clothing.GetValue();

        finalTotalDamage -= bodyPart.naturalDefense.GetValue();

        #region Adjust each type of damage to reflect the final total damage
        if (damageTypeCount == 1)
        {
            if (bluntDamage > 0)
                bluntDamage = finalTotalDamage;
            else if (pierceDamage > 0)
                pierceDamage = finalTotalDamage;
            else if (slashDamage > 0)
                slashDamage = finalTotalDamage;
            else if (cleaveDamage > 0)
                cleaveDamage = finalTotalDamage;
        }
        else if (startTotalDamage > 0)
        {
            float percentDamageReduction = (startTotalDamage - finalTotalDamage) / startTotalDamage;
            if (bluntDamage > 0)
                bluntDamage -= Mathf.FloorToInt(bluntDamage * percentDamageReduction);
            if (pierceDamage > 0)
                pierceDamage -= Mathf.FloorToInt(pierceDamage * percentDamageReduction);
            if (slashDamage > 0)
                slashDamage -= Mathf.FloorToInt(slashDamage * percentDamageReduction);
            if (cleaveDamage > 0)
                cleaveDamage = Mathf.FloorToInt(cleaveDamage * percentDamageReduction);

            int postDamageReductionTotalDamage = bluntDamage + pierceDamage + slashDamage + cleaveDamage;

            if (finalTotalDamage != postDamageReductionTotalDamage)
            {
                int difference = Mathf.Abs(finalTotalDamage - postDamageReductionTotalDamage);
                if (bluntDamage > 0 && difference > 0)
                {
                    bluntDamage--;
                    difference--;
                }
                if (pierceDamage > 0 && difference > 0)
                {
                    pierceDamage--;
                    difference--;
                }
                if (slashDamage > 0 && difference > 0)
                {
                    slashDamage--;
                    difference--;
                }
                if (cleaveDamage > 0 && difference > 0)
                {
                    cleaveDamage--;
                    difference--;
                }
            }
        }
        #endregion

        if (finalTotalDamage <= 0)
            finalTotalDamage = 0;
        else if ((armor == null || armorPenetrated) && (clothing == null || clothingPenetrated))
        {
            // If armor & clothing were penetrated or the character isn't wearing either, 
            // cause an injury based off of damage types and amounts relative to the character's max health for the body part attacked
            ApplyInjuries(characterManager, bodyPart, bluntDamage, pierceDamage, slashDamage, cleaveDamage, attackedFromBehind);
        }

        if (finalTotalDamage > 0)
        {
            TextPopup.CreateDamagePopup(characterManager.spriteRenderer, transform.position, finalTotalDamage, criticalHit);
            bodyPart.Damage(finalTotalDamage);
            if ((bodyPart.bodyPartType == BodyPartType.Torso || bodyPart.bodyPartType == BodyPartType.Head) && bodyPart.currentHealth <= 0)
                Die();
        }
        else
            TextPopup.CreateTextStringPopup(characterManager.spriteRenderer, transform.position, "Absorbed");

        return finalTotalDamage;
    }

    void ApplyInjuries(CharacterManager characterManager, BodyPart bodyPart, int bluntDamage, int pierceDamage, int slashDamage, int cleaveDamage, bool attackedFromBehind)
    {
        // If there are already injuries on this body part
        if (bodyPart.injuries.Count > 0)
        {
            int random = Random.Range(0, 100);
            if (random < 10 * bodyPart.injuries.Count) // 10% chance per injury to hit an existing injury
            {
                List<LocationalInjury> applicableInjuries = new List<LocationalInjury>();
                for (int i = 0; i < bodyPart.injuries.Count; i++)
                {
                    if (bodyPart.injuries[i].onBackOfBodyPart == attackedFromBehind)
                        applicableInjuries.Add(bodyPart.injuries[i]);
                }

                if (applicableInjuries.Count > 0)
                {
                    random = Random.Range(0, applicableInjuries.Count);

                    int random2 = Random.Range(0, 100);
                    if (applicableInjuries[random].bandage == null || random2 < 50 / applicableInjuries[random].bandage.quality)
                    {
                        applicableInjuries[random].Reinjure();
                        if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                            gm.flavorText.WriteLine_Reinjure(characterManager, applicableInjuries[random].injury, bodyPart.bodyPartType);
                    }
                }
            }
        }

        // Debug.Log(name + " was injured.");
        if (bluntDamage > 0)
        {

        }

        if (pierceDamage > 0)
        {

        }

        // Let cleave damage take priority, since the injuries caused by them are relatively similar to slashing injuries, but cleave injuries are more severe
        if (slashDamage > 0 && slashDamage > cleaveDamage)
        {
            Injury laceration = gm.healthSystem.GetLaceration(characterManager, bodyPart.bodyPartType, slashDamage);
            HealthSystem.ApplyInjury(characterManager, laceration, bodyPart.bodyPartType, attackedFromBehind);

            if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                gm.flavorText.WriteLine_Injury(characterManager, laceration, bodyPart.bodyPartType);
        }
        else if (cleaveDamage > 0)
        {

        }
    }

    public virtual void Die()
    {
        isDead = true;
        GameTiles.RemoveCharacter(transform.position);
        GameTiles.AddDeadCharacter(characterManager, transform.position);

        if (characterManager.isNPC) // If an NPC dies
        {
            gm.turnManager.npcs.Remove(characterManager);

            if (gm.turnManager.npcsFinishedTakingTurnCount >= gm.turnManager.npcs.Count)
                gm.turnManager.ReadyPlayersTurn();

            characterManager.stateController.enabled = false;
            characterManager.npcMovement.AIDestSetter.enabled = false;
            characterManager.npcMovement.AIPath.enabled = false;

            if (characterManager.IsNextToPlayer())
                gm.containerInvUI.GetItemsAroundPlayer();
        }
        
        gameObject.tag = "Dead Body";
        gameObject.layer = 14;

        characterManager.spriteRenderer.sortingLayerName = "Items";
        characterManager.spriteRenderer.sortingOrder = -1;

        if (characterManager.attack.isAttacking)
            characterManager.attack.CancelAttack();
        characterManager.attack.enabled = false;

        // Add equipped items to the body's inventory
        if (characterManager.equipmentManager != null)
        {
            for (int i = 0; i < characterManager.equipmentManager.currentEquipment.Length; i++)
            {
                if (characterManager.equipmentManager.currentEquipment[i] != null)
                {
                    characterManager.personalInventory.items.Add(characterManager.equipmentManager.currentEquipment[i]);
                    characterManager.equipmentManager.currentEquipment[i] = null;
                }
            }
        }

        // Add applied items to the body's inventory
        for (int i = 0; i < characterManager.status.bodyParts.Count; i++)
        {
            for (int j = 0; j < characterManager.status.bodyParts[i].injuries.Count; j++)
            {
                if (characterManager.status.bodyParts[i].injuries[j].bandageItemData != null)
                {
                    characterManager.status.bodyParts[i].injuries[j].bandageItemData.transform.SetParent(characterManager.personalInventory.itemsParent);
                    characterManager.personalInventory.items.Add(characterManager.status.bodyParts[i].injuries[j].bandageItemData);
                }
            }

            characterManager.status.bodyParts[i].injuries.Clear();
        }

        // Remove this character from other surrounding character's vision
        for (int i = 0; i < characterManager.vision.enemiesInRange.Count; i++)
        {
            if (characterManager.vision.enemiesInRange[i].vision.enemiesInRange.Contains(characterManager))
                characterManager.vision.enemiesInRange[i].vision.enemiesInRange.Remove(characterManager);

            if (characterManager.vision.enemiesInRange[i].vision.knownEnemiesInRange.Contains(characterManager))
                characterManager.vision.enemiesInRange[i].vision.knownEnemiesInRange.Remove(characterManager);

            if (characterManager.vision.enemiesInRange[i].isNPC && characterManager.vision.enemiesInRange[i].npcMovement.target == characterManager)
                characterManager.vision.enemiesInRange[i].npcAttack.SwitchTarget(characterManager.vision.enemiesInRange[i].vision.GetClosestKnownEnemy());
        }

        characterManager.vision.visionCollider.enabled = false;
        characterManager.vision.enabled = false;

        characterManager.movement.enabled = false;
        characterManager.ResetActionsQueue();

        characterManager.spriteManager.SetToDeathSprite(characterManager.spriteRenderer);

        if (gm.playerManager.CanSee(characterManager.spriteRenderer))
            gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine(Utilities.GetPronoun(characterManager, true, false) + "died."));
    }
    #endregion

    #region Heal
    void Heal_Passive(int timePassed)
    {
        // Apply natural healing first, if the character is healthy enough
        if (healthiness > 0)
        {
            AddBlood(naturalBloodProductionPerTurn);
            for (int i = 0; i < bodyParts.Count; i++)
            {
                if (BodyPartIsBleeding(bodyParts[i].bodyPartType) == false)
                    bodyParts[i].healingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * bodyParts[i].maxHealth.GetValue();
            }
        }

        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].healTimeRemaining > 0)
            {
                for (int j = 0; j < bodyParts.Count; j++)
                {
                    bodyParts[j].healingBuildup += buffs[i].healPercentPerTurn * bodyParts[j].maxHealth.GetValue();
                }

                buffs[i].healTimeRemaining -= timePassed;
                if (buffs[i].healTimeRemaining < 0)
                    buffs[i].healTimeRemaining = 0;
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

    void ApplyHealingBuildup()
    {
        int roundedHealingAmount = 0;
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (bodyParts[i].healingBuildup >= 1f)
            {
                roundedHealingAmount = Mathf.FloorToInt(bodyParts[i].healingBuildup);
                bodyParts[i].HealInstant_StaticValue(roundedHealingAmount);
                bodyParts[i].healingBuildup -= roundedHealingAmount;
            }
        }
    }
    #endregion

    #region Stamina
    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0)
            currentStamina = 0;
        else
            currentStamina = Mathf.RoundToInt(currentStamina * 100f) / 100f;

        StartStaminaRegenCooldown();

        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();
    }

    public void GainStamina(float amount)
    {
        currentStamina += amount;
        if (currentStamina > maxStamina.GetValue())
            currentStamina = maxStamina.GetValue();
        else
            currentStamina = Mathf.RoundToInt(currentStamina * 100f) / 100f;
        
        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();
    }

    public void RegenerateStamina(int timePassed = 6)
    {
        if (staminaRegenTurnCooldown > 0)
            staminaRegenTurnCooldown -= Mathf.RoundToInt(timePassed / TimeSystem.defaultTimeTickInSeconds);
        else
            GainStamina(staminaRegenPercent.GetValue() * maxStamina.GetValue() * (timePassed / TimeSystem.defaultTimeTickInSeconds)); // Divide by the amount of turns that have passed since last regen
    }

    public void StartStaminaRegenCooldown(int turnCount = 5)
    {
        staminaRegenTurnCooldown = turnCount;
    }

    public bool HasEnoughStamina(float amountNeeded)
    {
        if (currentStamina >= amountNeeded)
            return true;
        return false;
    }
    #endregion

    public virtual BodyPart GetBodyPart(BodyPartType bodyPartType)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (bodyParts[i].bodyPartType == bodyPartType)
                return bodyParts[i];
        }
        return null;
    }

    public virtual BodyPartType GetBodyPartToHit()
    {
        int random = Random.Range(0, 100);
        if (random < 40)
            return BodyPartType.Torso;
        else if (random < 48)
            return BodyPartType.Head;
        else if (random < 59)
            return BodyPartType.LeftArm;
        else if (random < 70)
            return BodyPartType.RightArm;
        else if (random < 80)
            return BodyPartType.LeftLeg;
        else if (random < 90)
            return BodyPartType.RightLeg;
        else if (random < 94)
            return BodyPartType.LeftHand;
        else if (random < 98)
            return BodyPartType.RightHand;
        else if (random < 99)
            return BodyPartType.LeftFoot;
        else
            return BodyPartType.RightFoot;
    }
}
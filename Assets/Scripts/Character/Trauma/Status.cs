using System.Collections;
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

    [Header("Healthiness")]
    public Stat maxBloodAmount;
    public float currentBloodAmount;
    public Stat maxFullness;
    public float currentFullness;
    public Stat maxWater;
    public float currentWater;

    [HideInInspector] public bool isDead;
    [HideInInspector] public CharacterManager characterManager;

    readonly float naturalHealingPercentPerTurn = 0.0001f; // 60 minutes (100 turns) to heal entire body 1% if healthiness == 1
    readonly float naturalBloodProductionPerTurn = 0.0164f; // 1 pint takes 48 hours IRL and 1 pint = 473 mL | (473mL / 48hr / 60min / 60sec) * 6sec/turn = 0.0164mL/turn

    // Healthiness signifies your overall bodily health, effecting your natural healing over time.
    // Eating healthy, among other things, slowly builds this up over time, but this can be easily reversed by ailments such as an infection or a concussion.
    float healthiness = 50f;
    readonly float minHealthiness = -200f;
    readonly float maxHealthiness = 200f;

    // Used whenever a critical hit happens (always happens with attacks from behind)
    readonly Vector2 criticalHitMultiplier = new Vector2(1.15f, 1.4f);

    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();

        currentBloodAmount = maxBloodAmount.GetValue();

        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].currentHealth = bodyParts[i].maxHealth.GetValue();
        }

        characterManager.equipmentManager.onWearableChanged += OnWearableChanged;
        characterManager.equipmentManager.onWeaponChanged += OnWeaponChanged;
    }

    public float GetHealthiness()
    {
        return healthiness;
    }

    public void AdjustHealthiness(float amount)
    {
        healthiness += amount;
        if (healthiness > maxHealthiness)
            healthiness = maxHealthiness;
        if (healthiness < minHealthiness)
            healthiness = minHealthiness;
    }

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
                    // Update bandage soil, if one has been applied to the wound
                    if (bodyParts[i].injuries[j].bandage != null)
                        bodyParts[i].injuries[j].SoilBandage(0.1f / bodyParts[i].injuries[j].bandage.quality);

                    // Update injury time remaining
                    bodyParts[i].injuries[j].injuryTimeRemaining -= Mathf.RoundToInt(timePassed * bodyParts[i].injuries[j].injuryHealMultiplier);
                    if (bodyParts[i].injuries[j].injuryTimeRemaining <= 0)
                        bodyParts[i].injuries.Remove(bodyParts[i].injuries[j]);
                }
            }
        }
    }

    void Bleed(int timePassed)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            for (int k = 0; k < bodyParts[i].injuries.Count; k++)
            {
                // If the injury is still bleeding
                if (bodyParts[i].injuries[k].bleedTimeRemaining > 0)
                {
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
                        bodyParts[i].injuries[k].bleedTimeRemaining = 0;
                }
            }

            // Show some flavor text if the injury is bleeding
            if (bodyParts[i].damageBuildup >= 1f && characterManager.isNPC == false)
                gm.flavorText.WriteBleedLine(characterManager, bodyParts[i].bodyPartType, Mathf.FloorToInt(bodyParts[i].damageBuildup));
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
            TextPopup.CreateDamagePopup(transform.position, damage, false);
        else
            TextPopup.CreateTextStringPopup(transform.position, "Absorbed");

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
            TextPopup.CreateDamagePopup(transform.position, finalTotalDamage, criticalHit);
            bodyPart.Damage(finalTotalDamage);
            if ((bodyPart.bodyPartType == BodyPartType.Torso || bodyPart.bodyPartType == BodyPartType.Head) && bodyPart.currentHealth <= 0)
                Die();
        }
        else
            TextPopup.CreateTextStringPopup(transform.position, "Absorbed");

        return finalTotalDamage;
    }

    void ApplyInjuries(CharacterManager characterManager, BodyPart bodyPart, int bluntDamage, int pierceDamage, int slashDamage, int cleaveDamage, bool attackedFromBehind)
    {
        // If there are already injuries on this body part
        if (bodyPart.injuries.Count > 0)
        {
            int random = Random.Range(1, 100);
            if (random < 100) // 15% chance to hit the same injury
            {
                List<LocationalInjury> applicableInjuries = new List<LocationalInjury>();
                for (int i = 0; i < bodyPart.injuries.Count; i++)
                {
                    if (bodyPart.injuries[i].onBackOfBodyPart == attackedFromBehind)
                        applicableInjuries.Add(bodyPart.injuries[i]);
                }

                random = Random.Range(0, applicableInjuries.Count);
                applicableInjuries[random].Reinjure();
                gm.flavorText.WriteReinjureLine(characterManager, applicableInjuries[random].injury, bodyPart.bodyPartType);
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
            Injury laceration = gm.traumaSystem.GetLaceration(characterManager, bodyPart.bodyPartType, slashDamage);
            TraumaSystem.ApplyInjury(characterManager, laceration, bodyPart.bodyPartType, attackedFromBehind);
            gm.flavorText.WriteInjuryLine(characterManager, laceration, bodyPart.bodyPartType);
        }
        else if (cleaveDamage > 0)
        {

        }
    }

    public virtual void Die()
    {
        isDead = true;

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

            characterManager.spriteRenderer.sortingLayerName = "Items";
            characterManager.spriteRenderer.sortingOrder = -1;

            gm.turnManager.npcs.Remove(characterManager);

            if (gm.turnManager.npcsFinishedTakingTurnCount >= gm.turnManager.npcs.Count)
                gm.turnManager.ReadyPlayersTurn();

            characterManager.stateController.enabled = false;
            characterManager.npcMovement.AIDestSetter.enabled = false;
            characterManager.npcMovement.AIPath.enabled = false;

            if (characterManager.IsNextToPlayer())
                gm.containerInvUI.GetItemsAroundPlayer();
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

        characterManager.movement.enabled = false;
        characterManager.ResetActionsQueue();

        characterManager.spriteManager.SetToDeathSprite(characterManager.spriteRenderer);

        gm.flavorText.StartCoroutine(gm.flavorText.DelayWriteLine(Utilities.GetPronoun(characterManager, true, false) + "died."));
    }

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

    public IEnumerator Consume(ItemData consumableItemData)
    {
        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetConsumeAPCost((Consumable)consumableItemData.item)));

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

    public virtual void OnWearableChanged(ItemData newItemData, ItemData oldItemData)
    {
        if (newItemData != null)
        {
            if (newItemData.item.IsShield() == false && newItemData.item.IsBag() == false)
            {
                Wearable wearable = (Wearable)newItemData.item;
                if (wearable.isClothing)
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Clothing.AddModifier(newItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Clothing.AddModifier(newItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Clothing.AddModifier(newItemData.tertiaryDefense);
                    }
                }
                else
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Armor.AddModifier(newItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Armor.AddModifier(newItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Armor.AddModifier(newItemData.tertiaryDefense);
                    }
                }
            }
        }

        if (oldItemData != null)
        {
            if (oldItemData.item.IsShield() == false && oldItemData.item.IsBag() == false)
            {
                Wearable wearable = (Wearable)oldItemData.item;
                if (wearable.isClothing)
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(oldItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(oldItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(oldItemData.tertiaryDefense);
                    }
                }
                else
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(oldItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(oldItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(oldItemData.tertiaryDefense);
                    }
                }
            }
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

    [Header("Health")]
    public Stat maxHealth;
    public int currentHealth;
    public List<LocationalInjury> injuries = new List<LocationalInjury>();

    [Header("Buildups")]
    public float damageBuildup;
    public float healingBuildup;

    [Header("Defense")]
    public Stat naturalDefense;
    public Stat addedDefense_Armor, addedDefense_Clothing;

    public BodyPart(BodyPartType bodyPartType, int baseMaxHealth)
    {
        this.bodyPartType = bodyPartType;
        maxHealth.SetBaseValue(baseMaxHealth);
        currentHealth = baseMaxHealth;
    }

    public int Damage(int damageAmount)
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

[System.Serializable]
public class LocationalInjury
{
    public Injury injury;
    public BodyPartType injuryLocation;
    public bool onBackOfBodyPart;

    public float injuryHealMultiplier = 1f;
    public bool sterilized;
    public MedicalSupply bandage;
    public float bandageSoil;

    public int injuryTimeRemaining;

    public float damagePerTurn;
    public int bleedTimeRemaining;
    public float bloodLossPerTurn;

    public LocationalInjury(Injury injury, BodyPartType injuryLocation, bool onBackOfBodyPart)
    {
        this.injury = injury;
        this.injuryLocation = injuryLocation;
        this.onBackOfBodyPart = onBackOfBodyPart;
        SetupInjuryVariables();
    }

    void SetupInjuryVariables()
    {
        damagePerTurn = Mathf.RoundToInt(Random.Range(injury.damagePerTurn.x, injury.damagePerTurn.y) * 100f) / 100f;

        injuryTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(injury.minInjuryHealTime), TimeSystem.GetTotalSeconds(injury.maxInjuryHealTime) + 1);

        Vector2Int bleedTimes = injury.GetBleedTime();
        if (bleedTimes.y > 0)
            bleedTimeRemaining = Random.Range(bleedTimes.x, bleedTimes.y + 1);

        Vector2 bloodLossValues = injury.GetBloodLossPerTurn();
        if (bloodLossValues.y > 0)
            bloodLossPerTurn = Random.Range(bloodLossValues.x, bloodLossValues.y);
    }

    public void ApplyBandage(ItemData bandageItemData)
    {
        this.bandage = (MedicalSupply)bandageItemData.item;
        bandageSoil = 100f - bandageItemData.freshness;
        injuryHealMultiplier += bandage.quality;

        // TODO: Create new ItemData object for the bandage
    }

    public void RemoveBandage(LocationalInjury injury)
    {
        // TODO: Create new ItemData object for the bandage and place in inventory or drop
    }

    public void SoilBandage(float amount)
    {
        if (bandage != null && bandageSoil < 100f)
        {
            bandageSoil += amount;
            if (bandageSoil >= 100f)
            {
                bandageSoil = 100f;
                injuryHealMultiplier -= bandage.quality;
            }
        }
    }

    public void Reinjure()
    {
        // If this is an injury that bleeds, re-open it or add onto the bleed time
        if (bloodLossPerTurn > 0)
        {
            Vector2Int bleedTimes = injury.GetBleedTime();
            bleedTimeRemaining += Random.Range(Mathf.RoundToInt(bleedTimes.x / 1.2f), Mathf.RoundToInt(bleedTimes.y / 1.2f));
            if (bleedTimeRemaining > bleedTimes.y)
                bleedTimeRemaining = bleedTimes.y;
        }

        // Also add to the total injury time remaining
        Vector2Int injuryTimes = new Vector2Int(TimeSystem.GetTotalSeconds(injury.minInjuryHealTime), TimeSystem.GetTotalSeconds(injury.maxInjuryHealTime) + 1);
        injuryTimeRemaining += Random.Range(injuryTimes.x / 2, injuryTimes.y / 2);
        if (injuryTimeRemaining > injuryTimes.y)
            injuryTimeRemaining = injuryTimes.y;
    }
}

[System.Serializable]
public class Buff
{
    public Consumable consumable;
    public int buffTimeRemaining;

    public float healPercentPerTurn;
    public int healTimeRemaining;

    public Buff(Consumable consumable)
    {
        this.consumable = consumable;
        SetupBuffVariables(consumable);
    }

    void SetupBuffVariables(Consumable consumable)
    {
        healTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(consumable.minGradualHealTime), TimeSystem.GetTotalSeconds(consumable.maxGradualHealTime) + 1);
        buffTimeRemaining = healTimeRemaining;
        healPercentPerTurn = consumable.gradualHealPercent / buffTimeRemaining;
    }
}

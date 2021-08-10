using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocomotionType { Inanimate, Humanoid, Biped, Quadruped, Hexapod, Octoped }

public class Status : MonoBehaviour
{
    public CharacterManager characterManager;
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

    readonly float naturalHealingPercentPerTurn = 0.0001f; // 60 minutes (100 turns) to heal entire body 1% if healthiness == 1
    readonly float naturalBloodProductionPerTurn = 0.0164f; // 1 pint takes 48 hours IRL and 1 pint = 473 mL | (473mL / 48hr / 60min / 60sec) * 6sec/turn = 0.0164mL/turn

    // Healthiness signifies your overall bodily health, effecting your natural healing over time.
    // Eating healthy, among other things, slowly builds this up over time, but this can be easily reversed by ailments such as an infection or a concussion.
    float healthiness = 100f;
    readonly float minHealthiness = -200f;
    readonly float maxHealthiness = 200f;

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

        for (int i = 0; i < bodyParts.Count; i++)
        {
            if (bodyParts[i].injuries.Count > 0)
            {
                int injuryCount = bodyParts[i].injuries.Count;
                for (int j = injuryCount - 1; j >= 0; j--)
                {
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
                if (bodyParts[i].injuries[k].bleedTimeRemaining > 0) // If the injury is still bleeding
                {
                    bodyParts[i].damageBuildup += bodyParts[i].injuries[k].damagePerTurn;

                    LoseBlood(bodyParts[i].injuries[k].bloodLossPerTurn);

                    bodyParts[i].injuries[k].bleedTimeRemaining -= Mathf.RoundToInt(timePassed * bodyParts[i].injuries[k].injuryHealMultiplier);
                    if (bodyParts[i].injuries[k].bleedTimeRemaining < 0)
                        bodyParts[i].injuries[k].bleedTimeRemaining = 0;
                }
            }
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
                TakeLocationalDamage_IgnoreArmor(Mathf.RoundToInt(bodyParts[i].damageBuildup), bodyParts[i].bodyPartType);
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

    public int TakeLocationalDamage(int bluntDamage, int pierceDamage, int slashDamage, int cleaveDamage, BodyPartType bodyPartType, EquipmentManager equipmentManager, Wearable armor, Wearable clothing, bool armorPenetrated, bool clothingPenetrated)
    {
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
            ApplyInjuries(characterManager, bodyPart, bluntDamage, pierceDamage, slashDamage, cleaveDamage);
        }

        if (finalTotalDamage > 0)
        {
            TextPopup.CreateDamagePopup(transform.position, finalTotalDamage, false);
            bodyPart.Damage(finalTotalDamage);
            if ((bodyPart.bodyPartType == BodyPartType.Torso || bodyPart.bodyPartType == BodyPartType.Head) && bodyPart.currentHealth <= 0)
                Die();
        }
        else
            TextPopup.CreateTextStringPopup(transform.position, "Absorbed");

        return finalTotalDamage;
    }

    void ApplyInjuries(CharacterManager characterManager, BodyPart bodyPart, int bluntDamage, int pierceDamage, int slashDamage, int cleaveDamage)
    {
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
            TraumaSystem.ApplyInjury(characterManager, laceration, bodyPart.bodyPartType);
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
    public int injuryTimeRemaining;

    public float damagePerTurn;
    public int bleedTimeRemaining;
    public float bloodLossPerTurn;
    public float injuryHealMultiplier = 1f;

    public LocationalInjury(Injury injury, BodyPartType injuryLocation)
    {
        this.injury = injury;
        this.injuryLocation = injuryLocation;
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

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
    public int currentBloodAmount;
    public Stat maxFullness;
    public float currentFullness;
    public Stat maxWater;
    public float currentWater;

    [HideInInspector] public bool isDead;

    readonly float naturalHealingPercentPerTurn = 0.0001f; // 60 minutes (100 turns) to heal entire body 1% if healthiness == 1

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

    void LoseBlood(int amount)
    {
        currentBloodAmount -= amount;
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
        int totalDamage = bluntDamage + pierceDamage + slashDamage + cleaveDamage;

        BodyPart bodyPart = GetBodyPart(bodyPartType);
        if (armorPenetrated == false && equipmentManager != null)
            totalDamage -= bodyPart.addedDefense_Armor.GetValue();

        if (clothingPenetrated == false && equipmentManager != null)
            totalDamage -= bodyPart.addedDefense_Clothing.GetValue();

        totalDamage -= bodyPart.naturalDefense.GetValue();
        
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

        bodyPart.Damage(totalDamage);

        if ((bodyPart.bodyPartType == BodyPartType.Torso || bodyPart.bodyPartType == BodyPartType.Head) && bodyPart.currentHealth <= 0)
            Die();

        return totalDamage;
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

    void Heal_Passive(int timePassed)
    {
        // Apply natural healing first, if the character is healthy enough
        if (healthiness > 0)
        {
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
            if (newItemData.item.IsShield() == false)
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
            if (oldItemData.item.IsShield() == false)
            {
                Wearable wearable = (Wearable)oldItemData.item;
                if (wearable.isClothing)
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(newItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(newItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(newItemData.tertiaryDefense);
                    }
                }
                else
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(newItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(newItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(newItemData.tertiaryDefense);
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
    public int bloodLossPerTurn;
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

        Vector2Int bloodLossValues = injury.GetBloodLossPerTurn();
        if (bloodLossValues.y > 0)
            bloodLossPerTurn = Random.Range(bloodLossValues.x, bloodLossValues.y + 1);
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

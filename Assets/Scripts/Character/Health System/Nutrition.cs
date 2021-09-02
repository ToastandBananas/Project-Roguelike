using System.Collections;
using UnityEngine;

public enum HungerLevel { Engorged, UncomfortablyFull, Stuffed, Sated, Full, Satisfied, Peckish, Hungry, VeryHungry, Famished, Starving, Ravenous }
public enum ThirstLevel { WaterIntoxicated, Overhydrated, Slaked, Sated, Quenched, Satisfied, Cottonmouthed, Thirsty, Parched, BoneDry, Dehydrated, DyingOfThirst }

public class Nutrition : MonoBehaviour
{
    public IntStat maxNourishment;
    public float currentNourishment;
    public IntStat maxWater;
    public float currentWater;

    public HungerLevel currentHungerLevel { get; private set; }
    public ThirstLevel currentThirstLevel { get; private set; }

    StatusEffect hungerStatusEffect;
    StatusEffect thirstStatusEffect;

    int starvingStaminaDetriment, dehydratedStaminaDetriment;
    int starvingSpeedDetriment, dehydratedSpeedDetriment;
    [SerializeField] float consumableStaminaBonus;

    // If this builds up to 100, the character pukes
    float nausea;
    bool vomitActionQueued;

    CharacterManager characterManager;
    GameManager gm;

    void Awake()
    {
        characterManager = GetComponent<CharacterManager>();

        currentNourishment = maxNourishment.GetValue() * Random.Range(0.5f, 0.8f);
        currentWater = maxWater.GetValue() * Random.Range(0.5f, 0.8f);
    }

    void Start()
    {
        gm = GameManager.instance;

        IncreaseStaminaBonus(50f * (currentNourishment / maxNourishment.GetValue()));
        characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();
        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();

        AddHungerEffects();
        AddThirstEffects();
    }

    #region Hunger
    public float GetHunger()
    {
        float hunger = 100f - Mathf.RoundToInt((currentNourishment / maxNourishment.GetValue()) * 10000f) / 100f;
        if (hunger > 100f)
            hunger = 100f;
        return hunger;
    }

    public float GetFullness()
    {
        float fullness = Mathf.RoundToInt((currentNourishment / maxNourishment.GetValue()) * 10000f) / 100f;
        if (fullness < 0f)
            fullness = 0f;
        return fullness;
    }

    public void IncreaseNourishment(float amount)
    {
        // Start back up at 0 if the character was starving before and remove the stamina detriment
        if (currentNourishment < 0)
        {
            currentNourishment = 0;
            characterManager.status.maxStamina.RemoveModifier(starvingStaminaDetriment);
            starvingStaminaDetriment = 0;
        }

        currentNourishment += amount;

        // Check if the character's hunger is low or high enough to trigger any effects
        if (currentHungerLevel != GetHungerLevel())
            AddHungerEffects();
    }

    public void DecreaseNourishment(float amount)
    {
        currentNourishment -= amount;

        // Check if the character's hunger is low or high enough to trigger any effects
        if (currentHungerLevel != GetHungerLevel())
            AddHungerEffects();
    }

    public void DrainNourishment(int timePassed = 6)
    {
        // Drain 100% of fullness in 6 hours game time (3600 turns) | 1 / 3600 = 0.000278
        currentNourishment -= maxNourishment.GetValue() * 0.000278f * (timePassed / TimeSystem.defaultTimeTickInSeconds);

        // Check if the character's hunger is high enough to trigger any negative effects
        UpdateHungerEffects();
    }

    void UpdateHungerEffects()
    {
        // Lower max stamina, stamina regen and speed if nourishment is low enough
        if (currentHungerLevel != GetHungerLevel())
            AddHungerEffects();

        if (hungerStatusEffect != null)
        {
            if (hungerStatusEffect.healthinessAdjustmentPerTurn != 0)
                characterManager.status.AdjustHealthiness(hungerStatusEffect.healthinessAdjustmentPerTurn);

            if (hungerStatusEffect.nauseaPerTurn != 0)
                AdjustNausea(hungerStatusEffect.nauseaPerTurn);
        }
    }

    void AddHungerEffects()
    {
        // Remove the previous max stamina/speed negative modifiers, if there were any
        if (starvingStaminaDetriment != 0)
            characterManager.status.maxStamina.RemoveModifier(starvingStaminaDetriment);

        if (starvingSpeedDetriment != 0)
            characterManager.characterStats.speed.RemoveModifier(starvingSpeedDetriment);

        // Remove the previous stamina regen modifier, if there was one
        if (hungerStatusEffect != null)
            characterManager.status.staminaRegenPercent.RemoveModifier(hungerStatusEffect.staminaRegenModifier);

        // Update our current thirst level
        SetCurrentHungerLevel();

        // Grab the new thirst status effect
        hungerStatusEffect = gm.healthSystem.GetHungerStatusEffect(characterManager);

        // Set these variables so we can reference them when removing the modifiers, since they're based off of values that can change as the player progresses
        starvingStaminaDetriment = Mathf.RoundToInt(characterManager.status.maxStamina.GetBaseValue() * hungerStatusEffect.maxStaminaMultiplier);
        starvingSpeedDetriment = Mathf.RoundToInt(characterManager.characterStats.speed.GetBaseValue() * hungerStatusEffect.speedMultiplier);

        // Add the new max stamina negative modifier and clamp current stamina
        characterManager.status.maxStamina.AddModifier(starvingStaminaDetriment);
        if (characterManager.status.currentStamina > characterManager.status.maxStamina.GetValue())
            characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();

        // Add the new negative speed modifier
        characterManager.characterStats.speed.AddModifier(starvingSpeedDetriment);

        // Add the new stamina regen modifier
        characterManager.status.staminaRegenPercent.AddModifier(hungerStatusEffect.staminaRegenModifier);

        // Update stamina in the UI
        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();
    }

    void SetCurrentHungerLevel()
    {
        currentHungerLevel = GetHungerLevel();
        gm.healthDisplay.UpdateHungerText();
    }

    HungerLevel GetHungerLevel()
    {
        if (NourishmentPercent() >= 120f)
            return HungerLevel.Engorged;
        else if (NourishmentPercent() >= 110f)
            return HungerLevel.UncomfortablyFull;
        else if (NourishmentPercent() >= 100f)
            return HungerLevel.Stuffed;
        else if (NourishmentPercent() >= 85f)
            return HungerLevel.Sated;
        else if (NourishmentPercent() >= 70f)
            return HungerLevel.Full;
        else if (NourishmentPercent() >= 45f)
            return HungerLevel.Satisfied;
        else if (NourishmentPercent() >= 30f)
            return HungerLevel.Peckish;
        else if (NourishmentPercent() >= 15f)
            return HungerLevel.Hungry;
        else if (NourishmentPercent() >= 0f)
            return HungerLevel.VeryHungry;
        else if (NourishmentPercent() >= -10f)
            return HungerLevel.Famished;
        else if (NourishmentPercent() >= -20f)
            return HungerLevel.Starving;
        else
            return HungerLevel.Ravenous;
    }

    float NourishmentPercent()
    {
        return (currentNourishment / maxNourishment.GetValue()) * 100f;
    }

    public bool IsStarving()
    {
        if (currentNourishment <= 0)
            return true;
        return false;
    }
    #endregion

    #region Thirst
    public float GetThirst()
    {
        float thirst = 100f - Mathf.RoundToInt((currentWater / maxWater.GetValue()) * 10000f) / 100f;
        if (thirst > 100)
            thirst = 100;
        return thirst;
    }

    public float GetThirstQuench()
    {
        float thirstQuench = Mathf.RoundToInt((currentWater / maxWater.GetValue()) * 10000f) / 100f;
        if (thirstQuench > 100f)
            thirstQuench = 100f;
        return thirstQuench;
    }

    public void IncreaseWater(float amount)
    {
        // Start back up at 0 if the character was completely dehydrated before and remove the stamina detriment
        if (currentWater < 0)
        {
            currentWater = 0;
            characterManager.status.maxStamina.RemoveModifier(dehydratedStaminaDetriment);
            dehydratedStaminaDetriment = 0;
        }

        currentWater += amount;

        // Check if the character's thirst is low or high enough to trigger any effects
        if (currentThirstLevel != GetThirstLevel())
            AddThirstEffects();
    }

    public void DecreaseWater(float amount)
    {
        currentWater -= amount;

        // Check if the character's thirst is low or high enough to trigger any effects
        if (currentThirstLevel != GetThirstLevel())
            AddThirstEffects();
    }

    public void DrainWater(int timePassed = 6)
    {
        // Drain 100% thirst every 4 hours game time (2400 turns) | 1 / 2400 = 0.000417
        currentWater -= maxWater.GetValue() * 0.000417f * (timePassed / TimeSystem.defaultTimeTickInSeconds);

        // Check if the character's thirst is high enough to trigger any negative effects
        UpdateThirstEffects();
    }

    void UpdateThirstEffects()
    {
        if (currentThirstLevel != GetThirstLevel())
            AddThirstEffects();

        if (thirstStatusEffect != null)
        {
            if (thirstStatusEffect.healthinessAdjustmentPerTurn != 0)
                characterManager.status.AdjustHealthiness(thirstStatusEffect.healthinessAdjustmentPerTurn);

            if (thirstStatusEffect.nauseaPerTurn != 0)
                AdjustNausea(thirstStatusEffect.nauseaPerTurn);
        }
    }

    void AddThirstEffects()
    {
        // Remove the previous max stamina/speed negative modifiers, if there were any
        if (dehydratedStaminaDetriment != 0)
            characterManager.status.maxStamina.RemoveModifier(dehydratedStaminaDetriment);

        if (dehydratedSpeedDetriment != 0)
            characterManager.characterStats.speed.RemoveModifier(dehydratedSpeedDetriment);

        // Remove the previous stamina regen modifier, if there was one
        if (thirstStatusEffect != null)
            characterManager.status.staminaRegenPercent.RemoveModifier(thirstStatusEffect.staminaRegenModifier);

        // Update our current thirst level
        SetCurrentThirstLevel();

        // Grab the new thirst status effect
        thirstStatusEffect = gm.healthSystem.GetThirstStatusEffect(characterManager);

        // Set these variables so we can reference them when removing the modifiers, since they're based off of values that can change as the player progresses
        dehydratedStaminaDetriment = Mathf.RoundToInt(characterManager.status.maxStamina.GetBaseValue() * thirstStatusEffect.maxStaminaMultiplier);
        dehydratedSpeedDetriment = Mathf.RoundToInt(characterManager.characterStats.speed.GetBaseValue() * thirstStatusEffect.speedMultiplier);

        // Add the new max stamina negative modifier and clamp current stamina
        characterManager.status.maxStamina.AddModifier(dehydratedStaminaDetriment);
        if (characterManager.status.currentStamina > characterManager.status.maxStamina.GetValue())
            characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();

        // Add the new negative speed modifier
        characterManager.characterStats.speed.AddModifier(dehydratedSpeedDetriment);

        // Add the new stamina regen modifier
        characterManager.status.staminaRegenPercent.AddModifier(thirstStatusEffect.staminaRegenModifier);

        // Update stamina in the UI
        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();
    }

    void SetCurrentThirstLevel()
    {
        currentThirstLevel = GetThirstLevel();
        gm.healthDisplay.UpdateThirstText();
    }

    ThirstLevel GetThirstLevel()
    {
        if (ThirstQuenchPercent() >= 120f)
            return ThirstLevel.WaterIntoxicated;
        else if (ThirstQuenchPercent() >= 110f)
            return ThirstLevel.Overhydrated;
        else if (ThirstQuenchPercent() >= 100f)
            return ThirstLevel.Slaked;
        else if (ThirstQuenchPercent() >= 85f)
            return ThirstLevel.Sated;
        else if (ThirstQuenchPercent() >= 70f)
            return ThirstLevel.Quenched;
        else if (ThirstQuenchPercent() >= 45f)
            return ThirstLevel.Satisfied;
        else if (ThirstQuenchPercent() >= 30f)
            return ThirstLevel.Cottonmouthed;
        else if (ThirstQuenchPercent() >= 15f)
            return ThirstLevel.Thirsty;
        else if (ThirstQuenchPercent() >= 0f)
            return ThirstLevel.Parched;
        else if (ThirstQuenchPercent() >= -10f)
            return ThirstLevel.BoneDry;
        else if (ThirstQuenchPercent() >= -20f)
            return ThirstLevel.Dehydrated;
        else
            return ThirstLevel.DyingOfThirst;
    }

    float ThirstQuenchPercent()
    {
        return (currentWater / maxWater.GetValue()) * 100f;
    }

    public bool IsFullyDehydrated()
    {
        if (currentWater <= 0)
            return true;
        return false;
    }
    #endregion

    #region Nausea
    public void AdjustNausea(float amount)
    {
        nausea += amount;
        if (nausea < 0)
            nausea = 0;
        else if (nausea >= 100 && vomitActionQueued == false)
        {
            vomitActionQueued = true;
            characterManager.QueueAction(Vomit(), gm.apManager.GetPukeCost(characterManager));
        }
    }

    public void DrainNausea()
    {
        if (nausea > 0 && (thirstStatusEffect == null || thirstStatusEffect.nauseaPerTurn <= 0) && (hungerStatusEffect == null || hungerStatusEffect.nauseaPerTurn <= 0))
            AdjustNausea(-0.05f);
    }
    #endregion

    #region Consumable Stamina Bonus
    void IncreaseStaminaBonus(float amount)
    {
        // Remove the old max stamina modifier, calculate the new one & then re-add it
        characterManager.status.maxStamina.RemoveModifier(Mathf.RoundToInt(consumableStaminaBonus));
        consumableStaminaBonus += amount;
        characterManager.status.maxStamina.AddModifier(Mathf.RoundToInt(consumableStaminaBonus));

        if (characterManager.status.currentStamina > characterManager.status.maxStamina.GetValue())
            characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();

        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();
    }

    void DecreaseStaminaBonus(float amount)
    {
        // Remove the old max stamina modifier, calculate the new one & then re-add it
        characterManager.status.maxStamina.RemoveModifier(Mathf.RoundToInt(consumableStaminaBonus));
        consumableStaminaBonus -= amount;

        if (consumableStaminaBonus <= 0f)
            consumableStaminaBonus = 0f;
        else
            characterManager.status.maxStamina.AddModifier(Mathf.RoundToInt(consumableStaminaBonus));

        if (characterManager.status.currentStamina > characterManager.status.maxStamina.GetValue())
            characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();

        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();
    }

    public void DrainStaminaBonus(int timePassed = TimeSystem.defaultTimeTickInSeconds)
    {
        if (consumableStaminaBonus > 0)
        {
            characterManager.status.maxStamina.RemoveModifier(Mathf.RoundToInt(consumableStaminaBonus));
            consumableStaminaBonus -= maxNourishment.GetValue() * 0.000278f * (timePassed / TimeSystem.defaultTimeTickInSeconds);

            if (consumableStaminaBonus <= 0f)
                consumableStaminaBonus = 0f;
            else
                characterManager.status.maxStamina.AddModifier(Mathf.RoundToInt(consumableStaminaBonus));

            if (characterManager.status.currentStamina > characterManager.status.maxStamina.GetValue())
                characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();

            if (characterManager.isNPC == false)
                gm.healthDisplay.UpdateCurrentStaminaText();
        }
    }
    #endregion

    #region Actions
    public IEnumerator Vomit()
    {
        if (characterManager.status.isDead) yield break;

        if (currentNourishment < 0f && currentWater < 0f) // Dry heave if there's nothing in your stomach
        {
            // Show some flavor text
            if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                gm.flavorText.WriteLine_DryHeave(characterManager);
        }
        else
        {
            // Lose a random percentage of your food/stamina bonus and/or water content if you're over 25% full/quenched
            // Otherwise, lose the rest of your food/stamina bonus and/or water content
            if (GetFullness() >= 25f)
            {
                DecreaseNourishment(currentNourishment * Random.Range(0.5f, 0.75f));
                if (consumableStaminaBonus > 0)
                    DecreaseStaminaBonus(consumableStaminaBonus * Random.Range(0.5f, 0.75f));
            }
            else if (currentNourishment > 0f)
            {
                DecreaseNourishment(currentNourishment);
                if (consumableStaminaBonus > 0)
                    DecreaseStaminaBonus(consumableStaminaBonus);
            }

            if (GetThirstQuench() >= 25f)
                DecreaseWater(currentWater * Random.Range(0.5f, 0.75f));
            else if (currentWater > 0f)
                DecreaseWater(currentWater);

            // Show some flavor text
            if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                gm.flavorText.WriteLine_Vomit(characterManager);
        }

        // Update thirst effects and current thirst level
        if (currentThirstLevel != GetThirstLevel())
            AddThirstEffects();

        // Lower nausea levels
        AdjustNausea(Random.Range(-35f, -15f));
        vomitActionQueued = false;

        characterManager.FinishAction();
    }

    public IEnumerator Consume(ItemData consumableItemData, Consumable consumable, int itemCount, float percentUsed, string itemName)
    {
        if (characterManager.status.isDead) yield break;

        // Adjust overall bodily healthiness
        if (consumable.healthinessAdjustment != 0)
            characterManager.status.AdjustHealthiness(consumable.healthinessAdjustment * itemCount * percentUsed);

        // Adjust max stamina bonus
        if (consumable.maxStaminaBonus > 0)
            IncreaseStaminaBonus(consumable.maxStaminaBonus * itemCount * percentUsed);

        // Adjust nourishment
        if (consumable.nourishment > 0)
            IncreaseNourishment(consumable.nourishment);

        // Adjust thirst quench
        if (consumable.thirstQuench > 0)
            IncreaseWater(consumable.thirstQuench);

        // Instantly heal entire body
        if (consumable.instantHealPercent > 0)
            characterManager.status.HealAllBodyParts_Percent(consumable.instantHealPercent * itemCount * percentUsed);

        // Apply heal over time buff
        if (consumable.gradualHealPercent > 0)
            HealthSystem.ApplyBuff(characterManager, consumable, itemCount, percentUsed);

        // Show some flavor text
        if (gm.playerManager.CanSee(characterManager.spriteRenderer))
        {
            if (consumableItemData.percentRemaining - (percentUsed * 100) > 0)
                gm.flavorText.WriteLine_Consume(characterManager, consumable, itemName, percentUsed);
            else
                gm.flavorText.WriteLine_Consume(characterManager, consumable, itemName, 1);

            if (NourishmentPercent() > 100f && characterManager.isNPC == false)
                gm.flavorText.WriteLine_NauseaOvereating();

            if (ThirstQuenchPercent() > 100f && characterManager.isNPC == false)
                gm.flavorText.WriteLine_NauseaOverdrinking();
        }

        characterManager.FinishAction();
    }
    #endregion
}

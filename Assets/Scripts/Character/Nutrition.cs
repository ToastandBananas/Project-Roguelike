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

    int starvingStaminaDetriment, dehydratedStaminaDetriment;
    int starvingSpeedDetriment, dehydratedSpeedDetriment;

    CharacterManager characterManager;
    GameManager gm;

    void Awake()
    {
        characterManager = GetComponent<CharacterManager>();

        currentNourishment = maxNourishment.GetValue() * 0.75f;
        currentWater = maxWater.GetValue() * 0.75f;
    }

    void Start()
    {
        gm = GameManager.instance;

        SetCurrentHungerLevel();
        SetCurrentThirstLevel();
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
        // TODO: Eat over your max nourishment and start to feel more and more nauseous & sluggish the more you over-eat, among other detriments. 
        //       Keep eating too much and you'll throw up, which will immediately subtract most of your nourishment and thirst levels.

        // Check if the character's hunger is low or high enough to trigger any detriments or bonuses
        UpdateHungerDetriments();
    }

    public void DrainNourishment(int timePassed = 6)
    {
        // Drain 100% of fullness in 6 hours game time (3600 turns) | 1 / 3600 = 0.000278
        currentNourishment -= maxNourishment.GetValue() * 0.000278f * (timePassed / TimeSystem.defaultTimeTickInSeconds);

        // Check if the character's hunger is low enough to trigger any negative effects
        UpdateHungerDetriments();
    }

    void UpdateHungerDetriments()
    {
        // Lower max stamina, stamina regen and speed if nourishment is low enough
        if (NourishmentPercent() >= 30f && NourishmentPercent() < 45f && currentHungerLevel != HungerLevel.Peckish)
            AddHungerStaminaDetriment(0f, 0f);
        else if (NourishmentPercent() >= 15f && NourishmentPercent() < 30f && currentHungerLevel != HungerLevel.Hungry)
            AddHungerStaminaDetriment(-0.15f, -0.05f);
        else if (NourishmentPercent() >= 0f && NourishmentPercent() < 15f && currentHungerLevel != HungerLevel.VeryHungry)
            AddHungerStaminaDetriment(-0.2f, -0.1f);
        else if (NourishmentPercent() >= -10f && NourishmentPercent() < 0f && currentHungerLevel != HungerLevel.Famished)
            AddHungerStaminaDetriment(-0.25f, -0.15f);
        else if (NourishmentPercent() >= -20f && NourishmentPercent() < -10f && currentHungerLevel != HungerLevel.Starving)
            AddHungerStaminaDetriment(-0.30f, -0.2f);
        else if (NourishmentPercent() < -20f && currentHungerLevel != HungerLevel.Ravenous)
            AddHungerStaminaDetriment(-0.35f, -0.25f);
    }

    void AddHungerStaminaDetriment(float maxStaminaDrainMultiplier, float speedDrainMultiplier)
    {
        // Remove the previous max stamina/speed negative modifiers, if there were any
        if (starvingStaminaDetriment != 0)
            characterManager.status.maxStamina.RemoveModifier(starvingStaminaDetriment);
        
        if (starvingSpeedDetriment != 0)
            characterManager.characterStats.speed.RemoveModifier(starvingSpeedDetriment);

        // Set these variables so we can reference them when removing the modifiers, since they're based off of values that can change as the player progresses
        starvingStaminaDetriment = Mathf.RoundToInt(characterManager.status.maxStamina.GetBaseValue() * maxStaminaDrainMultiplier);
        starvingSpeedDetriment = Mathf.RoundToInt(characterManager.characterStats.speed.GetBaseValue() * speedDrainMultiplier);
            
        // Add the new max stamina negative modifier and clamp current stamina
        characterManager.status.maxStamina.AddModifier(starvingStaminaDetriment);
        if (characterManager.status.currentStamina > characterManager.status.maxStamina.GetValue())
            characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();

        // Add the new negative speed modifier
        characterManager.characterStats.speed.AddModifier(starvingSpeedDetriment);

        // Update stamina in the UI
        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();

        // Remove the previous stamina regen modifier, if there was one
        characterManager.status.staminaRegenPercent.RemoveModifier(GetStaminaRegenModifier());

        // Update our current hunger level
        SetCurrentHungerLevel();

        // Add the new stamina regen modifier
        characterManager.status.staminaRegenPercent.AddModifier(GetStaminaRegenModifier());
    }

    float GetStaminaRegenModifier()
    {
        switch (currentHungerLevel)
        {
            case HungerLevel.Engorged:
                return 0f;
            case HungerLevel.UncomfortablyFull:
                return 0.0015f;
            case HungerLevel.Stuffed:
                return 0.0025f;
            case HungerLevel.Sated:
                return 0.0035f;
            case HungerLevel.Full:
                return 0.0025f;
            case HungerLevel.Satisfied:
                return 0.0015f;
            case HungerLevel.Peckish:
                return 0f;
            case HungerLevel.Hungry:
                return -0.0015f;
            case HungerLevel.VeryHungry:
                return -0.0025f;
            case HungerLevel.Famished:
                return -0.005f;
            case HungerLevel.Starving:
                return -0.0075f;
            case HungerLevel.Ravenous:
                return -0.01f;
            default:
                return 0f;
        }
    }

    void SetCurrentHungerLevel()
    {
        if (NourishmentPercent() >= 120f) 
            currentHungerLevel = HungerLevel.Engorged;
        else if (NourishmentPercent() >= 110f)
            currentHungerLevel = HungerLevel.UncomfortablyFull;
        else if (NourishmentPercent() >= 100f)
            currentHungerLevel = HungerLevel.Stuffed;
        else if (NourishmentPercent() >= 85f)
            currentHungerLevel = HungerLevel.Sated;
        else if (NourishmentPercent() >= 70f)
            currentHungerLevel = HungerLevel.Full;
        else if (NourishmentPercent() >= 45f)
            currentHungerLevel = HungerLevel.Satisfied;
        else if (NourishmentPercent() >= 30f)
            currentHungerLevel = HungerLevel.Peckish;
        else if (NourishmentPercent() >= 15f)
            currentHungerLevel = HungerLevel.Hungry;
        else if (NourishmentPercent() >= 0f)
            currentHungerLevel = HungerLevel.VeryHungry;
        else if (NourishmentPercent() >= -10f)
            currentHungerLevel = HungerLevel.Famished;
        else if (NourishmentPercent() >= -20f)
            currentHungerLevel = HungerLevel.Starving;
        else
            currentHungerLevel = HungerLevel.Ravenous;

        gm.healthDisplay.UpdateHungerText();
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
        // TODO: Similar to overeating, if you too much water, you will feel more and more nauseous & sluggish the more you over-drink.
        //       Keep drinking too much and you'll throw up, which will immediately subtract most of your nourishment and thirst levels.

        
    }

    public void DrainThirst(int timePassed = 6)
    {
        // Drain 100% thirst every 4 hours game time (2400 turns) | 1 / 2400 = 0.000417
        currentWater -= maxWater.GetValue() * 0.000417f * (timePassed / TimeSystem.defaultTimeTickInSeconds);

        if (currentWater < 0)
        {
            // Lower max stamina until the character drinks again
            dehydratedStaminaDetriment = Mathf.RoundToInt(characterManager.status.maxStamina.GetBaseValue() * -0.35f);
            characterManager.status.maxStamina.AddModifier(dehydratedStaminaDetriment);
            if (characterManager.status.currentStamina > characterManager.status.maxStamina.GetValue())
                characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();
        }
    }

    void UpdateThirstDetriments()
    {

    }

    void AddThirstStaminaDetriment()
    {

    }

    void SetCurrentThirstLevel()
    {
        if (ThirstQuenchPercent() >= 120f)
            currentThirstLevel = ThirstLevel.WaterIntoxicated;
        else if (ThirstQuenchPercent() >= 110f)
            currentThirstLevel = ThirstLevel.Overhydrated;
        else if (ThirstQuenchPercent() >= 100f)
            currentThirstLevel = ThirstLevel.Slaked;
        else if (ThirstQuenchPercent() >= 85f)
            currentThirstLevel = ThirstLevel.Sated;
        else if (ThirstQuenchPercent() >= 70f)
            currentThirstLevel = ThirstLevel.Quenched;
        else if (ThirstQuenchPercent() >= 45f)
            currentThirstLevel = ThirstLevel.Satisfied;
        else if (ThirstQuenchPercent() >= 30f)
            currentThirstLevel = ThirstLevel.Cottonmouthed;
        else if (ThirstQuenchPercent() >= 15f)
            currentThirstLevel = ThirstLevel.Thirsty;
        else if (ThirstQuenchPercent() >= 0f)
            currentThirstLevel = ThirstLevel.Parched;
        else if (ThirstQuenchPercent() >= -10f)
            currentThirstLevel = ThirstLevel.BoneDry;
        else if (ThirstQuenchPercent() >= -20f)
            currentThirstLevel = ThirstLevel.Dehydrated;
        else
            currentThirstLevel = ThirstLevel.DyingOfThirst;

        gm.healthDisplay.UpdateThirstText();
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
}

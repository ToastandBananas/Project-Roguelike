using UnityEngine;

public enum HungerLevel { Engorged, UncomfortablyFull, Stuffed, Sated, Full, Satisfied, Peckish, Hungry, QuiteHungry, Famished, Starving, Ravenous }

public class Nutrition : MonoBehaviour
{
    public IntStat maxNourishment;
    public float currentNourishment;
    public IntStat maxWater;
    public float currentWater;
    HungerLevel currentHungerLevel = HungerLevel.Satisfied;

    int starvingStaminaDetriment, dehydratedStaminaDetriment;

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
        UpdateHungerBuffsAndDetriments();

        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateHungerText();
    }

    public void DrainNourishment(int timePassed = 6)
    {
        // Drain 100% of fullness in 6 hours game time (3600 turns) | 1 / 3600 = 0.000278
        currentNourishment -= maxNourishment.GetValue() * 0.000278f * (timePassed / TimeSystem.defaultTimeTickInSeconds);

        // Check if the character's hunger is low or high enough to trigger any detriments or bonuses
        UpdateHungerBuffsAndDetriments();

        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateHungerText();
    }

    void UpdateHungerBuffsAndDetriments()
    {
        // Lower max stamina if nourishment is low enough
        if (currentNourishment >= 30f && currentNourishment < 45f && currentHungerLevel != HungerLevel.Peckish)
            AddHungerStaminaDetriment(0f);
        else if (currentNourishment >= 15f && currentNourishment < 30f && currentHungerLevel != HungerLevel.Hungry)
            AddHungerStaminaDetriment(0.15f);
        else if (currentNourishment >= 0f && currentNourishment < 15f && currentHungerLevel != HungerLevel.QuiteHungry)
            AddHungerStaminaDetriment(0.2f);
        else if (currentNourishment >= -10f && currentNourishment < 0f && currentHungerLevel != HungerLevel.Famished)
            AddHungerStaminaDetriment(0.25f);
        else if (currentNourishment >= -20f && currentNourishment < -10f && currentHungerLevel != HungerLevel.Starving)
            AddHungerStaminaDetriment(0.30f);
        else if (currentNourishment < -20f && currentHungerLevel != HungerLevel.Ravenous)
            AddHungerStaminaDetriment(0.35f);
    }

    void AddHungerStaminaDetriment(float staminaDrainMultiplier)
    {
        if (starvingStaminaDetriment != 0f)
            characterManager.status.maxStamina.RemoveModifier(starvingStaminaDetriment);

        starvingStaminaDetriment = Mathf.RoundToInt(characterManager.status.maxStamina.GetBaseValue() * -staminaDrainMultiplier);

        characterManager.status.maxStamina.AddModifier(starvingStaminaDetriment);
        if (characterManager.status.currentStamina > characterManager.status.maxStamina.GetValue())
            characterManager.status.currentStamina = characterManager.status.maxStamina.GetValue();

        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateCurrentStaminaText();

        SetCurrentHungerLevel();
    }

    void SetCurrentHungerLevel()
    {
        if (currentNourishment >= 120f)
            currentHungerLevel = HungerLevel.Engorged;
        else if (currentNourishment >= 110f)
            currentHungerLevel = HungerLevel.UncomfortablyFull;
        else if (currentNourishment >= 100f)
            currentHungerLevel = HungerLevel.Stuffed;
        else if (currentNourishment >= 85f)
            currentHungerLevel = HungerLevel.Sated;
        else if (currentNourishment >= 70f)
            currentHungerLevel = HungerLevel.Full;
        else if (currentNourishment >= 45f)
            currentHungerLevel = HungerLevel.Satisfied;
        else if (currentNourishment >= 30f)
            currentHungerLevel = HungerLevel.Peckish;
        else if (currentNourishment >= 15f)
            currentHungerLevel = HungerLevel.Hungry;
        else if (currentNourishment >= 0f)
            currentHungerLevel = HungerLevel.QuiteHungry;
        else if (currentNourishment >= -10f)
            currentHungerLevel = HungerLevel.Famished;
        else if (currentNourishment >= -20f)
            currentHungerLevel = HungerLevel.Starving;
        else
            currentHungerLevel = HungerLevel.Ravenous;
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


        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateThirstText();
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

        if (characterManager.isNPC == false)
            gm.healthDisplay.UpdateThirstText();
    }

    public bool IsFullyDehydrated()
    {
        if (currentWater <= 0)
            return true;
        return false;
    }
    #endregion
}

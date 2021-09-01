using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Bleeding Wounds")]
    public Injury[] abrasions;
    public Injury[] lacerations;
    public Injury[] gashes;
    public Injury[] stabWounds;

    [Header("Blunt Trauma")]
    public Injury[] bruises;

    [Header("Hunger/Thirst Effects")]
    public StatusEffect[] hungerEffects;
    public StatusEffect[] thirstEffects;

    static GameManager gm;

    #region Singleton
    public static HealthSystem instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of TraumaSystem. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        gm = GameManager.instance;
    }

    public static void ApplyInjury(CharacterManager character, Injury injury, BodyPartType injuryLocation, bool onBackOfBodyPart)
    {
        character.status.GetBodyPart(injuryLocation).injuries.Add(new LocationalInjury(character, injury, injuryLocation, onBackOfBodyPart));
    }

    public static void RemoveInjury(CharacterManager character, LocationalInjury personalInjury)
    {
        BodyPart bodyPart = character.status.GetBodyPart(personalInjury.injuryLocation);
        bodyPart.injuries.Remove(personalInjury);
        if (character.isNPC == false)
        {
            if ((gm.healthDisplay.focusedBodyPart != null && gm.healthDisplay.focusedBodyPart.bodyPart == bodyPart) || (gm.healthDisplay.selectedBodyPart != null && gm.healthDisplay.selectedBodyPart.bodyPart == bodyPart))
                gm.healthDisplay.UpdateTooltip();
            gm.healthDisplay.UpdateHealthHeaderColor(bodyPart.bodyPartType, bodyPart);
        }
    }

    public static void ApplyBuff(CharacterManager character, Consumable consumable, int itemCount, float percentUsed)
    {
        character.status.buffs.Add(new Buff(consumable, itemCount, percentUsed));
    }

    public static void RemoveBuff(CharacterManager character, Buff buff)
    {
        character.status.buffs.Remove(buff);
    }

    public Injury GetLaceration(CharacterManager characterManager, BodyPartType bodyPartType, int damage)
    {
        // Get the max health for the body part being cut
        float maxBodyPartHealth = characterManager.status.GetBodyPart(bodyPartType).maxHealth.GetValue();
        
        // Determine the severity of cut based off of the percent damage done in relation to the max health
        if (damage / maxBodyPartHealth <= 0.05f)
        {
            // Small Cut
            if (lacerations[0] == null)
                Debug.LogError("Small Cut Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return lacerations[0];
        }
        else if (damage / maxBodyPartHealth <= 0.1f)
        {
            // Minor Cut
            if (lacerations[1] == null)
                Debug.LogError("Minor Cut Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return lacerations[1];
        }
        else if (damage / maxBodyPartHealth <= 0.15f)
        {
            // Cut
            if (lacerations[2] == null)
                Debug.LogError("Cut not assigned in the TraumaSystem's inspector. Fix me!");
            return lacerations[2];
        }
        else if (damage / maxBodyPartHealth <= 0.2f)
        {
            // Bad Cut
            if (lacerations[3] == null)
                Debug.LogError("Bad Cut Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return lacerations[3];
        }
        else if (damage / maxBodyPartHealth <= 0.25f)
        {
            // Laceration
            if (lacerations[4] == null)
                Debug.LogError("Laceration Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return lacerations[4];
        }
        else if (damage / maxBodyPartHealth <= 0.3f)
        {
            // Deep Laceration
            if (lacerations[4] == null)
                Debug.LogError("Deep Laceration Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return lacerations[4];
        }
        else // if (damage / maxBodyPartHealth <= 0.35f)
        {
            // Severe Laceration
            if (lacerations[5] == null)
                Debug.LogError("Severe Laceration Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return lacerations[5];
        }
    }

    public StatusEffect GetHungerStatusEffect(CharacterManager characterManager)
    {
        switch (characterManager.nutrition.currentHungerLevel)
        {
            case HungerLevel.Engorged:
                return hungerEffects[0];
            case HungerLevel.UncomfortablyFull:
                return hungerEffects[1];
            case HungerLevel.Stuffed:
                return hungerEffects[2];
            case HungerLevel.Sated:
                return hungerEffects[3];
            case HungerLevel.Full:
                return hungerEffects[4];
            case HungerLevel.Satisfied:
                return hungerEffects[5];
            case HungerLevel.Peckish:
                return hungerEffects[6];
            case HungerLevel.Hungry:
                return hungerEffects[7];
            case HungerLevel.VeryHungry:
                return hungerEffects[8];
            case HungerLevel.Famished:
                return hungerEffects[9];
            case HungerLevel.Starving:
                return hungerEffects[10];
            case HungerLevel.Ravenous:
                return hungerEffects[11];
            default:
                return null;
        }
    }

    public StatusEffect GetThirstStatusEffect(CharacterManager characterManager)
    {
        switch (characterManager.nutrition.currentThirstLevel)
        {
            case ThirstLevel.WaterIntoxicated:
                return thirstEffects[0];
            case ThirstLevel.Overhydrated:
                return thirstEffects[1];
            case ThirstLevel.Slaked:
                return thirstEffects[2];
            case ThirstLevel.Sated:
                return thirstEffects[3];
            case ThirstLevel.Quenched:
                return thirstEffects[4];
            case ThirstLevel.Satisfied:
                return thirstEffects[5];
            case ThirstLevel.Cottonmouthed:
                return thirstEffects[6];
            case ThirstLevel.Thirsty:
                return thirstEffects[7];
            case ThirstLevel.Parched:
                return thirstEffects[8];
            case ThirstLevel.BoneDry:
                return thirstEffects[9];
            case ThirstLevel.Dehydrated:
                return thirstEffects[10];
            case ThirstLevel.DyingOfThirst:
                return thirstEffects[11];
            default:
                return null;
        }
    }
}
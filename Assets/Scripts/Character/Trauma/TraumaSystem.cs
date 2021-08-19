using UnityEngine;

public class TraumaSystem : MonoBehaviour
{
    [Header("Bleeding Wounds")]
    public Injury[] abrasions;
    public Injury[] lacerations;
    public Injury[] gashes;
    public Injury[] stabWounds;

    [Header("Blunt Trauma")]
    public Injury[] bruises;

    static GameManager gm;

    #region Singleton
    public static TraumaSystem instance;
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

    public static void ApplyBuff(CharacterManager character, Consumable consumable)
    {
        character.status.buffs.Add(new Buff(consumable));
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
}
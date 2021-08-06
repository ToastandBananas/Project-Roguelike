using UnityEngine;

public class TraumaSystem : MonoBehaviour
{
    [Header("Bleeding Wounds")]
    public Injury[] abrasions;
    public Injury[] cuts;
    public Injury[] gashes;
    public Injury[] stabWounds;

    [Header("Blunt Trauma")]
    public Injury[] bruises;

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

    public static void ApplyInjury(CharacterManager character, Injury injury, BodyPartType injuryLocation)
    {
        character.status.locationalInjuries.Add(new LocationalInjury(injury, injuryLocation));
    }

    public static void RemoveInjury(CharacterManager character, LocationalInjury personalInjury)
    {
        character.status.locationalInjuries.Remove(personalInjury);
    }

    public static void ApplyBuff(CharacterManager character, Consumable consumable)
    {
        character.status.buffs.Add(new Buff(consumable));
    }

    public static void RemoveBuff(CharacterManager character, Buff buff)
    {
        character.status.buffs.Remove(buff);
    }

    public Injury GetCut(CharacterManager characterManager, BodyPartType bodyPart, int damage)
    {
        // Get the max health for the body part being cut
        int maxBodyPartHealth = characterManager.characterStats.GetBodyPartsMaxHealth(bodyPart).GetValue();

        // Determine the severity of cut based off of the percent damage done in relation to the max health
        if (damage / maxBodyPartHealth <= 0.05f)
        {
            // Small Cut
            if (cuts[0] == null)
                Debug.LogError("Small Cut Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return cuts[0];
        }
        else if (damage / maxBodyPartHealth <= 0.1f)
        {
            // Minor Cut
            if (cuts[1] == null)
                Debug.LogError("Minor Cut Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return cuts[1];
        }
        else if (damage / maxBodyPartHealth <= 0.15f)
        {
            // Cut
            if (cuts[2] == null)
                Debug.LogError("Cut not assigned in the TraumaSystem's inspector. Fix me!");
            return cuts[2];
        }
        else if (damage / maxBodyPartHealth <= 0.2f)
        {
            // Bad Cut
            if (cuts[3] == null)
                Debug.LogError("Bad Cut Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return cuts[3];
        }
        else if (damage / maxBodyPartHealth <= 0.25f)
        {
            // Laceration
            if (cuts[4] == null)
                Debug.LogError("Laceration Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return cuts[4];
        }
        else if (damage / maxBodyPartHealth <= 0.3f)
        {
            // Deep Laceration
            if (cuts[4] == null)
                Debug.LogError("Deep Laceration Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return cuts[4];
        }
        else // if (damage / maxBodyPartHealth <= 0.35f)
        {
            // Severe Laceration
            if (cuts[5] == null)
                Debug.LogError("Severe Laceration Injury not assigned in the TraumaSystem's inspector. Fix me!");
            return cuts[5];
        }
    }
}
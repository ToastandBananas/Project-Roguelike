using UnityEngine;

public class TraumaSystem : MonoBehaviour
{
    [Header("Cuts and Scrapes")]
    public Injury[] abrasions;
    public Injury[] cuts;

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

    public static void ApplyInjury(CharacterManager character, Injury injury, BodyPart injuryLocation)
    {
        character.status.personalInjuries.Add(new PersonalInjury(injury, injuryLocation));
    }

    public static void RemoveInjury(CharacterManager character, PersonalInjury personalInjury)
    {
        character.status.personalInjuries.Remove(personalInjury);
    }

    public Injury GetCut(CharacterManager characterManager, int damage)
    {
        return cuts[0];
    }
}

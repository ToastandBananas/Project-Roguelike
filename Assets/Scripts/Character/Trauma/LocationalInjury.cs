using UnityEngine;

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

    CharacterManager characterManager;

    public LocationalInjury(CharacterManager characterManager, Injury injury, BodyPartType injuryLocation, bool onBackOfBodyPart)
    {
        this.characterManager = characterManager;
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

        if (characterManager.isNPC == false)
            HealthDisplay.instance.UpdateHealthHeaderColor(injuryLocation);
    }
}

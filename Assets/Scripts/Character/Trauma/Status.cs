using System.Collections.Generic;
using UnityEngine;

public class Status : MonoBehaviour
{
    public List<LocationalInjury> locationalInjuries = new List<LocationalInjury>();

    CharacterManager characterManager;

    float torsoDamageBuildup, headDamageBuildup, leftArmDamageBuildup, rightArmDamageBuildup, leftHandDamageBuildup, rightHandDamageBuildup,
        leftLegDamageBuildup, rightLegDamageBuildup, leftFootDamageBuildup, rightFootDamageBuildup;

    void Start()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public void UpdateAids(int timePassed = TimeSystem.defaultTimeTickInSeconds)
    {

    }

    public void UpdateInjuries(int timePassed = TimeSystem.defaultTimeTickInSeconds)
    {
        if (locationalInjuries.Count > 0)
        {
            Bleed(timePassed);

            int injuryCount = locationalInjuries.Count;
            for (int i = injuryCount - 1; i >= 0; i--)
            {
                locationalInjuries[i].injuryTimeRemaining -= Mathf.RoundToInt(timePassed * locationalInjuries[i].injuryHealMultiplier);
                if (locationalInjuries[i].injuryTimeRemaining <= 0)
                    locationalInjuries.Remove(locationalInjuries[i]);
            }
        }
    }

    void Bleed(int timePassed)
    {
        int injuryCount = locationalInjuries.Count;
        for (int i = injuryCount - 1; i >= 0; i--)
        {
            if (locationalInjuries[i].bleedTimeRemaining > 0)
            {
                switch (locationalInjuries[i].injuryLocation) // Body location of the injury
                {
                    case BodyPart.Torso:
                        torsoDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.Head:
                        headDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.LeftArm:
                        leftArmDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.RightArm:
                        rightArmDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.LeftLeg:
                        leftLegDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.RightLeg:
                        rightLegDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.LeftHand:
                        leftHandDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.RightHand:
                        rightHandDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.LeftFoot:
                        leftFootDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.RightFoot:
                        rightFootDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    default:
                        break;
                }

                LoseBlood(locationalInjuries[i].bloodLossPerTurn);

                locationalInjuries[i].bleedTimeRemaining -= Mathf.RoundToInt(timePassed * locationalInjuries[i].injuryHealMultiplier);
                if (locationalInjuries[i].bleedTimeRemaining < 0)
                    locationalInjuries[i].bleedTimeRemaining = 0;
            }
        }

        ApplyDamageBuildup();
    }

    void LoseBlood(int amount)
    {
        characterManager.characterStats.currentBloodAmount -= amount;
    }

    void ApplyDamageBuildup()
    {
        int roundedDamageAmount = 0;
        if (torsoDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(torsoDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(torsoDamageBuildup), BodyPart.Torso);
            torsoDamageBuildup -= roundedDamageAmount;
        }
        if (headDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(headDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(headDamageBuildup), BodyPart.Head);
            headDamageBuildup -= roundedDamageAmount;
        }
        if (leftArmDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(leftArmDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftArmDamageBuildup), BodyPart.LeftArm);
            leftArmDamageBuildup -= roundedDamageAmount;
        }
        if (rightArmDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(rightArmDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightArmDamageBuildup), BodyPart.RightArm);
            rightArmDamageBuildup -= roundedDamageAmount;
        }
        if (leftHandDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(leftHandDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftHandDamageBuildup), BodyPart.LeftHand);
            leftHandDamageBuildup -= roundedDamageAmount;
        }
        if (rightHandDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(rightHandDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightHandDamageBuildup), BodyPart.RightHand);
            rightHandDamageBuildup -= roundedDamageAmount;
        }
        if (leftLegDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(leftLegDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftLegDamageBuildup), BodyPart.LeftLeg);
            leftLegDamageBuildup -= roundedDamageAmount;
        }
        if (rightLegDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(rightLegDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightLegDamageBuildup), BodyPart.RightLeg);
            rightLegDamageBuildup -= roundedDamageAmount;
        }
        if (leftFootDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(leftFootDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftFootDamageBuildup), BodyPart.LeftFoot);
            leftFootDamageBuildup -= roundedDamageAmount;
        }
        if (rightFootDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.RoundToInt(rightFootDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightFootDamageBuildup), BodyPart.RightFoot);
            rightFootDamageBuildup -= roundedDamageAmount;
        }
    }
}

[System.Serializable]
public class LocationalInjury
{
    public Injury injury;
    public BodyPart injuryLocation;
    public float damagePerTurn;
    public int injuryTimeRemaining;
    public int bleedTimeRemaining;
    public int bloodLossPerTurn;
    public float injuryHealMultiplier = 1f;

    public LocationalInjury(Injury injury, BodyPart injuryLocation)
    {
        this.injury = injury;
        this.injuryLocation = injuryLocation;
        RandomizeInjuryVariables();
    }

    void RandomizeInjuryVariables()
    {
        damagePerTurn = Mathf.RoundToInt(Random.Range(injury.minDamagePerTurn, injury.maxDamagePerTurn) * 100f) / 100f;

        injuryTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(injury.minInjuryHealTime), TimeSystem.GetTotalSeconds(injury.maxInjuryHealTime) + 1);

        Vector2Int bleedTimes = injury.GetBleedTime();
        if (bleedTimes.y > 0)
            bleedTimeRemaining = Random.Range(bleedTimes.x, bleedTimes.y + 1);

        Vector2Int bloodLossValues = injury.GetBloodLossPerTurn();
        if (bloodLossValues.y > 0)
            bloodLossPerTurn = Random.Range(bloodLossValues.x, bloodLossValues.y + 1);
    }
}

/*[System.Serializable]
public class LocationalAid
{
    public Aid aid;
    public BodyPart aidLocation;
    public float healPerTurn;
    public int aidTimeRemaining;
}*/

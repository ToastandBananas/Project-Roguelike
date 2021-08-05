using System.Collections.Generic;
using UnityEngine;

public class Status : MonoBehaviour
{
    public List<PersonalInjury> personalInjuries = new List<PersonalInjury>();

    CharacterManager characterManager;

    float torsoDamageBuildup, headDamageBuildup, leftArmDamageBuildup, rightArmDamageBuildup, leftHandDamageBuildup, rightHandDamageBuildup,
        leftLegDamageBuildup, rightLegDamageBuildup, leftFootDamageBuildup, rightFootDamageBuildup;

    void Start()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public void UpdateInjuries(int timePassed = TimeSystem.defaultTimeTickInSeconds)
    {
        Bleed(timePassed);

        int injuryCount = personalInjuries.Count;
        for (int i = injuryCount - 1; i >= 0; i--)
        {
            personalInjuries[i].injuryTimeRemaining -= Mathf.RoundToInt(timePassed * personalInjuries[i].injuryHealMultiplier);
            if (personalInjuries[i].injuryTimeRemaining <= 0)
                personalInjuries.Remove(personalInjuries[i]);
        }
    }

    void Bleed(int timePassed)
    {
        int injuryCount = personalInjuries.Count;
        for (int i = injuryCount - 1; i >= 0; i--)
        {
            if (personalInjuries[i].bleedTimeRemaining > 0)
            {
                switch (personalInjuries[i].injuryLocation) // Body location of the injury
                {
                    case BodyPart.Torso:
                        torsoDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.Head:
                        headDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.LeftArm:
                        leftArmDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.RightArm:
                        rightArmDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.LeftLeg:
                        leftLegDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.RightLeg:
                        rightLegDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.LeftHand:
                        leftHandDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.RightHand:
                        rightHandDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.LeftFoot:
                        leftFootDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    case BodyPart.RightFoot:
                        rightFootDamageBuildup += personalInjuries[i].damagePerTurn;
                        break;
                    default:
                        break;
                }

                LoseBlood(personalInjuries[i].bloodLossPerTurn);

                personalInjuries[i].bleedTimeRemaining -= Mathf.RoundToInt(timePassed * personalInjuries[i].injuryHealMultiplier);
                if (personalInjuries[i].bleedTimeRemaining < 0)
                    personalInjuries[i].bleedTimeRemaining = 0;
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
public class PersonalInjury
{
    public Injury injury;
    public BodyPart injuryLocation;
    public float damagePerTurn;
    public int injuryTimeRemaining;
    public int bleedTimeRemaining;
    public int bloodLossPerTurn;
    public float injuryHealMultiplier = 1f;

    public PersonalInjury(Injury injury, BodyPart injuryLocation)
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

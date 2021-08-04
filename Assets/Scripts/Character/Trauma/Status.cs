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
                switch (personalInjuries[i].injuryLocation)
                {
                    case BodyPart.Torso:
                        torsoDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.Head:
                        headDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.LeftArm:
                        leftArmDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.RightArm:
                        rightArmDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.LeftLeg:
                        leftLegDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.RightLeg:
                        rightLegDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.LeftHand:
                        leftHandDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.RightHand:
                        rightHandDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.LeftFoot:
                        leftFootDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    case BodyPart.RightFoot:
                        rightFootDamageBuildup += personalInjuries[i].injury.damagePerTurn;
                        break;
                    default:
                        break;
                }

                personalInjuries[i].bleedTimeRemaining -= Mathf.RoundToInt(timePassed * personalInjuries[i].injuryHealMultiplier);
                if (personalInjuries[i].bleedTimeRemaining < 0)
                    personalInjuries[i].bleedTimeRemaining = 0;
            }
        }

        if (torsoDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(torsoDamageBuildup), BodyPart.Torso, null, false, false);
            torsoDamageBuildup = 0;
        }
        if (headDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(headDamageBuildup), BodyPart.Head, null, false, false);
            headDamageBuildup = 0;
        }
        if (leftArmDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(leftArmDamageBuildup), BodyPart.LeftArm, null, false, false);
            leftArmDamageBuildup = 0;
        }
        if (rightArmDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(rightArmDamageBuildup), BodyPart.RightArm, null, false, false);
            rightArmDamageBuildup = 0;
        }
        if (leftHandDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(leftHandDamageBuildup), BodyPart.LeftHand, null, false, false);
            leftHandDamageBuildup = 0;
        }
        if (rightHandDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(rightHandDamageBuildup), BodyPart.RightHand, null, false, false);
            rightHandDamageBuildup = 0;
        }
        if (leftLegDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(leftLegDamageBuildup), BodyPart.LeftLeg, null, false, false);
            leftLegDamageBuildup = 0;
        }
        if (rightLegDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(rightLegDamageBuildup), BodyPart.RightLeg, null, false, false);
            rightLegDamageBuildup = 0;
        }
        if (leftFootDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(leftFootDamageBuildup), BodyPart.LeftFoot, null, false, false);
            leftFootDamageBuildup = 0;
        }
        if (rightFootDamageBuildup >= 1f)
        {
            characterManager.characterStats.TakeLocationalDamage(Mathf.RoundToInt(rightFootDamageBuildup), BodyPart.RightFoot, null, false, false);
            rightFootDamageBuildup = 0;
        }
    }
}

[System.Serializable]
public class PersonalInjury
{
    public Injury injury;
    public BodyPart injuryLocation;
    public int injuryTimeRemaining;
    public int bleedTimeRemaining;
    public float injuryHealMultiplier = 1f;

    public PersonalInjury(Injury injury, BodyPart injuryLocation)
    {
        this.injury = injury;
        this.injuryLocation = injuryLocation;
        RandomizeInjuryTimes();
    }

    void RandomizeInjuryTimes()
    {
        injuryTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(injury.minInjuryHealTime), TimeSystem.GetTotalSeconds(injury.maxInjuryHealTime) + 1);

        if (injury.GetMaxBleedTime() > 0)
            bleedTimeRemaining = Random.Range(injury.GetMinBleedTime(), injury.GetMaxBleedTime() + 1);
    }
}

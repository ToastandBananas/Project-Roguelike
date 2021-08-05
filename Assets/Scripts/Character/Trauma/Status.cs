using System.Collections.Generic;
using UnityEngine;

public class Status : MonoBehaviour
{
    public CharacterManager characterManager;

    public List<LocationalInjury> locationalInjuries = new List<LocationalInjury>();
    public List<Buff> buffs = new List<Buff>();

    float torsoDamageBuildup, headDamageBuildup, leftArmDamageBuildup, rightArmDamageBuildup, leftHandDamageBuildup, rightHandDamageBuildup,
        leftLegDamageBuildup, rightLegDamageBuildup, leftFootDamageBuildup, rightFootDamageBuildup;

    float torsoHealingBuildup, headHealingBuildup, leftArmHealingBuildup, rightArmHealingBuildup, leftHandHealingBuildup, rightHandHealingBuildup,
        leftLegHealingBuildup, rightLegHealingBuildup, leftFootHealingBuildup, rightFootHealingBuildup;

    readonly float naturalHealingPercentPerTurn = 0.0001f; // 60 minutes (100 turns) to heal entire body 1% if healthiness == 1

    // Healthiness signifies your overall bodily health, effecting your natural healing over time.
    // Eating healthy, among other things, slowly builds this up over time, but this can be easily reversed by ailments such as an infection or a concussion.
    float healthiness = 100f;
    readonly float minHealthiness = -200f;
    readonly float maxHealthiness = 200f;

    void Start()
    {
        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();
    }

    public void AdjustHealthiness(float amount)
    {
        healthiness += amount;
        if (healthiness > maxHealthiness)
            healthiness = maxHealthiness;
        if (healthiness < minHealthiness)
            healthiness = minHealthiness;
    }

    public void UpdateBuffs(int timePassed = TimeSystem.defaultTimeTickInSeconds)
    {
        Heal(timePassed);
        ApplyHealingBuildup();

        if (buffs.Count > 0)
        {
            int buffCount = buffs.Count;
            for (int i = buffCount - 1; i >= 0; i--)
            {
                buffs[i].buffTimeRemaining -= timePassed;
                if (buffs[i].buffTimeRemaining <= 0)
                    buffs.Remove(buffs[i]);
            }
        }
    }

    public void UpdateInjuries(int timePassed = TimeSystem.defaultTimeTickInSeconds)
    {
        if (locationalInjuries.Count > 0)
        {
            Bleed(timePassed);
            ApplyDamageBuildup();

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
        for (int i = 0; i < locationalInjuries.Count; i++)
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
    }

    void LoseBlood(int amount)
    {
        characterManager.characterStats.currentBloodAmount -= amount;
    }

    public bool BodyPartIsBleeding(BodyPart bodyPart)
    {
        for (int i = 0; i < locationalInjuries.Count; i++)
        {
            if (locationalInjuries[i].injuryLocation == bodyPart && locationalInjuries[i].bleedTimeRemaining > 0)
                return true;
        }
        return false;
    }

    void ApplyDamageBuildup()
    {
        int roundedDamageAmount = 0;
        if (torsoDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(torsoDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(torsoDamageBuildup), BodyPart.Torso);
            torsoDamageBuildup -= roundedDamageAmount;
        }
        if (headDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(headDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(headDamageBuildup), BodyPart.Head);
            headDamageBuildup -= roundedDamageAmount;
        }
        if (leftArmDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(leftArmDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftArmDamageBuildup), BodyPart.LeftArm);
            leftArmDamageBuildup -= roundedDamageAmount;
        }
        if (rightArmDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(rightArmDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightArmDamageBuildup), BodyPart.RightArm);
            rightArmDamageBuildup -= roundedDamageAmount;
        }
        if (leftHandDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(leftHandDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftHandDamageBuildup), BodyPart.LeftHand);
            leftHandDamageBuildup -= roundedDamageAmount;
        }
        if (rightHandDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(rightHandDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightHandDamageBuildup), BodyPart.RightHand);
            rightHandDamageBuildup -= roundedDamageAmount;
        }
        if (leftLegDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(leftLegDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftLegDamageBuildup), BodyPart.LeftLeg);
            leftLegDamageBuildup -= roundedDamageAmount;
        }
        if (rightLegDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(rightLegDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightLegDamageBuildup), BodyPart.RightLeg);
            rightLegDamageBuildup -= roundedDamageAmount;
        }
        if (leftFootDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(leftFootDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftFootDamageBuildup), BodyPart.LeftFoot);
            leftFootDamageBuildup -= roundedDamageAmount;
        }
        if (rightFootDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(rightFootDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightFootDamageBuildup), BodyPart.RightFoot);
            rightFootDamageBuildup -= roundedDamageAmount;
        }
    }

    void Heal(int timePassed)
    {
        // Apply natural healing first, if the character is healthy enough
        if (healthiness > 0)
        {
            if (BodyPartIsBleeding(BodyPart.Torso) == false)
                torsoHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.Head) == false)
                headHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxHeadHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.LeftArm) == false)
                leftArmHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxLeftArmHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.RightArm) == false)
                rightArmHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxRightArmHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.LeftHand) == false)
                leftHandHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxLeftHandHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.RightHand) == false)
                rightHandHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxRightHandHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.LeftLeg) == false)
                leftLegHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxLeftLegHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.RightLeg) == false)
                rightLegHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxRightLegHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.LeftFoot) == false)
                leftFootHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxLeftFootHealth.GetValue();
            if (BodyPartIsBleeding(BodyPart.RightFoot) == false)
                rightFootHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.maxRightFootHealth.GetValue();
        }

        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].healPercentPerTurn > 0)
            {
                torsoHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxHealth.GetValue();
                headHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxHeadHealth.GetValue();
                leftArmHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxLeftArmHealth.GetValue();
                rightArmHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxRightArmHealth.GetValue();
                leftHandHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxLeftHandHealth.GetValue();
                rightHandHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxRightHandHealth.GetValue();
                leftLegHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxLeftLegHealth.GetValue();
                rightLegHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxRightLegHealth.GetValue();
                leftFootHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxLeftFootHealth.GetValue();
                rightFootHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.maxRightFootHealth.GetValue();
            }
        }
    }

    void ApplyHealingBuildup()
    {
        int roundedHealingAmount = 0;
        if (torsoHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(torsoHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.Torso, roundedHealingAmount);
            torsoHealingBuildup -= roundedHealingAmount;
        }
        if (headHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(headHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.Head, roundedHealingAmount);
            headHealingBuildup -= roundedHealingAmount;
        }
        if (leftArmHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(leftArmHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.LeftArm, roundedHealingAmount);
            leftArmHealingBuildup -= roundedHealingAmount;
        }
        if (rightArmHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(rightArmHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.RightArm, roundedHealingAmount);
            rightArmHealingBuildup -= roundedHealingAmount;
        }
        if (leftHandHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(leftHandHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.LeftHand, roundedHealingAmount);
            leftHandHealingBuildup -= roundedHealingAmount;
        }
        if (rightHandHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(rightHandHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.RightHand, roundedHealingAmount);
            rightHandHealingBuildup -= roundedHealingAmount;
        }
        if (leftLegHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(leftLegHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.LeftLeg, roundedHealingAmount);
            leftLegHealingBuildup -= roundedHealingAmount;
        }
        if (rightLegHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(rightLegHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.RightLeg, roundedHealingAmount);
            rightLegHealingBuildup -= roundedHealingAmount;
        }
        if (leftFootHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(leftFootHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.LeftFoot, roundedHealingAmount);
            leftFootHealingBuildup -= roundedHealingAmount;
        }
        if (rightFootHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(rightFootHealingBuildup);
            characterManager.characterStats.AddToCurrentHealth_Instant(BodyPart.RightFoot, roundedHealingAmount);
            rightFootHealingBuildup -= roundedHealingAmount;
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
        SetupInjuryVariables();
    }

    void SetupInjuryVariables()
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

[System.Serializable]
public class Buff
{
    public float healPercentPerTurn;
    public int buffTimeRemaining;

    public Buff(Consumable consumable)
    {
        SetupBuffVariables(consumable);
    }

    void SetupBuffVariables(Consumable consumable)
    {
        buffTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(consumable.minHealTime), TimeSystem.GetTotalSeconds(consumable.maxHealTime) + 1);
        healPercentPerTurn = consumable.gradualHealPercent / buffTimeRemaining;
    }
}

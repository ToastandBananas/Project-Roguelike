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
                    case BodyPartType.Torso:
                        torsoDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.Head:
                        headDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.LeftArm:
                        leftArmDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.RightArm:
                        rightArmDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.LeftLeg:
                        leftLegDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.RightLeg:
                        rightLegDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.LeftHand:
                        leftHandDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.RightHand:
                        rightHandDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.LeftFoot:
                        leftFootDamageBuildup += locationalInjuries[i].damagePerTurn;
                        break;
                    case BodyPartType.RightFoot:
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

    public bool BodyPartIsBleeding(BodyPartType bodyPart)
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
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(torsoDamageBuildup), BodyPartType.Torso);
            torsoDamageBuildup -= roundedDamageAmount;
        }
        if (headDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(headDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(headDamageBuildup), BodyPartType.Head);
            headDamageBuildup -= roundedDamageAmount;
        }
        if (leftArmDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(leftArmDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftArmDamageBuildup), BodyPartType.LeftArm);
            leftArmDamageBuildup -= roundedDamageAmount;
        }
        if (rightArmDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(rightArmDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightArmDamageBuildup), BodyPartType.RightArm);
            rightArmDamageBuildup -= roundedDamageAmount;
        }
        if (leftHandDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(leftHandDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftHandDamageBuildup), BodyPartType.LeftHand);
            leftHandDamageBuildup -= roundedDamageAmount;
        }
        if (rightHandDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(rightHandDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightHandDamageBuildup), BodyPartType.RightHand);
            rightHandDamageBuildup -= roundedDamageAmount;
        }
        if (leftLegDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(leftLegDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftLegDamageBuildup), BodyPartType.LeftLeg);
            leftLegDamageBuildup -= roundedDamageAmount;
        }
        if (rightLegDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(rightLegDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightLegDamageBuildup), BodyPartType.RightLeg);
            rightLegDamageBuildup -= roundedDamageAmount;
        }
        if (leftFootDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(leftFootDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(leftFootDamageBuildup), BodyPartType.LeftFoot);
            leftFootDamageBuildup -= roundedDamageAmount;
        }
        if (rightFootDamageBuildup >= 1f)
        {
            roundedDamageAmount = Mathf.FloorToInt(rightFootDamageBuildup);
            characterManager.characterStats.TakeStaticLocationalDamage(Mathf.RoundToInt(rightFootDamageBuildup), BodyPartType.RightFoot);
            rightFootDamageBuildup -= roundedDamageAmount;
        }
    }

    void Heal(int timePassed)
    {
        // Apply natural healing first, if the character is healthy enough
        if (healthiness > 0)
        {
            if (BodyPartIsBleeding(BodyPartType.Torso) == false)
                torsoHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.torso.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.Head) == false)
                headHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.head.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.LeftArm) == false)
                leftArmHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.leftArm.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.RightArm) == false)
                rightArmHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.rightArm.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.LeftHand) == false)
                leftHandHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.leftHand.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.RightHand) == false)
                rightHandHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.rightHand.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.LeftLeg) == false)
                leftLegHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.leftLeg.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.RightLeg) == false)
                rightLegHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.rightLeg.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.LeftFoot) == false)
                leftFootHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.leftFoot.maxHealth.GetValue();
            if (BodyPartIsBleeding(BodyPartType.RightFoot) == false)
                rightFootHealingBuildup += naturalHealingPercentPerTurn * (healthiness / 100f) * characterManager.characterStats.rightFoot.maxHealth.GetValue();
        }

        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].healPercentPerTurn > 0)
            {
                torsoHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.torso.maxHealth.GetValue();
                headHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.head.maxHealth.GetValue();
                leftArmHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.leftArm.maxHealth.GetValue();
                rightArmHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.rightArm.maxHealth.GetValue();
                leftHandHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.leftHand.maxHealth.GetValue();
                rightHandHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.rightHand.maxHealth.GetValue();
                leftLegHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.leftLeg.maxHealth.GetValue();
                rightLegHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.rightLeg.maxHealth.GetValue();
                leftFootHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.leftFoot.maxHealth.GetValue();
                rightFootHealingBuildup += buffs[i].healPercentPerTurn * characterManager.characterStats.rightFoot.maxHealth.GetValue();
            }
        }
    }

    void ApplyHealingBuildup()
    {
        int roundedHealingAmount = 0;
        if (torsoHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(torsoHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.Torso).HealInstant_StaticValue(roundedHealingAmount);
            torsoHealingBuildup -= roundedHealingAmount;
        }
        if (headHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(headHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.Head).HealInstant_StaticValue(roundedHealingAmount);
            headHealingBuildup -= roundedHealingAmount;
        }
        if (leftArmHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(leftArmHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.LeftArm).HealInstant_StaticValue(roundedHealingAmount);
            leftArmHealingBuildup -= roundedHealingAmount;
        }
        if (rightArmHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(rightArmHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.RightArm).HealInstant_StaticValue(roundedHealingAmount);
            rightArmHealingBuildup -= roundedHealingAmount;
        }
        if (leftHandHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(leftHandHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.LeftHand).HealInstant_StaticValue(roundedHealingAmount);
            leftHandHealingBuildup -= roundedHealingAmount;
        }
        if (rightHandHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(rightHandHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.RightHand).HealInstant_StaticValue(roundedHealingAmount);
            rightHandHealingBuildup -= roundedHealingAmount;
        }
        if (leftLegHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(leftLegHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.LeftLeg).HealInstant_StaticValue(roundedHealingAmount);
            leftLegHealingBuildup -= roundedHealingAmount;
        }
        if (rightLegHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(rightLegHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.RightLeg).HealInstant_StaticValue(roundedHealingAmount);
            rightLegHealingBuildup -= roundedHealingAmount;
        }
        if (leftFootHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(leftFootHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.LeftFoot).HealInstant_StaticValue(roundedHealingAmount);
            leftFootHealingBuildup -= roundedHealingAmount;
        }
        if (rightFootHealingBuildup >= 1f)
        {
            roundedHealingAmount = Mathf.FloorToInt(rightFootHealingBuildup);
            characterManager.characterStats.GetBodyPart(BodyPartType.RightFoot).HealInstant_StaticValue(roundedHealingAmount);
            rightFootHealingBuildup -= roundedHealingAmount;
        }
    }
}

[System.Serializable]
public class LocationalInjury
{
    public Injury injury;
    public BodyPartType injuryLocation;
    public int injuryTimeRemaining;

    public float damagePerTurn;
    public int bleedTimeRemaining;
    public int bloodLossPerTurn;
    public float injuryHealMultiplier = 1f;

    public LocationalInjury(Injury injury, BodyPartType injuryLocation)
    {
        this.injury = injury;
        this.injuryLocation = injuryLocation;
        SetupInjuryVariables();
    }

    void SetupInjuryVariables()
    {
        damagePerTurn = Mathf.RoundToInt(Random.Range(injury.damagePerTurn.x, injury.damagePerTurn.y) * 100f) / 100f;

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
    public Consumable consumable;
    public int buffTimeRemaining;

    public float healPercentPerTurn;
    public int healTimeRemaining;

    public Buff(Consumable consumable)
    {
        this.consumable = consumable;
        SetupBuffVariables(consumable);
    }

    void SetupBuffVariables(Consumable consumable)
    {
        healTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(consumable.minHealTime), TimeSystem.GetTotalSeconds(consumable.maxHealTime) + 1);
        buffTimeRemaining = healTimeRemaining;
        healPercentPerTurn = consumable.gradualHealPercent / buffTimeRemaining;
    }
}

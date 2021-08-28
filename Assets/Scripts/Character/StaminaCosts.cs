using UnityEngine;

public class StaminaCosts : MonoBehaviour
{
    const float baseRunCost = 1f;

    public static float GetRunCost(CharacterManager characterRunning, bool diaganol)
    {
        // The greater the characters carry weight, the greater the stamina cost
        float cost = baseRunCost + ((0.05f * characterRunning.totalCarryWeight) - (0.025f * characterRunning.characterStats.strength.GetValue()));
        if (diaganol)
            cost *= 1.414214f;

        if (cost < baseRunCost)
            cost = baseRunCost;

        return cost;
    }

    public static float GetOverEncumberedMoveCost(CharacterManager characterMoving, bool diaganol)
    {
        // The greater the over-encumbrance, the greater the stamina cost
        float cost = (characterMoving.totalCarryWeight - characterMoving.characterStats.GetMaximumWeightCapacity()) * 0.2f;
        if (diaganol)
            cost *= 1.414214f;

        // TODO: Account for tile type

        return cost;
    }

    public static float GetAttackCost(CharacterManager characterAttacking, Weapon weapon)
    {
        if (weapon == null)
            return 5;

        float cost = weapon.weight * 2f;
        if (characterAttacking.equipmentManager.isTwoHanding)
        {
            if (characterAttacking.characterStats.strength.GetValue() < weapon.StrengthRequired_TwoHand())
                cost += 0.2f * (weapon.StrengthRequired_TwoHand() - characterAttacking.characterStats.strength.GetValue());
            else
            {
                cost += 0.1f * (weapon.StrengthRequired_TwoHand() - characterAttacking.characterStats.strength.GetValue());
                if (cost < weapon.weight)
                    cost = weapon.weight;
            }
        }
        else if (characterAttacking.equipmentManager.isTwoHanding == false)
        {
            if (weapon.CanOneHand(characterAttacking) == false)
                cost += 0.2f * (weapon.strengthRequirement_OneHand - characterAttacking.characterStats.strength.GetValue());
            else
            {
                cost += 0.1f * (weapon.strengthRequirement_OneHand - characterAttacking.characterStats.strength.GetValue());
                if (cost < weapon.weight)
                    cost = weapon.weight;
            }
        }

        return cost;
    }

    public static float GetBlockCost(CharacterManager characterBlocking, Weapon weapon)
    {
        float cost;
        if (weapon != null)
            cost = weapon.weight * 1.5f;
        else
            cost = 2;

        // Strength under 60 will cost more stamina to block, strength over 60 will cost less 
        cost -= (characterBlocking.characterStats.strength.GetValue() - 60) / 10;
        if (cost < 1)
            cost = 1; // Minimum cost of 1 stamina

        return cost;
    }
}

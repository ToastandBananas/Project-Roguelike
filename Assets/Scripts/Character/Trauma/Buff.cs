using UnityEngine;

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
        healTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(consumable.minGradualHealTime), TimeSystem.GetTotalSeconds(consumable.maxGradualHealTime) + 1);
        buffTimeRemaining = healTimeRemaining;
        healPercentPerTurn = consumable.gradualHealPercent / buffTimeRemaining;
    }
}
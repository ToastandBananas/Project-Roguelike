using UnityEngine;

[System.Serializable]
public class Buff
{
    public Consumable consumable;
    public int buffTimeRemaining;

    public float healPercentPerTurn;
    public int healTimeRemaining;

    public Buff(Consumable consumable, int itemCount, float percentUsed)
    {
        this.consumable = consumable;
        SetupBuffVariables(consumable, itemCount, percentUsed);
    }

    void SetupBuffVariables(Consumable consumable, int itemCount, float percentUsed)
    {
        healTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(consumable.minGradualHealTime), TimeSystem.GetTotalSeconds(consumable.maxGradualHealTime) + 1);
        buffTimeRemaining = healTimeRemaining;
        healPercentPerTurn = (consumable.gradualHealPercent / buffTimeRemaining) * itemCount * percentUsed;
    }
}
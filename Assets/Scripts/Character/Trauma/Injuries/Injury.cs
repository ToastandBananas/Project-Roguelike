using UnityEngine;

public enum InjuryType { Abrasion, Laceration, Puncture, Avulsion, Bruise }

public class Injury : ScriptableObject
{
    new public string name = "New Injury";
    public string description;
    public InjuryType injuryType;
    public int severity;

    [Header("Bleed Variables")]
    public Vector3Int minInjuryHealTime;
    public Vector3Int maxInjuryHealTime;
    public Vector2 damagePerTurn;

    public Vector2Int GetInjuryTimesInSeconds()
    {
        return new Vector2Int(TimeSystem.GetTotalSeconds(minInjuryHealTime), TimeSystem.GetTotalSeconds(maxInjuryHealTime));
    }

    public virtual Vector2Int GetBleedTime()
    {
        return Vector2Int.zero;
    }

    public virtual Vector2 GetBloodLossPerTurn()
    {
        return Vector2.zero;
    }

    public bool CanBandage()
    {
        if (injuryType == InjuryType.Abrasion || injuryType == InjuryType.Laceration || injuryType == InjuryType.Puncture || injuryType == InjuryType.Bruise)
            return true;
        return false;
    }
}

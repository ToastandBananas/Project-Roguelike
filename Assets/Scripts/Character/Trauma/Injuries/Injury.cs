using UnityEngine;

public enum InjuryType { Cut, Abrasion, Bruise }

public class Injury : ScriptableObject
{
    new public string name = "New Injury";
    public string description;
    public InjuryType injuryType;

    [Header("Bleed Variables")]
    public Vector3Int minInjuryHealTime;
    public Vector3Int maxInjuryHealTime;
    public Vector2 damagePerTurn;

    public virtual Vector2Int GetBleedTime()
    {
        return Vector2Int.zero;
    }

    public virtual Vector2Int GetBloodLossPerTurn()
    {
        return Vector2Int.zero;
    }
}

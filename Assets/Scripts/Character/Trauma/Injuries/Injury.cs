using UnityEngine;

public enum InjuryType { Cut, Abrasion, Bruise }

public class Injury : ScriptableObject
{
    new public string name = "New Injury";
    public string description;
    public InjuryType injuryType;
    public Vector3Int minInjuryHealTime;
    public Vector3Int maxInjuryHealTime;
    public float damagePerTurn = 1f;

    public virtual int GetMinBleedTime()
    {
        return 0;
    }

    public virtual int GetMaxBleedTime()
    {
        return 0;
    }
}

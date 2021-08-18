using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntStat
{
    [SerializeField] int baseValue = 20;

    [SerializeField] List<int> modifiers = new List<int>();

    public int GetValue()
    {
        int finalValue = baseValue;
        modifiers.ForEach(mod => finalValue += mod);

        return finalValue;
    }

    public void SetBaseValue(int value)
    {
        baseValue = value;
    }

    public void AddModifier(int modifier)
    {
        if (modifier != 0)
            modifiers.Add(modifier);
    }

    public void RemoveModifier(int modifier)
    {
        if (modifier != 0)
            modifiers.Remove(modifier);
    }
}
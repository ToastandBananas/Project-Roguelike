using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloatStat
{
    [SerializeField] float baseValue = 20f;

    [SerializeField] List<float> modifiers = new List<float>();

    public float GetValue()
    {
        float finalValue = baseValue;
        modifiers.ForEach(mod => finalValue += mod);

        return finalValue;
    }

    public void SetBaseValue(float value)
    {
        baseValue = value;
    }

    public void AddModifier(float modifier)
    {
        if (modifier != 0)
            modifiers.Add(modifier);
    }

    public void RemoveModifier(float modifier)
    {
        if (modifier != 0)
            modifiers.Remove(modifier);
    }
}
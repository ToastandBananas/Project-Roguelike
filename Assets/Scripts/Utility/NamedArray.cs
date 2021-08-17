using UnityEngine;

public class NamedArray : PropertyAttribute
{
    public readonly string[] names;
    public NamedArray(string[] names) { this.names = names; }
}
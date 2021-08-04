using UnityEngine;

[CreateAssetMenu(fileName = "New Cut Injury", menuName = "Trauma/Injury/Cut")]
public class Cut : Injury
{
    [Tooltip("Measured in seconds")] public int minBleedTime = 30;
    [Tooltip("Measured in seconds")] public int maxBleedTime = 60;

    public override int GetMinBleedTime()
    {
        return minBleedTime;
    }

    public override int GetMaxBleedTime()
    {
        return maxBleedTime;
    }
}

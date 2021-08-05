using UnityEngine;

[CreateAssetMenu(fileName = "New Cut Injury", menuName = "Trauma/Injury/Cut")]
public class Cut : Injury
{
    [Tooltip("Measured in seconds")] public int minBleedTime = 30;
    [Tooltip("Measured in seconds")] public int maxBleedTime = 60;
    [Tooltip("Measured in mL")] public int minBloodLossPerTurn = 10;
    [Tooltip("Measured in mL")] public int maxBloodLossPerTurn = 20;

    public override Vector2Int GetBleedTime()
    {
        return new Vector2Int(minBleedTime, maxBleedTime);
    }

    public override Vector2Int GetBloodLossPerTurn()
    {
        return new Vector2Int(minBloodLossPerTurn, maxBloodLossPerTurn);
    }
}

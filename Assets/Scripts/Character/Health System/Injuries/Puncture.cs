using UnityEngine;

[CreateAssetMenu(fileName = "New Puncture", menuName = "Health System/Injury/Puncture")]
public class Puncture : Injury
{
    [Header("Bleed")]
    [Tooltip("Measured in seconds")] public Vector2Int bleedTime = new Vector2Int(30, 60);
    [Tooltip("Measured in mL")] public Vector2 immediateBloodLoss = new Vector2(20f, 30f);
    [Tooltip("Measured in mL")] public Vector2 bloodLossPerTurn = new Vector2(10f, 20f);

    public override Vector2Int BleedTime()
    {
        return bleedTime;
    }

    public override Vector2 BloodLossPerTurn()
    {
        return bloodLossPerTurn;
    }

    public override Vector2 ImmediateBloodLoss()
    {
        return immediateBloodLoss;
    }
}

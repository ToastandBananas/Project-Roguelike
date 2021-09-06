using UnityEngine;

[CreateAssetMenu(fileName = "New Laceration", menuName = "Health System/Injury/Laceration")]
public class Laceration : Injury
{
    [Header("Bleed")]
    [Tooltip("Measured in seconds")] public Vector2Int bleedTime = new Vector2Int(30, 60);
    [Tooltip("Measured in mL")] public Vector2 bloodLossPerTurn = new Vector2(10f, 20f);

    public override Vector2Int BleedTime()
    {
        return bleedTime;
    }

    public override Vector2 BloodLossPerTurn()
    {
        return bloodLossPerTurn;
    }
}

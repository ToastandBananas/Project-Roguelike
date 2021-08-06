using UnityEngine;

[CreateAssetMenu(fileName = "New Laceration", menuName = "Trauma/Injury/Laceration")]
public class Laceration : Injury
{
    [Tooltip("Measured in seconds")] public Vector2Int bleedTime = new Vector2Int(30, 60);
    [Tooltip("Measured in mL")] public Vector2Int bloodLossPerTurn = new Vector2Int(10, 20);

    public override Vector2Int GetBleedTime()
    {
        return bleedTime;
    }

    public override Vector2Int GetBloodLossPerTurn()
    {
        return bloodLossPerTurn;
    }
}

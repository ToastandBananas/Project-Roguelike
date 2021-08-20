using UnityEngine;

[CreateAssetMenu(fileName = "New Key", menuName = "Inventory/Key")]
public class DoorKey : Item
{
    public override bool IsKey()
    {
        return true;
    }
}

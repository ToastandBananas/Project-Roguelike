//using UnityEngine;

public class PlayerStats : CharacterStats
{
    public Stat treeDamage;

    public override void Die()
    {
        base.Die();
    }

    public override void OnWearableChanged(ItemData newItemData, ItemData oldItemData)
    {
        base.OnWearableChanged(newItemData, oldItemData);

        /*if (newItem != null)
        {
            
        }

        if (oldItem != null)
        {
            
        }*/
    }

    public override void OnWeaponChanged(ItemData newItemData, ItemData oldItemData)
    {
        base.OnWeaponChanged(newItemData, oldItemData);

        if (newItemData != null)
        {
            treeDamage.AddModifier(newItemData.treeDamage);
        }

        if (oldItemData != null)
        {
            treeDamage.RemoveModifier(oldItemData.treeDamage);
        }
    }
}

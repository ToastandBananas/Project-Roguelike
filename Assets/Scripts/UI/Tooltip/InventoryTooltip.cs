public class InventoryTooltip : Tooltip
{
    public override void BuildTooltip(ItemData itemData)
    {
        stringBuilder.Clear();

        // Item name
        stringBuilder.Append("<b><size=26>" + itemData.itemName + "</size></b>\n");

        if (itemData.item.IsWeapon())
        {
            Weapon weapon = (Weapon)itemData.item;
            
            // One/two handed
            if (weapon.isTwoHanded)
                stringBuilder.Append("<i>Two-Handed -  ");
            else
                stringBuilder.Append("<i>One-Handed -  ");

            stringBuilder.Append(weapon.weaponType + "</i>\n");
        }

        stringBuilder.Append("\n");

        // Description
        if (itemData.item.description != "")
            stringBuilder.Append(Utilities.FormatStringIntoParagraphs(itemData.item.description, 30) + "\n\n");
        
        if (itemData.item.IsWeapon()) 
        {
            Weapon weapon = (Weapon)itemData.item;

            // Attack range
            stringBuilder.Append("Attack Range: " + weapon.attackRange + "\n\n");

            // Damage
            stringBuilder.Append("Damage: " + itemData.damage + "\n");
            
            // Tree damage
            if (itemData.treeDamage > 0)
                stringBuilder.Append("Tree Damage : " + itemData.treeDamage + "\n\n");
        }
        else if (itemData.item.IsShield())
        {
            // Shield bash damage
            stringBuilder.Append("Shield Bash Damage: " + itemData.shieldBashDamage + "\n\n");
        }
        else if (itemData.item.IsWearable())
        {
            Wearable wearable = (Wearable)itemData.item;
            
            // Defense
            if (itemData.headDefense > 0)
                stringBuilder.Append("Head Defense: " + itemData.headDefense + "\n");

            if (itemData.torsoDefense > 0)
                stringBuilder.Append("Torso Defense: " + itemData.torsoDefense + "\n");

            if (itemData.armDefense > 0)
                stringBuilder.Append("Arms Defense: " + itemData.armDefense + "\n");

            if (itemData.handDefense > 0)
                stringBuilder.Append("Hands Defense: " + itemData.handDefense + "\n");

            if (itemData.legDefense > 0)
                stringBuilder.Append("Legs Defense: " + itemData.legDefense + "\n");

            if (itemData.footDefense > 0)
                stringBuilder.Append("Feet Defense: " + itemData.footDefense + "\n");

            // Cold resistance
            if (wearable.coldResistance > 0)
                stringBuilder.Append("Cold Resistance: " + wearable.coldResistance + "° F\n");

            // Heat resistance
            if (wearable.heatResistance > 0)
                stringBuilder.Append("Heat Resistance: " + wearable.heatResistance + "° F\n");

            stringBuilder.Append("\n");
        }
        else if (itemData.item.IsConsumable())
        {
            Consumable consumable = (Consumable)itemData.item;

            // Freshness
            stringBuilder.Append("Freshness: " + itemData.freshness + "%\n");

            // Thirst quench
            if (consumable.thirstQuench > 0)
                stringBuilder.Append("Thirst Quench: " + consumable.thirstQuench + "%\n");

            // Nourishment
            if (consumable.nourishment > 0)
                stringBuilder.Append("Nourishment: " + consumable.nourishment + "%\n");

            stringBuilder.Append("\n");
        }
        else if (itemData.item.IsBag())
        {
            Bag bag = (Bag)itemData.item;

            // Weight
            stringBuilder.Append("Inventory Weight: " + itemData.bagInventory.currentWeight.ToString("#0.00") + " / " + bag.maxWeight + "\n");

            // Volume
            stringBuilder.Append("Inventory Volume: " + itemData.bagInventory.currentVolume.ToString("#0.00") + " / " + bag.maxVolume + "\n\n");

            // Single item volume limit
            if (bag.singleItemVolumeLimit > 0)
                stringBuilder.Append("Single item volume limit: " + bag.singleItemVolumeLimit + "\n");

            stringBuilder.Append("\n");
        }
        else if (itemData.item.IsPortableContainer())
        {
            PortableContainer portableContainer = (PortableContainer)itemData.item;

            // Weight
            stringBuilder.Append("Inventory Weight: " + itemData.bagInventory.currentWeight.ToString("#0.00") + " / " + portableContainer.maxWeight + "\n");

            // Volume
            stringBuilder.Append("Inventory Volume: " + itemData.bagInventory.currentVolume.ToString("#0.00") + " / " + portableContainer.maxVolume + "\n\n");

            // Single item volume limit
            if (portableContainer.singleItemVolumeLimit > 0)
                stringBuilder.Append("Single item volume limit: " + portableContainer.singleItemVolumeLimit + "\n");

            stringBuilder.Append("\n");
        }
        else if (itemData.item.IsKey())
        {
            // Perhaps put which door it goes to if the player knows? (And if not, maybe just put "???" for the door name?)
        }

        if (itemData.item.IsEquipment())
        {
            // Durability
            if (itemData.maxDurability > 0)
                stringBuilder.Append("Durability: " + itemData.durability + " / " + itemData.maxDurability + "\n\n");
        }

        // Weight/volume
        stringBuilder.Append("Weight: " + itemData.item.weight + "\n");
        stringBuilder.Append("Volume: " + itemData.item.volume + "\n\n");

        // Value
        stringBuilder.Append("Estimated Value: " + itemData.value + " gold");
    }
}

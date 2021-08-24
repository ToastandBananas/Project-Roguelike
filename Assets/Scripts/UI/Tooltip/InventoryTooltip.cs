using UnityEngine;

public class InventoryTooltip : Tooltip
{
    public override void BuildTooltip(ItemData itemData)
    {
        stringBuilder.Clear();

        // Item name
        stringBuilder.Append("<b><size=26>" + Utilities.FormatStringIntoParagraph(itemData.GetItemName(itemData.currentStackSize), 18) + "</size></b>\n");

        if (PlayerEquipmentManager.instance.ItemIsEquipped(itemData))
            stringBuilder.Append("<i><size=23>(Equipped)</size></i>\n");

        // Item material
        stringBuilder.Append("<i>" + Utilities.FormatEnumStringWithSpaces(itemData.item.mainMaterial.ToString(), false) + "</i>");

        if (itemData.item.IsWeapon())
        {
            Weapon weapon = (Weapon)itemData.item;
            
            // One/two handed
            if (weapon.IsTwoHanded(PlayerManager.instance))
                stringBuilder.Append(" <i>Two-Handed ");
            else
                stringBuilder.Append(" <i>One-Handed ");

            stringBuilder.Append(Utilities.FormatEnumStringWithSpaces(weapon.weaponType.ToString(), false) + "</i>\n");
        }
        else
            stringBuilder.Append("\n");

        stringBuilder.Append("\n");

        // Description
        if (itemData.item.description != "")
            stringBuilder.Append(Utilities.FormatStringIntoParagraph(itemData.item.description, 30) + "\n\n");

        if (itemData.item.canUsePartial)
            stringBuilder.Append("Remaining: " + itemData.percentRemaining + "%\n\n");
        
        if (itemData.item.IsWeapon()) 
        {
            Weapon weapon = (Weapon)itemData.item;

            // Attack range
            stringBuilder.Append("Attack Range: " + weapon.attackRange + "\n\n");

            // Damage
            stringBuilder.Append("Attack Power: " + GetAttackPower(itemData, weapon) + "\n");

            // Block chance multiplier
            stringBuilder.Append("Block Chance Multiplier: " + itemData.blockChanceMultiplier + "\n\n");
        }
        else if (itemData.item.IsShield())
        {
            // Block chance multiplier
            stringBuilder.Append("Block Chance Multiplier: " + itemData.blockChanceMultiplier + "\n");

            // Shield bash damage
            stringBuilder.Append("Shield Bash Damage: " + itemData.shieldBashDamage + "\n\n");
        }
        else if (itemData.item.IsWearable())
        {
            Wearable wearable = (Wearable)itemData.item;

            // Defense
            if (itemData.primaryDefense > 0)
                stringBuilder.Append(GetDefenseHeader(wearable, wearable.primaryBodyPartsCovered) + " Defense: " + itemData.primaryDefense + "\n");

            if (itemData.secondaryDefense > 0)
                stringBuilder.Append(GetDefenseHeader(wearable, wearable.secondaryBodyPartsCovered) + " Defense: " + itemData.secondaryDefense + "\n");

            if (itemData.tertiaryDefense > 0)
                stringBuilder.Append(GetDefenseHeader(wearable, wearable.tertiaryBodyPartsCovered) + " Defense: " + itemData.tertiaryDefense + "\n");

            if (itemData.primaryDefense > 0)
                stringBuilder.Append("\n");

            // Cold resistance
            if (wearable.coldResistance > 0)
                stringBuilder.Append("Cold Resistance: " + wearable.coldResistance + "° F\n");

            // Heat resistance
            if (wearable.heatResistance > 0)
                stringBuilder.Append("Heat Resistance: " + wearable.heatResistance + "° F\n");

            if (wearable.coldResistance > 0 || wearable.heatResistance > 0)
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
            stringBuilder.Append("Inventory Weight: " + itemData.bagInventory.currentWeight + " / " + bag.maxWeight + "\n");

            // Volume
            stringBuilder.Append("Inventory Volume: " + itemData.bagInventory.currentVolume + " / " + bag.maxVolume + "\n\n");

            // Single item volume limit
            if (bag.singleItemVolumeLimit > 0)
                stringBuilder.Append("Single item volume limit: " + bag.singleItemVolumeLimit + "\n");

            stringBuilder.Append("\n");
        }
        else if (itemData.item.IsPortableContainer())
        {
            PortableContainer portableContainer = (PortableContainer)itemData.item;

            // Weight
            stringBuilder.Append("Inventory Weight: " + itemData.bagInventory.currentWeight + " / " + portableContainer.maxWeight + "\n");

            // Volume
            stringBuilder.Append("Inventory Volume: " + itemData.bagInventory.currentVolume + " / " + portableContainer.maxVolume + "\n\n");

            // Single item volume limit
            if (portableContainer.singleItemVolumeLimit > 0)
                stringBuilder.Append("Single item volume limit: " + portableContainer.singleItemVolumeLimit + "\n");

            stringBuilder.Append("\n");
        }
        else if (itemData.item.IsMedicalSupply())
        {
            MedicalSupply medicalSupply = (MedicalSupply)itemData.item;

            // Medical quality
            stringBuilder.Append("Quality: " + medicalSupply.quality.ToString("#0.00") + "\n");

            // Soilage
            stringBuilder.Append("Condition: " + itemData.GetSoilageText() + "\n");

            stringBuilder.Append("\n");
        }
        else if (itemData.item.IsKey())
        {
            // Perhaps put which door it goes to if the player knows? (And if not, maybe just put "???" for the door name?)
            stringBuilder.Append("Unlocks: ???");
        }

        if (itemData.item.IsEquipment())
        {
            // Durability
            if (itemData.maxDurability > 0)
                stringBuilder.Append("Condition: " + GetDurabilityText(itemData) + "\n\n");
        }
        
        if (itemData.item.IsWearable() && itemData.pocketsVolume > 0)
            stringBuilder.Append("Pockets' Capacity: " + itemData.pocketsVolume + "\n\n");

        // Weight/volume
        stringBuilder.Append("Weight: " + itemData.item.weight * itemData.GetPercentRemaining_Decimal() + "\n");
        stringBuilder.Append("Volume: " + itemData.item.volume * itemData.GetPercentRemaining_Decimal() + "\n\n");

        // Value
        stringBuilder.Append("Estimated Value: " + itemData.value + " gold");
    }

    int GetAttackPower(ItemData itemData, Weapon weapon)
    {
        if (weapon.defaultMeleeAttackType == MeleeAttackType.Swipe)
            return itemData.bluntDamage_Swipe + itemData.pierceDamage_Swipe + itemData.slashDamage_Swipe + itemData.cleaveDamage_Swipe;
        else if (weapon.defaultMeleeAttackType == MeleeAttackType.Thrust)
            return itemData.bluntDamage_Thrust + itemData.pierceDamage_Thrust + itemData.slashDamage_Thrust + itemData.cleaveDamage_Thrust;
        else if (weapon.defaultMeleeAttackType == MeleeAttackType.Overhead)
            return itemData.bluntDamage_Overhead + itemData.pierceDamage_Overhead + itemData.slashDamage_Overhead + itemData.cleaveDamage_Overhead;
        return 0;
    }

    string GetDefenseHeader(Wearable wearable, BodyPartType[] bodyPartsCovered)
    {
        if (bodyPartsCovered.Length == 0)
        {
            if (bodyPartsCovered == wearable.primaryBodyPartsCovered)
                Debug.LogError("PrimaryBodyPartsCovered has not been assigned any BodyPartTypes, but there are corresponding defensive values assigned. Fix me!");
            else if (bodyPartsCovered == wearable.secondaryBodyPartsCovered)
                Debug.LogError("SecondaryBodyPartsCovered has not been assigned any BodyPartTypes, but there are corresponding defensive values assigned. Fix me!");
            else if (bodyPartsCovered == wearable.tertiaryBodyPartsCovered)
                Debug.LogError("TertiaryBodyPartsCovered has not been assigned any BodyPartTypes, but there are corresponding defensive values assigned. Fix me!");
        }
        else if (bodyPartsCovered.Length == 1)
            return bodyPartsCovered[0].ToString();
        else
        {
            if (bodyPartsCovered[0] == BodyPartType.LeftArm || bodyPartsCovered[0] == BodyPartType.RightArm)
                return "Arms";
            else if (bodyPartsCovered[0] == BodyPartType.LeftLeg || bodyPartsCovered[0] == BodyPartType.RightLeg)
                return "Legs";
            else if (bodyPartsCovered[0] == BodyPartType.LeftHand || bodyPartsCovered[0] == BodyPartType.RightHand)
                return "Hands";
            else if (bodyPartsCovered[0] == BodyPartType.LeftFoot || bodyPartsCovered[0] == BodyPartType.RightFoot)
                return "Foot";
        }
        return "";
    }

    string GetDurabilityText(ItemData itemData)
    {
        if (itemData.durability > itemData.maxDurability)
            return "Reinforced";
        else if (itemData.durability == itemData.maxDurability)
            return "Pristine";
        else
        {
            ItemMaterial mat = itemData.item.mainMaterial;
            if (mat == ItemMaterial.Linen || mat == ItemMaterial.QuiltedLinen || mat == ItemMaterial.Cotton || mat == ItemMaterial.Wool || mat == ItemMaterial.QuiltedWool || mat == ItemMaterial.Silk
                || mat == ItemMaterial.Hemp || mat == ItemMaterial.Fur)
            {
                if ((itemData.durability / itemData.maxDurability) >= 0.9f)
                    return "Lightly Worn";
                else if ((itemData.durability / itemData.maxDurability) >= 0.675f)
                    return "Ripped";
                else if ((itemData.durability / itemData.maxDurability) >= 0.45f)
                    return "Torn";
                else if ((itemData.durability / itemData.maxDurability) >= 0.225f)
                    return "Ragged";
                else if ((itemData.durability / itemData.maxDurability) > 0f)
                    return "Threadbare";
                else
                    return "Shredded";
            }
            else if (mat == ItemMaterial.Bone || mat == ItemMaterial.Chitin || mat == ItemMaterial.Keratin || mat == ItemMaterial.Wood || mat == ItemMaterial.Bark)
            {
                if ((itemData.durability / itemData.maxDurability) >= 0.9f)
                    return "Lightly Scratched";
                else if ((itemData.durability / itemData.maxDurability) >= 0.675f)
                    return "Scratched";
                else if ((itemData.durability / itemData.maxDurability) >= 0.45f)
                    return "Chipped";
                else if ((itemData.durability / itemData.maxDurability) >= 0.225f)
                    return "Cracked";
                else if ((itemData.durability / itemData.maxDurability) > 0f)
                    return "Fragmented";
                else
                    return "Shattered";
            }
            else if (mat == ItemMaterial.Rawhide || mat == ItemMaterial.SoftLeather || mat == ItemMaterial.HardLeather)
            {
                if ((itemData.durability / itemData.maxDurability) >= 0.9f)
                    return "Lightly Scratched";
                else if ((itemData.durability / itemData.maxDurability) >= 0.675f)
                    return "Scratched";
                else if ((itemData.durability / itemData.maxDurability) >= 0.45f)
                    return "Cut";
                else if ((itemData.durability / itemData.maxDurability) >= 0.225f)
                    return "Tattered";
                else if ((itemData.durability / itemData.maxDurability) > 0f)
                    return "Decaying";
                else
                    return "Dilapidated";
            }
            else if (mat == ItemMaterial.Copper || mat == ItemMaterial.Bronze || mat == ItemMaterial.Iron || mat == ItemMaterial.Brass || mat == ItemMaterial.Steel 
                || mat == ItemMaterial.Mithril || mat == ItemMaterial.Dragonscale)
            {
                if (itemData.item.IsWearable() || itemData.item.IsShield())
                {
                    if ((itemData.durability / itemData.maxDurability) >= 0.9f)
                        return "Lightly Dented";
                    else if ((itemData.durability / itemData.maxDurability) >= 0.675f)
                        return "Dented";
                    else if ((itemData.durability / itemData.maxDurability) >= 0.45f)
                        return "Cracked";
                    else if ((itemData.durability / itemData.maxDurability) >= 0.225f)
                        return "Crushed";
                    else if ((itemData.durability / itemData.maxDurability) > 0f)
                        return "Pulverized";
                    else
                        return "Destroyed";
                }
                else if (itemData.item.IsWeapon())
                {
                    if ((itemData.durability / itemData.maxDurability) >= 0.9f)
                        return "Lightly Scratched";
                    else if ((itemData.durability / itemData.maxDurability) >= 0.675f)
                        return "Scratched";
                    else if ((itemData.durability / itemData.maxDurability) >= 0.45f)
                        return "Notched";
                    else if ((itemData.durability / itemData.maxDurability) >= 0.225f)
                        return "Bent";
                    else if ((itemData.durability / itemData.maxDurability) > 0f)
                        return "Warped";
                    else
                        return "Broken";
                }
            }
        }

        Debug.LogWarning(itemData.item.mainMaterial + " material not accounted for. Fix me!");
        return "";
    }
}

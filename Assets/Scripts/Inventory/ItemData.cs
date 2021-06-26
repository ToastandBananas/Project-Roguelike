using System.Collections;
using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Unique }

/// <summary> This class will read data from its Item scriptable object, potentially randomize some of the stats and then store the new data here. </summary>
public class ItemData : MonoBehaviour
{
    public bool hasBeenRandomized = false;

    [Header("Item Class")]
    public Item item;

    [Header("General Data")]
    public string itemName;
    public int value;
    public int currentStackSize = 1;
    public int maxDurability;
    public float durability;

    [Header("Weapon Data")]
    public int damage;
    public int treeDamage;

    [Header("Armor Data")]
    public int defense;

    [Header("Shield Data")]
    public int shieldBashDamage;

    [Header("Consumable Data")]
    public int freshness = 100;
    public int uses = 1;

    void Awake()
    {
        if (hasBeenRandomized)
            value = CalculateItemValue();
        else if (item != null)
            RandomizeData();
    }

    public void TransferData(ItemData dataGiver, ItemData dataReceiver)
    {
        // Randomization Data
        dataReceiver.hasBeenRandomized = dataGiver.hasBeenRandomized;

        // Item Class
        dataReceiver.item = dataGiver.item;

        // General Data
        dataReceiver.itemName = dataGiver.itemName;
        dataReceiver.value = dataGiver.value;
        dataReceiver.currentStackSize = dataGiver.currentStackSize;
        dataReceiver.maxDurability = dataGiver.maxDurability;
        dataReceiver.durability = dataGiver.durability;

        // Consumable Data
        dataReceiver.freshness = dataGiver.freshness;
        dataReceiver.uses = dataGiver.uses;

        // Weapon Data
        dataReceiver.damage = dataGiver.damage;
        dataReceiver.treeDamage = dataGiver.treeDamage;

        // Armor Data
        dataReceiver.defense = dataGiver.defense;

        // Shield Data
        dataReceiver.shieldBashDamage = dataGiver.shieldBashDamage;
    }

    public IEnumerator TransferDataWithDelay(ItemData dataGiver, ItemData dataReceiver)
    {
        yield return new WaitForSeconds(0.1f);
        TransferData(dataGiver, dataReceiver);
    }

    public void SwapData(ItemData dataSet1, ItemData dataSet2)
    {
        ItemData tempData1 = new ItemData(); // Our temporary ItemData classes
        ItemData tempData2 = new ItemData();

        TransferData(dataSet1, tempData1); // Transfer data to temp1 from dataSet1
        TransferData(dataSet2, tempData2); // Transfer data to temp2 from dataSet2

        TransferData(tempData1, dataSet2); // Transfer data to dataSet2 from tempData1
        TransferData(tempData2, dataSet1); // Transfer data to dataSet1 from tempData2
    }

    public void RandomizeData()
    {
        // Item class data
        itemName = item.name;

        // Equipment class data
        if (item.IsEquipment())
        {
            Equipment equipment = (Equipment)item;
            if (equipment.maxBaseDurability > 0)
                maxDurability = Random.Range(equipment.minBaseDurability, equipment.maxBaseDurability + 1);

            durability = maxDurability;

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                damage = Random.Range(weapon.minBaseDamage, weapon.maxBaseDamage + 1);
                treeDamage = Random.Range(weapon.minBaseTreeDamage, weapon.maxBaseTreeDamage + 1);
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                defense = Random.Range(wearable.minBaseDefense, wearable.maxBaseDefense + 1);
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                defense = Random.Range(shield.minBaseDefense, shield.maxBaseDefense + 1);
                shieldBashDamage = Random.Range(shield.minShieldBashDamage, shield.maxShieldBashDamage + 1);
            }

            if (equipment.maxStackSize > 1)
                currentStackSize = Random.Range(1, equipment.maxStackSize + 1);
        }
        // Consumable class data
        else if (item.IsConsumable())
        {
            Consumable consumable = (Consumable)item;

            freshness = Random.Range(consumable.minBaseFreshness, consumable.maxBaseFreshness + 1);

            if (consumable.maxUses > 1)
            {
                int randomNum = Random.Range(1, 3);
                if (randomNum == 1)
                    uses = consumable.maxUses;
                else
                {
                    // Choose a random use amount
                    randomNum = Random.Range(1, consumable.maxUses + 1);
                    uses = randomNum;
                }
            }
        }

        value = CalculateItemValue();

        hasBeenRandomized = true;
    }

    public IEnumerator RandomizeDataWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        RandomizeData();
    }

    public void ClearData()
    {
        hasBeenRandomized = false;
        
        item = null;
        
        itemName = "";
        value = 0;
        currentStackSize = 1;
        maxDurability = 0;
        durability = 0;
        
        damage = 0;
        treeDamage = 0;
        shieldBashDamage = 0;
        defense = 0;
        freshness = 0;
        uses = 0;
}

    int CalculateItemValue()
    {
        int itemValue = 0;

        if ((item.IsEquipment() || item.IsConsumable()) && item.minBaseValue != 0 && item.maxBaseValue != 0)
            itemValue = Mathf.RoundToInt(item.minBaseValue + ((item.maxBaseValue - item.minBaseValue) * CalculatePercentPointValue()));
        else
            itemValue = item.staticValue;
        
        return itemValue;
    }

    float GetTotalPointValue()
    {
        // Add up all the possible points that can be added to our stats when randomized (damage, defense, etc)
        float totalPointsPossible = 0;

        if (item.IsEquipment())
        {
            Equipment equipment = (Equipment)item;
            if (equipment.maxBaseDurability > 0)
                totalPointsPossible += (equipment.maxBaseDurability - equipment.minBaseDurability);

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                totalPointsPossible += (weapon.maxBaseDamage - weapon.minBaseDamage) * 2;
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                totalPointsPossible += (wearable.maxBaseDefense - wearable.minBaseDefense) * 2;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                totalPointsPossible += (shield.maxBaseDefense - shield.minBaseDefense) * 2;
                totalPointsPossible += (shield.maxShieldBashDamage - shield.minShieldBashDamage);
            }
        }
        else if (item.IsConsumable())
        {
            Consumable consumable = (Consumable)item;
            if (consumable.consumableType == ConsumableType.Food)
                totalPointsPossible += (consumable.maxBaseFreshness - consumable.minBaseFreshness);

            if (consumable.maxUses > 1)
                totalPointsPossible += (consumable.maxUses);
        }
        
        return totalPointsPossible;
    }

    float CalculatePercentPointValue()
    {
        // Calculate the percentage of points that were added to the item's stats when randomized (compared to the total possible points)
        float pointIncrease = 0; // Amount the stats have been increased by in relation to its base stat values, in total
        float percent = 0; // Percent of possible stat increase this item has

        ClampValues();

        if (item.IsEquipment())
        {
            Equipment equipment = (Equipment)item;
            if (equipment.maxBaseDurability > 0)
                pointIncrease += (maxDurability - equipment.minBaseDurability);

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                pointIncrease += (damage - weapon.minBaseDamage) * 2; // Damage contributes to value twice as much
                pointIncrease += (treeDamage - weapon.minBaseTreeDamage);
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                pointIncrease += (defense - wearable.minBaseDefense) * 2; // Defense contributes to value twice as much
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                pointIncrease += (defense - shield.minBaseDefense) * 2; // Defense contributes to value twice as much
                pointIncrease += (shieldBashDamage = shield.minShieldBashDamage);
            }
        }
        else if (item.IsConsumable())
        {
            Consumable consumable = (Consumable)item;
            if (consumable.consumableType == ConsumableType.Food)
                pointIncrease += (freshness - consumable.minBaseFreshness);

            if (consumable.maxUses > 1)
                pointIncrease += (uses);
        }

        percent = pointIncrease / GetTotalPointValue();

        return percent;
    }

    void ClampValues()
    {
        // This function just makes sure that our stats aren't too high or too low
        if (item.IsEquipment())
        {
            Equipment equipment = (Equipment)item;
            if (maxDurability > equipment.maxBaseDurability)
            {
                maxDurability = equipment.maxBaseDurability;
                durability = maxDurability;
            }
            else if (maxDurability < equipment.minBaseDurability)
            {
                maxDurability = equipment.minBaseDurability;
                durability = maxDurability;
            }

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                if (damage > weapon.maxBaseDamage)
                    damage = weapon.maxBaseDamage;
                else if (damage < weapon.minBaseDamage)
                    damage = weapon.minBaseDamage;
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                if (defense > wearable.maxBaseDefense)
                    defense = wearable.maxBaseDefense;
                else if (defense < wearable.minBaseDefense)
                    defense = wearable.minBaseDefense;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                if (defense > shield.maxBaseDefense)
                    defense = shield.maxBaseDefense;
                else if (defense < shield.minBaseDefense)
                    defense = shield.minBaseDefense;

                if (shieldBashDamage > shield.maxShieldBashDamage)
                    shieldBashDamage = shield.maxShieldBashDamage;
                else if (shieldBashDamage < shield.minShieldBashDamage)
                    shieldBashDamage = shield.minShieldBashDamage;
            }
        }
        else if (item.IsConsumable())
        {
            Consumable consumable = (Consumable)item;
            if (consumable.consumableType == ConsumableType.Food)
            {
                if (freshness > consumable.maxBaseFreshness)
                    freshness = consumable.maxBaseFreshness;
                else if (freshness < consumable.minBaseFreshness)
                    freshness = consumable.minBaseFreshness;

                if (uses > consumable.maxUses)
                    uses = consumable.maxUses;
                else if (uses < 0)
                    uses = 0;
            }
        }
    }
}

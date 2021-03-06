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
    public string namePrefix;
    public int value;
    public int currentStackSize = 1;
    public int maxDurability;
    public float durability;

    [Header("Weapon Data")]
    PhysicalDamageType swipeMainPhysicalDamageType;
    PhysicalDamageType thrustMainPhysicalDamageType, overheadMainPhysicalDamageType;
    public int bluntDamage_Swipe, pierceDamage_Swipe, slashDamage_Swipe, cleaveDamage_Swipe;             // Swipe
    public int bluntDamage_Thrust, pierceDamage_Thrust, slashDamage_Thrust, cleaveDamage_Thrust;         // Thrust
    public int bluntDamage_Overhead, pierceDamage_Overhead, slashDamage_Overhead, cleaveDamage_Overhead; // Overhead

    [Header("Armor Data")]
    public int primaryDefense;
    public int secondaryDefense, tertiaryDefense;
    public float pocketsVolume;

    [Header("Shield Data")]
    public int shieldBashDamage;
    public float blockChanceMultiplier;

    [Header("Consumable Data")]
    public float freshness = 100f;
    public int percentRemaining = 100;
    
    [HideInInspector] public Inventory bagInventory;
    [HideInInspector] public Inventory parentInventory;

    void Awake()
    {
        if (hasBeenRandomized)
        {
            value = CalculateItemValue();
            SetNamePrefix();
        }
        else if (item != null)
            RandomizeData();

        TryGetComponent(out bagInventory);
    }

    public void TransferData(ItemData dataGiver, ItemData dataReceiver)
    {
        // Randomization Data
        dataReceiver.hasBeenRandomized = dataGiver.hasBeenRandomized;

        // Item Class
        dataReceiver.item = dataGiver.item;

        // General Data
        dataReceiver.namePrefix = dataGiver.namePrefix;
        dataReceiver.value = dataGiver.value;
        dataReceiver.currentStackSize = dataGiver.currentStackSize;
        dataReceiver.maxDurability = dataGiver.maxDurability;
        dataReceiver.durability = dataGiver.durability;

        // Consumable Data
        dataReceiver.freshness = dataGiver.freshness;
        dataReceiver.percentRemaining = dataGiver.percentRemaining;

        // Weapon Data
        dataReceiver.bluntDamage_Swipe = dataGiver.bluntDamage_Swipe;
        dataReceiver.pierceDamage_Swipe = dataGiver.pierceDamage_Swipe;
        dataReceiver.slashDamage_Swipe = dataGiver.slashDamage_Swipe;
        dataReceiver.cleaveDamage_Swipe = dataGiver.cleaveDamage_Swipe;
        dataReceiver.bluntDamage_Thrust = dataGiver.bluntDamage_Thrust;
        dataReceiver.pierceDamage_Thrust = dataGiver.pierceDamage_Thrust;
        dataReceiver.slashDamage_Thrust = dataGiver.slashDamage_Thrust;
        dataReceiver.cleaveDamage_Thrust = dataGiver.cleaveDamage_Thrust;
        dataReceiver.bluntDamage_Overhead = dataGiver.bluntDamage_Overhead;
        dataReceiver.pierceDamage_Overhead = dataGiver.pierceDamage_Overhead;
        dataReceiver.slashDamage_Overhead = dataGiver.slashDamage_Overhead;
        dataReceiver.cleaveDamage_Overhead = dataGiver.cleaveDamage_Overhead;

        // Wearable Data
        dataReceiver.primaryDefense = dataGiver.primaryDefense;
        dataReceiver.secondaryDefense = dataGiver.secondaryDefense;
        dataReceiver.tertiaryDefense = dataGiver.tertiaryDefense;
        dataReceiver.pocketsVolume = dataGiver.pocketsVolume;

        // Shield Data
        dataReceiver.shieldBashDamage = dataGiver.shieldBashDamage;
        dataReceiver.blockChanceMultiplier = dataGiver.blockChanceMultiplier;
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
        // Equipment class data
        if (item.IsEquipment())
        {
            Equipment equipment = (Equipment)item;
            if (equipment.baseDurability.y > 0)
                maxDurability = Random.Range(equipment.baseDurability.x, equipment.baseDurability.y + 1);

            RandomizeDurability();

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                bluntDamage_Swipe = Random.Range(weapon.bluntDamage_Swipe.x, weapon.bluntDamage_Swipe.y + 1);
                pierceDamage_Swipe = Random.Range(weapon.pierceDamage_Swipe.x, weapon.pierceDamage_Swipe.y + 1);
                slashDamage_Swipe = Random.Range(weapon.slashDamage_Swipe.x, weapon.slashDamage_Swipe.y + 1);
                cleaveDamage_Swipe = Random.Range(weapon.cleaveDamage_Swipe.x, weapon.cleaveDamage_Swipe.y + 1);
                bluntDamage_Thrust = Random.Range(weapon.bluntDamage_Thrust.x, weapon.bluntDamage_Thrust.y + 1);
                pierceDamage_Thrust = Random.Range(weapon.pierceDamage_Thrust.x, weapon.pierceDamage_Thrust.y + 1);
                slashDamage_Thrust = Random.Range(weapon.slashDamage_Thrust.x, weapon.slashDamage_Thrust.y + 1);
                cleaveDamage_Thrust = Random.Range(weapon.cleaveDamage_Thrust.x, weapon.cleaveDamage_Thrust.y + 1);
                bluntDamage_Overhead = Random.Range(weapon.bluntDamage_Overhead.x, weapon.bluntDamage_Overhead.y + 1);
                pierceDamage_Overhead = Random.Range(weapon.pierceDamage_Overhead.x, weapon.pierceDamage_Overhead.y + 1);
                slashDamage_Overhead = Random.Range(weapon.slashDamage_Overhead.x, weapon.slashDamage_Overhead.y + 1);
                cleaveDamage_Overhead = Random.Range(weapon.cleaveDamage_Overhead.x, weapon.cleaveDamage_Overhead.y + 1);

                blockChanceMultiplier = Mathf.RoundToInt(Random.Range(weapon.minBlockChanceMultiplier, weapon.maxBlockChanceMultiplier) * 100f) / 100f;

                SetMeleeAttackPhysicalDamageTypes();
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                primaryDefense = Random.Range(wearable.primaryDefense.x, wearable.primaryDefense.y + 1);
                secondaryDefense = Random.Range(wearable.secondaryDefense.x, wearable.secondaryDefense.y + 1);
                tertiaryDefense = Random.Range(wearable.tertiaryDefense.x, wearable.tertiaryDefense.y + 1);
                pocketsVolume = Mathf.RoundToInt(Random.Range(wearable.pocketsVolume.x, wearable.pocketsVolume.y) * 100f) / 100f;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                shieldBashDamage = Random.Range(shield.shieldBashDamage.x, shield.shieldBashDamage.y + 1);
                blockChanceMultiplier = Mathf.RoundToInt(Random.Range(shield.minBlockChanceMultiplier, shield.maxBlockChanceMultiplier) * 100f) / 100f;
            }
        }
        // Consumable class data
        else if (item.IsConsumable())
        {
            Consumable consumable = (Consumable)item;
            freshness = Random.Range(consumable.minBaseFreshness, consumable.maxBaseFreshness + 1);
        }

        if (item.maxStackSize > 1)
            currentStackSize = Random.Range(1, item.maxStackSize + 1);

        if (item.canUsePartial && currentStackSize == 1)
        {
            int randomNum = Random.Range(1, 3);
            if (randomNum == 1)
                percentRemaining = 100;
            else
            {
                // Choose a random use amount
                randomNum = Random.Range(1, 100 + 1);
                percentRemaining = Mathf.RoundToInt(randomNum / 10) * 10;
            }
        }

        SetNamePrefix();

        value = CalculateItemValue();

        hasBeenRandomized = true;
    }

    public IEnumerator RandomizeDataWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        RandomizeData();
    }

    void RandomizeDurability()
    {
        int random = Random.Range(0, 10);
        if (random < 2)
            durability = maxDurability;
        else if (random < 4)
            durability = Mathf.RoundToInt(Random.Range(maxDurability * 0.8f, maxDurability * 0.99f) * 100f) / 100f;
        else if (random < 7)
            durability = Mathf.RoundToInt(Random.Range(maxDurability * 0.5f, maxDurability * 0.79f) * 100f) / 100f;
        else if (random < 9)
            durability = Mathf.RoundToInt(Random.Range(maxDurability * 0.3f, maxDurability * 0.49f) * 100f) / 100f;
        else
            durability = Mathf.RoundToInt(Random.Range(maxDurability * 0.15f, maxDurability * 0.29f) * 100f) / 100f;
    }

    /// <summary> This is used when dragging and dropping an item onto another item of the same type and stats (as long as they are stackable of course) </summary>
    public void AddToItemsStack(InventoryItem invItemTakingFrom)
    {
        int invItemTakingFromStackSize = invItemTakingFrom.itemData.currentStackSize;
        int amountAdded = 0;
        for (int j = 0; j < invItemTakingFromStackSize; j++)
        {
            if (currentStackSize < item.maxStackSize)
            {
                amountAdded++;
                currentStackSize++;
                invItemTakingFrom.itemData.currentStackSize--;
            }
            else
                break;
        }

        invItemTakingFrom.UpdateItemNumberTexts();
    }

    public bool StackableItemsDataIsEqual(ItemData itemData1, ItemData itemData2)
    {
        if (itemData1.item == itemData2.item && itemData1.value == itemData2.value && itemData1.freshness == itemData2.freshness && itemData1.percentRemaining == itemData2.percentRemaining)
            return true;

        return false;
    }

    public void UsePartial(PartialAmount partialAmount)
    {
        percentRemaining -= item.GetPartialAmountsPercentage(partialAmount);
        if (percentRemaining <= 0)
        {
            if (currentStackSize > 1)
                currentStackSize--;
            else
                RemoveItemData();
        }
        else
            SetNamePrefix();
    }

    public void RemoveItemData()
    {
        // Remove the item from the ItemDatas dictionary if it was on the ground
        if (IsPickup())
            GameTiles.RemoveItemData(this, transform.position);

        InventoryItem itemDatasInvItem = GetItemDatasInventoryItem();
        if (parentInventory != null) // If using an item that's inside and inventory
        {
            // Remove it from the inventory
            item.RemoveFromInventory(parentInventory, itemDatasInvItem, this, currentStackSize);
        }
        else if (itemDatasInvItem != null && itemDatasInvItem.myEquipmentManager == null) // If using an item that was on the ground
        {
            itemDatasInvItem.gm.containerInvUI.GetItemsListFromActiveDirection().Remove(itemDatasInvItem.itemData);
            itemDatasInvItem.ClearItem();
        }
        else
            GameManager.instance.StartCoroutine(DelayReturnToObjectPool());
    }

    public void ClearData()
    {
        hasBeenRandomized = false;
        parentInventory = null;
        
        item = null;
        
        namePrefix = "";
        value = 0;
        currentStackSize = 0;
        maxDurability = 0;
        durability = 0;
        
        bluntDamage_Swipe = 0;
        pierceDamage_Swipe = 0;
        slashDamage_Swipe = 0;
        cleaveDamage_Swipe = 0;
        bluntDamage_Thrust = 0;
        pierceDamage_Thrust = 0;
        slashDamage_Thrust = 0;
        cleaveDamage_Thrust = 0;
        bluntDamage_Overhead = 0;
        pierceDamage_Overhead = 0;
        slashDamage_Overhead = 0;
        cleaveDamage_Overhead = 0;

        shieldBashDamage = 0;
        blockChanceMultiplier = 0;

        primaryDefense = 0;
        secondaryDefense = 0;
        tertiaryDefense = 0;
        pocketsVolume = 0;

        freshness = 0;
        percentRemaining = 100;
}

    int CalculateItemValue()
    {
        int itemValue = 0;
        if (item.value.x != 0 && item.value.y != 0)
            itemValue = Mathf.RoundToInt(item.value.x + ((item.value.y - item.value.x) * CalculatePercentPointValue()));
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
            if (equipment.baseDurability.y > 0)
                totalPointsPossible += (equipment.baseDurability.y - equipment.baseDurability.x);

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                totalPointsPossible += ((weapon.bluntDamage_Swipe.y + weapon.pierceDamage_Swipe.y + weapon.slashDamage_Swipe.y + weapon.cleaveDamage_Swipe.y 
                    + weapon.bluntDamage_Thrust.y + weapon.pierceDamage_Thrust.y + weapon.slashDamage_Thrust.y + weapon.cleaveDamage_Thrust.y 
                    + weapon.bluntDamage_Overhead.y + weapon.pierceDamage_Overhead.y + weapon.slashDamage_Overhead.y + weapon.cleaveDamage_Overhead.y) 
                    - (weapon.bluntDamage_Swipe.x + weapon.pierceDamage_Swipe.x + weapon.slashDamage_Swipe.x + weapon.cleaveDamage_Swipe.x 
                    + weapon.bluntDamage_Thrust.x + weapon.pierceDamage_Thrust.x + weapon.slashDamage_Thrust.x + weapon.cleaveDamage_Thrust.x 
                    + weapon.bluntDamage_Overhead.x + weapon.pierceDamage_Overhead.x + weapon.slashDamage_Overhead.x + weapon.cleaveDamage_Overhead.x)) * 2;

                totalPointsPossible += (weapon.maxBlockChanceMultiplier - weapon.minBlockChanceMultiplier) * 10;
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                totalPointsPossible += (wearable.primaryDefense.y - wearable.primaryDefense.x) * 2;
                totalPointsPossible += (wearable.secondaryDefense.y - wearable.secondaryDefense.x) * 2;
                totalPointsPossible += (wearable.tertiaryDefense.y - wearable.tertiaryDefense.x) * 2;
                totalPointsPossible += (wearable.pocketsVolume.y - wearable.pocketsVolume.x) * 2;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                totalPointsPossible += (shield.shieldBashDamage.y - shield.shieldBashDamage.x);
                totalPointsPossible += (shield.maxBlockChanceMultiplier - shield.minBlockChanceMultiplier) * 10;
            }
        }
        else if (item.IsConsumable())
        {
            Consumable consumable = (Consumable)item;
            if (consumable.consumableType == ConsumableType.Food)
                totalPointsPossible += (consumable.maxBaseFreshness - consumable.minBaseFreshness);
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
            if (equipment.baseDurability.y > 0)
                pointIncrease += (maxDurability - equipment.baseDurability.x);

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                pointIncrease += ((bluntDamage_Swipe - weapon.bluntDamage_Swipe.x) + (pierceDamage_Swipe - weapon.pierceDamage_Swipe.x) + (slashDamage_Swipe - weapon.slashDamage_Swipe.x) + (cleaveDamage_Swipe - weapon.cleaveDamage_Swipe.x)
                    + (bluntDamage_Thrust - weapon.bluntDamage_Thrust.x) + (pierceDamage_Thrust - weapon.pierceDamage_Thrust.x) + (slashDamage_Thrust - weapon.slashDamage_Thrust.x) + (cleaveDamage_Thrust - weapon.cleaveDamage_Thrust.x)
                    + (bluntDamage_Overhead - weapon.bluntDamage_Overhead.x) + (pierceDamage_Overhead - weapon.pierceDamage_Overhead.x) + (slashDamage_Overhead - weapon.slashDamage_Overhead.x) + (cleaveDamage_Overhead - weapon.cleaveDamage_Overhead.x)) * 2; // Damage contributes to value twice as much
                pointIncrease += (blockChanceMultiplier - weapon.minBlockChanceMultiplier) * 10;
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                pointIncrease += (primaryDefense - wearable.primaryDefense.x) * 2; // Defense contributes to value twice as much
                pointIncrease += (secondaryDefense - wearable.secondaryDefense.x) * 2;
                pointIncrease += (tertiaryDefense - wearable.tertiaryDefense.x) * 2;
                pointIncrease += (pocketsVolume - wearable.pocketsVolume.x) * 2;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                pointIncrease += (shieldBashDamage - shield.shieldBashDamage.x);
                pointIncrease += (blockChanceMultiplier - shield.minBlockChanceMultiplier) * 10;
            }
        }
        else if (item.IsConsumable())
        {
            Consumable consumable = (Consumable)item;
            if (consumable.consumableType == ConsumableType.Food)
                pointIncrease += (freshness - consumable.minBaseFreshness);
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
            if (maxDurability > equipment.baseDurability.y)
                maxDurability = equipment.baseDurability.y;
            else if (maxDurability < equipment.baseDurability.x)
                maxDurability = equipment.baseDurability.x;

            if (durability > maxDurability)
                durability = maxDurability;

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                if (bluntDamage_Swipe > weapon.bluntDamage_Swipe.y)
                    bluntDamage_Swipe = weapon.bluntDamage_Swipe.y;
                else if (bluntDamage_Swipe < weapon.bluntDamage_Swipe.x)
                    bluntDamage_Swipe = weapon.bluntDamage_Swipe.x;

                if (pierceDamage_Swipe > weapon.pierceDamage_Swipe.y)
                    pierceDamage_Swipe = weapon.pierceDamage_Swipe.y;
                else if (pierceDamage_Swipe < weapon.pierceDamage_Swipe.x)
                    pierceDamage_Swipe = weapon.pierceDamage_Swipe.x;

                if (slashDamage_Swipe > weapon.slashDamage_Swipe.y)
                    slashDamage_Swipe = weapon.slashDamage_Swipe.y;
                else if (slashDamage_Swipe < weapon.slashDamage_Swipe.x)
                    slashDamage_Swipe = weapon.slashDamage_Swipe.x;

                if (cleaveDamage_Swipe > weapon.cleaveDamage_Swipe.y)
                    cleaveDamage_Swipe = weapon.cleaveDamage_Swipe.y;
                else if (cleaveDamage_Swipe < weapon.cleaveDamage_Swipe.x)
                    cleaveDamage_Swipe = weapon.cleaveDamage_Swipe.x;

                if (bluntDamage_Thrust > weapon.bluntDamage_Thrust.y)
                    bluntDamage_Thrust = weapon.bluntDamage_Thrust.y;
                else if (bluntDamage_Thrust < weapon.bluntDamage_Thrust.x)
                    bluntDamage_Thrust = weapon.bluntDamage_Thrust.x;

                if (pierceDamage_Thrust > weapon.pierceDamage_Thrust.y)
                    pierceDamage_Thrust = weapon.pierceDamage_Thrust.y;
                else if (pierceDamage_Thrust < weapon.pierceDamage_Thrust.x)
                    pierceDamage_Thrust = weapon.pierceDamage_Thrust.x;

                if (slashDamage_Thrust > weapon.slashDamage_Thrust.y)
                    slashDamage_Thrust = weapon.slashDamage_Thrust.y;
                else if (slashDamage_Thrust < weapon.slashDamage_Thrust.x)
                    slashDamage_Thrust = weapon.slashDamage_Thrust.x;

                if (cleaveDamage_Thrust > weapon.cleaveDamage_Thrust.y)
                    cleaveDamage_Thrust = weapon.cleaveDamage_Thrust.y;
                else if (cleaveDamage_Thrust < weapon.cleaveDamage_Thrust.x)
                    cleaveDamage_Thrust = weapon.cleaveDamage_Thrust.x;

                if (bluntDamage_Overhead > weapon.bluntDamage_Overhead.y)
                    bluntDamage_Overhead = weapon.bluntDamage_Overhead.y;
                else if (bluntDamage_Overhead < weapon.bluntDamage_Overhead.x)
                    bluntDamage_Overhead = weapon.bluntDamage_Overhead.x;

                if (pierceDamage_Overhead > weapon.pierceDamage_Overhead.y)
                    pierceDamage_Overhead = weapon.pierceDamage_Overhead.y;
                else if (pierceDamage_Overhead < weapon.pierceDamage_Overhead.x)
                    pierceDamage_Overhead = weapon.pierceDamage_Overhead.x;

                if (slashDamage_Overhead > weapon.slashDamage_Overhead.y)
                    slashDamage_Overhead = weapon.slashDamage_Overhead.y;
                else if (slashDamage_Overhead < weapon.slashDamage_Overhead.x)
                    slashDamage_Overhead = weapon.slashDamage_Overhead.x;

                if (cleaveDamage_Overhead > weapon.cleaveDamage_Overhead.y)
                    cleaveDamage_Overhead = weapon.cleaveDamage_Overhead.y;
                else if (cleaveDamage_Overhead < weapon.cleaveDamage_Overhead.x)
                    cleaveDamage_Overhead = weapon.cleaveDamage_Overhead.x;

                if (blockChanceMultiplier > weapon.maxBlockChanceMultiplier)
                    blockChanceMultiplier = weapon.maxBlockChanceMultiplier;
                else if (blockChanceMultiplier < weapon.minBlockChanceMultiplier)
                    blockChanceMultiplier = weapon.minBlockChanceMultiplier;
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                if (primaryDefense > wearable.primaryDefense.y)
                    primaryDefense = wearable.primaryDefense.y;
                else if (primaryDefense < wearable.primaryDefense.x)
                    primaryDefense = wearable.primaryDefense.x;

                if (secondaryDefense > wearable.secondaryDefense.y)
                    secondaryDefense = wearable.secondaryDefense.y;
                else if (secondaryDefense < wearable.secondaryDefense.x)
                    secondaryDefense = wearable.secondaryDefense.x;

                if (tertiaryDefense > wearable.tertiaryDefense.y)
                    tertiaryDefense = wearable.tertiaryDefense.y;
                else if (tertiaryDefense < wearable.tertiaryDefense.x)
                    tertiaryDefense = wearable.tertiaryDefense.x;

                if (pocketsVolume > wearable.pocketsVolume.y)
                    pocketsVolume = wearable.pocketsVolume.y;
                else if (pocketsVolume < wearable.pocketsVolume.x)
                    pocketsVolume = wearable.pocketsVolume.x;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                if (shieldBashDamage > shield.shieldBashDamage.y)
                    shieldBashDamage = shield.shieldBashDamage.y;
                else if (shieldBashDamage < shield.shieldBashDamage.x)
                    shieldBashDamage = shield.shieldBashDamage.x;

                if (blockChanceMultiplier > shield.maxBlockChanceMultiplier)
                    blockChanceMultiplier = shield.maxBlockChanceMultiplier;
                else if (blockChanceMultiplier < shield.minBlockChanceMultiplier)
                    blockChanceMultiplier = shield.minBlockChanceMultiplier;
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
            }
        }

        if (item.canUsePartial)
        {
            if (percentRemaining > 100)
                percentRemaining = 100;
            else if (percentRemaining < 10)
                percentRemaining = 10;
        }
    }

    public void DamageDurability()
    {
        DamageDurability(Random.Range(0.2f, 1f));
    }

    public void DamageDurability(float damageAmount)
    {
        durability -= damageAmount;
        durability = Mathf.RoundToInt(durability * 100f) / 100f;

        if (durability <= 0)
        {
            durability = 0;
            SetNamePrefix();
        }
    }

    public void DamageDurability(int damageAmount, bool wearablePenetrated)
    {
        if (wearablePenetrated)
            durability -= damageAmount / Random.Range(3f, 5f);
        else
            durability -= damageAmount / Random.Range(9f, 11f);

        durability = Mathf.RoundToInt(durability * 100f) / 100f;

        if (durability <= 0)
        {
            durability = 0;
            SetNamePrefix();
        }
    }

    void SetMeleeAttackPhysicalDamageTypes()
    {
        // Swipe
        if (pierceDamage_Swipe >= bluntDamage_Swipe && pierceDamage_Swipe >= slashDamage_Swipe && pierceDamage_Swipe >= cleaveDamage_Swipe)
            swipeMainPhysicalDamageType = PhysicalDamageType.Pierce;
        else if (cleaveDamage_Swipe >= bluntDamage_Swipe && cleaveDamage_Swipe >= pierceDamage_Swipe && cleaveDamage_Swipe >= slashDamage_Swipe)
            swipeMainPhysicalDamageType = PhysicalDamageType.Cleave;
        else if (slashDamage_Swipe >= bluntDamage_Swipe && slashDamage_Swipe >= pierceDamage_Swipe && slashDamage_Swipe >= cleaveDamage_Swipe)
            swipeMainPhysicalDamageType = PhysicalDamageType.Slash;
        else
            swipeMainPhysicalDamageType = PhysicalDamageType.Blunt;

        // Thrust
        if (pierceDamage_Thrust >= bluntDamage_Thrust && pierceDamage_Thrust >= slashDamage_Thrust && pierceDamage_Thrust >= cleaveDamage_Thrust)
            thrustMainPhysicalDamageType = PhysicalDamageType.Pierce;
        else if (cleaveDamage_Thrust >= bluntDamage_Thrust && cleaveDamage_Thrust >= pierceDamage_Thrust && cleaveDamage_Thrust >= slashDamage_Thrust)
            thrustMainPhysicalDamageType = PhysicalDamageType.Cleave;
        else if (slashDamage_Thrust >= bluntDamage_Thrust && slashDamage_Thrust >= pierceDamage_Thrust && slashDamage_Thrust >= cleaveDamage_Thrust)
            thrustMainPhysicalDamageType = PhysicalDamageType.Slash;
        else
            thrustMainPhysicalDamageType = PhysicalDamageType.Blunt;

        // Overhead
        if (pierceDamage_Overhead >= bluntDamage_Overhead && pierceDamage_Overhead >= slashDamage_Overhead && pierceDamage_Overhead >= cleaveDamage_Overhead)
            overheadMainPhysicalDamageType = PhysicalDamageType.Pierce;
        else if (cleaveDamage_Overhead >= bluntDamage_Overhead && cleaveDamage_Overhead >= pierceDamage_Overhead && cleaveDamage_Overhead >= slashDamage_Overhead)
            overheadMainPhysicalDamageType = PhysicalDamageType.Cleave;
        else if (slashDamage_Overhead >= bluntDamage_Overhead && slashDamage_Overhead >= pierceDamage_Overhead && slashDamage_Overhead >= cleaveDamage_Overhead)
            overheadMainPhysicalDamageType = PhysicalDamageType.Slash;
        else
            overheadMainPhysicalDamageType = PhysicalDamageType.Blunt;
    }

    public PhysicalDamageType GetMeleeAttacksPhysicalDamageType(MeleeAttackType meleeAttackType)
    {
        switch (meleeAttackType)
        {
            case MeleeAttackType.Swipe:
                return swipeMainPhysicalDamageType;
            case MeleeAttackType.Thrust:
                return thrustMainPhysicalDamageType;
            case MeleeAttackType.Overhead:
                return overheadMainPhysicalDamageType;
            default:
                return swipeMainPhysicalDamageType;
        }
    }

    /// <summary> 
    /// Prevents items with an inventory (such as bags) from having their current weight/volume being greater than their max weight/volume, 
    /// by looping through each item in the inventory and taking turns subtracting 1 from their current stack sizes.
    /// </summary>
    public IEnumerator ClampItemCounts()
    {
        int itemsWithStackSizeOfOne;
        while (bagInventory.currentWeight > bagInventory.maxWeight || bagInventory.currentVolume > bagInventory.maxVolume)
        {
            itemsWithStackSizeOfOne = 0;
            for (int i = 0; i < bagInventory.items.Count; i++)
            {
                if (bagInventory.items[i].currentStackSize == 1)
                    itemsWithStackSizeOfOne++;
            }
            
            if (itemsWithStackSizeOfOne == bagInventory.items.Count)
                break;

            for (int i = 0; i < bagInventory.items.Count; i++)
            {
                if (bagInventory.items[i].currentStackSize > 1)
                {
                    bagInventory.items[i].currentStackSize--;
                    bagInventory.currentWeight -= Mathf.RoundToInt(bagInventory.items[i].item.weight * bagInventory.items[i].GetPercentRemaining_Decimal() * 100f) / 100f;
                    bagInventory.currentVolume -= Mathf.RoundToInt(bagInventory.items[i].item.volume * bagInventory.items[i].GetPercentRemaining_Decimal() * 100f) / 100f;
                }

                if (bagInventory.currentWeight <= bagInventory.maxWeight && bagInventory.currentVolume <= bagInventory.maxVolume)
                    break;
            }

            yield return null;
        }
    }

    public bool IsPickup()
    {
        if (CompareTag("Item Pickup"))
            return true;
        return false;
    }

    public bool IsEquipped()
    {
        for (int i = 0; i < GameManager.instance.playerManager.playerEquipmentManager.currentEquipment.Length; i++)
        {
            if (GameManager.instance.playerManager.playerEquipmentManager.currentEquipment[i] == this)
                return true;
        }
        return false;
    }

    public Inventory GetItemDatasPlayerInventory()
    {
        for (int i = 0; i < PlayerManager.instance.personalInventory.items.Count; i++)
        {
            if (PlayerManager.instance.personalInventory.items[i] == this)
                return PlayerManager.instance.personalInventory;
        }

        if (PlayerManager.instance.backpackInventory != null)
        {
            for (int i = 0; i < PlayerManager.instance.backpackInventory.items.Count; i++)
            {
                if (PlayerManager.instance.backpackInventory.items[i] == this)
                    return PlayerManager.instance.backpackInventory;
            }
        }

        if (PlayerManager.instance.leftHipPouchInventory != null)
        {
            for (int i = 0; i < PlayerManager.instance.leftHipPouchInventory.items.Count; i++)
            {
                if (PlayerManager.instance.leftHipPouchInventory.items[i] == this)
                    return PlayerManager.instance.leftHipPouchInventory;
            }
        }

        if (PlayerManager.instance.rightHipPouchInventory != null)
        {
            for (int i = 0; i < PlayerManager.instance.rightHipPouchInventory.items.Count; i++)
            {
                if (PlayerManager.instance.rightHipPouchInventory.items[i] == this)
                    return PlayerManager.instance.rightHipPouchInventory;
            }
        }

        if (PlayerManager.instance.quiverInventory != null)
        {
            for (int i = 0; i < PlayerManager.instance.quiverInventory.items.Count; i++)
            {
                if (PlayerManager.instance.quiverInventory.items[i] == this)
                    return PlayerManager.instance.quiverInventory;
            }
        }

        for (int i = 0; i < PlayerManager.instance.keysInventory.items.Count; i++)
        {
            if (PlayerManager.instance.keysInventory.items[i] == this)
                return PlayerManager.instance.keysInventory;
        }

        return null;
    }

    public InventoryItem GetItemDatasInventoryItem()
    {
        for (int i = 0; i < ObjectPoolManager.instance.playerInventoryItemObjectPool.activePooledInventoryItems.Count; i++)
        {
            if (ObjectPoolManager.instance.playerInventoryItemObjectPool.activePooledInventoryItems[i].itemData == this)
                return ObjectPoolManager.instance.playerInventoryItemObjectPool.activePooledInventoryItems[i];
        }

        for (int i = 0; i < ObjectPoolManager.instance.containerInventoryItemObjectPool.activePooledInventoryItems.Count; i++)
        {
            if (ObjectPoolManager.instance.containerInventoryItemObjectPool.activePooledInventoryItems[i].itemData == this)
                return ObjectPoolManager.instance.containerInventoryItemObjectPool.activePooledInventoryItems[i];
        }

        return null;
    }

    public float GetSoilage()
    {
        return 100f - freshness;
    }

    public string GetSoilageText()
    {
        if (freshness == 100)
            return "Clean";
        else if (freshness >= 90)
            return "Barely Soiled";
        else if (freshness >= 75)
            return "Lightly Soiled";
        else if (freshness >= 50)
            return "Moderately Soiled";
        else if (freshness >= 25)
            return "Heavily Soiled";
        else
            return "Filthy";
    }

    public string GetItemName(float amount)
    {
        string itemName = namePrefix;
        if (amount > 1)
            itemName += GetPluralName();
        else
            itemName += item.name;
        return itemName;
    }

    public string GetPluralName()
    {
        if (item.pluralName != "")
            return item.pluralName;
        else
            return item.name + "s";
    }

    public void SetNamePrefix()
    {
        if (item.IsEquipment() && durability == 0)
            namePrefix = "Broken ";
        else if (item.canUsePartial)
        {
            if (percentRemaining == 75)
                namePrefix = "Three Quarters ";
            else if (percentRemaining == 50)
                namePrefix = "Half ";
            else if (percentRemaining == 25)
                namePrefix = "Quarter ";
            else if (percentRemaining < 100)
                namePrefix = "Partial ";
            else
                namePrefix = "";
        }
        else
            namePrefix = "";
    }

    public float GetPercentRemaining_Decimal()
    {
        return percentRemaining / 100f;
    }

    public IEnumerator DelayReturnToObjectPool()
    {
        yield return null;
        ReturnToObjectPool();
    }

    public void ReturnToObjectPool()
    {
        if (bagInventory != null)
            ReturnToItemDataContainerObjectPool();
        else
            ReturnToItemDataObjectPool();
    }

    public void ReturnToItemDataObjectPool()
    {
        transform.SetParent(ObjectPoolManager.instance.itemDataObjectPool.transform);

        ClearData();
        gameObject.SetActive(false);
    }

    public void ReturnToItemDataContainerObjectPool()
    {
        transform.SetParent(ObjectPoolManager.instance.itemDataContainerObjectPool.transform);

        bagInventory.inventoryOwner = null;
        bagInventory.myInvUI = null;
        bagInventory.items.Clear();
        ClearData();
        gameObject.SetActive(false);
    }
}

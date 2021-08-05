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
    PhysicalDamageType swipeMainPhysicalDamageType;
    PhysicalDamageType thrustMainPhysicalDamageType, overheadMainPhysicalDamageType;
    public int bluntDamage_Swipe, pierceDamage_Swipe, slashDamage_Swipe, cleaveDamage_Swipe;             // Swipe
    public int bluntDamage_Thrust, pierceDamage_Thrust, slashDamage_Thrust, cleaveDamage_Thrust;         // Thrust
    public int bluntDamage_Overhead, pierceDamage_Overhead, slashDamage_Overhead, cleaveDamage_Overhead; // Overhead

    [Header("Armor Data")]
    public int torsoDefense;
    public int headDefense, armDefense, handDefense, legDefense, footDefense;

    [Header("Shield Data")]
    public int shieldBashDamage;
    public float blockChanceMultiplier;

    [Header("Consumable Data")]
    public int freshness = 100;
    public int uses = 1;
    
    [HideInInspector] public Inventory bagInventory;

    GameManager gm;

    void Awake()
    {
        if (hasBeenRandomized)
            value = CalculateItemValue();
        else if (item != null)
            RandomizeData();

        TryGetComponent(out bagInventory);
    }

    void Start()
    {
        gm = GameManager.instance;
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

        // Armor Data
        dataReceiver.torsoDefense = dataGiver.torsoDefense;
        dataReceiver.headDefense = dataGiver.headDefense;
        dataReceiver.armDefense = dataGiver.armDefense;
        dataReceiver.handDefense = dataGiver.handDefense;
        dataReceiver.legDefense = dataGiver.legDefense;
        dataReceiver.footDefense = dataGiver.footDefense;

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
        // Item class data
        itemName = item.name;

        // Equipment class data
        if (item.IsEquipment())
        {
            Equipment equipment = (Equipment)item;
            if (equipment.maxBaseDurability > 0)
                maxDurability = Random.Range(equipment.minBaseDurability, equipment.maxBaseDurability + 1);

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

            if (item.IsWeapon())
            {
                Weapon weapon = (Weapon)equipment;
                bluntDamage_Swipe = Random.Range(weapon.minBaseBluntDamage_Swipe, weapon.maxBaseBluntDamage_Swipe + 1);
                pierceDamage_Swipe = Random.Range(weapon.minBasePierceDamage_Swipe, weapon.maxBasePierceDamage_Swipe + 1);
                slashDamage_Swipe = Random.Range(weapon.minBaseSlashDamage_Swipe, weapon.maxBaseSlashDamage_Swipe + 1);
                cleaveDamage_Swipe = Random.Range(weapon.minBaseCleaveDamage_Swipe, weapon.maxBaseCleaveDamage_Swipe + 1);
                bluntDamage_Thrust = Random.Range(weapon.minBaseBluntDamage_Thrust, weapon.maxBaseBluntDamage_Thrust + 1);
                pierceDamage_Thrust = Random.Range(weapon.minBasePierceDamage_Thrust, weapon.maxBasePierceDamage_Thrust + 1);
                slashDamage_Thrust = Random.Range(weapon.minBaseSlashDamage_Thrust, weapon.maxBaseSlashDamage_Thrust + 1);
                cleaveDamage_Thrust = Random.Range(weapon.minBaseCleaveDamage_Thrust, weapon.maxBaseCleaveDamage_Thrust + 1);
                bluntDamage_Overhead = Random.Range(weapon.minBaseBluntDamage_Overhead, weapon.maxBaseBluntDamage_Overhead + 1);
                pierceDamage_Overhead = Random.Range(weapon.minBasePierceDamage_Overhead, weapon.maxBasePierceDamage_Overhead + 1);
                slashDamage_Overhead = Random.Range(weapon.minBaseSlashDamage_Overhead, weapon.maxBaseSlashDamage_Overhead + 1);
                cleaveDamage_Overhead = Random.Range(weapon.minBaseCleaveDamage_Overhead, weapon.maxBaseCleaveDamage_Overhead + 1);

                blockChanceMultiplier = Mathf.RoundToInt(Random.Range(weapon.minBlockChanceMultiplier, weapon.maxBlockChanceMultiplier) * 100f) / 100f;

                SetMeleeAttackPhysicalDamageTypes();
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                torsoDefense = Random.Range(wearable.minBaseTorsoDefense, wearable.maxBaseTorsoDefense + 1);
                headDefense = Random.Range(wearable.minBaseHeadDefense, wearable.maxBaseHeadDefense + 1);
                armDefense = Random.Range(wearable.minBaseArmDefense, wearable.maxBaseArmDefense + 1);
                handDefense = Random.Range(wearable.minBaseHandDefense, wearable.maxBaseHandDefense + 1);
                legDefense = Random.Range(wearable.minBaseLegDefense, wearable.maxBaseLegDefense + 1);
                footDefense = Random.Range(wearable.minBaseFootDefense, wearable.maxBaseFootDefense + 1);
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                shieldBashDamage = Random.Range(shield.minShieldBashDamage, shield.maxShieldBashDamage + 1);
                blockChanceMultiplier = Mathf.RoundToInt(Random.Range(shield.minBlockChanceMultiplier, shield.maxBlockChanceMultiplier) * 100f) / 100f;
            }
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

        if (item.maxStackSize > 1)
            currentStackSize = Random.Range(1, item.maxStackSize + 1);

        value = CalculateItemValue();

        hasBeenRandomized = true;
    }

    public IEnumerator RandomizeDataWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        RandomizeData();
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
        if (itemData1.item == itemData2.item && itemData1.value == itemData2.value && itemData1.freshness == itemData2.freshness)
            return true;

        return false;
    }

    public void ClearData()
    {
        hasBeenRandomized = false;
        
        item = null;
        
        itemName = "";
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

        torsoDefense = 0;
        headDefense = 0;
        armDefense = 0;
        handDefense = 0;
        legDefense = 0;
        footDefense = 0;

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
                totalPointsPossible += ((weapon.maxBaseBluntDamage_Swipe + weapon.maxBasePierceDamage_Swipe + weapon.maxBaseSlashDamage_Swipe + weapon.maxBaseCleaveDamage_Swipe 
                    + weapon.maxBaseBluntDamage_Thrust + weapon.maxBasePierceDamage_Thrust + weapon.maxBaseSlashDamage_Thrust + weapon.maxBaseCleaveDamage_Thrust 
                    + weapon.maxBaseBluntDamage_Overhead + weapon.maxBasePierceDamage_Overhead + weapon.maxBaseSlashDamage_Overhead + weapon.maxBaseCleaveDamage_Overhead) 
                    - (weapon.minBaseBluntDamage_Swipe + weapon.minBasePierceDamage_Swipe + weapon.minBaseSlashDamage_Swipe + weapon.minBaseCleaveDamage_Swipe 
                    + weapon.minBaseBluntDamage_Thrust + weapon.minBasePierceDamage_Thrust + weapon.minBaseSlashDamage_Thrust + weapon.minBaseCleaveDamage_Thrust 
                    + weapon.minBaseBluntDamage_Overhead + weapon.minBasePierceDamage_Overhead + weapon.minBaseSlashDamage_Overhead + weapon.minBaseCleaveDamage_Overhead)) * 2;
                totalPointsPossible += (weapon.maxBlockChanceMultiplier - weapon.minBlockChanceMultiplier) * 10;
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                totalPointsPossible += (wearable.maxBaseTorsoDefense - wearable.minBaseTorsoDefense) * 2;
                totalPointsPossible += (wearable.maxBaseHeadDefense - wearable.minBaseHeadDefense) * 2;
                totalPointsPossible += (wearable.maxBaseArmDefense - wearable.minBaseArmDefense) * 2;
                totalPointsPossible += (wearable.maxBaseHandDefense - wearable.minBaseHandDefense) * 2;
                totalPointsPossible += (wearable.maxBaseLegDefense - wearable.minBaseLegDefense) * 2;
                totalPointsPossible += (wearable.maxBaseFootDefense - wearable.minBaseFootDefense) * 2;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                totalPointsPossible += (shield.maxShieldBashDamage - shield.minShieldBashDamage);
                totalPointsPossible += (shield.maxBlockChanceMultiplier - shield.minBlockChanceMultiplier) * 10;
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
                pointIncrease += ((bluntDamage_Swipe - weapon.minBaseBluntDamage_Swipe) + (pierceDamage_Swipe - weapon.minBasePierceDamage_Swipe) + (slashDamage_Swipe - weapon.minBaseSlashDamage_Swipe) + (cleaveDamage_Swipe - weapon.minBaseCleaveDamage_Swipe)
                    + (bluntDamage_Thrust - weapon.minBaseBluntDamage_Thrust) + (pierceDamage_Thrust - weapon.minBasePierceDamage_Thrust) + (slashDamage_Thrust - weapon.minBaseSlashDamage_Thrust) + (cleaveDamage_Thrust - weapon.minBaseCleaveDamage_Thrust)
                    + (bluntDamage_Overhead - weapon.minBaseBluntDamage_Overhead) + (pierceDamage_Overhead - weapon.minBasePierceDamage_Overhead) + (slashDamage_Overhead - weapon.minBaseSlashDamage_Overhead) + (cleaveDamage_Overhead - weapon.minBaseCleaveDamage_Overhead)) * 2; // Damage contributes to value twice as much
                pointIncrease += (blockChanceMultiplier - weapon.minBlockChanceMultiplier) * 10;
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                pointIncrease += (torsoDefense - wearable.minBaseTorsoDefense) * 2; // Defense contributes to value twice as much
                pointIncrease += (headDefense - wearable.minBaseHeadDefense) * 2;
                pointIncrease += (armDefense - wearable.minBaseArmDefense) * 2;
                pointIncrease += (handDefense - wearable.minBaseHandDefense) * 2;
                pointIncrease += (legDefense - wearable.minBaseLegDefense) * 2;
                pointIncrease += (footDefense - wearable.minBaseFootDefense) * 2;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                pointIncrease += (shieldBashDamage - shield.minShieldBashDamage);
                pointIncrease += (blockChanceMultiplier - shield.minBlockChanceMultiplier) * 10;
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
                if (bluntDamage_Swipe > weapon.maxBaseBluntDamage_Swipe)
                    bluntDamage_Swipe = weapon.maxBaseBluntDamage_Swipe;
                else if (bluntDamage_Swipe < weapon.minBaseBluntDamage_Swipe)
                    bluntDamage_Swipe = weapon.minBaseBluntDamage_Swipe;

                if (pierceDamage_Swipe > weapon.maxBasePierceDamage_Swipe)
                    pierceDamage_Swipe = weapon.maxBasePierceDamage_Swipe;
                else if (pierceDamage_Swipe < weapon.minBasePierceDamage_Swipe)
                    pierceDamage_Swipe = weapon.minBasePierceDamage_Swipe;

                if (slashDamage_Swipe > weapon.maxBaseSlashDamage_Swipe)
                    slashDamage_Swipe = weapon.maxBaseSlashDamage_Swipe;
                else if (slashDamage_Swipe < weapon.minBaseSlashDamage_Swipe)
                    slashDamage_Swipe = weapon.minBaseSlashDamage_Swipe;

                if (cleaveDamage_Swipe > weapon.maxBaseCleaveDamage_Swipe)
                    cleaveDamage_Swipe = weapon.maxBaseCleaveDamage_Swipe;
                else if (cleaveDamage_Swipe < weapon.minBaseCleaveDamage_Swipe)
                    cleaveDamage_Swipe = weapon.minBaseCleaveDamage_Swipe;

                if (bluntDamage_Thrust > weapon.maxBaseBluntDamage_Thrust)
                    bluntDamage_Thrust = weapon.maxBaseBluntDamage_Thrust;
                else if (bluntDamage_Thrust < weapon.minBaseBluntDamage_Thrust)
                    bluntDamage_Thrust = weapon.minBaseBluntDamage_Thrust;

                if (pierceDamage_Thrust > weapon.maxBasePierceDamage_Thrust)
                    pierceDamage_Thrust = weapon.maxBasePierceDamage_Thrust;
                else if (pierceDamage_Thrust < weapon.minBasePierceDamage_Thrust)
                    pierceDamage_Thrust = weapon.minBasePierceDamage_Thrust;

                if (slashDamage_Thrust > weapon.maxBaseSlashDamage_Thrust)
                    slashDamage_Thrust = weapon.maxBaseSlashDamage_Thrust;
                else if (slashDamage_Thrust < weapon.minBaseSlashDamage_Thrust)
                    slashDamage_Thrust = weapon.minBaseSlashDamage_Thrust;

                if (cleaveDamage_Thrust > weapon.maxBaseCleaveDamage_Thrust)
                    cleaveDamage_Thrust = weapon.maxBaseCleaveDamage_Thrust;
                else if (cleaveDamage_Thrust < weapon.minBaseCleaveDamage_Thrust)
                    cleaveDamage_Thrust = weapon.minBaseCleaveDamage_Thrust;

                if (bluntDamage_Overhead > weapon.maxBaseBluntDamage_Overhead)
                    bluntDamage_Overhead = weapon.maxBaseBluntDamage_Overhead;
                else if (bluntDamage_Overhead < weapon.minBaseBluntDamage_Overhead)
                    bluntDamage_Overhead = weapon.minBaseBluntDamage_Overhead;

                if (pierceDamage_Overhead > weapon.maxBasePierceDamage_Overhead)
                    pierceDamage_Overhead = weapon.maxBasePierceDamage_Overhead;
                else if (pierceDamage_Overhead < weapon.minBasePierceDamage_Overhead)
                    pierceDamage_Overhead = weapon.minBasePierceDamage_Overhead;

                if (slashDamage_Overhead > weapon.maxBaseSlashDamage_Overhead)
                    slashDamage_Overhead = weapon.maxBaseSlashDamage_Overhead;
                else if (slashDamage_Overhead < weapon.minBaseSlashDamage_Overhead)
                    slashDamage_Overhead = weapon.minBaseSlashDamage_Overhead;

                if (cleaveDamage_Overhead > weapon.maxBaseCleaveDamage_Overhead)
                    cleaveDamage_Overhead = weapon.maxBaseCleaveDamage_Overhead;
                else if (cleaveDamage_Overhead < weapon.minBaseCleaveDamage_Overhead)
                    cleaveDamage_Overhead = weapon.minBaseCleaveDamage_Overhead;

                if (blockChanceMultiplier > weapon.maxBlockChanceMultiplier)
                    blockChanceMultiplier = weapon.maxBlockChanceMultiplier;
                else if (blockChanceMultiplier < weapon.minBlockChanceMultiplier)
                    blockChanceMultiplier = weapon.minBlockChanceMultiplier;
            }
            else if (item.IsWearable())
            {
                Wearable wearable = (Wearable)equipment;
                if (torsoDefense > wearable.maxBaseTorsoDefense)
                    torsoDefense = wearable.maxBaseTorsoDefense;
                else if (torsoDefense < wearable.minBaseTorsoDefense)
                    torsoDefense = wearable.minBaseTorsoDefense;

                if (headDefense > wearable.maxBaseHeadDefense)
                    headDefense = wearable.maxBaseHeadDefense;
                else if (headDefense < wearable.minBaseHeadDefense)
                    headDefense = wearable.minBaseHeadDefense;

                if (armDefense > wearable.maxBaseArmDefense)
                    armDefense = wearable.maxBaseArmDefense;
                else if (armDefense < wearable.minBaseArmDefense)
                    armDefense = wearable.minBaseArmDefense;

                if (handDefense > wearable.maxBaseHandDefense)
                    handDefense = wearable.maxBaseHandDefense;
                else if (handDefense < wearable.minBaseHandDefense)
                    handDefense = wearable.minBaseHandDefense;

                if (legDefense > wearable.maxBaseLegDefense)
                    legDefense = wearable.maxBaseLegDefense;
                else if (legDefense < wearable.minBaseLegDefense)
                    legDefense = wearable.minBaseLegDefense;

                if (footDefense > wearable.maxBaseFootDefense)
                    footDefense = wearable.maxBaseFootDefense;
                else if (footDefense < wearable.minBaseFootDefense)
                    footDefense = wearable.minBaseFootDefense;
            }
            else if (item.IsShield())
            {
                Shield shield = (Shield)equipment;
                if (shieldBashDamage > shield.maxShieldBashDamage)
                    shieldBashDamage = shield.maxShieldBashDamage;
                else if (shieldBashDamage < shield.minShieldBashDamage)
                    shieldBashDamage = shield.minShieldBashDamage;

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

                if (uses > consumable.maxUses)
                    uses = consumable.maxUses;
                else if (uses < 0)
                    uses = 0;
            }
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
            BreakItem();
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
            BreakItem();
        }
    }

    public void BreakItem()
    {
        itemName = "Broken " + itemName;
    }

    void SetMeleeAttackPhysicalDamageTypes()
    {
        // Swipe
        if (pierceDamage_Swipe > bluntDamage_Swipe && pierceDamage_Swipe > slashDamage_Swipe && pierceDamage_Swipe > cleaveDamage_Swipe)
            swipeMainPhysicalDamageType = PhysicalDamageType.Pierce;
        else if (slashDamage_Swipe > bluntDamage_Swipe && slashDamage_Swipe > pierceDamage_Swipe && slashDamage_Swipe > cleaveDamage_Swipe)
            swipeMainPhysicalDamageType = PhysicalDamageType.Slash;
        else if (cleaveDamage_Swipe > bluntDamage_Swipe && cleaveDamage_Swipe > pierceDamage_Swipe && cleaveDamage_Swipe > slashDamage_Swipe)
            swipeMainPhysicalDamageType = PhysicalDamageType.Cleave;
        else
            swipeMainPhysicalDamageType = PhysicalDamageType.Blunt;

        // Thrust
        if (pierceDamage_Thrust > bluntDamage_Thrust && pierceDamage_Thrust > slashDamage_Thrust && pierceDamage_Thrust > cleaveDamage_Thrust)
            thrustMainPhysicalDamageType = PhysicalDamageType.Pierce;
        else if (slashDamage_Thrust > bluntDamage_Thrust && slashDamage_Thrust > pierceDamage_Thrust && slashDamage_Thrust > cleaveDamage_Thrust)
            thrustMainPhysicalDamageType = PhysicalDamageType.Slash;
        else if (cleaveDamage_Thrust > bluntDamage_Thrust && cleaveDamage_Thrust > pierceDamage_Thrust && cleaveDamage_Thrust > slashDamage_Thrust)
            thrustMainPhysicalDamageType = PhysicalDamageType.Cleave;
        else
            thrustMainPhysicalDamageType = PhysicalDamageType.Blunt;

        // Overhead
        if (pierceDamage_Overhead > bluntDamage_Overhead && pierceDamage_Overhead > slashDamage_Overhead && pierceDamage_Overhead > cleaveDamage_Overhead)
            overheadMainPhysicalDamageType = PhysicalDamageType.Pierce;
        else if (slashDamage_Overhead > bluntDamage_Overhead && slashDamage_Overhead > pierceDamage_Overhead && slashDamage_Overhead > cleaveDamage_Overhead)
            overheadMainPhysicalDamageType = PhysicalDamageType.Slash;
        else if (cleaveDamage_Overhead > bluntDamage_Overhead && cleaveDamage_Overhead > pierceDamage_Overhead && cleaveDamage_Overhead > slashDamage_Overhead)
            overheadMainPhysicalDamageType = PhysicalDamageType.Cleave;
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
                    bagInventory.currentWeight -= Mathf.RoundToInt(bagInventory.items[i].item.weight * 100f) / 100f;
                    bagInventory.currentVolume -= Mathf.RoundToInt(bagInventory.items[i].item.volume * 100f) / 100f;
                }

                if (bagInventory.currentWeight <= bagInventory.maxWeight && bagInventory.currentVolume <= bagInventory.maxVolume)
                    break;
            }

            yield return null;
        }
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
        if (gm == null) gm = GameManager.instance;

        transform.SetParent(gm.objectPoolManager.itemDataObjectPool.transform);
        if (gm.objectPoolManager.itemDataObjectPool.pooledObjects.Contains(gameObject) == false)
        {
            gm.objectPoolManager.itemDataObjectPool.pooledObjects.Add(gameObject);
            gm.objectPoolManager.itemDataObjectPool.pooledItemDatas.Add(this);
        }

        ClearData();
        gameObject.SetActive(false);
    }

    public void ReturnToItemDataContainerObjectPool()
    {
        if (gm == null) gm = GameManager.instance;
        transform.SetParent(gm.objectPoolManager.itemDataContainerObjectPool.transform);
        if (gm.objectPoolManager.itemDataContainerObjectPool.pooledObjects.Contains(gameObject) == false)
        {
            gm.objectPoolManager.itemDataContainerObjectPool.pooledObjects.Add(gameObject);
            gm.objectPoolManager.itemDataContainerObjectPool.pooledItemDatas.Add(this);
        }

        bagInventory.items.Clear();
        ClearData();
        gameObject.SetActive(false);
    }
}

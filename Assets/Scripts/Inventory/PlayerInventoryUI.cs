using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventoryUI : InventoryUI
{
    public TextMeshProUGUI totalWeightText;
    public TextMeshProUGUI totalVolumeText;

    [Header("Inventories")]
    public Inventory personalInventory;
    public Inventory bag1Inventory, bag2Inventory, bag3Inventory, bag4Inventory, bag5Inventory, keysInventory;

    #region Singleton
    public static PlayerInventoryUI instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of PlayerInventoryUI. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    public override void Start()
    {
        base.Start();

        inventoryItemObjectPool.Init();
        InitInventories();

        personalInventory.maxWeight = playerManager.playerStats.maxPersonalInvWeight.GetValue();
        personalInventory.maxVolume = playerManager.playerStats.maxPersonalInvVolume.GetValue();

        PopulateInventoryUI(personalInventory.items, PlayerInventoryType.Personal);
    }

    // This method runs when the user clicks on a container side bar icon
    public void PopulateInventoryUI(List<ItemData> itemsList, PlayerInventoryType playerInvType)
    {
        ClearInventoryUI();

        for (int i = 0; i < itemsList.Count; i++)
        {
            InventoryItem invItem = inventoryItemObjectPool.GetPooledInventoryItem();
            Debug.Log(itemsList);
            Debug.Log(invItem.itemData);
            itemsList[i].TransferData(itemsList[i], invItem.itemData);
            invItem.itemNameText.text = invItem.itemData.itemName;
            invItem.itemAmountText.text = invItem.itemData.currentStackSize.ToString();
            invItem.itemTypeText.text = invItem.itemData.item.itemType.ToString();
            invItem.itemWeightText.text = (invItem.itemData.item.weight * invItem.itemData.currentStackSize).ToString();
            invItem.itemVolumeText.text = (invItem.itemData.item.volume * invItem.itemData.currentStackSize).ToString();
            invItem.gameObject.SetActive(true);
        }

        // Set header/volume/weight text
        SetUpInventoryUI(playerInvType);
    }

    public void PopulateInventoryUI(ItemData[] currentEquipment, PlayerInventoryType playerInvType)
    {
        ClearInventoryUI();

        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
            {
                InventoryItem invItem = inventoryItemObjectPool.GetPooledInventoryItem();
                currentEquipment[i].TransferData(currentEquipment[i], invItem.itemData);
                invItem.itemNameText.text = invItem.itemData.itemName;
                invItem.itemAmountText.text = invItem.itemData.currentStackSize.ToString();
                invItem.itemTypeText.text = invItem.itemData.item.itemType.ToString();
                invItem.itemWeightText.text = (invItem.itemData.item.weight * invItem.itemData.currentStackSize).ToString();
                invItem.itemVolumeText.text = (invItem.itemData.item.volume * invItem.itemData.currentStackSize).ToString();
                invItem.gameObject.SetActive(true);
            }
        }

        // Set container open icon sprite (when applicable) and header/volume/weight text
        SetUpInventoryUI(playerInvType);
    }

    void SetUpInventoryUI(PlayerInventoryType playerInvType)
    {
        switch (playerInvType)
        {
            case PlayerInventoryType.Personal:
                inventoryNameText.text = "Personal Inventory";
                weightText.text = GetTotalWeight(personalInventory.items).ToString() + "/" + playerManager.playerStats.maxPersonalInvWeight.GetValue().ToString();
                volumeText.text = GetTotalVolume(personalInventory.items).ToString() + "/" + playerManager.playerStats.maxPersonalInvVolume.GetValue().ToString();
                break;
            case PlayerInventoryType.Bag1:
                inventoryNameText.text = "Bag 1 Inventory";
                weightText.text = GetTotalWeight(bag1Inventory.items).ToString() + "/" + bag1Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag1Inventory.items).ToString() + "/" + bag1Inventory.maxVolume.ToString();
                break;
            case PlayerInventoryType.Bag2:
                inventoryNameText.text = "Bag 2  Inventory";
                weightText.text = GetTotalWeight(bag2Inventory.items).ToString() + "/" + bag2Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag2Inventory.items).ToString() + "/" + bag2Inventory.maxVolume.ToString();
                break;
            case PlayerInventoryType.Bag3:
                inventoryNameText.text = "Bag 3  Inventory";
                weightText.text = GetTotalWeight(bag3Inventory.items).ToString() + "/" + bag3Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag3Inventory.items).ToString() + "/" + bag3Inventory.maxVolume.ToString();
                break;
            case PlayerInventoryType.Bag4:
                inventoryNameText.text = "Bag 4  Inventory";
                weightText.text = GetTotalWeight(bag4Inventory.items).ToString() + "/" + bag4Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag4Inventory.items).ToString() + "/" + bag4Inventory.maxVolume.ToString();
                break;
            case PlayerInventoryType.Bag5:
                inventoryNameText.text = "Bag 5  Inventory";
                weightText.text = GetTotalWeight(bag5Inventory.items).ToString() + "/" + bag5Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag5Inventory.items).ToString() + "/" + bag5Inventory.maxVolume.ToString();
                break;
            case PlayerInventoryType.Keys:
                inventoryNameText.text = "Keys";
                weightText.text = GetTotalWeight(keysInventory.items).ToString();
                volumeText.text = GetTotalVolume(keysInventory.items).ToString();
                break;
            case PlayerInventoryType.EquippedItems:
                inventoryNameText.text = "Equipped Items";
                weightText.text = GetTotalWeight(playerManager.equipmentManager.currentEquipment).ToString();
                volumeText.text = GetTotalVolume(playerManager.equipmentManager.currentEquipment).ToString();
                break;
            default:
                break;
        }

        totalWeightText.text = GetTotalCarredWeight().ToString();
        totalVolumeText.text = GetTotalCarriedVolume().ToString();
    }

    float GetTotalCarredWeight()
    {
        float totalWeight = 0;
        for (int i = 0; i < personalInventory.items.Count; i++)
        {
            totalWeight += personalInventory.items[i].item.weight * personalInventory.items[i].currentStackSize;
        }

        if (bag1Inventory != null)
        {
            for (int i = 0; i < bag1Inventory.items.Count; i++)
            {
                totalWeight += bag1Inventory.items[i].item.weight * bag1Inventory.items[i].currentStackSize;
            }
        }

        if (bag2Inventory != null)
        {
            for (int i = 0; i < bag2Inventory.items.Count; i++)
            {
                totalWeight += bag2Inventory.items[i].item.weight * bag2Inventory.items[i].currentStackSize;
            }
        }

        if (bag3Inventory != null)
        {
            for (int i = 0; i < bag3Inventory.items.Count; i++)
            {
                totalWeight += bag3Inventory.items[i].item.weight * bag3Inventory.items[i].currentStackSize;
            }
        }

        if (bag4Inventory != null)
        {
            for (int i = 0; i < bag4Inventory.items.Count; i++)
            {
                totalWeight += bag4Inventory.items[i].item.weight * bag4Inventory.items[i].currentStackSize;
            }
        }

        if (bag5Inventory != null)
        {
            for (int i = 0; i < bag5Inventory.items.Count; i++)
            {
                totalWeight += bag5Inventory.items[i].item.weight * bag5Inventory.items[i].currentStackSize;
            }
        }

        if (keysInventory != null)
        {
            for (int i = 0; i < keysInventory.items.Count; i++)
            {
                totalWeight += keysInventory.items[i].item.weight * keysInventory.items[i].currentStackSize;
            }
        }

        for (int i = 0; i < playerManager.equipmentManager.currentEquipment.Length; i++)
        {
            if (playerManager.equipmentManager.currentEquipment[i] != null)
                totalWeight += playerManager.equipmentManager.currentEquipment[i].item.weight * playerManager.equipmentManager.currentEquipment[i].currentStackSize;
        }

        return totalWeight;
    }

    float GetTotalCarriedVolume()
    {
        float totalVolume = 0;
        for (int i = 0; i < personalInventory.items.Count; i++)
        {
            totalVolume += personalInventory.items[i].item.volume * personalInventory.items[i].currentStackSize;
        }

        if (bag1Inventory != null)
        {
            for (int i = 0; i < bag1Inventory.items.Count; i++)
            {
                totalVolume += bag1Inventory.items[i].item.volume * bag1Inventory.items[i].currentStackSize;
            }
        }

        if (bag2Inventory != null)
        {
            for (int i = 0; i < bag2Inventory.items.Count; i++)
            {
                totalVolume += bag2Inventory.items[i].item.volume * bag2Inventory.items[i].currentStackSize;
            }
        }

        if (bag3Inventory != null)
        {
            for (int i = 0; i < bag3Inventory.items.Count; i++)
            {
                totalVolume += bag3Inventory.items[i].item.volume * bag3Inventory.items[i].currentStackSize;
            }
        }

        if (bag4Inventory != null)
        {
            for (int i = 0; i < bag4Inventory.items.Count; i++)
            {
                totalVolume += bag4Inventory.items[i].item.volume * bag4Inventory.items[i].currentStackSize;
            }
        }

        if (bag5Inventory != null)
        {
            for (int i = 0; i < bag5Inventory.items.Count; i++)
            {
                totalVolume += bag5Inventory.items[i].item.volume * bag5Inventory.items[i].currentStackSize;
            }
        }

        if (keysInventory != null)
        {
            for (int i = 0; i < keysInventory.items.Count; i++)
            {
                totalVolume += keysInventory.items[i].item.volume * keysInventory.items[i].currentStackSize;
            }
        }

        for (int i = 0; i < playerManager.equipmentManager.currentEquipment.Length; i++)
        {
            if (playerManager.equipmentManager.currentEquipment[i] != null)
                totalVolume += playerManager.equipmentManager.currentEquipment[i].item.volume * playerManager.equipmentManager.currentEquipment[i].currentStackSize;
        }

        return totalVolume;
    }

    void InitInventories()
    {
        personalInventory.Init();
        bag1Inventory.Init();
        bag2Inventory.Init();
        bag3Inventory.Init();
        bag4Inventory.Init();
        bag5Inventory.Init();
        keysInventory.Init();
    }
}

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
    
    public bool bag1Active, bag2Active, bag3Active, bag4Active, bag5Active;

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

        personalInventory.maxWeight = gm.playerManager.playerStats.maxPersonalInvWeight.GetValue();
        personalInventory.maxVolume = gm.playerManager.playerStats.maxPersonalInvVolume.GetValue();
        
        PopulateInventoryUI(personalInventory.items, PlayerInventoryType.Personal);
    }

    // This method runs when the user clicks on a container side bar icon
    public void PopulateInventoryUI(List<ItemData> itemsList, PlayerInventoryType playerInvType)
    {
        ClearInventoryUI();

        for (int i = 0; i < itemsList.Count; i++)
        {
            InventoryItem invItem = ShowNewInventoryItem(itemsList[i]);
            AssignInventoryOrEquipmentManagerToInventoryItem(invItem, playerInvType);
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
                InventoryItem invItem = ShowNewInventoryItem(currentEquipment[i]);
                AssignInventoryOrEquipmentManagerToInventoryItem(invItem, playerInvType);
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
                weightText.text = GetTotalWeight(personalInventory.items).ToString() + "/" + gm.playerManager.playerStats.maxPersonalInvWeight.GetValue().ToString();
                volumeText.text = GetTotalVolume(personalInventory.items).ToString() + "/" + gm.playerManager.playerStats.maxPersonalInvVolume.GetValue().ToString();
                activeInventory = personalInventory;
                break;
            case PlayerInventoryType.Bag1:
                inventoryNameText.text = "Bag 1 Inventory";
                weightText.text = GetTotalWeight(bag1Inventory.items).ToString() + "/" + bag1Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag1Inventory.items).ToString() + "/" + bag1Inventory.maxVolume.ToString();
                activeInventory = bag1Inventory;
                break;
            case PlayerInventoryType.Bag2:
                inventoryNameText.text = "Bag 2  Inventory";
                weightText.text = GetTotalWeight(bag2Inventory.items).ToString() + "/" + bag2Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag2Inventory.items).ToString() + "/" + bag2Inventory.maxVolume.ToString();
                activeInventory = bag2Inventory;
                break;
            case PlayerInventoryType.Bag3:
                inventoryNameText.text = "Bag 3  Inventory";
                weightText.text = GetTotalWeight(bag3Inventory.items).ToString() + "/" + bag3Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag3Inventory.items).ToString() + "/" + bag3Inventory.maxVolume.ToString();
                activeInventory = bag3Inventory;
                break;
            case PlayerInventoryType.Bag4:
                inventoryNameText.text = "Bag 4  Inventory";
                weightText.text = GetTotalWeight(bag4Inventory.items).ToString() + "/" + bag4Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag4Inventory.items).ToString() + "/" + bag4Inventory.maxVolume.ToString();
                activeInventory = bag4Inventory;
                break;
            case PlayerInventoryType.Bag5:
                inventoryNameText.text = "Bag 5  Inventory";
                weightText.text = GetTotalWeight(bag5Inventory.items).ToString() + "/" + bag5Inventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(bag5Inventory.items).ToString() + "/" + bag5Inventory.maxVolume.ToString();
                activeInventory = bag5Inventory;
                break;
            case PlayerInventoryType.Keys:
                inventoryNameText.text = "Keys";
                weightText.text = GetTotalWeight(keysInventory.items).ToString();
                volumeText.text = GetTotalVolume(keysInventory.items).ToString();
                activeInventory = keysInventory;
                break;
            case PlayerInventoryType.EquippedItems:
                inventoryNameText.text = "Equipped Items";
                weightText.text = GetTotalWeight(gm.playerManager.equipmentManager.currentEquipment).ToString();
                volumeText.text = GetTotalVolume(gm.playerManager.equipmentManager.currentEquipment).ToString();
                activeInventory = null;
                break;
            default:
                break;
        }

        totalWeightText.text = GetTotalCarriedWeight().ToString();
        totalVolumeText.text = GetTotalCarriedVolume().ToString();
    }

    void AssignInventoryOrEquipmentManagerToInventoryItem(InventoryItem invItem, PlayerInventoryType playerInvType)
    {
        switch (playerInvType)
        {
            case PlayerInventoryType.Personal:
                invItem.myEquipmentManager = null;
                invItem.myInventory = personalInventory;
                break;
            case PlayerInventoryType.Bag1:
                invItem.myEquipmentManager = null;
                invItem.myInventory = bag1Inventory;
                break;
            case PlayerInventoryType.Bag2:
                invItem.myEquipmentManager = null;
                invItem.myInventory = bag2Inventory;
                break;
            case PlayerInventoryType.Bag3:
                invItem.myEquipmentManager = null;
                invItem.myInventory = bag3Inventory;
                break;
            case PlayerInventoryType.Bag4:
                invItem.myEquipmentManager = null;
                invItem.myInventory = bag4Inventory;
                break;
            case PlayerInventoryType.Bag5:
                invItem.myEquipmentManager = null;
                invItem.myInventory = bag5Inventory;
                break;
            case PlayerInventoryType.Keys:
                invItem.myEquipmentManager = null;
                invItem.myInventory = keysInventory;
                break;
            case PlayerInventoryType.EquippedItems:
                invItem.myEquipmentManager = gm.playerManager.playerEquipmentManager;
                invItem.myInventory = null;
                break;
            default:
                break;
        }
    }

    public override void UpdateUINumbers()
    {
        if (activeInventory != null)
        {
            if (activeInventory == keysInventory)
            {
                weightText.text = (Mathf.RoundToInt(activeInventory.currentWeight * 100f) / 100f).ToString();
                volumeText.text = (Mathf.RoundToInt(activeInventory.currentVolume * 100f) / 100f).ToString();
            }
            else
            {
                weightText.text = (Mathf.RoundToInt(activeInventory.currentWeight * 100f) / 100f).ToString() + "/" + activeInventory.maxWeight.ToString();
                volumeText.text = (Mathf.RoundToInt(activeInventory.currentVolume * 100f) / 100f).ToString() + "/" + activeInventory.maxVolume.ToString();
            }
        }
        else
        {
            weightText.text = GetTotalWeight(gm.playerManager.playerEquipmentManager.currentEquipment).ToString();
            volumeText.text = GetTotalVolume(gm.playerManager.playerEquipmentManager.currentEquipment).ToString();
        }

        totalWeightText.text = GetTotalCarriedWeight().ToString();
        totalVolumeText.text = GetTotalCarriedVolume().ToString();
    }

    float GetTotalCarriedWeight()
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

        for (int i = 0; i < gm.playerManager.equipmentManager.currentEquipment.Length; i++)
        {
            if (gm.playerManager.equipmentManager.currentEquipment[i] != null)
                totalWeight += gm.playerManager.equipmentManager.currentEquipment[i].item.weight * gm.playerManager.equipmentManager.currentEquipment[i].currentStackSize;
        }

        return Mathf.RoundToInt(totalWeight * 100f) / 100f;
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

        for (int i = 0; i < gm.playerManager.equipmentManager.currentEquipment.Length; i++)
        {
            if (gm.playerManager.equipmentManager.currentEquipment[i] != null)
                totalVolume += gm.playerManager.equipmentManager.currentEquipment[i].item.volume * gm.playerManager.equipmentManager.currentEquipment[i].currentStackSize;
        }

        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
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

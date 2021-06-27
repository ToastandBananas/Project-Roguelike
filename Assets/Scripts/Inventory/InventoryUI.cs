using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public Transform slotsParent;
    public GameObject inventoryParent;
    public InventoryItemObjectPool inventoryItemObjectPool;

    [Header("Texts")]
    public TextMeshProUGUI inventoryNameText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI volumeText;

    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public Inventory activeInventory;

    DropItemController dropItemController;
    UIManager uiManager;

    public virtual void Start()
    {
        dropItemController = DropItemController.instance;
        uiManager = UIManager.instance;

        UpdateVisibleSlots();

        if (inventoryParent.activeSelf)
            inventoryParent.SetActive(false);

        playerManager = PlayerManager.instance;

    }

    void AddItem(ItemData itemData, int itemCount)
    {
        int itemCountRemaining = itemCount;
        // TODO
    }

    void RemoveItem(ItemData itemData, int itemCount, InventoryItem inventorySlot)
    {
        int itemCountRemaining = itemCount;
        // TODO
    }

    public void ClearInventoryUI()
    {
        for (int i = 0; i < inventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            if (inventoryItemObjectPool.pooledInventoryItems[i].gameObject.activeSelf)
            {
                inventoryItemObjectPool.pooledInventoryItems[i].ClearUI();
                inventoryItemObjectPool.pooledInventoryItems[i].gameObject.SetActive(false);
            }
        }
    }

    public InventoryItem ShowNewInventoryItem(ItemData newItemData)
    {
        InventoryItem invItem = inventoryItemObjectPool.GetPooledInventoryItem();
        newItemData.TransferData(newItemData, invItem.itemData);
        invItem.originItemData = newItemData;
        invItem.itemNameText.text = invItem.itemData.itemName;
        invItem.itemAmountText.text = invItem.itemData.currentStackSize.ToString();
        invItem.itemTypeText.text = invItem.itemData.item.itemType.ToString();
        invItem.itemWeightText.text = (invItem.itemData.item.weight * invItem.itemData.currentStackSize).ToString();
        invItem.itemVolumeText.text = (invItem.itemData.item.volume * invItem.itemData.currentStackSize).ToString();
        invItem.myInventory = activeInventory;
        invItem.gameObject.SetActive(true);

        return invItem;
    }

    public virtual void UpdateUINumbers()
    {
        // This is just meant to be overridden
    }

    public void ToggleInventoryMenu()
    {
        inventoryParent.SetActive(!inventoryParent.activeSelf);

        // If the Inventory was menu was closed
        if (inventoryParent.activeSelf == false)
        {
            // Close the Context Menu
            //if (uiManager.contextMenu.isActive)
                //uiManager.contextMenu.DisableContextMenu();

            // Close the Stack Size Selector
            //if (uiManager.stackSizeSelector.isActive)
                //uiManager.stackSizeSelector.HideStackSizeSelector();

            //uiManager.tooltipManager.HideAllTooltips();
        }
    }

    public float GetTotalWeight(List<ItemData> itemsList)
    {
        float totalWeight = 0f;
        for (int i = 0; i < itemsList.Count; i++)
        {
            totalWeight += itemsList[i].item.weight * itemsList[i].currentStackSize;
        }

        return (totalWeight * 100f) / 100f;
    }

    public float GetTotalWeight(ItemData[] currentEquipment)
    {
        float totalWeight = 0f;
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
                totalWeight += currentEquipment[i].item.weight * currentEquipment[i].currentStackSize;
        }

        return (totalWeight * 100f) / 100f;
    }

    public float GetTotalVolume(List<ItemData> itemsList)
    {
        float totalVolume = 0f;
        for (int i = 0; i < itemsList.Count; i++)
        {
            totalVolume += itemsList[i].item.volume * itemsList[i].currentStackSize;
        }

        return (totalVolume * 100f) / 100f;
    }

    public float GetTotalVolume(ItemData[] currentEquipment)
    {
        float totalVolume = 0f;
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
                totalVolume += currentEquipment[i].item.volume * currentEquipment[i].currentStackSize;
        }

        return (totalVolume * 100f) / 100f;
    }

    public void UpdateVisibleSlots()
    {
        // TODO
    }
}

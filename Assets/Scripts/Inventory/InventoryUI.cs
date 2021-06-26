using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI inventoryNameText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI volumeText;
    public Transform slotsParent;
    public GameObject inventoryParent;
    public InventoryItemObjectPool inventoryItemObjectPool;
    
    [HideInInspector] public PlayerManager playerManager;

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
                inventoryItemObjectPool.pooledInventoryItems[i].ClearItem();
                inventoryItemObjectPool.pooledInventoryItems[i].gameObject.SetActive(false);
            }
        }
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

        return totalWeight;
    }

    public float GetTotalWeight(ItemData[] currentEquipment)
    {
        float totalWeight = 0f;
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
                totalWeight += currentEquipment[i].item.weight * currentEquipment[i].currentStackSize;
        }

        return totalWeight;
    }

    public float GetTotalVolume(List<ItemData> itemsList)
    {
        float totalVolume = 0f;
        for (int i = 0; i < itemsList.Count; i++)
        {
            totalVolume += itemsList[i].item.volume * itemsList[i].currentStackSize;
        }

        return totalVolume;
    }

    public float GetTotalVolume(ItemData[] currentEquipment)
    {
        float totalVolume = 0f;
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
                totalVolume += currentEquipment[i].item.volume * currentEquipment[i].currentStackSize;
        }

        return totalVolume;
    }

    public bool IsRoomInInventory(ItemData itemData, int itemCount)
    {
        /*if (item.maxStackSize == 1)
        {
            for (int i = 0; i < inventory.space; i++)
            {
                if (slots[i].gameObject.activeSelf && slots[i].IsEmpty())
                    return true;
            }
        }
        else
        {
            int remainingItemCount = itemCount;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == null)
                    return true;
                else if (item.maxStackSize > 1 && slots[i].item == item)
                    remainingItemCount -= item.maxStackSize - slots[i].currentStackSize;
            }

            if (remainingItemCount <= 0)
                return true;
        }*/
        
        // TODO

        Debug.Log("Not enough room in Inventory...");
        return false;
    }

    public void UpdateVisibleSlots()
    {
        // TODO
    }
}

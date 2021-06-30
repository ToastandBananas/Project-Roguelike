using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public Transform slotsParent;
    public GameObject inventoryParent, background, sideBarParent, minimizeButtonText;
    public InventoryItemObjectPool inventoryItemObjectPool;

    [Header("Texts")]
    public TextMeshProUGUI inventoryNameText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI volumeText;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public Inventory activeInventory;

    public virtual void Start()
    {
        gm = GameManager.instance;

        UpdateVisibleSlots();

        if (inventoryParent.activeSelf)
            inventoryParent.SetActive(false);
    }

    public void ClearInventoryUI()
    {
        for (int i = 0; i < inventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            if (inventoryItemObjectPool.pooledInventoryItems[i].gameObject.activeSelf)
            {
                inventoryItemObjectPool.pooledInventoryItems[i].gameObject.SetActive(false);
                inventoryItemObjectPool.pooledInventoryItems[i].ResetInvItem();
            }
        }
    }

    public InventoryItem ShowNewInventoryItem(ItemData newItemData)
    {
        InventoryItem invItem = inventoryItemObjectPool.GetPooledInventoryItem();
        invItem.itemData = newItemData;
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
            if (gm.contextMenu.isActive)
                gm.contextMenu.DisableContextMenu();

            // Close the Stack Size Selector
            if (gm.stackSizeSelector.isActive)
                gm.stackSizeSelector.HideStackSizeSelector();

            //gm.tooltipManager.HideAllTooltips();

            if (gm.containerInvUI == this)
                gm.uiManager.activeContainerSideBarButton = null;
            else if (gm.playerInvUI == this)
                gm.uiManager.activePlayerInvSideBarButton = null;
        }
        else if (background.activeSelf == false)
        {
            // Make sure these are active, in case the inventory had been minimized previously
            background.SetActive(true);
            sideBarParent.SetActive(true);
            minimizeButtonText.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    public void ToggleMinimization()
    {
        background.SetActive(!background.activeSelf);
        sideBarParent.SetActive(!sideBarParent.activeSelf);

        // If the inventory was minimized
        if (background.activeSelf == false)
        {
            minimizeButtonText.transform.rotation = Quaternion.Euler(Vector3.zero);

            // Close the Context Menu
            if (gm.contextMenu.isActive)
                gm.contextMenu.DisableContextMenu();

            // Close the Stack Size Selector
            if (gm.stackSizeSelector.isActive)
                gm.stackSizeSelector.HideStackSizeSelector();

            //gm.tooltipManager.HideAllTooltips();
        }
        else
            minimizeButtonText.transform.rotation = Quaternion.Euler(0, 0, 180);
    }

    public float GetTotalWeight(List<ItemData> itemsList)
    {
        float totalWeight = 0f;
        for (int i = 0; i < itemsList.Count; i++)
        {
            totalWeight += itemsList[i].item.weight * itemsList[i].currentStackSize;
        }

        return Mathf.RoundToInt(totalWeight * 100f) / 100f;
    }

    public float GetTotalWeight(ItemData[] currentEquipment)
    {
        float totalWeight = 0f;
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
                totalWeight += currentEquipment[i].item.weight * currentEquipment[i].currentStackSize;
        }

        return Mathf.RoundToInt(totalWeight * 100f) / 100f;
    }

    public float GetTotalVolume(List<ItemData> itemsList)
    {
        float totalVolume = 0f;
        for (int i = 0; i < itemsList.Count; i++)
        {
            totalVolume += itemsList[i].item.volume * itemsList[i].currentStackSize;
        }

        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
    }

    public float GetTotalVolume(ItemData[] currentEquipment)
    {
        float totalVolume = 0f;
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
                totalVolume += currentEquipment[i].item.volume * currentEquipment[i].currentStackSize;
        }

        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
    }

    public void UpdateVisibleSlots()
    {
        // TODO
    }

    public InventoryItem GetItemDatasInventoryItem(ItemData itemData)
    {
        if (this == gm.playerInvUI)
        {
            for (int i = 0; i < gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems.Count; i++)
            {
                if (gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems[i].itemData == itemData)
                    return gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems[i];
            }
        }
        else if (this == gm.containerInvUI)
        {
            for (int i = 0; i < gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems.Count; i++)
            {
                if (gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems[i].itemData == itemData)
                    return gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems[i];
            }
        }

        return null;
    }
}

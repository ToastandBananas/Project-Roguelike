using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public delegate void OnItemAdded(ItemData itemData, int itemCount);
    public OnItemAdded onItemAddedCallback;

    public delegate void OnItemRemoved(ItemData itemData, int itemCount, InventoryItem invItem);
    public OnItemRemoved onItemRemovedCallback;

    public float maxWeight = 20;
    public float maxVolume = 20;

    public float currentWeight, currentVolume;

    public List<ItemData> items = new List<ItemData>();

    [HideInInspector] public Transform itemsParent;
    [HideInInspector] public Container container;
    [HideInInspector] public InventoryUI myInventoryUI;
    [HideInInspector] public GameObject inventoryOwner;

    [HideInInspector] public bool hasBeenInitialized;
    
    GameManager gm;

    public virtual void Start()
    {
        Init();
    }

    public void Init()
    {
        if (hasBeenInitialized == false)
        {
            gm = GameManager.instance;
            inventoryOwner = gameObject;
            itemsParent = transform.Find("Items");
            TryGetComponent(out container);

            for (int i = 0; i < itemsParent.childCount; i++)
            {
                ItemData itemData = itemsParent.GetChild(i).GetComponent<ItemData>();
                items.Add(itemData);
                currentWeight += itemData.item.weight * itemData.currentStackSize;
                currentWeight = Mathf.RoundToInt(currentWeight * 100f) / 100f;
                currentVolume += itemData.item.volume * itemData.currentStackSize;
                currentVolume = Mathf.RoundToInt(currentVolume * 100f) / 100f;
            }

            if (items.Count > 0)
            {
                // Clamp currentStackSize values if we accidentally set them over the Item's maxStackSize or less than 1
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].currentStackSize > items[i].item.maxStackSize)
                        items[i].currentStackSize = items[i].item.maxStackSize;
                    else if (items[i].currentStackSize <= 0)
                        items[i].currentStackSize = 1;
                }
            }

            hasBeenInitialized = true;
        }
    }

    public bool Add(InventoryItem invItemComingFrom, ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom)
    {
        int startingStackSize = itemDataComingFrom.currentStackSize;
        bool hasRoom = HasRoomInInventory(itemDataComingFrom, itemCount);

        if (hasRoom == false)
        {
            Debug.Log("Not enough room in Inventory...");
            return false;
        }

        currentWeight += itemDataComingFrom.item.weight * itemCount;
        currentWeight = (currentWeight * 100f) / 100f;
        currentVolume += itemDataComingFrom.item.volume * itemCount;
        currentVolume = (currentVolume * 100f) / 100f;

        int amountAddedToExistingStacks = 0;
        if (itemDataComingFrom.item.maxStackSize > 1 && InventoryContainsSameItem(itemDataComingFrom)) // Try adding to existing stacks first
            amountAddedToExistingStacks = AddToExistingStacks(itemDataComingFrom, itemCount, invComingFrom);

        itemCount -= amountAddedToExistingStacks;

        if ((itemCount == 1 && amountAddedToExistingStacks == 0) || (itemCount > 1 && itemDataComingFrom.currentStackSize > 0)) // If there's still some left to add
        {
            if (invItemComingFrom != null)
                invItemComingFrom.UpdateItemTexts();

            ItemData itemDataToAdd = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();
            itemDataToAdd.TransferData(itemDataComingFrom, itemDataToAdd);
            if (itemCount == 1)
                itemDataToAdd.currentStackSize = 1;
            items.Add(itemDataToAdd);
            if (myInventoryUI == gm.containerInvUI)
                gm.containerInvUI.AddItemToList(itemDataToAdd);
            itemDataToAdd.transform.SetParent(itemsParent);
            itemDataToAdd.gameObject.SetActive(true);

            if (myInventoryUI.activeInventory == this)
                myInventoryUI.ShowNewInventoryItem(itemDataToAdd);

            #if UNITY_EDITOR
                itemDataToAdd.gameObject.name = itemDataToAdd.itemName;
            #endif

            if (invComingFrom != null)
            {
                invComingFrom.currentWeight -= itemDataComingFrom.item.weight * itemCount;
                invComingFrom.currentWeight = Mathf.RoundToInt(invComingFrom.currentWeight * 100f) / 100f;
                invComingFrom.currentVolume -= itemDataComingFrom.item.volume * itemCount;
                invComingFrom.currentVolume = Mathf.RoundToInt(invComingFrom.currentVolume * 100f) / 100f;
            }

            if (itemCount == 1)
                itemDataComingFrom.currentStackSize--;
            else
                itemDataComingFrom.currentStackSize = 0;
        }

        return true;
    }

    public void Remove(ItemData itemData, int itemCount, InventoryItem invItem)
    {
        // TODO
    }

    public int AddToExistingStacks(ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom)
    {
        int amountAdded = 0;
        for (int i = 0; i < items.Count; i++) // The Items list refers to our ItemData GameObjects
        {
            if (itemDataComingFrom.StackableItemsDataIsEqual(items[i], itemDataComingFrom) && items[i].currentStackSize < items[i].item.maxStackSize)
            {
                InventoryItem itemDatasInvItem = myInventoryUI.GetItemDatasInventoryItem(items[i]); // Get the InventoryItem using the ItemData we're adding to
                for (int j = 0; j < itemCount; j++)
                {
                    if (items[i].currentStackSize < items[i].item.maxStackSize)
                    {
                        items[i].currentStackSize++;
                        amountAdded++;
                        itemDataComingFrom.currentStackSize--;

                        if (invComingFrom != null)
                        {
                            invComingFrom.currentWeight -= itemDataComingFrom.item.weight;
                            invComingFrom.currentWeight = Mathf.RoundToInt(invComingFrom.currentWeight * 100f) / 100f;
                            invComingFrom.currentVolume -= itemDataComingFrom.item.volume;
                            invComingFrom.currentVolume = Mathf.RoundToInt(invComingFrom.currentVolume * 100f) / 100f;
                        }

                        if (itemDatasInvItem != null)
                            itemDatasInvItem.UpdateItemTexts();

                        if (itemDataComingFrom.currentStackSize == 0)
                            return amountAdded;
                    }
                    else
                        break;
                }
            }
        }

        return amountAdded;
    }

    public bool InventoryContainsSameItem(ItemData itemData)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == itemData.item)
                return true;
        }

        return false;
    }

    public bool HasRoomInInventory(ItemData itemData, int itemCount)
    {
        float itemWeight = itemData.item.weight * itemCount;
        float itemVolume = itemData.item.volume * itemCount;
        if (maxWeight - currentWeight >= itemWeight && maxVolume - currentVolume >= itemVolume)
            return true;

        return false;
    }

    public bool HasEnough(ItemData itemData, int itemAmountRequired)
    {
        int itemCountInInventory = 0;

        // Add up how much of this Item that we have in our Inventory
        for (int i = 0; i < myInventoryUI.inventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            if (myInventoryUI.inventoryItemObjectPool.pooledInventoryItems[i].itemData != null && myInventoryUI.inventoryItemObjectPool.pooledInventoryItems[i].itemData.item == itemData.item)
                itemCountInInventory += myInventoryUI.inventoryItemObjectPool.pooledInventoryItems[i].itemData.currentStackSize;
        }

        if (itemCountInInventory >= itemAmountRequired)
            return true;

        return false;
    }
}

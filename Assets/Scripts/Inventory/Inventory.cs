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

    [HideInInspector] public float currentWeight, currentVolume;

    public List<ItemData> items = new List<ItemData>();

    [HideInInspector] public Transform itemsParent;
    [HideInInspector] public Container container;
    [HideInInspector] public InventoryUI myInventoryUI;
    [HideInInspector] public GameObject inventoryOwner;

    [HideInInspector] public bool hasBeenInitialized;

    ItemDataObjectPool itemDataObjectPool;

    public virtual void Start()
    {
        Init();
    }

    public void Init()
    {
        if (hasBeenInitialized == false)
        {
            itemDataObjectPool = (ItemDataObjectPool)ObjectPoolManager.instance.itemDataObjectPool;
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
                // Clamp itemCount values if we accidentally set them over the Item's maxStackSize
                for (int i = 0; i < items.Count; i++)
                {
                    // TODO
                }
            }

            hasBeenInitialized = true;
        }
    }

    public bool Add(ItemData itemData, int itemCount, Inventory invComingFrom)
    {
        bool hasRoom = HasRoomInInventory(itemData, itemCount);

        if (hasRoom == false)
        {
            Debug.Log("Not enough room in Inventory...");
            return false;
        }

        currentWeight += itemData.item.weight * itemCount;
        currentWeight = (currentWeight * 100f) / 100f;
        currentVolume += itemData.item.volume * itemCount;
        currentVolume = (currentVolume * 100f) / 100f;

        if (itemData.item.maxStackSize > 1 && InventoryContainsSameItem(itemData)) // Try adding to existing stacks first
            AddToExistingStacks(itemData, invComingFrom);

        if (itemData.currentStackSize > 0) // If there's still some left to add
        {
            ItemData itemDataToAdd = itemDataObjectPool.GetPooledItemData();
            itemDataToAdd.TransferData(itemData, itemDataToAdd);
            items.Add(itemDataToAdd);
            itemDataToAdd.transform.SetParent(itemsParent);
            itemDataToAdd.gameObject.SetActive(true);

            if (myInventoryUI.activeInventory == this)
                myInventoryUI.ShowNewInventoryItem(itemDataToAdd);

            #if UNITY_EDITOR
                itemDataToAdd.gameObject.name = itemDataToAdd.itemName;
            #endif

            if (invComingFrom != null)
            {
                invComingFrom.currentWeight -= itemData.item.weight * itemData.currentStackSize;
                invComingFrom.currentWeight = Mathf.RoundToInt(invComingFrom.currentWeight * 100f) / 100f;
                invComingFrom.currentVolume -= itemData.item.volume * itemData.currentStackSize;
                invComingFrom.currentVolume = Mathf.RoundToInt(invComingFrom.currentVolume * 100f) / 100f;
            }

            itemData.currentStackSize = 0;
        }

        return true;
    }

    public void Remove(ItemData itemData, int itemCount, InventoryItem invItem)
    {
        // TODO
    }

    public void AddToExistingStacks(ItemData itemData, Inventory invComingFrom)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (itemData.StackableItemsDataIsEqual(items[i], itemData))
            {
                for (int j = 0; j < itemData.currentStackSize; j++)
                {
                    if (items[i].currentStackSize < items[i].item.maxStackSize)
                    {
                        items[i].currentStackSize++;
                        itemData.currentStackSize--;
                        
                        if (invComingFrom != null)
                        {
                            invComingFrom.currentWeight -= itemData.item.weight;
                            invComingFrom.currentWeight = Mathf.RoundToInt(invComingFrom.currentWeight * 100f) / 100f;
                            invComingFrom.currentVolume -= itemData.item.volume;
                            invComingFrom.currentVolume = Mathf.RoundToInt(invComingFrom.currentVolume * 100f) / 100f;
                        }

                        if (itemData.currentStackSize == 0)
                            return;
                    }
                }
            }
        }
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
            if (myInventoryUI.inventoryItemObjectPool.pooledInventoryItems[i].itemData.item == itemData.item)
                itemCountInInventory += myInventoryUI.inventoryItemObjectPool.pooledInventoryItems[i].currentStackSize;
        }

        if (itemCountInInventory >= itemAmountRequired)
            return true;

        return false;
    }
}

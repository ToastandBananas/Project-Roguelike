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
                currentWeight += Mathf.RoundToInt(itemData.item.weight * itemData.currentStackSize * 100f) / 100f;
                currentVolume += Mathf.RoundToInt(itemData.item.volume * itemData.currentStackSize * 100f) / 100f;
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
        bool hasRoom = HasRoomInInventory(itemDataComingFrom, itemCount);
        
        // Make sure we have room, before we start adding the item
        if (hasRoom == false)
        {
            Debug.Log("Not enough room in Inventory...");
            return false;
        }

        // Add to this Inventory's weight and volume
        currentWeight += itemDataComingFrom.item.weight * itemCount;
        currentWeight = (currentWeight * 100f) / 100f;
        currentVolume += itemDataComingFrom.item.volume * itemCount;
        currentVolume = (currentVolume * 100f) / 100f;

        // Try adding to existing stacks first, while keeping track of how many we added to existing stacks
        int amountAddedToExistingStacks = 0;
        if (itemDataComingFrom.item.maxStackSize > 1 && InventoryContainsSameItem(itemDataComingFrom))
            amountAddedToExistingStacks = AddToExistingStacks(itemDataComingFrom, itemCount, invComingFrom);

        // Subtract the amount we added to existing stacks from our itemCount
        itemCount -= amountAddedToExistingStacks;

        // If there's still some left to add
        if ((itemCount == 1 && amountAddedToExistingStacks == 0) || (itemCount > 1 && itemDataComingFrom.currentStackSize > 0))
        {
            // Update the InventoryItem we're taking from's texts
            if (invItemComingFrom != null)
                invItemComingFrom.UpdateItemTexts();

            // Create a new ItemData Object
            ItemData itemDataToAdd = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();

            // Transfer data to the new ItemData
            itemDataToAdd.TransferData(itemDataComingFrom, itemDataToAdd);

            // Since we transferred data from the old ItemData, we need to make sure to set the currentStackSize to 1 if we were only adding one of the item
            if (itemCount == 1)
                itemDataToAdd.currentStackSize = 1;

            // Add the new ItemData to this Inventory's items list
            items.Add(itemDataToAdd);

            // If we're adding an item to a container, add the new ItemData to the appropriate list
            if (myInventoryUI == gm.containerInvUI)
                gm.containerInvUI.AddItemToList(itemDataToAdd);

            // Set the parent of this new ItemData to the Inventory's itemsParent and set the gameObject as active
            itemDataToAdd.transform.SetParent(itemsParent);
            itemDataToAdd.gameObject.SetActive(true);

            // If this Inventory is active in the menu, create a new InventoryItem
            if (myInventoryUI.activeInventory == this)
                myInventoryUI.ShowNewInventoryItem(itemDataToAdd);

            #if UNITY_EDITOR
                itemDataToAdd.gameObject.name = itemDataToAdd.itemName;
            #endif

            // If we're taking this item from another Inventory, update it's weight and volume
            if (invComingFrom != null)
            {
                invComingFrom.currentWeight -= Mathf.RoundToInt(itemDataComingFrom.item.weight * itemCount * 100f) / 100f;
                invComingFrom.currentVolume -= Mathf.RoundToInt(itemDataComingFrom.item.volume * itemCount * 100f) / 100f;
            }

            // If we're only taking 1 count of the item, subtract 1 from the currentStackSize, otherwise it should now be 0
            if (itemCount == 1)
                itemDataComingFrom.currentStackSize--;
            else
                itemDataComingFrom.currentStackSize = 0;
        }

        return true;
    }

    public void Remove(ItemData itemData, int itemCount, InventoryItem invItem)
    {
        // Subtract from current weight and volume
        currentWeight -= Mathf.RoundToInt(itemData.item.weight * itemCount * 100f) / 100f;
        currentVolume -= Mathf.RoundToInt(itemData.item.volume * itemCount * 100f) / 100f;

        // Update InventoryUI
        if (myInventoryUI.activeInventory == this)
            myInventoryUI.UpdateUINumbers();

        // Subtract itemCount from the item's currentStackSize
        itemData.currentStackSize -= itemCount;

        // If there's none left:
        if (itemData.currentStackSize <= 0)
        {
            // Remove from Items list
            items.Remove(itemData);

            // Clear out the InventoryItem
            invItem.ClearItem();
        }
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
                            invComingFrom.currentWeight -= Mathf.RoundToInt(itemDataComingFrom.item.weight * 100f) / 100f;
                            invComingFrom.currentVolume -= Mathf.RoundToInt(itemDataComingFrom.item.volume * 100f) / 100f;
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

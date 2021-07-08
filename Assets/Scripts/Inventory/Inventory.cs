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

    [Header("Items")]
    public Transform itemsParent;
    public List<ItemData> items = new List<ItemData>();

    [HideInInspector] public Container container;
    [HideInInspector] public InventoryUI myInventoryUI;

    [HideInInspector] public bool hasBeenInitialized;
    
    GameManager gm;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if (hasBeenInitialized == false)
        {
            gm = GameManager.instance;
            TryGetComponent(out container);

            // Populate the items list if there are any children on the itemsParent
            for (int i = 0; i < itemsParent.childCount; i++)
            {
                ItemData itemData = itemsParent.GetChild(i).GetComponent<ItemData>();
                if (items.Contains(itemData) == false)
                    items.Add(itemData);
            }

            GetCurrentWeightAndVolume();

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

    public bool AddItem(InventoryItem invItemComingFrom, ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom)
    {
        // Prevent putting bags in bags
        if (invItemComingFrom.itemData.item.IsBag() && gameObject.CompareTag("Item Pickup"))
        {
            Debug.Log("You can't put that in here.");
            return false;
        }
        
        // Make sure we have room, before we start adding the item
        if (HasRoomInInventory(itemDataComingFrom, itemCount) == false)
        {
            Debug.Log("Not enough room in Inventory...");
            return false;
        }

        // Add to this Inventory's weight and volume
        AddItemsWeightAndVolumeToInventory(itemDataComingFrom, this, itemCount);

        // Try adding to existing stacks first, while keeping track of how many we added to existing stacks
        int amountAddedToExistingStacks = 0;
        if (itemDataComingFrom.item.maxStackSize > 1 && InventoryContainsSameItem(itemDataComingFrom))
            amountAddedToExistingStacks = AddToExistingStacks(itemDataComingFrom, itemCount, invComingFrom);

        // Subtract the amount we added to existing stacks from our itemCount
        itemCount -= amountAddedToExistingStacks;

        // If there's still some left to add
        if ((itemCount == 1 && amountAddedToExistingStacks == 0) || (itemCount > 1 && itemDataComingFrom.currentStackSize > 0))
        {
            // Create a new ItemData Object
            ItemData itemDataToAdd = null;
            if (itemDataComingFrom.item.IsBag())
            {
                itemDataToAdd = gm.objectPoolManager.itemDataContainerObjectPool.GetPooledItemData();
                itemDataToAdd.bagInventory.items.Clear();
            }
            else
                itemDataToAdd = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();

            // Transfer data to the new ItemData
            itemDataToAdd.TransferData(itemDataComingFrom, itemDataToAdd);

            // Since we transferred data from the old ItemData, we need to make sure to set the currentStackSize to 1 if we were only adding one of the item
            if (itemCount == 1)
                itemDataToAdd.currentStackSize = 1;

            // Add the new ItemData to this Inventory's items list
            items.Add(itemDataToAdd);

            // If we're adding an item to a container, add the new ItemData to the appropriate list (unless it's a bag on the ground)
            if (gm.uiManager.activeContainerSideBarButton != null && gm.uiManager.activeContainerSideBarButton.GetInventory().CompareTag("Item Pickup") == false
                && this == gm.uiManager.activeContainerSideBarButton.GetInventory())
            {
                gm.containerInvUI.AddItemToListFromDirection(itemDataToAdd, gm.uiManager.activeContainerSideBarButton.directionFromPlayer);
            }
            else if (gm.uiManager.activeContainerSideBarButton == null && myInventoryUI == gm.containerInvUI && gm.containerInvUI.activeInventory.CompareTag("Item Pickup") == false)
                gm.containerInvUI.AddItemToActiveDirectionList(itemDataToAdd);

            // Set the parent of this new ItemData to the Inventory's itemsParent and set the gameObject as active
            itemDataToAdd.transform.SetParent(itemsParent);
            itemDataToAdd.gameObject.SetActive(true);

            // If the item is a bag, add any items it had to the "new" bag object's Inventory
            if (itemDataComingFrom.item.IsBag() || itemDataComingFrom.item.itemType == ItemType.PortableContainer)
            {
                Inventory itemDataComingFromsInv = null;
                if (invComingFrom == null)
                    itemDataComingFromsInv = gm.playerInvUI.GetInventoryFromBagEquipSlot(itemDataComingFrom);
                else if (itemDataComingFrom.CompareTag("Item Pickup")) // If this is a bag we're picking up from the ground
                    itemDataComingFromsInv = invComingFrom;
                else
                    itemDataComingFromsInv = itemDataComingFrom.bagInventory; // If this bag is inside a container or one of the player's inventories
                
                for (int i = 0; i < itemDataComingFromsInv.items.Count; i++)
                {
                    // Add new ItemData Objects to the items parent of the new bag and transfer data to them
                    ItemData newItemDataObject = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();
                    newItemDataObject.transform.SetParent(itemDataToAdd.bagInventory.itemsParent);
                    newItemDataObject.gameObject.SetActive(true);
                    itemDataComingFromsInv.items[i].TransferData(itemDataComingFromsInv.items[i], newItemDataObject);

                    // Populate the new bag's inventory, but make sure it's not already in the items list (because of the Inventory's Init method, which populates this list)
                    if (itemDataToAdd.bagInventory.items.Contains(newItemDataObject) == false)
                        itemDataToAdd.bagInventory.items.Add(newItemDataObject);

                    #if UNITY_EDITOR
                        newItemDataObject.name = newItemDataObject.itemName;
                    #endif
                }

                // If the bag is coming from an Inventory or EquipmentManager (and not from the ground), subtract the bag's weight/volume, including the items inside it
                if (invComingFrom == null)
                    SubtractItemsWeightAndVolumeFromInventory(itemDataComingFrom, itemDataComingFromsInv, invItemComingFrom, itemCount, true);
                else if (itemDataComingFrom.CompareTag("Item Pickup") == false)
                {
                    SubtractItemsWeightAndVolumeFromInventory(itemDataComingFrom, itemDataComingFromsInv, invItemComingFrom, itemCount, true);
                    SubtractItemsWeightAndVolumeFromInventory(itemDataComingFrom, invComingFrom, invItemComingFrom, itemCount, true);
                }
                else
                    invComingFrom.ResetWeightAndVolume(); // Else if it is a pickup, just set the weight and volume to 0

                for (int i = 0; i < itemDataComingFromsInv.items.Count; i++)
                {
                    // Return the item we took out of the "old" bag back to it's object pool
                    itemDataComingFromsInv.items[i].ReturnToItemDataObjectPool();
                }
                
                // Clear out the items list of the "old" bag
                itemDataComingFromsInv.items.Clear();

                // Set the weight and volume of the "new" bag
                itemDataToAdd.bagInventory.GetCurrentWeightAndVolume();
            }

            // If this Inventory is active in the menu, create a new InventoryItem
            if (myInventoryUI.activeInventory == this)
            {
                // If we're adding the item to a bag on the ground, before we show the item, expand the list if it's not expanded
                if (gameObject.CompareTag("Item Pickup"))
                {
                    InventoryItem bagInvItem = myInventoryUI.GetBagItemFromInventory(this);
                    if (bagInvItem.disclosureWidget.isExpanded == false)
                        bagInvItem.disclosureWidget.ExpandDisclosureWidget();
                    else
                    {
                        InventoryItem newInvItem = myInventoryUI.ShowNewBagItem(itemDataToAdd, bagInvItem);
                        newInvItem.transform.SetAsLastSibling();
                    }

                    bagInvItem.UpdateItemNumberTexts();
                }
                else // Otherwise just show a normal InventoryItem
                    myInventoryUI.ShowNewInventoryItem(itemDataToAdd);
            }

            #if UNITY_EDITOR
                itemDataToAdd.gameObject.name = itemDataToAdd.itemName;
            #endif

            // If we're taking this item from another Inventory and it's not a bag or portable container, update its weight and volume
            if (invComingFrom != null && itemDataComingFrom.item.IsBag() == false && itemDataComingFrom.item.itemType != ItemType.PortableContainer)
                SubtractItemsWeightAndVolumeFromInventory(itemDataComingFrom, invComingFrom, invItemComingFrom, itemCount, true);

            // If we're only taking 1 count of the item, subtract 1 from the currentStackSize, otherwise it should now be 0
            if (itemCount == 1)
                itemDataComingFrom.currentStackSize--;
            else
                itemDataComingFrom.currentStackSize = 0;
        }

        // Update the InventoryItem we're taking from's texts
        if (invItemComingFrom != null)
        {
            invItemComingFrom.UpdateItemNumberTexts();
            if (invItemComingFrom.parentInvItem != null)
                invItemComingFrom.parentInvItem.UpdateItemNumberTexts();
        }

        return true;
    }

    public void RemoveItem(ItemData itemData, int itemCount, InventoryItem invItem)
    {
        // Subtract from current weight and volume
        SubtractItemsWeightAndVolumeFromInventory(itemData, this, invItem, itemCount, true);
        
        // Update InventoryUI
        if (invItem.myInvUI.activeInventory == this)
            invItem.myInvUI.UpdateUI();

        // Subtract itemCount from the item's currentStackSize
        itemData.currentStackSize -= itemCount;
        if (itemData.currentStackSize < 0)
            itemData.currentStackSize = 0;

        if (invItem.parentInvItem != null)
            invItem.parentInvItem.UpdateItemNumberTexts();

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
        for (int i = 0; i < items.Count; i++) // The "items" list refers to our ItemData GameObjects
        {
            if (itemDataComingFrom.StackableItemsDataIsEqual(items[i], itemDataComingFrom) && items[i].currentStackSize < items[i].item.maxStackSize)
            {
                // Get the InventoryItem using the ItemData we're adding to
                InventoryItem itemDatasInvItem = myInventoryUI.GetItemDatasInventoryItem(items[i]);
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
                            itemDatasInvItem.UpdateItemNumberTexts();

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
        float itemWeight = Mathf.RoundToInt(itemData.item.weight * itemCount * 100f) / 100f;
        float itemVolume = Mathf.RoundToInt(itemData.item.volume * itemCount * 100f) / 100f;

        if (itemData.item.IsBag() || itemData.item.itemType == ItemType.PortableContainer)
        {
            if (itemData.transform.parent.parent != null && itemData.transform.parent.parent.name == "Equipped Items") // If the bag is equipped
            {
                Inventory bagInv = gm.playerInvUI.GetInventoryFromBagEquipSlot(itemData);
                for (int i = 0; i < bagInv.items.Count; i++)
                {
                    itemWeight += Mathf.RoundToInt(bagInv.items[i].item.weight * bagInv.items[i].currentStackSize * 100f) / 100f;
                    itemVolume += Mathf.RoundToInt(bagInv.items[i].item.volume * bagInv.items[i].currentStackSize * 100f) / 100f;
                }
            }
            else
            {
                for (int i = 0; i < itemData.bagInventory.items.Count; i++)
                {
                    itemWeight += Mathf.RoundToInt(itemData.bagInventory.items[i].item.weight * itemData.bagInventory.items[i].currentStackSize * 100f) / 100f;
                    itemVolume += Mathf.RoundToInt(itemData.bagInventory.items[i].item.volume * itemData.bagInventory.items[i].currentStackSize * 100f) / 100f;
                }
            }
        }

        // Debug.Log(itemWeight + " / " + itemVolume);

        if (maxWeight - currentWeight >= itemWeight && maxVolume - currentVolume >= itemVolume)
            return true;

        return false;
    }

    public bool HasEnough(ItemData itemData, int itemAmountRequired)
    {
        int itemCountInInventory = 0;

        // Add up how much of this Item that we have in our Inventory
        for (int i = 0; i < myInventoryUI.inventoryItemObjectPool.activePooledInventoryItems.Count; i++)
        {
            if (myInventoryUI.inventoryItemObjectPool.activePooledInventoryItems[i].itemData.item == itemData.item)
                itemCountInInventory += myInventoryUI.inventoryItemObjectPool.activePooledInventoryItems[i].itemData.currentStackSize;
        }

        if (itemCountInInventory >= itemAmountRequired)
            return true;

        return false;
    }

    public void AddItemsWeightAndVolumeToInventory(ItemData itemDataAdding, Inventory invAddingItemTo, int itemCount)
    {
        invAddingItemTo.currentWeight += Mathf.RoundToInt(itemDataAdding.item.weight * itemCount * 100f) / 100f;
        invAddingItemTo.currentVolume += Mathf.RoundToInt(itemDataAdding.item.volume * itemCount * 100f) / 100f;

        if (itemDataAdding.item.IsBag() || itemDataAdding.item.itemType == ItemType.PortableContainer)
        {
            for (int i = 0; i < itemDataAdding.bagInventory.items.Count; i++)
            {
                invAddingItemTo.currentWeight += Mathf.RoundToInt(itemDataAdding.bagInventory.items[i].item.weight * itemDataAdding.bagInventory.items[i].currentStackSize * 100f) / 100f;
                invAddingItemTo.currentVolume += Mathf.RoundToInt(itemDataAdding.bagInventory.items[i].item.volume * itemDataAdding.bagInventory.items[i].currentStackSize * 100f) / 100f;
            }
        }
    }

    public void SubtractItemsWeightAndVolumeFromInventory(ItemData itemDataRemoving, Inventory invRemovingItemFrom, InventoryItem invItemComingFrom, int itemCount, bool shouldSubtractItemRemovingWeighAndVolume)
    {
        if (itemDataRemoving.item == null || invRemovingItemFrom == null) // This happens when equipping a bag
            return;
        
        if (shouldSubtractItemRemovingWeighAndVolume)
        {
            invRemovingItemFrom.currentWeight -= Mathf.RoundToInt(itemDataRemoving.item.weight * itemCount * 100f) / 100f;
            invRemovingItemFrom.currentVolume -= Mathf.RoundToInt(itemDataRemoving.item.volume * itemCount * 100f) / 100f;

            if (invItemComingFrom != null && invItemComingFrom.parentInvItem != null && invItemComingFrom.parentInvItem.myInventory != null && invRemovingItemFrom != invItemComingFrom.parentInvItem.myInventory)
            {
                invItemComingFrom.parentInvItem.myInventory.currentWeight -= Mathf.RoundToInt(itemDataRemoving.item.weight * itemCount * 100f) / 100f;
                invItemComingFrom.parentInvItem.myInventory.currentVolume -= Mathf.RoundToInt(itemDataRemoving.item.volume * itemCount * 100f) / 100f;
                invItemComingFrom.parentInvItem.UpdateItemNumberTexts();
            }
        }

        if (itemDataRemoving.item.IsBag() || itemDataRemoving.item.itemType == ItemType.PortableContainer)
        {
            for (int i = 0; i < itemDataRemoving.bagInventory.items.Count; i++)
            {
                invRemovingItemFrom.currentWeight -= Mathf.RoundToInt(itemDataRemoving.bagInventory.items[i].item.weight * itemDataRemoving.bagInventory.items[i].currentStackSize * 100f) / 100f;
                invRemovingItemFrom.currentVolume -= Mathf.RoundToInt(itemDataRemoving.bagInventory.items[i].item.volume * itemDataRemoving.bagInventory.items[i].currentStackSize * 100f) / 100f;
            }
        }
        
        if (invRemovingItemFrom.currentWeight <= 0.001f)
            invRemovingItemFrom.currentWeight = 0;

        if (invRemovingItemFrom.currentVolume <= 0.001f)
            invRemovingItemFrom.currentVolume = 0;

        if (myInventoryUI != null)
            myInventoryUI.UpdateUI();
    }

    public void GetCurrentWeightAndVolume()
    {
        ResetWeightAndVolume();
        for (int i = 0; i < items.Count; i++)
        {
            currentWeight += Mathf.RoundToInt(items[i].item.weight * items[i].currentStackSize * 100f) / 100f;
            currentVolume += Mathf.RoundToInt(items[i].item.volume * items[i].currentStackSize * 100f) / 100f;
        }
    }

    public void ResetWeightAndVolume()
    {
        currentVolume = 0;
        currentWeight = 0;
    }
}

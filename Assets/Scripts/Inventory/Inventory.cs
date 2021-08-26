using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public delegate void OnItemAdded(ItemData itemData, int itemCount);
    public OnItemAdded onItemAddedCallback;

    public delegate void OnItemRemoved(ItemData itemData, int itemCount, InventoryItem invItem);
    public OnItemRemoved onItemRemovedCallback;

    public float maxWeight = 20f;
    public float maxVolume = 20f;
    public float singleItemVolumeLimit;

    public float currentWeight, currentVolume;

    [Header("Items")]
    public Transform itemsParent;
    public List<ItemData> items = new List<ItemData>();

    [HideInInspector] public CharacterManager inventoryOwner;
    [HideInInspector] public Container container;
    [HideInInspector] public InventoryUI myInvUI;
    [HideInInspector] public ItemData myItemData; // For bags and the like

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
            TryGetComponent(out inventoryOwner);
            TryGetComponent(out myItemData);

            // Populate the items list if there are any children on the itemsParent
            for (int i = 0; i < itemsParent.childCount; i++)
            {
                ItemData itemData = itemsParent.GetChild(i).GetComponent<ItemData>();
                if (items.Contains(itemData) == false)
                    items.Add(itemData);
            }

            UpdateCurrentWeightAndVolume();

            if (items.Count > 0)
            {
                // Clamp currentStackSize values if we accidentally set them over the Item's maxStackSize or less than 1
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].currentStackSize > items[i].item.maxStackSize)
                        items[i].currentStackSize = items[i].item.maxStackSize;
                    else if (items[i].currentStackSize <= 0)
                        items[i].currentStackSize = 1;

                    // Also set the parent inventory
                    items[i].parentInventory = this;
                }
            }

            hasBeenInitialized = true;
        }
    }

    public bool AddItem(ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom, bool shouldUpdateWeightAndVolume)
    {
        // Prevent putting bags in bags
        if (itemDataComingFrom.item.IsBag() && gameObject.CompareTag("Item Pickup"))
        {
            Debug.Log("You can't put that in here.");
            return false;
        }
        
        // Make sure we have room, before we start adding the item
        if (HasRoomInInventory(itemDataComingFrom, itemCount) == false)
            return false;

        // Try adding to existing stacks first, while keeping track of how many we added to existing stacks
        int amountAddedToExistingStacks = 0;
        if (itemDataComingFrom.item.maxStackSize > 1 && InventoryContainsSameItem(itemDataComingFrom))
            amountAddedToExistingStacks = AddToExistingStacks(itemDataComingFrom, itemCount, invComingFrom, shouldUpdateWeightAndVolume);
        Debug.Log(amountAddedToExistingStacks);
        // Subtract the amount we added to existing stacks from our itemCount
        itemCount -= amountAddedToExistingStacks;
        
        // If there's still some left to add
        if ((itemCount == 1 && amountAddedToExistingStacks == 0) || (itemCount > 0 && itemDataComingFrom.currentStackSize > 0))
        {
            Debug.Log("Here");
            // Create a new ItemData Object
            ItemData itemDataToAdd = gm.objectPoolManager.GetItemDataFromPool(itemDataComingFrom.item, this);

            // Transfer data to the new ItemData
            itemDataToAdd.TransferData(itemDataComingFrom, itemDataToAdd);

            if (itemDataToAdd.item.IsBag())
            {
                Bag bag = (Bag)itemDataToAdd.item;
                bag.SetupBagInventory(itemDataToAdd.bagInventory);
            }
            else if (itemDataToAdd.item.IsPortableContainer())
            {
                PortableContainer portableContainer = (PortableContainer)itemDataToAdd.item;
                portableContainer.SetupPortableContainerInventory(itemDataToAdd.bagInventory);
            }

            // Since we transferred data from the old ItemData, we need to make sure to set the currentStackSize to 1 if we were only adding one of the item
            //if (itemCount == 1)
                itemDataToAdd.currentStackSize = itemCount;

            // Add the new ItemData to this Inventory's items list
            items.Add(itemDataToAdd);

            // If we're adding an item to a container, add the new ItemData to the appropriate list (unless it's a bag on the ground)
            if (gm.uiManager.activeContainerSideBarButton != null && gm.uiManager.activeContainerSideBarButton.GetInventory().CompareTag("Item Pickup") == false
                && this == gm.uiManager.activeContainerSideBarButton.GetInventory())
            {
                gm.containerInvUI.AddItemToDirectionalListFromDirection(itemDataToAdd, gm.uiManager.activeContainerSideBarButton.directionFromPlayer);
            }
            else if (gm.uiManager.activeContainerSideBarButton == null && myInvUI == gm.containerInvUI && gm.containerInvUI.activeInventory != null && gm.containerInvUI.activeInventory.CompareTag("Item Pickup") == false)
                gm.containerInvUI.AddItemToActiveDirectionList(itemDataToAdd);

            // Set the parent of this new ItemData to the Inventory's itemsParent and set the gameObject as active
            itemDataToAdd.transform.SetParent(itemsParent);
            itemDataToAdd.gameObject.SetActive(true);

            // If the item is a bag, add any items it had to the "new" bag object's Inventory
            if (itemDataComingFrom.item.IsBag() || itemDataComingFrom.item.IsPortableContainer())
            {
                Inventory itemDataComingFromsInv = null;
                if (itemDataComingFrom.IsPickup() && itemDataComingFrom.item.IsBag()) // If this is a bag we're picking up from the ground
                    itemDataComingFromsInv = itemDataComingFrom.bagInventory;
                else if (invComingFrom == null && itemDataComingFrom.item.IsBag())
                    itemDataComingFromsInv = gm.playerInvUI.GetInventoryFromBagEquipSlot(itemDataComingFrom);
                else
                    itemDataComingFromsInv = itemDataComingFrom.bagInventory; // If this bag is inside a container or one of the player's inventories
                
                for (int i = 0; i < itemDataComingFromsInv.items.Count; i++)
                {
                    gm.uiManager.CreateNewItemDataChild(itemDataComingFromsInv.items[i], itemDataToAdd.bagInventory, itemDataToAdd.bagInventory.itemsParent, true);
                }

                for (int i = 0; i < itemDataComingFromsInv.items.Count; i++)
                {
                    // Return the item we took out of the "old" bag back to it's object pool
                    itemDataComingFromsInv.items[i].ReturnToObjectPool();
                }
                
                // Clear out the items list of the "old" bag
                itemDataComingFromsInv.items.Clear();

                // Set the weight and volume of the "new" bag
                itemDataToAdd.bagInventory.UpdateCurrentWeightAndVolume();
            }
            
            // If this Inventory is active in the menu, create a new InventoryItem
            if (myInvUI != null && myInvUI.activeInventory == this)
            {
                // If we're adding the item to a bag on the ground, before we show the item, expand the list if it's not expanded
                if (gameObject.CompareTag("Item Pickup"))
                {
                    InventoryItem bagInvItem = myInvUI.GetBagItemFromInventory(this);
                    if (bagInvItem.disclosureWidget.isExpanded == false)
                        bagInvItem.disclosureWidget.ExpandDisclosureWidget();
                    else
                        myInvUI.ShowNewBagItem(itemDataToAdd, bagInvItem).transform.SetAsLastSibling();

                    bagInvItem.UpdateItemNumberTexts();
                }
                else // Otherwise just show a normal InventoryItem
                    myInvUI.ShowNewInventoryItem(itemDataToAdd);
            }

            #if UNITY_EDITOR
                itemDataToAdd.gameObject.name = itemDataToAdd.GetItemName(itemDataToAdd.currentStackSize);
            #endif

            itemDataToAdd.parentInventory = this;
            
            if (itemDataComingFrom.parentInventory != null && itemDataComingFrom.parentInventory.inventoryOwner != null)
                itemDataComingFrom.parentInventory.inventoryOwner.RemoveCarriedItem(itemDataComingFrom, itemCount);
            else
                itemDataComingFrom.currentStackSize -= itemCount;
        }

        if (inventoryOwner != null)
        {
            inventoryOwner.RemoveCarriedItem(itemDataComingFrom, itemCount);
            inventoryOwner.SetTotalCarriedWeightAndVolume();
        }

        UpdateCurrentWeightAndVolume();
        if (myInvUI == gm.containerInvUI && gm.containerInvUI.activeInventory == this)
            gm.containerInvUI.UpdateUI();
        else if (myInvUI == gm.playerInvUI && gm.playerInvUI.activeInventory == this)
            gm.playerInvUI.UpdateUI();

        return true;
    }

    public void RemoveItem(ItemData itemData, int itemCount, InventoryItem invItem)
    {
        // Subtract itemCount from the item's currentStackSize
        itemData.currentStackSize -= itemCount;
        if (itemData.currentStackSize < 0)
            itemData.currentStackSize = 0;

        if (invItem != null)
        {
            // Update InventoryUI and the InvItem's UI
            if (invItem.itemData != null && invItem.itemData.currentStackSize > 0)
                invItem.UpdateInventoryWeightAndVolume();
            if (invItem.myInvUI.activeInventory == this)
                invItem.myInvUI.UpdateUI();
        }

        // If there's none left:
        if (itemData.currentStackSize <= 0)
        {
            // Remove from Items list
            items.Remove(itemData);

            // Clear out the InventoryItem
            if (invItem != null)
                invItem.ClearItem();
        }

        UpdateCurrentWeightAndVolume();

        if (inventoryOwner != null)
            inventoryOwner.SetTotalCarriedWeightAndVolume();
    }

    public bool AddItemToInventory_OneAtATime(Inventory invComingFrom, ItemData itemData, InventoryItem invItem)
    {
        int stackSize = itemData.currentStackSize;
        float bagInvWeight = 0;
        float bagInvVolume = 0;

        if (itemData.item.IsBag() || itemData.item.IsPortableContainer())
        {
            bagInvWeight = itemData.bagInventory.currentWeight;
            bagInvVolume = itemData.bagInventory.currentVolume;
        }

        bool someAdded = false;
        for (int i = 0; i < stackSize; i++)
        {
            // Try to add a single item to the inventory
            if (AddItem(itemData, 1, invComingFrom, true))
            {
                // If we were able to add one, set someAdded to true
                someAdded = true;
                if (itemData.currentStackSize == 0) // If the entire stack was added
                {
                    if (itemData.item.IsBag())
                        gm.containerInvUI.RemoveBagFromGround(itemData.bagInventory);

                    // Calculate and use AP
                    if (invComingFrom != null)
                        gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, stackSize, bagInvWeight, bagInvVolume, true)));
                    else
                    {
                        GameTiles.RemoveItemData(itemData, itemData.transform.position);
                        gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, stackSize, bagInvWeight, bagInvVolume, false)));
                    }

                    // Write some flavor text
                    gm.flavorText.WriteLine_TakeItem(itemData, stackSize, invComingFrom, this);

                    if (invItem != null)
                        invItem.ClearItem();

                    return someAdded;
                }
            }
            else // If there's no longer any room, break out of the loop & update the UI numbers
            {
                if (itemData.item.IsBag())
                    gm.containerInvUI.RemoveBagFromGround(itemData.bagInventory);

                // Calculate and use AP
                int amountAdded = stackSize - itemData.currentStackSize;
                if (invComingFrom != null)
                    gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, amountAdded, bagInvWeight, bagInvVolume, true)));
                else
                    gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, amountAdded, bagInvWeight, bagInvVolume, false)));

                // Write some flavor text
                if (someAdded)
                    gm.flavorText.WriteLine_TakeItem(itemData, stackSize - itemData.currentStackSize, invComingFrom, this);

                gm.containerInvUI.UpdateUI();
                return someAdded;
            }
        }

        return someAdded;
    }

    public int AddToExistingStacks(ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom, bool shouldUpdateWeightAndVolume)
    {
        // Get the InventoryItem using the ItemData we're adding to (and make sure it has an InventoryUI)
        InventoryItem itemDatasInvItem = itemDataComingFrom.GetItemDatasInventoryItem();
        int amountAdded = 0;
        for (int i = 0; i < items.Count; i++) // The "items" list refers to our ItemData GameObjects
        {
            if (itemDataComingFrom.StackableItemsDataIsEqual(items[i], itemDataComingFrom) && items[i].currentStackSize < items[i].item.maxStackSize)
            {
                InventoryItem invItem = items[i].GetItemDatasInventoryItem();
                for (int j = 0; j < itemCount; j++)
                {
                    if (items[i] == itemDataComingFrom)
                        continue;

                    if (items[i].currentStackSize < items[i].item.maxStackSize)
                    {
                        items[i].currentStackSize++;
                        amountAdded++;
                        itemDataComingFrom.currentStackSize--;

                        if (invComingFrom != null && shouldUpdateWeightAndVolume)
                        {
                            invComingFrom.currentWeight -= Mathf.RoundToInt(itemDataComingFrom.item.weight * itemDataComingFrom.GetPercentRemaining_Decimal() * 100f) / 100f;
                            invComingFrom.currentVolume -= Mathf.RoundToInt(itemDataComingFrom.item.volume * itemDataComingFrom.GetPercentRemaining_Decimal() * 100f) / 100f;
                        }

                        if (itemDataComingFrom.currentStackSize == 0)
                        {
                            if (invItem != null)
                                invItem.UpdateItemNumberTexts();

                            if (invComingFrom != null && invComingFrom.inventoryOwner != null)
                                invComingFrom.inventoryOwner.SetTotalCarriedWeightAndVolume();

                            return amountAdded;
                        }
                    }
                    else
                        break;
                }

                if (invItem != null)
                    invItem.UpdateItemNumberTexts();

                if (invComingFrom != null && invComingFrom.inventoryOwner != null)
                    invComingFrom.inventoryOwner.SetTotalCarriedWeightAndVolume();
            }

            if (amountAdded == itemCount)
                break;
        }

        if (amountAdded > 0 && itemDatasInvItem != null)
            itemDatasInvItem.UpdateItemNumberTexts();
        
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
        if (singleItemVolumeLimit > 0 && itemData.item.volume * itemData.GetPercentRemaining_Decimal() > singleItemVolumeLimit)
        {
            if (inventoryOwner.isNPC == false)
                gm.flavorText.WriteLine_ItemTooLarge(inventoryOwner, itemData, this);
            return false;
        }

        float itemWeight = itemData.item.weight * itemData.GetPercentRemaining_Decimal() * itemCount;
        float itemVolume = itemData.item.volume * itemData.GetPercentRemaining_Decimal() * itemCount;

        if (itemData.item.IsBag() || itemData.item.IsPortableContainer())
        {
            itemWeight += itemData.bagInventory.currentWeight;
            itemVolume += itemData.bagInventory.currentVolume;
        }

        itemWeight = Mathf.RoundToInt(itemWeight * 100f) / 100f;
        itemVolume = Mathf.RoundToInt(itemVolume * 100f) / 100f;

        if (((inventoryOwner != null && this == inventoryOwner.personalInventory) || maxWeight - currentWeight >= itemWeight) && maxVolume - currentVolume >= itemVolume)
            return true;

        Debug.Log("Not enough room in " + name + " inventory for " + itemCount + " " + itemData.GetItemName(itemCount));
        return false;
    }

    public bool HasEnough(ItemData itemData, int itemAmountRequired)
    {
        int itemCountInInventory = 0;

        // Add up how much of this Item that we have in our Inventory
        for (int i = 0; i < myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count; i++)
        {
            if (myInvUI.inventoryItemObjectPool.activePooledInventoryItems[i].itemData.item == itemData.item)
                itemCountInInventory += myInvUI.inventoryItemObjectPool.activePooledInventoryItems[i].itemData.currentStackSize;
        }

        if (itemCountInInventory >= itemAmountRequired)
            return true;

        return false;
    }

    public void DropExcessItems()
    {
        if (items.Count <= 0)
        {
            Debug.LogWarning("You reached this method, yet there are no items to drop. There must be an error in the volume and/or weight calculations for this inventory somewhere. Fix me!");
            return;
        }

        // Organize items by weight first
        List<ItemData> itemDatas = new List<ItemData>(items);
        itemDatas.Sort((item1, item2) => (item1.item.volume * item1.GetPercentRemaining_Decimal() * item1.currentStackSize).CompareTo(item2.item.volume * item2.GetPercentRemaining_Decimal() * item2.currentStackSize));

        for (int i = itemDatas.Count - 1; i >= 0; i--)
        {
            Debug.Log("Dropping: " + itemDatas[i].GetItemName(itemDatas[i].currentStackSize));
            gm.dropItemController.ForceDropNearest(inventoryOwner, itemDatas[i], itemDatas[i].currentStackSize, this, itemDatas[i].GetItemDatasInventoryItem());
            items.Remove(itemDatas[i]);
            UpdateCurrentWeightAndVolume();
            if (currentVolume <= maxVolume && ((inventoryOwner != null && this == inventoryOwner.personalInventory) || currentWeight <= maxWeight))
                return;
        }
    }

    public void UpdateCurrentWeightAndVolume()
    {
        ResetWeightAndVolume();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item != null)
            {
                currentWeight += items[i].item.weight * items[i].GetPercentRemaining_Decimal() * items[i].currentStackSize;
                currentVolume += items[i].item.volume * items[i].GetPercentRemaining_Decimal() * items[i].currentStackSize;

                if (items[i].item.IsBag() || items[i].item.IsPortableContainer())
                {
                    for (int j = 0; j < items[i].bagInventory.items.Count; j++)
                    {
                        if (items[i].bagInventory.items[j].item != null)
                        {
                            currentWeight += items[i].bagInventory.items[j].item.weight * items[i].bagInventory.items[j].GetPercentRemaining_Decimal() * items[i].bagInventory.items[j].currentStackSize;
                            currentVolume += items[i].bagInventory.items[j].item.volume * items[i].bagInventory.items[j].GetPercentRemaining_Decimal() * items[i].bagInventory.items[j].currentStackSize;

                            if (items[i].bagInventory.items[j].item.IsBag() || items[i].bagInventory.items[j].item.IsPortableContainer())
                            {
                                for (int k = 0; k < items[i].bagInventory.items[j].bagInventory.items.Count; k++)
                                {
                                    if (items[i].bagInventory.items[j].bagInventory.items[k].item != null)
                                    {
                                        currentWeight += items[i].bagInventory.items[j].bagInventory.items[k].item.weight * items[i].bagInventory.items[j].bagInventory.items[k].GetPercentRemaining_Decimal() * items[i].bagInventory.items[j].bagInventory.items[k].currentStackSize;
                                        currentVolume += items[i].bagInventory.items[j].bagInventory.items[k].item.volume * items[i].bagInventory.items[j].bagInventory.items[k].GetPercentRemaining_Decimal() * items[i].bagInventory.items[j].bagInventory.items[k].currentStackSize;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        currentWeight = Mathf.RoundToInt(currentWeight * 100f) / 100f;
        currentVolume = Mathf.RoundToInt(currentVolume * 100f) / 100f;
    }

    public void ResetWeightAndVolume()
    {
        currentVolume = 0;
        currentWeight = 0;
    }
}

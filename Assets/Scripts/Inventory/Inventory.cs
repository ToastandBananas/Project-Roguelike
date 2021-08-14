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
                }
            }

            hasBeenInitialized = true;
        }
    }

    public bool AddItem(InventoryItem invItemComingFrom, ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom, bool shouldUpdateWeightAndVolume)
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

        // Subtract the amount we added to existing stacks from our itemCount
        itemCount -= amountAddedToExistingStacks;

        // If there's still some left to add
        if ((itemCount == 1 && amountAddedToExistingStacks == 0) || (itemCount > 1 && itemDataComingFrom.currentStackSize > 0))
        {
            // Create a new ItemData Object
            ItemData itemDataToAdd = null;
            if (itemDataComingFrom.item.IsBag() || itemDataComingFrom.item.IsPortableContainer())
            {
                itemDataToAdd = gm.objectPoolManager.itemDataContainerObjectPool.GetPooledItemData();
                itemDataToAdd.bagInventory.items.Clear();
            }
            else
                itemDataToAdd = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();

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
            if (itemCount == 1)
                itemDataToAdd.currentStackSize = 1;

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
                    gm.uiManager.CreateNewItemDataChild(itemDataComingFromsInv.items[i], itemDataToAdd.bagInventory, true);
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
                    {
                        InventoryItem newInvItem = myInvUI.ShowNewBagItem(itemDataToAdd, bagInvItem);
                        newInvItem.transform.SetAsLastSibling();
                    }

                    bagInvItem.UpdateItemNumberTexts();
                }
                else // Otherwise just show a normal InventoryItem
                    myInvUI.ShowNewInventoryItem(itemDataToAdd);
            }

            #if UNITY_EDITOR
                itemDataToAdd.gameObject.name = itemDataToAdd.itemName;
            #endif

            // If we're only taking 1 count of the item, subtract 1 from the currentStackSize, otherwise it should now be 0
            if (itemCount == 1)
                itemDataComingFrom.currentStackSize--;
            else
                itemDataComingFrom.currentStackSize = 0;
        }

        return true;
    }

    public void RemoveItem(ItemData itemData, int itemCount, InventoryItem invItem)
    {
        // Update InventoryUI
        if (invItem != null && invItem.myInvUI.activeInventory == this)
            invItem.myInvUI.UpdateUI();

        // Subtract itemCount from the item's currentStackSize
        itemData.currentStackSize -= itemCount;
        if (itemData.currentStackSize < 0)
            itemData.currentStackSize = 0;

        if (invItem != null)
            invItem.UpdateInventoryWeightAndVolume();

        // If there's none left:
        if (itemData.currentStackSize <= 0)
        {
            // Remove from Items list
            items.Remove(itemData);

            // Clear out the InventoryItem
            if (invItem != null)
                invItem.ClearItem();
        }
    }

    public int AddToExistingStacks(ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom, bool shouldUpdateWeightAndVolume)
    {
        int amountAdded = 0;
        for (int i = 0; i < items.Count; i++) // The "items" list refers to our ItemData GameObjects
        {
            if (itemDataComingFrom.StackableItemsDataIsEqual(items[i], itemDataComingFrom) && items[i].currentStackSize < items[i].item.maxStackSize)
            {
                // Get the InventoryItem using the ItemData we're adding to (and make sure it has an InventoryUI)
                if (myInvUI == null) myInvUI = invComingFrom.myInvUI;
                InventoryItem itemDatasInvItem = myInvUI.GetItemDatasInventoryItem(items[i]);

                for (int j = 0; j < itemCount; j++)
                {
                    if (items[i].currentStackSize < items[i].item.maxStackSize)
                    {
                        items[i].currentStackSize++;
                        amountAdded++;
                        itemDataComingFrom.currentStackSize--;

                        if (invComingFrom != null && shouldUpdateWeightAndVolume)
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
        if (singleItemVolumeLimit > 0 && itemData.item.volume > singleItemVolumeLimit)
        {
            Debug.Log(itemData.itemName + " is too large to fit in this inventory.");
            return false;
        }

        float itemWeight = Mathf.RoundToInt(itemData.item.weight * itemCount * 100f) / 100f;
        float itemVolume = Mathf.RoundToInt(itemData.item.volume * itemCount * 100f) / 100f;

        if (itemData.item.IsBag() || itemData.item.IsPortableContainer())
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
        
        if (maxWeight - currentWeight >= itemWeight && maxVolume - currentVolume >= itemVolume)
            return true;

        Debug.Log("Not enough room in inventory.");
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

    public void UpdateCurrentWeightAndVolume()
    {
        ResetWeightAndVolume();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item != null)
            {
                currentWeight += Mathf.RoundToInt(items[i].item.weight * items[i].currentStackSize * 100f) / 100f;
                currentVolume += Mathf.RoundToInt(items[i].item.volume * items[i].currentStackSize * 100f) / 100f;

                if (items[i].item.IsBag() || items[i].item.IsPortableContainer())
                {
                    for (int j = 0; j < items[i].bagInventory.items.Count; j++)
                    {
                        if (items[i].bagInventory.items[j].item != null)
                        {
                            currentWeight += Mathf.RoundToInt(items[i].bagInventory.items[j].item.weight * items[i].bagInventory.items[j].currentStackSize * 100f) / 100f;
                            currentVolume += Mathf.RoundToInt(items[i].bagInventory.items[j].item.volume * items[i].bagInventory.items[j].currentStackSize * 100f) / 100f;

                            if (items[i].bagInventory.items[j].item.IsBag() || items[i].bagInventory.items[j].item.IsPortableContainer())
                            {
                                for (int k = 0; k < items[i].bagInventory.items[j].bagInventory.items.Count; k++)
                                {
                                    if (items[i].bagInventory.items[j].bagInventory.items[k].item != null)
                                    {
                                        currentWeight += Mathf.RoundToInt(items[i].bagInventory.items[j].bagInventory.items[k].item.weight * items[i].bagInventory.items[j].bagInventory.items[k].currentStackSize * 100f) / 100f;
                                        currentVolume += Mathf.RoundToInt(items[i].bagInventory.items[j].bagInventory.items[k].item.volume * items[i].bagInventory.items[j].bagInventory.items[k].currentStackSize * 100f) / 100f;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void ResetWeightAndVolume()
    {
        currentVolume = 0;
        currentWeight = 0;
    }
}

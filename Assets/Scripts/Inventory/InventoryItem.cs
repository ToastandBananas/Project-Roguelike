using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    public ItemData itemData;
    
    [HideInInspector] public Image slotImage;
    [HideInInspector] public RectTransform rectTransform;

    [HideInInspector] public TextMeshProUGUI itemNameText, itemAmountText, itemTypeText, itemWeightText, itemVolumeText;

    [HideInInspector] public Inventory myInventory;

    GameManager gm;

    public void Init()
    {
        slotImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        itemNameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        itemAmountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        itemTypeText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        itemWeightText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        itemVolumeText = transform.GetChild(4).GetComponent<TextMeshProUGUI>();

        gm = GameManager.instance;
    }

    public void Reset()
    {
        itemData = null;
    }

    public void ClearItem()
    {
        gm.containerInvUI.RemoveItemFromList(itemData);

        if (myInventory != null)
            myInventory.items.Remove(itemData);

        if (itemData.CompareTag("Item Data Object"))
        {
            itemData.transform.SetParent(gm.objectPoolManager.itemDataObjectPool.transform);
            if (gm.objectPoolManager.itemDataObjectPool.pooledObjects.Contains(itemData.gameObject) == false)
            {
                gm.objectPoolManager.itemDataObjectPool.pooledObjects.Add(itemData.gameObject);
                gm.objectPoolManager.itemDataObjectPool.pooledItemDatas.Add(itemData);
            }
        }

        itemData.ClearData();
        itemData.gameObject.SetActive(false);

        Reset();
        gm.containerInvUI.UpdateUINumbers();
        gameObject.SetActive(false);
    }

    public bool IsEmpty()
    {
        if (itemData == null)
            return true;

        return false;
    }

    public void UseItem(int amountToUse = 1)
    {
        if (itemData != null)
        {
            if (myInventory.HasEnough(itemData, amountToUse))
                itemData.item.Use(gm.playerManager.playerEquipmentManager, myInventory, this, amountToUse);
            else
                Debug.Log("You don't have enough " + itemData.name);
        }
    }

    public void SelectItem()
    {
        // TODO
    }

    public void PlaceSelectedItem()
    {
        // TODO
    }

    public void UpdateItemTexts()
    {
        itemAmountText.text = itemData.currentStackSize.ToString();
        itemWeightText.text = (Mathf.RoundToInt(itemData.item.weight * itemData.currentStackSize * 100f) / 100f).ToString();
        itemVolumeText.text = (Mathf.RoundToInt(itemData.item.volume * itemData.currentStackSize * 100f) / 100f).ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1) // Double click (Transfer item)
        {
            if (myInventory == null || myInventory.myInventoryUI == gm.containerInvUI) // If we're taking this item from a container or the ground
            {
                // We don't want to add items directly to the keys inv (unless it's a key) or to the current equipment inv
                if (gm.playerInvUI.activeInventory != null && gm.playerInvUI.activeInventory != gm.playerInvUI.keysInventory && itemData.item.itemType != ItemType.Key)
                {
                    if (gm.playerInvUI.activeInventory.Add(this, itemData, itemData.currentStackSize, myInventory)) // If there's room in the inventory
                    {
                        ClearItem();
                    }
                    else // If there wasn't enough room in the inventory, try adding 1 at a time, until we can't fit anymore
                    {
                        if (itemData.item.maxStackSize > 1)
                            AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.activeInventory, itemData);

                        // Now try to fit the item in other bags (if there are any)
                        if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag1Active && gm.playerInvUI.activeInventory != gm.playerInvUI.bag1Inventory)
                            AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag1Inventory, itemData);
                        
                        if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag2Active && gm.playerInvUI.activeInventory != gm.playerInvUI.bag2Inventory)
                            AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag2Inventory, itemData);
                        
                        if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag3Active && gm.playerInvUI.activeInventory != gm.playerInvUI.bag3Inventory)
                            AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag3Inventory, itemData);
                        
                        if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag4Active && gm.playerInvUI.activeInventory != gm.playerInvUI.bag4Inventory)
                            AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag4Inventory, itemData);
                        
                        if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag5Active && gm.playerInvUI.activeInventory != gm.playerInvUI.bag5Inventory)
                            AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag5Inventory, itemData);

                        // Now try to fit the item in the player's personal inventory
                        if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.activeInventory != gm.playerInvUI.personalInventory)
                            AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.personalInventory, itemData);
                    }
                }
                else if (itemData.item.itemType == ItemType.Key) // If the item is a key, add it directly to the keys inventory
                {
                    gm.playerInvUI.keysInventory.Add(this, itemData, itemData.currentStackSize, myInventory);
                    ClearItem();
                }
                else // Otherwise, add the item to the first available bag, or the personal inventory if there's no bag or no room in any of the bags
                {
                    if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag1Active)
                        AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag1Inventory, itemData);

                    if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag2Active)
                        AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag2Inventory, itemData);

                    if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag3Active)
                        AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag3Inventory, itemData);

                    if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag4Active)
                        AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag4Inventory, itemData);

                    if (itemData != null && itemData.currentStackSize > 0 && gm.playerInvUI.bag5Active)
                        AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.bag5Inventory, itemData);

                    // Now try to fit the item in the player's personal inventory
                    if (itemData != null && itemData.currentStackSize > 0)
                        AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.personalInventory, itemData);
                }

                gm.playerInvUI.UpdateUINumbers();
            }
            else // If we're taking this item from the player's inventory
            {
                if (gm.containerInvUI.activeInventory != null) // If we're trying to place the item in a container
                {
                    if (gm.containerInvUI.activeInventory.Add(this, itemData, itemData.currentStackSize, myInventory)) // Try adding the item's entire stack
                        ClearItem();
                    else if (itemData.currentStackSize > 1) // If there wasn't room for all of the items, try adding them one at a time
                    {
                        AddItemToInventory_OneAtATime(myInventory, gm.containerInvUI.activeInventory, itemData);
                        UpdateItemTexts();
                    }
                }
                else // If we're trying to place the item on the ground
                {
                    if (itemData.item.maxStackSize > 1) // Try adding to existing stacks first, if the item is stackable
                    {
                        AddToExistingStacksOnGround(itemData, itemData.currentStackSize, gm.playerInvUI.activeInventory);
                        UpdateItemTexts();
                        gm.containerInvUI.UpdateUINumbers();
                    }

                    if (itemData.currentStackSize > 0) // If there's still some left to drop
                    {
                        List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
                        if (IsRoomOnGround(itemData, itemsListAddingTo))
                        {
                            ItemPickup newItemPickup = gm.dropItemController.DropItem(gm.playerManager.transform.position, itemData, itemData.currentStackSize);
                            gm.containerInvUI.playerPositionItems.Add(newItemPickup.itemData);

                            if (gm.containerInvUI.activeDirection == Direction.Center)
                                gm.containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);

                            gm.playerInvUI.activeInventory.items.Remove(itemData);
                            gm.playerInvUI.activeInventory.currentWeight -= itemData.item.weight * itemData.currentStackSize;
                            gm.playerInvUI.activeInventory.currentWeight = Mathf.RoundToInt(gm.playerInvUI.activeInventory.currentWeight * 100f) / 100f;
                            gm.playerInvUI.activeInventory.currentVolume -= itemData.item.volume * itemData.currentStackSize;
                            gm.playerInvUI.activeInventory.currentVolume = Mathf.RoundToInt(gm.playerInvUI.activeInventory.currentVolume * 100f) / 100f;

                            ClearItem();
                        }
                        else
                            Debug.Log("Not enough room on ground to drop item...");
                    }
                    else
                        ClearItem();
                }

                gm.playerInvUI.UpdateUINumbers();
            }
        }
    }

    void AddItemToInventory_OneAtATime(Inventory invComingFrom, Inventory invAddingTo, ItemData itemData)
    {
        int stackSize = itemData.currentStackSize;
        for (int i = 0; i < stackSize; i++)
        {
            if (invAddingTo.Add(this, itemData, 1, invComingFrom))
            {
                if (itemData.currentStackSize == 0)
                {
                    ClearItem();
                    break;
                }
            }
            else
            {
                gm.containerInvUI.UpdateUINumbers();
                break; // If there's no longer any room, break out of the loop & update the UI numbers
            }
        }
    }

    void AddToExistingStacksOnGround(ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom)
    {
        List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
        for (int i = 0; i < itemsListAddingTo.Count; i++) // The Items list refers to our ItemData GameObjects
        {
            if (itemDataComingFrom.StackableItemsDataIsEqual(itemsListAddingTo[i], itemDataComingFrom))
            {
                InventoryItem itemDatasInvItem = gm.containerInvUI.GetItemDatasInventoryItem(itemsListAddingTo[i]);
                for (int j = 0; j < itemCount; j++)
                {
                    if (itemsListAddingTo[i].currentStackSize < itemsListAddingTo[i].item.maxStackSize)
                    {
                        if (IsRoomOnGround(itemDataComingFrom, itemsListAddingTo))
                        {
                            itemsListAddingTo[i].currentStackSize++;
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
                                return;
                        }
                        else
                        {
                            Debug.Log("Not enough room on ground to drop item...");
                            return;
                        }
                    }
                    else
                        break;
                }
            }
        }
    }

    bool IsRoomOnGround(ItemData itemDataComingFrom, List<ItemData> itemsListAddingTo)
    {
        if (gm.containerInvUI.emptyTileMaxVolume - gm.containerInvUI.GetTotalVolume(itemsListAddingTo) - itemDataComingFrom.item.volume >= 0)
            return true;

        return false;
    }
}

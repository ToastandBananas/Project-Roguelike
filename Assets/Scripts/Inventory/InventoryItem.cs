using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite defaultSprite, highlightedSprite;
    public ItemData itemData;
    
    [HideInInspector] public Image backgroundImage;
    [HideInInspector] public RectTransform rectTransform;

    [HideInInspector] public TextMeshProUGUI itemNameText, itemAmountText, itemTypeText, itemWeightText, itemVolumeText;

    [HideInInspector] public InventoryUI myInvUI;
    [HideInInspector] public Inventory myInventory;
    [HideInInspector] public EquipmentManager myEquipmentManager;
    [HideInInspector] public GameManager gm;

    [HideInInspector] public bool isHidden, isGhostItem;

    public void Init()
    {
        backgroundImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        itemNameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        itemAmountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        itemTypeText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        itemWeightText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        itemVolumeText = transform.GetChild(4).GetComponent<TextMeshProUGUI>();

        gm = GameManager.instance;
    }

    public void ResetInvItem()
    {
        itemData = null;
    }

    public void ClearItem()
    {
        if (isHidden)
            Show();

        myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Remove(this);

        if (gm.uiManager.activeInvItem == this)
        {
            gm.uiManager.activeInvItem = null;
            backgroundImage.sprite = defaultSprite;
        }

        gm.containerInvUI.RemoveItemFromList(itemData);

        if (myInventory != null)
            myInventory.items.Remove(itemData);

        if (itemData.CompareTag("Item Data Object"))
            itemData.ReturnToObjectPool();

        itemData.ClearData();
        itemData.gameObject.SetActive(false);

        ResetInvItem();
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
            itemData.item.Use(gm.playerManager.playerEquipmentManager, myInventory, this, amountToUse);
    }

    public void UpdateAllItemTexts()
    {
        itemNameText.text = itemData.itemName;
        itemTypeText.text = itemData.item.itemType.ToString();
        UpdateItemNumberTexts();
    }

    public void UpdateItemNumberTexts()
    {
        itemAmountText.text = itemData.currentStackSize.ToString();
        itemWeightText.text = (Mathf.RoundToInt(itemData.item.weight * itemData.currentStackSize * 100f) / 100f).ToString();
        itemVolumeText.text = (Mathf.RoundToInt(itemData.item.volume * itemData.currentStackSize * 100f) / 100f).ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isGhostItem == false)
        {
            gm.uiManager.activeInvItem = this;
            Highlight();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isGhostItem == false && gm.uiManager.activeInvItem == this)
        {
            if (gm.uiManager.selectedItems.Contains(this) == false)
                RemoveHighlight();

            gm.uiManager.activeInvItem = null;
        }
    }

    public void Highlight()
    {
        backgroundImage.sprite = highlightedSprite;
    }

    public void RemoveHighlight()
    {
        backgroundImage.sprite = defaultSprite;
    }

    public void TransferItem()
    {
        if (myEquipmentManager == null && (myInventory == null || myInventory.myInventoryUI == gm.containerInvUI)) // If we're taking this item from a container or the ground
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
                {
                    if (myEquipmentManager != null)
                    {
                        EquipmentSlot equipmentSlot = myEquipmentManager.GetEquipmentSlotFromItemData(itemData);
                        myEquipmentManager.Unequip(equipmentSlot, false);
                    }
                    else
                        ClearItem();
                }
                else if (itemData.currentStackSize > 1) // If there wasn't room for all of the items, try adding them one at a time
                {
                    AddItemToInventory_OneAtATime(myInventory, gm.containerInvUI.activeInventory, itemData);
                    UpdateItemNumberTexts();
                }
            }
            else // If we're trying to place the item on the ground
            {
                if (itemData.item.maxStackSize > 1) // Try adding to existing stacks first, if the item is stackable
                {
                    AddToExistingStacksOnGround(itemData, itemData.currentStackSize, gm.playerInvUI.activeInventory);
                    UpdateItemNumberTexts();
                    gm.containerInvUI.UpdateUINumbers();
                }

                if (itemData.currentStackSize > 0) // If there's still some left to drop or if the item's maxStackSize is 1
                {
                    List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
                    if (IsRoomOnGround(itemData, itemsListAddingTo))
                    {
                        gm.dropItemController.DropItem(gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection(), itemData, itemData.currentStackSize);

                        if (gm.playerInvUI.activeInventory != null)
                        {
                            gm.playerInvUI.activeInventory.items.Remove(itemData);
                            gm.playerInvUI.activeInventory.currentWeight -= Mathf.RoundToInt(itemData.item.weight * itemData.currentStackSize * 100f) / 100f;
                            gm.playerInvUI.activeInventory.currentVolume -= Mathf.RoundToInt(itemData.item.volume * itemData.currentStackSize * 100f) / 100f;
                        }

                        if (myEquipmentManager != null)
                        {
                            EquipmentSlot equipmentSlot = myEquipmentManager.GetEquipmentSlotFromItemData(itemData);
                            myEquipmentManager.Unequip(equipmentSlot, false);
                        }
                        else
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
                                invComingFrom.currentWeight -= Mathf.RoundToInt(itemDataComingFrom.item.weight * 100f) / 100f;
                                invComingFrom.currentVolume -= Mathf.RoundToInt(itemDataComingFrom.item.volume * 100f) / 100f;
                            }

                            if (itemDatasInvItem != null)
                                itemDatasInvItem.UpdateItemNumberTexts();

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

    public void Hide()
    {
        isHidden = true;
        backgroundImage.enabled = false;
        itemNameText.enabled = false;
        itemAmountText.enabled = false;
        itemTypeText.enabled = false;
        itemWeightText.enabled = false;
        itemVolumeText.enabled = false;
    }

    public void Show()
    {
        isHidden = false;
        backgroundImage.enabled = true;
        itemNameText.enabled = true;
        itemAmountText.enabled = true;
        itemTypeText.enabled = true;
        itemWeightText.enabled = true;
        itemVolumeText.enabled = true;
        RemoveHighlight();
    }

    bool IsRoomOnGround(ItemData itemDataComingFrom, List<ItemData> itemsListAddingTo)
    {
        if (gm.containerInvUI.emptyTileMaxVolume - gm.containerInvUI.GetTotalVolume(itemsListAddingTo) - itemDataComingFrom.item.volume >= 0)
            return true;

        return false;
    }
}

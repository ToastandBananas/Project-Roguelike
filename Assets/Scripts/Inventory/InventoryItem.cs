using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryItem : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite highlightedSprite, blueHighlightedSprite;

    [Header("Components")]
    public Image backgroundImage;
    public DisclosureWidget disclosureWidget;
    public TextMeshProUGUI itemNameText, itemAmountText, itemTypeText, itemWeightText, itemVolumeText;

    [HideInInspector] public ItemData itemData;
    [HideInInspector] public Inventory myInventory;
    [HideInInspector] public InventoryUI myInvUI;
    [HideInInspector] public InventoryItem parentInvItem;
    [HideInInspector] public EquipmentManager myEquipmentManager;
    [HideInInspector] public GameManager gm;

    [HideInInspector] public bool isHidden, isGhostItem, isItemInsideBag, isItemInsidePortableContainer;
    [HideInInspector] public bool canDragToCurrentLocation = true;
    [HideInInspector] public int originalSiblingIndex;

    public void Init()
    {
        gm = GameManager.instance;
        canDragToCurrentLocation = true;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isGhostItem == false && gm.uiManager.activeInvItem != this)
        {
            gm.uiManager.lastActiveItem = this;
            gm.uiManager.activeInvItem = this;
            Highlight();

            if (gm.contextMenu.isActive == false)
                gm.tooltipManager.ShowInventoryTooltip(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isGhostItem == false && gm.uiManager.activeInvItem == this)
        {
            if (gm.uiManager.selectedItems.Contains(this) == false)
                RemoveHighlight();

            gm.tooltipManager.HideInventoryTooltip();

            gm.uiManager.activeInvItem = null;
        }
    }

    public void ResetInvItem()
    {
        itemData = null;
        isItemInsideBag = false;
        isItemInsidePortableContainer = false;
        backgroundImage.sprite = defaultSprite;
        parentInvItem = null;
        canDragToCurrentLocation = true;

        if (disclosureWidget != null && disclosureWidget.isEnabled)
            disclosureWidget.DisableDisclosureWidget();
    }

    public void ClearItem()
    {
        if (isHidden)
            Show();

        myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Remove(this);

        // Update the scrollbar if necessary
        if (myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count > myInvUI.maxInvItems)
            myInvUI.EditInventoryItemsParentHeight(-myInvUI.invItemHeight);
        else if (myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count == myInvUI.maxInvItems)
            myInvUI.ResetInventoryItemsParentHeight();

        if (gm.uiManager.activeInvItem == this)
            gm.uiManager.activeInvItem = null;

        if (gm.uiManager.lastActiveItem == this)
            gm.uiManager.lastActiveItem = null;

        gm.containerInvUI.RemoveItemFromActiveDirectionalList(itemData);

        if (myInventory != null)
            myInventory.items.Remove(itemData);

        // If the item being cleared is a bag or portable container, contract the disclosure widget (if it's expanded)
        if (itemData != null && (itemData.item.IsBag() || itemData.item.itemType == ItemType.Container) && disclosureWidget.isExpanded)
            disclosureWidget.ContractDisclosureWidget();

        // Return the itemData on this InventoryItem back to the appropriate object pool
        if (itemData.CompareTag("Item Data Object"))
            itemData.ReturnToItemDataObjectPool();
        else if (itemData.CompareTag("Item Data Container Object"))
            itemData.ReturnToItemDataContainerObjectPool();
        else
        {
            itemData.ClearData();
            itemData.gameObject.SetActive(false);
        }

        // If this item was inside a bag or portable container's inventory and it was the last item inside of it, contract the disclosure widget (if it's expanded)
        if (parentInvItem != null && parentInvItem.itemData.bagInventory.items.Count == 0 && parentInvItem.disclosureWidget.isExpanded)
            parentInvItem.disclosureWidget.ContractDisclosureWidget();

        ResetInvItem();
        gm.containerInvUI.UpdateUI();
        gameObject.SetActive(false);
    }

    public void CollapseItem()
    {
        if (isHidden)
            Show();

        myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Remove(this);

        if (gm.uiManager.activeInvItem == this)
        {
            gm.uiManager.activeInvItem = null;
            backgroundImage.sprite = defaultSprite;
        }

        ResetInvItem();
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
        EquipmentSlot equipSlot = 0;
        if (itemData.item.IsEquipment())
        {
            if (myEquipmentManager != null)
                equipSlot = gm.playerManager.playerEquipmentManager.GetEquipmentSlotFromItemData(itemData);
            else
            {
                Equipment equipment = (Equipment)itemData.item;
                equipSlot = equipment.equipmentSlot;
            }
        }

        if (itemData != null && gm.playerManager.isMyTurn && gm.playerManager.actionQueued == false)
            itemData.item.Use(gm.playerManager.playerEquipmentManager, equipSlot, myInventory, this, amountToUse);
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

        float totalWeight = Mathf.RoundToInt(itemData.item.weight * itemData.currentStackSize * 100f) / 100f;
        float totalVolume = Mathf.RoundToInt(itemData.item.volume * itemData.currentStackSize * 100f) / 100f;

        if (itemData.item.IsBag() || itemData.item.IsPortableContainer())
        {
            for (int i = 0; i < itemData.bagInventory.items.Count; i++)
            {
                if (itemData.bagInventory.items[i].item != null)
                {
                    totalWeight += Mathf.RoundToInt(itemData.bagInventory.items[i].item.weight * itemData.bagInventory.items[i].currentStackSize * 100f) / 100f;
                    totalVolume += Mathf.RoundToInt(itemData.bagInventory.items[i].item.volume * itemData.bagInventory.items[i].currentStackSize * 100f) / 100f;

                    if (itemData.bagInventory.items[i].item.IsBag() || itemData.bagInventory.items[i].item.IsPortableContainer())
                    {
                        for (int j = 0; j < itemData.bagInventory.items[i].bagInventory.items.Count; j++)
                        {
                            totalWeight += Mathf.RoundToInt(itemData.bagInventory.items[i].bagInventory.items[j].item.weight * itemData.bagInventory.items[i].bagInventory.items[j].currentStackSize * 100f) / 100f;
                            totalVolume += Mathf.RoundToInt(itemData.bagInventory.items[i].bagInventory.items[j].item.volume * itemData.bagInventory.items[i].bagInventory.items[j].currentStackSize * 100f) / 100f;
                        }
                    }
                }
            }
        }

        itemWeightText.text = totalWeight.ToString();
        itemVolumeText.text = totalVolume.ToString();
    }

    public void Highlight()
    {
        backgroundImage.sprite = highlightedSprite;
    }

    public void RemoveHighlight()
    {
        if (isItemInsideBag)
            backgroundImage.sprite = blueHighlightedSprite;
        else
            backgroundImage.sprite = defaultSprite;
    }

    public void TransferItem()
    {
        int startingItemCount = itemData.currentStackSize;
        float bagInvWeight = 0;
        float bagInvVolume = 0;

        if (itemData.item.IsBag() || itemData.item.IsPortableContainer())
        {
            bagInvWeight = itemData.bagInventory.currentWeight;
            bagInvVolume = itemData.bagInventory.currentVolume;
        }

        // If we're taking this item from a container or the ground
        if (myEquipmentManager == null && (myInventory == null || myInventory.myInvUI == gm.containerInvUI))
        {
            // We don't want to add items directly to the keys inv (unless it's a key) or to the current equipment inv
            if (gm.playerInvUI.activeInventory != null && gm.playerInvUI.activeInventory != gm.playerInvUI.keysInventory && itemData.item.itemType != ItemType.Key && itemData.item.itemType != ItemType.Ammo)
            {
                if (gm.playerInvUI.activeInventory.AddItem(this, itemData, itemData.currentStackSize, myInventory, true)) // If there's room in the inventory
                {
                    gm.playerInvUI.activeInventory.UpdateCurrentWeightAndVolume();
                    UpdateInventoryWeightAndVolume();
                    myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.activePlayerInvSideBarButton));

                    // Calculate and use AP
                    if (myInventory != null && (itemData.item.IsBag() == false || itemData.bagInventory != gm.containerInvUI.activeInventory))
                        gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, true));
                    else
                        gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false));

                    // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                    if (itemData.item.IsBag() && gm.containerInvUI.activeInventory == itemData.bagInventory)
                        gm.containerInvUI.RemoveBagFromGround();

                    ClearItem();
                }
                else // If there wasn't enough room in the inventory, try adding 1 at a time, until we can't fit anymore
                    AddItemToOtherBags(itemData);
            }
            else if (itemData.item.itemType == ItemType.Key) // If the item is a key, add it directly to the keys inventory
            {
                gm.playerInvUI.keysInventory.AddItem(this, itemData, itemData.currentStackSize, myInventory, true);
                gm.playerInvUI.keysInventory.UpdateCurrentWeightAndVolume();
                UpdateInventoryWeightAndVolume();
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.keysSideBarButton));

                // Calculate and use AP
                if (myInventory != null)
                    gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, true));
                else
                    gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false));

                ClearItem();
            }
            else if (itemData.item.itemType == ItemType.Ammo)
            {
                if (gm.playerInvUI.quiverEquipped)
                {
                    if (gm.playerInvUI.quiverInventory.AddItem(this, itemData, itemData.currentStackSize, myInventory, true) == false)
                        AddItemToOtherBags(itemData);
                    else
                    {
                        gm.playerInvUI.quiverInventory.UpdateCurrentWeightAndVolume();
                        UpdateInventoryWeightAndVolume();
                        myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.quiverSidebarButton));

                        // Calculate and use AP
                        if (myInventory != null)
                            gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, true));
                        else
                            gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false));

                        ClearItem();
                    }
                }
                else
                    AddItemToOtherBags(itemData);
            }
            else // Otherwise, add the item to the first available bag, or the personal inventory if there's no bag or no room in any of the bags
                AddItemToOtherBags(itemData);

            gm.playerInvUI.UpdateUI();
        }
        else // If we're taking this item from the player's inventory
        {
            if (gm.containerInvUI.activeInventory != null) // If we're trying to place the item in a container
            {
                if (gm.containerInvUI.activeInventory.AddItem(this, itemData, itemData.currentStackSize, myInventory, true)) // Try adding the item's entire stack
                {
                    gm.containerInvUI.activeInventory.UpdateCurrentWeightAndVolume();
                    UpdateInventoryWeightAndVolume();
                    myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));

                    if (myEquipmentManager != null)
                    {
                        // Create a temporary ItemData object for use in the UseAPAndSetupEquipment method
                        ItemData tempItemData = gm.objectPoolManager.GetItemDataFromPool(itemData.item);
                        tempItemData.gameObject.SetActive(true);
                        tempItemData.TransferData(itemData, tempItemData);
                        if (itemData.bagInventory != null)
                        {
                            tempItemData.bagInventory.currentWeight = itemData.bagInventory.currentWeight;
                            tempItemData.bagInventory.currentVolume = itemData.bagInventory.currentVolume;
                        }

                        EquipmentSlot equipmentSlot = myEquipmentManager.GetEquipmentSlotFromItemData(itemData);
                        myEquipmentManager.Unequip(equipmentSlot, false, false);

                        // Calculate and use AP
                        myEquipmentManager.StartCoroutine(myEquipmentManager.UseAPAndSetupEquipment((Equipment)tempItemData.item, equipmentSlot, null, tempItemData));
                    }
                    else
                    {
                        // Calculate and use AP
                        if (myInventory != null)
                            gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, true));
                        else
                            gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false));

                        ClearItem();
                    }
                }
                else if (itemData.currentStackSize > 1) // If there wasn't room for all of the items, try adding them one at a time
                {
                    bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.containerInvUI.activeInventory, itemData);
                    if (someAdded)
                    {
                        gm.containerInvUI.activeInventory.UpdateCurrentWeightAndVolume();
                        UpdateInventoryWeightAndVolume();
                        myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));
                    }
                }
            }
            else // If we're trying to place the item on the ground
            {
                float startingStackSize = itemData.currentStackSize;
                if (itemData.item.maxStackSize > 1) // Try adding to existing stacks first, if the item is stackable
                {
                    AddToExistingStacksOnGround(itemData, itemData.currentStackSize, gm.playerInvUI.activeInventory);

                    if (itemData.currentStackSize < startingStackSize)
                    {
                        UpdateInventoryWeightAndVolume();
                        myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));
                    }
                }

                if (itemData.currentStackSize > 0) // If there's still some left to drop or if the item's maxStackSize is 1
                {
                    List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
                    Vector3 dropPos = gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection();

                    // Create a temporary ItemData object for use in the UseAPAndSetupEquipment method
                    ItemData tempItemData = gm.objectPoolManager.GetItemDataFromPool(itemData.item);
                    tempItemData.gameObject.SetActive(true);
                    tempItemData.TransferData(itemData, tempItemData);
                    if (itemData.item.IsBag())
                    {
                        if (myEquipmentManager != null)
                        {
                            Inventory playersInv = gm.playerInvUI.GetInventoryFromBagEquipSlot(itemData);
                            tempItemData.bagInventory.currentWeight = playersInv.currentWeight;
                            tempItemData.bagInventory.currentVolume = playersInv.currentVolume;
                        }
                        else
                        {
                            tempItemData.bagInventory.currentWeight = itemData.bagInventory.currentWeight;
                            tempItemData.bagInventory.currentVolume = itemData.bagInventory.currentVolume;
                        }
                    }

                    if (gm.uiManager.IsRoomOnGround(itemData, itemsListAddingTo, dropPos))
                    {
                        gm.dropItemController.DropItem(dropPos, itemData, itemData.currentStackSize, gm.playerInvUI.activeInventory, this);

                        if (gm.playerInvUI.activeInventory != null)
                            gm.playerInvUI.activeInventory.items.Remove(itemData);

                        UpdateInventoryWeightAndVolume();

                        if (myEquipmentManager != null)
                        {
                            EquipmentSlot equipmentSlot = myEquipmentManager.GetEquipmentSlotFromItemData(itemData);
                            myEquipmentManager.Unequip(equipmentSlot, false, false);

                            // Calculate and use AP
                            myEquipmentManager.StartCoroutine(myEquipmentManager.UseAPAndSetupEquipment((Equipment)tempItemData.item, equipmentSlot, null, tempItemData));
                        }
                        else
                        {
                            // Calculate and use AP
                            gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false));

                            ClearItem();
                        }
                    }
                }
                else
                {
                    // Calculate and use AP
                    gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false));

                    ClearItem();
                }
            }

            gm.playerInvUI.UpdateUI();
        }
    }

    void AddItemToOtherBags(ItemData itemToAdd)
    {
        // Cache the item in case the itemData gets cleared out before we can do the add item effect
        Item itemAdding = itemToAdd.item;

        // If the item is ammunition, try adding here first
        if (itemAdding.itemType == ItemType.Ammo && gm.playerInvUI.quiverEquipped && itemToAdd.currentStackSize > 0)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.quiverInventory, itemToAdd);
            if (someAdded)
            {
                gm.playerInvUI.quiverInventory.UpdateCurrentWeightAndVolume();
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.quiverSidebarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemAdding.IsBag() && gm.containerInvUI.activeInventory == itemToAdd.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        // Try adding to the active inventory first
        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.activeInventory != null && (gm.playerInvUI.activeInventory != gm.playerInvUI.keysInventory || itemToAdd.item.itemType == ItemType.Key) && itemToAdd.item.maxStackSize > 1
            && gm.playerInvUI.activeInventory != gm.playerInvUI.quiverInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.activeInventory, itemToAdd);
            if (someAdded)
            {
                gm.playerInvUI.activeInventory.UpdateCurrentWeightAndVolume();
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.activePlayerInvSideBarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemAdding.IsBag() && gm.containerInvUI.activeInventory == itemToAdd.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        // Now try to fit the item in other equipped bags
        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.backpackEquipped && gm.playerInvUI.activeInventory != gm.playerInvUI.backpackInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.backpackInventory, itemToAdd);
            if (someAdded)
            {
                gm.playerInvUI.backpackInventory.UpdateCurrentWeightAndVolume();
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemAdding.IsBag() && gm.containerInvUI.activeInventory == itemToAdd.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.leftHipPouchEquipped && gm.playerInvUI.activeInventory != gm.playerInvUI.leftHipPouchInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.leftHipPouchInventory, itemToAdd);
            if (someAdded)
            {
                gm.playerInvUI.leftHipPouchInventory.UpdateCurrentWeightAndVolume();
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemAdding.IsBag() && gm.containerInvUI.activeInventory == itemToAdd.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.rightHipPouchEquipped && gm.playerInvUI.activeInventory != gm.playerInvUI.rightHipPouchInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.rightHipPouchInventory, itemToAdd);
            if (someAdded)
            {
                gm.playerInvUI.rightHipPouchInventory.UpdateCurrentWeightAndVolume();
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemAdding.IsBag() && gm.containerInvUI.activeInventory == itemToAdd.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        // Now try to fit the item in the player's personal inventory
        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.activeInventory != gm.playerInvUI.personalInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.personalInventory, itemToAdd);
            if (someAdded)
            {
                gm.playerInvUI.personalInventory.UpdateCurrentWeightAndVolume();
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.personalInventorySideBarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemAdding.IsBag() && gm.containerInvUI.activeInventory == itemToAdd.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }
    }

    bool AddItemToInventory_OneAtATime(Inventory invComingFrom, Inventory invAddingTo, ItemData itemData)
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
            if (invAddingTo.AddItem(this, itemData, 1, invComingFrom, true))
            {
                // If we were able to add one, set someAdded to true
                someAdded = true;
                if (itemData.currentStackSize == 0) // If the entire stack was added
                {
                    gm.containerInvUI.GetItemDatasInventoryItem(itemData).UpdateInventoryWeightAndVolume();
                    gm.containerInvUI.RemoveBagFromGround();

                    // Calculate and use AP
                    if (invComingFrom != null)
                        gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, stackSize, bagInvWeight, bagInvVolume, true));
                    else
                        gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, stackSize, bagInvWeight, bagInvVolume, false));

                    ClearItem();
                    return someAdded;
                }
            }
            else // If there's no longer any room, break out of the loop & update the UI numbers
            {
                gm.containerInvUI.RemoveBagFromGround();

                // Calculate and use AP
                int amountAdded = stackSize - itemData.currentStackSize;
                if (invComingFrom != null)
                    gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, amountAdded, bagInvWeight, bagInvVolume, true));
                else
                    gm.uiManager.StartCoroutine(gm.uiManager.UseAPAndTransferItem(itemData.item, amountAdded, bagInvWeight, bagInvVolume, false));

                gm.containerInvUI.UpdateUI();
                return someAdded;
            }
        }

        return someAdded;
    }

    void AddToExistingStacksOnGround(ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom)
    {
        List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
        for (int i = 0; i < itemsListAddingTo.Count; i++) // The Items list refers to our ItemData GameObjects
        {
            if (itemDataComingFrom.StackableItemsDataIsEqual(itemsListAddingTo[i], itemDataComingFrom))
            {
                InventoryItem itemDatasInvItem = gm.containerInvUI.GetItemDatasInventoryItem(itemsListAddingTo[i]);
                Vector3 dropPos = gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection();

                for (int j = 0; j < itemCount; j++)
                {
                    if (itemsListAddingTo[i].currentStackSize < itemsListAddingTo[i].item.maxStackSize)
                    {
                        if (gm.uiManager.IsRoomOnGround(itemDataComingFrom, itemsListAddingTo, dropPos))
                        {
                            itemsListAddingTo[i].currentStackSize++;
                            itemDataComingFrom.currentStackSize--;

                            if (invComingFrom != null)
                            {
                                invComingFrom.currentWeight -= Mathf.RoundToInt(itemDataComingFrom.item.weight * 100f) / 100f;
                                invComingFrom.currentVolume -= Mathf.RoundToInt(itemDataComingFrom.item.volume * 100f) / 100f;
                            }

                            if (itemDatasInvItem != null)
                                itemDatasInvItem.UpdateInventoryWeightAndVolume();

                            if (itemDataComingFrom.currentStackSize == 0)
                                return;
                        }
                        else
                            return;
                    }
                    else
                        break;
                }
            }
        }
    }

    public void UpdateInventoryWeightAndVolume()
    {
        if (itemData.bagInventory != null)
            itemData.bagInventory.UpdateCurrentWeightAndVolume();

        if (parentInvItem != null)
        {
            parentInvItem.itemData.bagInventory.UpdateCurrentWeightAndVolume();
            parentInvItem.UpdateItemNumberTexts();

            if (parentInvItem.parentInvItem != null)
            {
                parentInvItem.parentInvItem.itemData.bagInventory.UpdateCurrentWeightAndVolume();
                parentInvItem.parentInvItem.UpdateItemNumberTexts();

                if (parentInvItem.parentInvItem.myInventory != null)
                    parentInvItem.parentInvItem.myInventory.UpdateCurrentWeightAndVolume();
            }
            else if (parentInvItem.myInventory != null)
                parentInvItem.myInventory.UpdateCurrentWeightAndVolume();
        }
        else if (myInventory != null)
            myInventory.UpdateCurrentWeightAndVolume();

        UpdateItemNumberTexts();
        myInvUI.UpdateUI();
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

        if ((itemData.item.itemType == ItemType.Bag || itemData.item.itemType == ItemType.Container) && disclosureWidget != null)
        {
            if (disclosureWidget.isExpanded)
                disclosureWidget.ContractDisclosureWidget();

            disclosureWidget.DisableDisclosureWidget();
        }
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

        if (itemData != null && myEquipmentManager == null && (itemData.item.itemType == ItemType.Bag || itemData.item.itemType == ItemType.Container))
            disclosureWidget.EnableDisclosureWidget();
    }
}

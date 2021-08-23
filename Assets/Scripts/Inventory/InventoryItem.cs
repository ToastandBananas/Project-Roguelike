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

            gm.tooltipManager.HideAllTooltips();

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
        myInventory = null;
        myEquipmentManager = null;

        if (disclosureWidget != null && disclosureWidget.isEnabled)
            disclosureWidget.DisableDisclosureWidget();
    }

    public void ClearItem()
    {
        if (isHidden)
            Show();

        myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Remove(this);

        // Update the scrollbar if necessary
        if (myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count > myInvUI.MaxInvItems())
            myInvUI.EditInventoryItemsParentHeight(myInvUI.InvItemHeight());
        else if (myInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count == myInvUI.MaxInvItems())
            myInvUI.ResetInventoryItemsParentHeight();

        if (gm.uiManager.activeInvItem == this)
            gm.uiManager.activeInvItem = null;

        if (gm.uiManager.lastActiveItem == this)
            gm.uiManager.lastActiveItem = null;

        gm.containerInvUI.RemoveItemFromActiveDirectionalList(itemData);

        if (myInventory != null)
            myInventory.items.Remove(itemData);

        // If the item being cleared is a bag or portable container, contract the disclosure widget (if it's expanded)
        if (itemData != null && disclosureWidget != null && disclosureWidget.isExpanded)
            disclosureWidget.ContractDisclosureWidget();

        // Return the itemData on this InventoryItem back to the appropriate object pool
        if (itemData != null)
        {
            if (itemData.CompareTag("Item Data Object"))
                itemData.ReturnToItemDataObjectPool();
            else if (itemData.CompareTag("Item Data Container Object"))
                itemData.ReturnToItemDataContainerObjectPool();
            else
            {
                itemData.ClearData();
                itemData.gameObject.SetActive(false);
            }
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

    public void UseItem(int amountToUse = 1, PartialAmount partialAmountToUse = PartialAmount.Whole)
    {
        if (itemData.item.isUsable)
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

            if (itemData != null && gm.playerManager.isMyTurn && gm.playerManager.actionsQueued == 0)
                itemData.item.Use(gm.playerManager, myInventory, this, itemData, amountToUse, partialAmountToUse, equipSlot);
        }
    }

    public void UpdateAllItemTexts()
    {
        if (gm.playerManager.carriedItems.Contains(itemData))
            itemNameText.text = "<b>(Carried)</b> ";
        else if (gm.playerManager.personalInventory.items.Contains(itemData))
            itemNameText.text = "<b>(Pockets)</b> ";
        else if (gm.playerManager.equipmentManager.ItemIsEquipped(itemData) && (itemData.item.IsWeapon() || itemData.item.IsShield()))
            itemNameText.text = GetSheathedVerb(itemData);
        else
            itemNameText.text = "";

        itemNameText.text += itemData.GetItemName(itemData.currentStackSize);

        itemTypeText.text = Utilities.FormatEnumStringWithSpaces(itemData.item.itemType.ToString(), false);
        UpdateItemNumberTexts();
    }

    public void UpdateItemNumberTexts()
    {
        itemAmountText.text = itemData.currentStackSize.ToString();

        float totalWeight = itemData.item.weight * itemData.currentStackSize;
        float totalVolume = itemData.item.volume * itemData.currentStackSize;

        if (itemData.item.IsBag() || itemData.item.IsPortableContainer())
        {
            for (int i = 0; i < itemData.bagInventory.items.Count; i++)
            {
                if (itemData.bagInventory.items[i].item != null)
                {
                    totalWeight += itemData.bagInventory.items[i].item.weight * itemData.bagInventory.items[i].currentStackSize;
                    totalVolume += itemData.bagInventory.items[i].item.volume * itemData.bagInventory.items[i].currentStackSize;

                    if (itemData.bagInventory.items[i].item.IsBag() || itemData.bagInventory.items[i].item.IsPortableContainer())
                    {
                        for (int j = 0; j < itemData.bagInventory.items[i].bagInventory.items.Count; j++)
                        {
                            totalWeight += itemData.bagInventory.items[i].bagInventory.items[j].item.weight * itemData.bagInventory.items[i].bagInventory.items[j].currentStackSize;
                            totalVolume += itemData.bagInventory.items[i].bagInventory.items[j].item.volume * itemData.bagInventory.items[i].bagInventory.items[j].currentStackSize;
                        }
                    }
                }
            }
        }

        totalWeight = Mathf.RoundToInt(totalWeight * 100f) / 100f;
        totalVolume = Mathf.RoundToInt(totalVolume * 100f) / 100f;
        itemWeightText.text = totalWeight.ToString();
        itemVolumeText.text = totalVolume.ToString();
    }

    string GetSheathedVerb(ItemData weaponItemData)
    {
        if ((itemData == gm.playerManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem] && gm.playerManager.equipmentManager.LeftWeaponSheathed())
            || (itemData == gm.playerManager.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem] && gm.playerManager.equipmentManager.RightWeaponSheathed()))
        {
            if (itemData.item.IsWeapon())
            {
                Weapon weapon = (Weapon)itemData.item;
                if (weapon.weaponType == WeaponType.Sword || weapon.weaponType == WeaponType.Dagger)
                    return "<b>(Sheathed)</b> ";
            }
            return "<b>(Stowed)</b> ";
        }
        return "";
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
            // We don't want to add items directly to the keys inv (unless it's a key) or to the quiver (unless it's ammunition)
            if (gm.playerInvUI.activeInventory != null && gm.playerInvUI.activeInventory != gm.playerManager.keysInventory && itemData.item.itemType != ItemType.Key && itemData.item.itemType != ItemType.Ammo)
            {
                if (gm.playerInvUI.activeInventory.AddItem(itemData, itemData.currentStackSize, myInventory, true)) // If there's room in the inventory
                {
                    gm.playerInvUI.activeInventory.UpdateCurrentWeightAndVolume();
                    UpdateInventoryWeightAndVolume();
                    myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.activePlayerInvSideBarButton));

                    // Calculate and use AP
                    if (myInventory != null && (itemData.item.IsBag() == false || itemData.bagInventory != gm.containerInvUI.activeInventory))
                        gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, true)));
                    else
                    {
                        GameTiles.RemoveItemData(itemData, itemData.transform.position);
                        gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false)));
                    }

                    // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                    if (itemData.item.IsBag() && gm.containerInvUI.activeInventory == itemData.bagInventory)
                        gm.containerInvUI.RemoveBagFromGround(itemData.bagInventory);

                    // Write some flavor text
                    gm.flavorText.WriteLine_TakeItem(itemData, startingItemCount, myInventory, gm.playerInvUI.activeInventory);

                    ClearItem();
                }
                else // If there wasn't enough room in the inventory, try adding 1 at a time, until we can't fit anymore
                    gm.playerManager.AddItemToOtherBags(itemData, this);
            }
            else if (itemData.item.itemType == ItemType.Key) // If the item is a key, add it directly to the keys inventory
            {
                gm.playerManager.keysInventory.AddItem(itemData, itemData.currentStackSize, myInventory, true);
                gm.playerManager.keysInventory.UpdateCurrentWeightAndVolume();
                UpdateInventoryWeightAndVolume();
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.keysSideBarButton));

                // Calculate and use AP
                if (myInventory != null)
                    gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, true)));
                else
                {
                    GameTiles.RemoveItemData(itemData, itemData.transform.position);
                    gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false)));
                }

                // Write some flavor text
                gm.flavorText.WriteLine_TakeItem(itemData, startingItemCount, myInventory, gm.playerManager.keysInventory);

                ClearItem();
            }
            else if (itemData.item.itemType == ItemType.Ammo)
            {
                if (gm.playerManager.quiverInventory != null)
                {
                    if (gm.playerManager.quiverInventory.AddItem(itemData, itemData.currentStackSize, myInventory, true) == false)
                        gm.playerManager.AddItemToOtherBags(itemData, this);
                    else
                    {
                        gm.playerManager.quiverInventory.UpdateCurrentWeightAndVolume();
                        UpdateInventoryWeightAndVolume();
                        myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.quiverSidebarButton));

                        // Calculate and use AP
                        if (myInventory != null)
                            gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, true)));
                        else
                        {
                            GameTiles.RemoveItemData(itemData, itemData.transform.position);
                            gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false)));
                        }

                        // Write some flavor text
                        gm.flavorText.WriteLine_TakeItem(itemData, startingItemCount, myInventory, gm.playerManager.quiverInventory);

                        ClearItem();
                    }
                }
                else
                    gm.playerManager.AddItemToOtherBags(itemData, this);
            }
            else // Otherwise, add the item to the first available bag, or the personal inventory if there's no bag or no room in any of the bags
                gm.playerManager.AddItemToOtherBags(itemData, this);

            gm.playerInvUI.UpdateUI();
        }
        else // If we're taking this item from the player's inventory
        {
            if (gm.containerInvUI.activeInventory != null) // If we're trying to place the item in a container
            {
                if (gm.containerInvUI.activeInventory.CompareTag("Dead Body"))
                {
                    Debug.Log("You cannot store items on a dead body.");
                    return;
                }

                if (gm.containerInvUI.activeInventory.AddItem(itemData, itemData.currentStackSize, myInventory, true)) // Try adding the item's entire stack
                {
                    gm.containerInvUI.activeInventory.UpdateCurrentWeightAndVolume();
                    UpdateInventoryWeightAndVolume();
                    myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));

                    if (myEquipmentManager != null)
                    {
                        // Create a temporary ItemData object for use in the SetupEquipment method
                        ItemData tempItemData = gm.objectPoolManager.GetItemDataFromPool(itemData.item, gm.containerInvUI.activeInventory);
                        tempItemData.gameObject.SetActive(true);
                        tempItemData.TransferData(itemData, tempItemData);
                        if (itemData.bagInventory != null)
                        {
                            tempItemData.bagInventory.currentWeight = itemData.bagInventory.currentWeight;
                            tempItemData.bagInventory.currentVolume = itemData.bagInventory.currentVolume;
                        }

                        EquipmentSlot equipmentSlot = myEquipmentManager.GetEquipmentSlotFromItemData(itemData);
                        myEquipmentManager.Unequip(equipmentSlot, false, false, false);

                        // Write some flavor text
                        gm.flavorText.WriteLine_TransferItem(itemData, startingItemCount, myEquipmentManager, myInventory, gm.containerInvUI.activeInventory);

                        // Calculate and use AP
                        float bagWeight = 0;
                        if (tempItemData.bagInventory != null)
                            bagWeight += tempItemData.bagInventory.currentWeight;

                        gm.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetEquipAPCost((Equipment)tempItemData.item, bagWeight)));
                        myEquipmentManager.StartCoroutine(myEquipmentManager.SetUpEquipment(null, tempItemData, (Equipment)tempItemData.item, equipmentSlot, false));
                    }
                    else
                    {
                        // Calculate and use AP
                        if (myInventory != null)
                            gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, true)));
                        else
                            gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false)));

                        // Write some flavor text
                        gm.flavorText.WriteLine_TransferItem(itemData, startingItemCount, myEquipmentManager, myInventory, gm.containerInvUI.activeInventory);

                        ClearItem();
                    }
                }
                else if (itemData.currentStackSize > 1) // If there wasn't room for all of the items, try adding them one at a time
                {
                    bool someAdded = gm.containerInvUI.activeInventory.AddItemToInventory_OneAtATime(myInventory, itemData, this);
                    if (someAdded)
                    {
                        gm.containerInvUI.activeInventory.UpdateCurrentWeightAndVolume();
                        UpdateInventoryWeightAndVolume();
                        myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));

                        // Write some flavor text
                        gm.flavorText.WriteLine_TransferItem(itemData, startingItemCount - itemData.currentStackSize, myEquipmentManager, myInventory, gm.containerInvUI.activeInventory);
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

                    // Create a temporary ItemData object for use in the SetupEquipment method
                    ItemData tempItemData = gm.objectPoolManager.GetItemDataFromPool(itemData.item, null);
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
                        gm.dropItemController.DropItem(gm.playerManager, dropPos, itemData, itemData.currentStackSize, gm.playerInvUI.activeInventory, this);

                        if (gm.playerInvUI.activeInventory != null)
                            gm.playerInvUI.activeInventory.items.Remove(itemData);

                        UpdateInventoryWeightAndVolume();

                        if (myEquipmentManager != null)
                        {
                            EquipmentSlot equipmentSlot = myEquipmentManager.GetEquipmentSlotFromItemData(itemData);
                            EquipmentManager equipmentManager = myEquipmentManager;
                            myEquipmentManager.Unequip(equipmentSlot, false, false, false);

                            // Calculate and use AP
                            float bagWeight = 0;
                            if (tempItemData.bagInventory != null)
                                bagWeight += tempItemData.bagInventory.currentWeight;
                            gm.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetEquipAPCost((Equipment)tempItemData.item, bagWeight)));
                            equipmentManager.StartCoroutine(equipmentManager.SetUpEquipment(null, tempItemData, (Equipment)tempItemData.item, equipmentSlot, false));
                        }
                        else
                        {
                            // Calculate and use AP
                            gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false)));

                            ClearItem();
                        }
                    }
                }
                else
                {
                    // Calculate and use AP
                    gm.uiManager.StartCoroutine(gm.apManager.UseAP(gm.playerManager, gm.apManager.GetTransferItemCost(itemData.item, startingItemCount, bagInvWeight, bagInvVolume, false)));

                    ClearItem();
                }
            }

            gm.playerInvUI.UpdateUI();
        }
    }

    void AddToExistingStacksOnGround(ItemData itemDataComingFrom, int itemCount, Inventory invComingFrom)
    {
        List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
        for (int i = 0; i < itemsListAddingTo.Count; i++) // The Items list refers to our ItemData GameObjects
        {
            if (itemDataComingFrom.StackableItemsDataIsEqual(itemsListAddingTo[i], itemDataComingFrom))
            {
                InventoryItem itemDatasInvItem = itemsListAddingTo[i].GetItemDatasInventoryItem();
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
                            {
                                // Write some flavor text
                                gm.flavorText.WriteLine_DropItem(gm.playerManager, itemDataComingFrom, itemCount);
                                return;
                            }
                        }
                        else
                        {
                            // Write some flavor text
                            gm.flavorText.WriteLine_DropItem(gm.playerManager, itemDataComingFrom, itemCount - itemDataComingFrom.currentStackSize);
                            return;
                        }
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

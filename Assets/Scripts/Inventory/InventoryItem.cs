using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryItem : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite highlightedSprite, rightArrowSprite, downArrowSprite;

    [Header("Components")]
    public Image backgroundImage;
    public Image disclosureWidget;
    public TextMeshProUGUI itemNameText, itemAmountText, itemTypeText, itemWeightText, itemVolumeText;

    [HideInInspector] public ItemData itemData;
    [HideInInspector] public InventoryUI myInvUI;
    [HideInInspector] public Inventory myInventory;
    [HideInInspector] public EquipmentManager myEquipmentManager;
    [HideInInspector] public GameManager gm;

    [HideInInspector] public bool isHidden, isGhostItem;

    LayerMask dropBagObstacleMask;

    public void Init()
    {
        gm = GameManager.instance;

        dropBagObstacleMask = LayerMask.GetMask("Interactable", "Interactable Objects", "Objects", "Walls", "Character");
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isGhostItem == false && gm.uiManager.activeInvItem != this)
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

    public void ResetInvItem()
    {
        itemData = null;

        if (disclosureWidget != null && disclosureWidget.enabled)
        {
            disclosureWidget.sprite = rightArrowSprite;
            disclosureWidget.enabled = false;
        }
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
            itemData.ReturnToItemDataObjectPool();
        else if (itemData.CompareTag("Item Data Container Object"))
            itemData.ReturnToItemDataContainerObjectPool();
        else
        {
            itemData.ClearData();
            itemData.gameObject.SetActive(false);
        }

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
        // If we're taking this item from a container or the ground
        if (myEquipmentManager == null && (myInventory == null || myInventory.myInventoryUI == gm.containerInvUI))
        {
            // We don't want to add items directly to the keys inv (unless it's a key) or to the current equipment inv
            if (gm.playerInvUI.activeInventory != null && gm.playerInvUI.activeInventory != gm.playerInvUI.keysInventory && itemData.item.itemType != ItemType.Key && itemData.item.itemType != ItemType.Ammo)
            {
                if (gm.playerInvUI.activeInventory.Add(this, itemData, itemData.currentStackSize, myInventory)) // If there's room in the inventory
                {
                    myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.activePlayerInvSideBarButton));

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
                gm.playerInvUI.keysInventory.Add(this, itemData, itemData.currentStackSize, myInventory);
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.keysSideBarButton));
                ClearItem();
            }
            else if (itemData.item.itemType == ItemType.Ammo)
            {
                if (gm.playerInvUI.quiverEquipped)
                {
                    if (gm.playerInvUI.quiverInventory.Add(this, itemData, itemData.currentStackSize, myInventory) == false)
                        AddItemToOtherBags(itemData);
                }
            }
            else // Otherwise, add the item to the first available bag, or the personal inventory if there's no bag or no room in any of the bags
                AddItemToOtherBags(itemData);

            gm.playerInvUI.UpdateUINumbers();
        }
        else // If we're taking this item from the player's inventory
        {
            if (gm.containerInvUI.activeInventory != null) // If we're trying to place the item in a container
            {
                if (gm.containerInvUI.activeInventory.Add(this, itemData, itemData.currentStackSize, myInventory)) // Try adding the item's entire stack
                {
                    myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));

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
                    bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.containerInvUI.activeInventory, itemData);
                    if (someAdded) myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));
                    UpdateItemNumberTexts();
                }
            }
            else // If we're trying to place the item on the ground
            {
                float startingStackSize = itemData.currentStackSize;
                if (itemData.item.maxStackSize > 1) // Try adding to existing stacks first, if the item is stackable
                {
                    AddToExistingStacksOnGround(itemData, itemData.currentStackSize, gm.playerInvUI.activeInventory);
                    UpdateItemNumberTexts();
                    gm.containerInvUI.UpdateUINumbers();

                    if (itemData.currentStackSize < startingStackSize)
                        myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));
                }

                if (itemData.currentStackSize > 0) // If there's still some left to drop or if the item's maxStackSize is 1
                {
                    List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
                    Vector3 dropPos = gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection();

                    if (IsRoomOnGround(itemData, itemsListAddingTo, dropPos))
                    {
                        gm.dropItemController.DropItem(dropPos, itemData, itemData.currentStackSize, gm.playerInvUI.activeInventory);

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

    void AddItemToOtherBags(ItemData itemToAdd)
    {
        // Cache the item in case the itemData gets cleared out before we can do the add item effect
        Item itemAdding = itemToAdd.item;

        // Try adding to the active inventory first
        if (gm.playerInvUI.activeInventory != null && itemToAdd.item.maxStackSize > 1)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.activeInventory, itemToAdd);
            if (someAdded)
            {
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.activePlayerInvSideBarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemData.item.IsBag() && gm.containerInvUI.activeInventory == itemData.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        // If the item is ammunition, try adding here second
        if (itemToAdd != null && itemToAdd.item.itemType == ItemType.Ammo && itemToAdd.currentStackSize > 0 && gm.playerInvUI.quiverEquipped && gm.playerInvUI.activeInventory != gm.playerInvUI.quiverInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.quiverInventory, itemToAdd);
            if (someAdded)
            {
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.quiverSidebarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemData.item.IsBag() && gm.containerInvUI.activeInventory == itemData.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        // Now try to fit the item in other equipped bags
        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.backpackEquipped && gm.playerInvUI.activeInventory != gm.playerInvUI.backpackInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.backpackInventory, itemToAdd);
            if (someAdded)
            {
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemData.item.IsBag() && gm.containerInvUI.activeInventory == itemData.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.leftHipPouchEquipped && gm.playerInvUI.activeInventory != gm.playerInvUI.leftHipPouchInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.leftHipPouchInventory, itemToAdd);
            if (someAdded)
            {
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemData.item.IsBag() && gm.containerInvUI.activeInventory == itemData.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.rightHipPouchEquipped && gm.playerInvUI.activeInventory != gm.playerInvUI.rightHipPouchInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.rightHipPouchInventory, itemToAdd);
            if (someAdded)
            {
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemData.item.IsBag() && gm.containerInvUI.activeInventory == itemData.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }

        // Now try to fit the item in the player's personal inventory
        if (itemToAdd != null && itemToAdd.currentStackSize > 0 && gm.playerInvUI.activeInventory != gm.playerInvUI.personalInventory)
        {
            bool someAdded = AddItemToInventory_OneAtATime(myInventory, gm.playerInvUI.personalInventory, itemToAdd);
            if (someAdded)
            {
                myInvUI.StartCoroutine(myInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.personalInventorySideBarButton));

                // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
                if (itemData.item.IsBag() && gm.containerInvUI.activeInventory == itemData.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
        }
    }

    bool AddItemToInventory_OneAtATime(Inventory invComingFrom, Inventory invAddingTo, ItemData itemData)
    {
        int stackSize = itemData.currentStackSize;
        bool someAdded = false;
        for (int i = 0; i < stackSize; i++)
        {
            // Try to add a single item to the inventory
            if (invAddingTo.Add(this, itemData, 1, invComingFrom))
            {
                // If we were able to add one, set someAdded to true
                someAdded = true;
                if (itemData.currentStackSize == 0) // If the entire stack was added
                {
                    ClearItem();
                    return someAdded;
                }
            }
            else // If there's no longer any room, break out of the loop & update the UI numbers
            {
                gm.containerInvUI.UpdateUINumbers();
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
                        if (IsRoomOnGround(itemDataComingFrom, itemsListAddingTo, dropPos))
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

        // TODO: Collapse disclosure widget if it's open

        if (itemData.item.itemType == ItemType.Bag || itemData.item.itemType == ItemType.PortableContainer)
            disclosureWidget.enabled = false;
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

        if (itemData != null && (itemData.item.itemType == ItemType.Bag || itemData.item.itemType == ItemType.PortableContainer))
        {
            disclosureWidget.enabled = true;
            disclosureWidget.sprite = rightArrowSprite;
        }
    }

    public bool IsRoomOnGround(ItemData itemDataComingFrom, List<ItemData> itemsListAddingTo, Vector2 groundPosition)
    {
        if (itemDataComingFrom.item.IsBag())
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(groundPosition, Vector2.zero, 1f, dropBagObstacleMask);
            if (hits.Length == 0 && gm.containerInvUI.emptyTileMaxVolume - gm.containerInvUI.GetTotalVolume(itemsListAddingTo) - itemDataComingFrom.item.volume >= 0)
                return true;
        }
        else if (gm.containerInvUI.emptyTileMaxVolume - gm.containerInvUI.GetTotalVolume(itemsListAddingTo) - itemDataComingFrom.item.volume >= 0)
            return true;

        return false;
    }
}

using System.Collections.Generic;
using UnityEngine;

public enum Direction { Center, North, South, West, East, Northwest, Northeast, Southwest, Southeast }

public class UIManager : MonoBehaviour
{
    [HideInInspector] public ContainerSideBarButton activeContainerSideBarButton;
    [HideInInspector] public PlayerInventorySidebarButton activePlayerInvSideBarButton;
    [HideInInspector] public InventoryItem activeInvItem, lastActiveItem;
    [HideInInspector] public InventoryUI activeInvUI;
    [HideInInspector] public ContextMenuButton activeContextMenuButton;

    [HideInInspector] public List<InventoryItem> selectedItems = new List<InventoryItem>();
    [HideInInspector] public List<InventoryItem> invItemsDragging = new List<InventoryItem>();
    List<InventoryItem> activeGhostInvItems = new List<InventoryItem>();

    InventoryItem firstSelectedItem;

    GameManager gm;

    readonly float minDragTime = 0.1f;
    float dragTimer;
    int activeInvItemCount;

    LayerMask dropBagObstacleMask;

    #region Singleton
    public static UIManager instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("There's more than one instance of UIManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        gm = GameManager.instance;

        dropBagObstacleMask = LayerMask.GetMask("Interactable", "Interactable Objects", "Objects", "Walls");
    }

    void Update()
    {
        if (gm.playerManager.playerMovement.isMoving == false)
        {
            // Skip these button presses if we're currently dragging items
            if (invItemsDragging.Count == 0)
            {
                // Toggle the Player's Inventory
                if (GameControls.gamePlayActions.playerInventory.WasPressed)
                {
                    ClearSelectedItems(false);
                    ToggleInventory();
                }

                // Disable Inventory menus if they are open and tab is pressed
                if (GameControls.gamePlayActions.tab.WasPressed)
                {
                    ClearSelectedItems(false);
                    DisableInventoryMenus();
                }

                // Take everything from an open container or the ground
                if (GameControls.gamePlayActions.menuContainerTakeAll.WasPressed && gm.containerInvUI.inventoryParent.activeSelf)
                {
                    ClearSelectedItems(false);
                    gm.containerInvUI.TakeAll();
                }

                // Build the context menu
                if (GameControls.gamePlayActions.menuContext.WasPressed)
                {
                    ClearSelectedItems(false);
                    dragTimer = 0;

                    if (activeInvItem != null)
                        SelectActiveItem();

                    if (gm.contextMenu.isActive == false && activeInvItem != null && activeInvItem.itemData != null)
                        gm.contextMenu.BuildContextMenu(activeInvItem);
                    else if (gm.contextMenu.isActive)
                        gm.contextMenu.DisableContextMenu();

                    if (gm.stackSizeSelector.isActive)
                        gm.stackSizeSelector.HideStackSizeSelector();
                }

                // Split stack
                if (GameControls.gamePlayActions.menuSelect.WasPressed && GameControls.gamePlayActions.leftAlt.IsPressed)
                {
                    ClearSelectedItems(false);
                    dragTimer = 0;

                    if (activeInvItem != null && activeInvItem.itemData != null && activeInvItem.itemData.currentStackSize > 1 && activeInvItem != gm.stackSizeSelector.selectedInvItem)
                    {
                        // Show the Stack Size Selector
                        gm.stackSizeSelector.ShowStackSizeSelector(activeInvItem);
                    }
                    else if (gm.stackSizeSelector.isActive && (activeInvItem == null || activeInvItem.itemData == null || activeInvItem.itemData.currentStackSize == 1))
                    {
                        // Hide the Stack Size Selector
                        gm.stackSizeSelector.HideStackSizeSelector();
                    }
                }

                // Use Item
                if (GameControls.gamePlayActions.menuUseItem.WasPressed)
                {
                    ClearSelectedItems(false);

                    if (activeInvItem != null)
                        activeInvItem.UseItem();

                    DisableInventoryUIComponents();
                }

                // Select all
                if (GameControls.gamePlayActions.leftCtrl.IsPressed && GameControls.gamePlayActions.a.WasPressed)
                    SelectAll();

                // Clear selected items if we only left click on an item that's not already selected
                if (GameControls.gamePlayActions.menuSelect.WasPressed && GameControls.gamePlayActions.leftCtrl.IsPressed == false && GameControls.gamePlayActions.leftShift.IsPressed == false
                    && selectedItems.Contains(activeInvItem) == false)
                {
                    ClearSelectedItems(false);
                }
            }

            // Transfer or drop item
            if (GameControls.gamePlayActions.menuSelect.WasReleased)
            {
                if ((GameControls.gamePlayActions.leftCtrl.IsPressed == false && GameControls.gamePlayActions.leftShift.IsPressed == false && GameControls.gamePlayActions.leftAlt.IsPressed == false)
                    || activeGhostInvItems.Count > 0)
                {
                    // Transfer Item
                    if (dragTimer < minDragTime && activeInvItem != null && (activeInvItem.disclosureWidget == null || activeInvItem.disclosureWidget.isSelected == false))
                    {
                        // Don't transfer unless the context menu is closed, to prevent misclicks
                        if (gm.contextMenu.isActive == false)
                            activeInvItem.TransferItem();
                        else
                            gm.contextMenu.DisableContextMenu();

                        if (gm.stackSizeSelector.isActive)
                            gm.stackSizeSelector.HideStackSizeSelector();
                    }
                    else if (invItemsDragging.Count > 0)
                    {
                        for (int i = 0; i < invItemsDragging.Count; i++)
                        {
                            DragAndDrop_DropItem(invItemsDragging[i]);
                        }
                    }

                    // Remove highlighting for any selected items
                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        selectedItems[i].RemoveHighlight();
                    }

                    gm.containerInvUI.UpdateUI();
                    gm.playerInvUI.UpdateUI();

                    // Reset variables and clear out our lists:
                    Reset();
                }
                else if (activeInvItem != null)
                {
                    if (GameControls.gamePlayActions.leftShift.IsPressed) // If left clicking while holding down left shift
                        ShiftSelect();
                    else if (activeInvItem != null && GameControls.gamePlayActions.leftCtrl.IsPressed) // If left clicking while holding down left ctrl
                    {
                        if (selectedItems.Contains(activeInvItem) == false)
                            SelectActiveItem();
                        else
                            DeselectActiveItem();

                        // If we selected an expanded bag, deselect any of its children that are selected
                        if ((activeInvItem.itemData.item.IsBag() || activeInvItem.itemData.item.itemType == ItemType.PortableContainer) && activeInvItem.disclosureWidget.isExpanded)
                        {
                            for (int i = 0; i < activeInvItem.disclosureWidget.expandedItems.Count; i++)
                            {
                                if (selectedItems.Contains(activeInvItem.disclosureWidget.expandedItems[i]))
                                    DeselectItem(activeInvItem.disclosureWidget.expandedItems[i]);
                            }
                        } // If we select an item that is inside a bag/portable container and the bag/portable container is already selected, deselect the bag/portable container
                        else if (activeInvItem.parentInvItem != null)
                        {
                            if (selectedItems.Contains(activeInvItem.parentInvItem))
                                DeselectItem(activeInvItem.parentInvItem);
                        }
                    }
                }

                dragTimer = 0;
            }

            // Drag Item
            if (GameControls.gamePlayActions.menuSelect.IsPressed
                && ((GameControls.gamePlayActions.leftCtrl.IsPressed == false && GameControls.gamePlayActions.leftShift.IsPressed == false && activeInvItem != null) || activeGhostInvItems.Count > 0))
            {
                // Add to the drag timer each frame
                dragTimer += Time.deltaTime;

                // Once the timer is above the minDragTime, start dragging
                if (dragTimer >= minDragTime && (activeGhostInvItems.Count > 0 || (activeInvItem != null && activeInvItem.myInvUI.scrollbarSelected == false)))
                {
                    // Setup the image for the item we're dragging
                    if (activeGhostInvItems.Count == 0)
                    {
                        if (invItemsDragging.Contains(activeInvItem))
                        {
                            for (int i = 0; i < invItemsDragging.Count; i++)
                            {
                                CreateNewGhostItem(invItemsDragging[i]);
                            }
                        }
                    }

                    // Have the item we're dragging follow the mouse cursor
                    if (activeGhostInvItems.Count > 0)
                        activeGhostInvItems[0].transform.parent.position = Input.mousePosition;

                    if (selectedItems.Count > 0)
                    {
                        for (int i = 0; i < selectedItems.Count; i++)
                        {
                            DragAndDrop_DragItem(selectedItems[i]);
                        }
                    }
                    else
                        DragAndDrop_DragItem(activeInvItem);
                }
            }

            // Disable the context menu if we left click
            if (GameControls.gamePlayActions.menuSelect.WasPressed && gm.contextMenu.isActive && activeContextMenuButton == null)
                StartCoroutine(gm.contextMenu.DelayDisableContextMenu());
        }
    }

    void CreateNewGhostItem(InventoryItem originalItem)
    {
        InventoryItem newGhostInvItem = gm.objectPoolManager.ghostImageInventoryItemObjectPool.GetPooledInventoryItem();
        activeGhostInvItems.Add(newGhostInvItem);
        newGhostInvItem.isGhostItem = true;
        newGhostInvItem.itemData = originalItem.itemData;
        newGhostInvItem.UpdateAllItemTexts();
        newGhostInvItem.gameObject.SetActive(true);
        originalItem.originalSiblingIndex = originalItem.transform.GetSiblingIndex();
        originalItem.Hide();
    }

    void SelectItem(InventoryItem invItem)
    {
        if (firstSelectedItem == null)
            firstSelectedItem = invItem;

        selectedItems.Add(invItem);
        invItem.Highlight();

        DisableInventoryUIComponents();
    }

    void SelectActiveItem()
    {
        SelectItem(activeInvItem);
    }

    void DeselectItem(InventoryItem invItem)
    {
        selectedItems.Remove(invItem);
        invItem.RemoveHighlight();

        if (invItem == firstSelectedItem)
        {
            firstSelectedItem = null;
            if (selectedItems.Count > 0)
                firstSelectedItem = selectedItems[0];
        }

        DisableInventoryUIComponents();
    }

    void DeselectActiveItem()
    {
        DeselectItem(activeInvItem);
    }

    void ClearSelectedItems(bool keepFirstSelectedItem)
    {
        for (int i = 0; i < selectedItems.Count; i++)
        {
            selectedItems[i].RemoveHighlight();
        }

        selectedItems.Clear();

        if (keepFirstSelectedItem == false)
            firstSelectedItem = null;
        else if (firstSelectedItem != null)
            SelectItem(firstSelectedItem);
    }

    void SelectAll()
    {
        ClearSelectedItems(false);

        if (activeInvUI != null)
        {
            for (int i = 0; i < activeInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count; i++)
            {
                if (selectedItems.Contains(activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i]) == false)
                {
                    selectedItems.Add(activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i]);
                    activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i].Highlight();
                }
            }
        }

        DisableInventoryUIComponents();
    }

    void ShiftSelect()
    {
        if (selectedItems.Count == 0) // If we haven't selected an item yet, select the item we just selected and highlight the item
            SelectActiveItem();
        else
        {
            if (activeInvItem == firstSelectedItem) // If the item we selected is the same as the fist item we selected, clear the selected items list and remove highlighting
                ClearSelectedItems(false);
            else if (activeInvItem.transform.position.y > firstSelectedItem.transform.position.y) // If the item we selected is above the first item we selected
            {
                // Clear out the selected items (but not the first selected item), so that we can recalculate which items to select
                ClearSelectedItems(true);

                // Go through our pooled inventory items
                for (int i = 0; i < activeInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count; i++)
                {
                    // If the pooled inventory item is active and is not the first inventory item we selected
                    if (activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i] != firstSelectedItem)
                    {
                        // If the mouse pointer is above the pooled inventory item and the pooled inventory item is above the first item we selected, or if the pooled inventory item is the item we clicked on
                        if (activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i] == activeInvItem 
                            || (Input.mousePosition.y > activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i].transform.position.y 
                            && activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i].transform.position.y > firstSelectedItem.transform.position.y))
                        {
                            SelectItem(activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i]);
                        }
                    }
                }
            }
            else // If the item we selected is below the first item we selected
            {
                // Clear out the selected items (but not the first selected item), so that we can recalculate which items to select
                ClearSelectedItems(true);

                for (int i = 0; i < activeInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count; i++) // Go through our pooled inventory items
                {
                    // If the pooled inventory item is active and is not the first inventory item we selected
                    if (activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i] != firstSelectedItem)
                    {
                        // If the mouse pointer is below the pooled inventory item and the pooled inventory item is below the first item we selected, or if the pooled inventory item is the item we clicked on
                        if (activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i] == activeInvItem 
                            || (Input.mousePosition.y < activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i].transform.position.y
                            && activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i].transform.position.y < firstSelectedItem.transform.position.y))
                        {
                            SelectItem(activeInvUI.inventoryItemObjectPool.activePooledInventoryItems[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < selectedItems.Count; i++)
            {
                // If a parent bag item is selected while at least one of its children is also selected, deselect the parent bag item
                if (selectedItems[i].parentInvItem != null && selectedItems.Contains(selectedItems[i].parentInvItem))
                    DeselectItem(selectedItems[i].parentInvItem);
            }
        }
    }

    void DragAndDrop_DragItem(InventoryItem draggedInvItem)
    {
        // Failsafe in case the dragged item is somehow null
        if (draggedInvItem == null)
        {
            ClearSelectedItems(false);
            return;
        }

        DisableInventoryUIComponents();

        // Make sure both inventory menus are active and not minimized
        if (gm.containerInvUI.isActive == false)
            gm.containerInvUI.ToggleInventoryMenu();
        else if (gm.containerInvUI.isMinimized)
            gm.containerInvUI.ToggleMinimization();

        if (gm.playerInvUI.isActive == false)
            gm.playerInvUI.ToggleInventoryMenu();
        else if (gm.playerInvUI.isMinimized)
            gm.playerInvUI.ToggleMinimization();

        // If we're just trying to drag one item, add it to our invItemsDragging list
        if (selectedItems.Count == 0)
        {
            selectedItems.Add(draggedInvItem);
            invItemsDragging.Add(draggedInvItem);
        }
        else if (selectedItems.Count != invItemsDragging.Count)
        {
            // Otherwise, add the selected items to our invItemsDragging list
            for (int i = 0; i < selectedItems.Count; i++)
            {
                invItemsDragging.Add(selectedItems[i]);
            }
        }

        // Calculate the activeInvItemCount if we haven't done so already
        if (draggedInvItem.myInvUI == gm.containerInvUI)
            activeInvItemCount = gm.containerInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count;
        else
            activeInvItemCount = gm.playerInvUI.inventoryItemObjectPool.activePooledInventoryItems.Count;

        // If the mouse cursor is over an InventoryItem, rearrange items when necessary (but not in the equipment menu)
        if (activeInvItem != null && activeInvItem.myEquipmentManager == null && activeGhostInvItems.Count == 1)
            RearrangeItems(draggedInvItem);
    }

    void DragAndDrop_DropItem(InventoryItem draggedInvItem)
    {
        if (activeInvUI != null && activeInvUI == draggedInvItem.myInvUI)
        {
            if (draggedInvItem.canDragToCurrentLocation == false)
            {
                draggedInvItem.transform.SetSiblingIndex(draggedInvItem.originalSiblingIndex);
                draggedInvItem.Show();
                return;
            }
        }

        int startingItemCount = draggedInvItem.itemData.currentStackSize;

        // If we drag and drop an item onto a container sidebar button, place the item in the corresponding inventory/ground space
        if (activeContainerSideBarButton != null)
        {
            // Get the side bar button's corresponding inventory, if it has one
            Inventory inv = activeContainerSideBarButton.GetInventory();

            // If putting in a container
            if (inv != null)
            {
                // Try adding the item to the corresponding inventory
                if (inv.AddItem(draggedInvItem, draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize, draggedInvItem.myInventory))
                {
                    // If the item was added to the inventory:
                    // Play the add item effect
                    StartCoroutine(gm.containerInvUI.PlayAddItemEffect(draggedInvItem.itemData.item.pickupSprite, activeContainerSideBarButton, null));

                    // If the item was a bag and we took it from the ground
                    if (draggedInvItem.itemData.item.IsBag() && gm.containerInvUI.activeInventory == draggedInvItem.itemData.bagInventory)
                        gm.containerInvUI.RemoveBagFromGround();

                    // If we took the item from an inventory, remove the item
                    RemoveDraggedItem(draggedInvItem, startingItemCount);
                }
            }
            else // If putting on the ground
            {
                List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromDirection(activeContainerSideBarButton.directionFromPlayer);
                Vector3 dropPos = gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromDirection(activeContainerSideBarButton.directionFromPlayer);

                if (IsRoomOnGround(draggedInvItem.itemData, itemsListAddingTo, dropPos))
                {
                    // Drop the item
                    gm.dropItemController.DropItem(dropPos, draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize, draggedInvItem.myInventory, draggedInvItem);

                    // If the item was a bag and we took it from the ground
                    if (draggedInvItem.itemData.item.IsBag() && gm.containerInvUI.activeInventory == draggedInvItem.itemData.bagInventory)
                        gm.containerInvUI.RemoveBagFromGround();

                    // Remove, unequip or clear the item we were dragging, depending where it's coming from
                    RemoveDraggedItem(draggedInvItem, startingItemCount);
                }
            }
        }
        // If we drag and drop an item onto a player inv sidebar button, place the item in the corresponding inventory
        else if (activePlayerInvSideBarButton != null && activePlayerInvSideBarButton.playerInventoryType != PlayerInventoryType.EquippedItems
            && (activePlayerInvSideBarButton.playerInventoryType != PlayerInventoryType.Keys || draggedInvItem.itemData.item.IsKey()))
        {
            // Get the side bar button's corresponding inventory
            Inventory inv = activePlayerInvSideBarButton.GetInventory();

            // Try adding the item to the corresponding inventory
            if (inv.AddItem(draggedInvItem, draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize, draggedInvItem.myInventory))
            {
                // If the item was added to the inventory:
                // Play the add item effect
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(draggedInvItem.itemData.item.pickupSprite, null, activePlayerInvSideBarButton));

                // If the item was a bag and we took it from the ground
                if (draggedInvItem.itemData.item.IsBag() && gm.containerInvUI.activeInventory == draggedInvItem.itemData.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();

                // Remove, unequip or clear the item we were dragging, depending where it's coming from
                RemoveDraggedItem(draggedInvItem, startingItemCount);
            }
        }
        // If we try to drop it directly on the background of the container or player inventory menu
        else if (activeInvUI != null && draggedInvItem.myInvUI != activeInvUI // If we're not dropping the item onto the same invUI it came from
            && (draggedInvItem.itemData.item.IsKey() || activeInvUI.activeInventory != gm.playerInvUI.keysInventory) // If the item is a key and we're dragging onto the keys inventory, or if the item is not a key
            && (activeInvUI == gm.containerInvUI || activeInvUI.activeInventory != null)) // If we're not dragging onto the equipment inventory
        {
            bool wasAddedToInventory = false;

            // If the activeInvUI has an inventory open
            if (activeInvUI.activeInventory != null)
            {
                // Try adding the item to the inventory
                wasAddedToInventory = activeInvUI.activeInventory.AddItem(draggedInvItem, draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize, draggedInvItem.myInventory);

                // If the item was a bag and we took it from the ground
                if (draggedInvItem.itemData.item.IsBag() && gm.containerInvUI.activeInventory == draggedInvItem.itemData.bagInventory)
                    gm.containerInvUI.RemoveBagFromGround();
            }
            else // If the activeInvUI does not have an inventory open
            {
                List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
                Vector3 dropPos = gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection();

                // Drop the item if there's room on the ground
                if (IsRoomOnGround(draggedInvItem.itemData, itemsListAddingTo, dropPos))
                {
                    gm.dropItemController.DropItem(dropPos, draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize, draggedInvItem.myInventory, draggedInvItem);

                    // Remove, unequip or clear the item we were dragging, depending where it's coming from
                    RemoveDraggedItem(draggedInvItem, startingItemCount);
                }
            }

            if (wasAddedToInventory)
            {
                // Play the add item effect
                if (activeInvUI == gm.containerInvUI)
                    StartCoroutine(gm.containerInvUI.PlayAddItemEffect(draggedInvItem.itemData.item.pickupSprite, gm.containerInvUI.activeContainerSideBarButton, null));
                else
                    StartCoroutine(gm.containerInvUI.PlayAddItemEffect(draggedInvItem.itemData.item.pickupSprite, null, gm.playerInvUI.activePlayerInvSideBarButton));
                
                // Remove, unequip or clear the item we were dragging, depending where it's coming from
                RemoveDraggedItem(draggedInvItem, startingItemCount);
            }
        }
        // If we're dropping the item onto open space (not on a menu or sidebar button) and the item is not already on the ground
        else if (activeInvUI == null && (draggedInvItem.myInvUI == gm.playerInvUI || draggedInvItem.myInventory != null) && activePlayerInvSideBarButton == null && activeContainerSideBarButton == null)
        {
            Vector3 dropPos = gm.playerManager.transform.position;
            if (IsRoomOnGround(draggedInvItem.itemData, gm.containerInvUI.playerPositionItems, dropPos))
            {
                // Drop the item
                gm.dropItemController.DropItem(dropPos, draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize, draggedInvItem.myInventory, draggedInvItem);

                // Remove, unequip or clear the item we were dragging, depending where it's coming from
                RemoveDraggedItem(draggedInvItem, startingItemCount);
            }
        }

        // Re-enable components for the dragged item, since they were disabled previously
        if (draggedInvItem != null)
            draggedInvItem.Show();

        // Disable all of the ghost items
        ResetandHideGhostItems();
    }

    public void ResetandHideGhostItems()
    {
        for (int i = 0; i < activeGhostInvItems.Count; i++)
        {
            activeGhostInvItems[i].ResetInvItem();
            activeGhostInvItems[i].gameObject.SetActive(false);
            gm.objectPoolManager.ghostImageInventoryItemObjectPool.activePooledInventoryItems.Remove(activeGhostInvItems[i]);
        }
    }

    void RemoveDraggedItem(InventoryItem draggedInvItem, int startingItemCount)
    {
        // If we're taking the item from an inventory, remove the item
        if (draggedInvItem.myInventory != null)
            draggedInvItem.myInventory.RemoveItem(draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize, draggedInvItem);
        else if (draggedInvItem.myEquipmentManager != null)
        {
            // If we're taking the item from the player's equipment menu, unequip the item
            draggedInvItem.itemData.currentStackSize = startingItemCount;
            draggedInvItem.myEquipmentManager.Unequip(draggedInvItem.myEquipmentManager.GetEquipmentSlotFromItemData(draggedInvItem.itemData), false, false);
        }
        else // If the item was on the ground, simply clear out the InventoryItem we were dragging
            draggedInvItem.ClearItem();
    }

    void RearrangeItems(InventoryItem draggedInvItem)
    {
        // Drag the InventoryItem up (if the mouse position is above the draggedItem's original position)
        if (draggedInvItem.transform.GetSiblingIndex() != 0 && draggedInvItem.myInvUI == activeInvUI && activeInvItem != draggedInvItem && Input.mousePosition.y > draggedInvItem.transform.position.y + 16)
        {
            // Set the new sibling index of the item
            if (draggedInvItem.parentInvItem == null || draggedInvItem.parentInvItem.itemData.CompareTag("Item Pickup") == false
                || draggedInvItem.transform.GetSiblingIndex() >= draggedInvItem.parentInvItem.transform.GetSiblingIndex() + 2) // If the parent bag item of the item we're dragging is on the ground, make sure not to move the dragged item above the bag item
            {
                draggedInvItem.transform.SetSiblingIndex(draggedInvItem.transform.GetSiblingIndex() - 1);
            }

            // If the item is in a bag and we drag it above its bag item
            if (draggedInvItem.parentInvItem != null && draggedInvItem.parentInvItem.itemData.CompareTag("Item Pickup") == false
                && draggedInvItem.transform.GetSiblingIndex() < draggedInvItem.parentInvItem.transform.GetSiblingIndex())
            {
                draggedInvItem.canDragToCurrentLocation = true;
                draggedInvItem.originalSiblingIndex = draggedInvItem.transform.GetSiblingIndex();

                // The item is no longer in the bag, so remove it from the bag
                int indexOfBag = draggedInvItem.parentInvItem.myInventory.items.IndexOf(draggedInvItem.parentInvItem.itemData);
                draggedInvItem.myInventory = draggedInvItem.parentInvItem.myInventory;
                draggedInvItem.myInventory.items.Insert(indexOfBag, draggedInvItem.itemData);
                draggedInvItem.parentInvItem.itemData.bagInventory.items.Remove(draggedInvItem.itemData);
                draggedInvItem.itemData.transform.SetParent(draggedInvItem.myInventory.itemsParent);
                draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Remove(draggedInvItem);

                // Update weight/volume for the bag
                draggedInvItem.parentInvItem.itemData.bagInventory.currentWeight -= Mathf.RoundToInt(draggedInvItem.itemData.item.weight * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                draggedInvItem.parentInvItem.itemData.bagInventory.currentVolume -= Mathf.RoundToInt(draggedInvItem.itemData.item.volume * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                draggedInvItem.parentInvItem.UpdateItemNumberTexts();

                // If there's now nothing left in the bag, contract the disclosure widget
                if (draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Count == 0)
                    draggedInvItem.parentInvItem.disclosureWidget.ContractDisclosureWidget();

                draggedInvItem.parentInvItem = null;
                draggedInvItem.isItemInsideBag = false;

                // If this is a container UI item, rearrange the appropriate containerUI's directional items list
                if (draggedInvItem.myInvUI == gm.containerInvUI && draggedInvItem.parentInvItem == null)
                    gm.containerInvUI.GetItemsListFromActiveDirection().Insert(indexOfBag, draggedInvItem.itemData);
            }
            // If the item is not in a bag, but we drag it in between items in an expanded bag or in between the bag itself and an item inside it
            else if (draggedInvItem.parentInvItem == null && lastActiveItem != null && lastActiveItem != draggedInvItem && lastActiveItem.isItemInsideBag
                && draggedInvItem.transform.GetSiblingIndex() < lastActiveItem.transform.GetSiblingIndex() && draggedInvItem.itemData.item.IsBag() == false 
                && (draggedInvItem.itemData.item.itemType != ItemType.PortableContainer || lastActiveItem.parentInvItem.itemData.item.IsBag()))
            {
                if (lastActiveItem.parentInvItem.itemData.bagInventory.HasRoomInInventory(draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize))
                {
                    draggedInvItem.canDragToCurrentLocation = true;
                    draggedInvItem.originalSiblingIndex = draggedInvItem.transform.GetSiblingIndex();

                    draggedInvItem.myInventory.items.Remove(draggedInvItem.itemData);
                    draggedInvItem.myInventory = lastActiveItem.parentInvItem.itemData.bagInventory;
                    draggedInvItem.parentInvItem = lastActiveItem.parentInvItem;
                    draggedInvItem.myInventory.items.Insert(draggedInvItem.myInventory.items.Count - 1, draggedInvItem.itemData);
                    draggedInvItem.itemData.transform.SetParent(draggedInvItem.myInventory.itemsParent);
                    draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Insert(draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Count - 1, draggedInvItem);
                    draggedInvItem.isItemInsideBag = true;

                    // Update weight/volume for the bag
                    draggedInvItem.parentInvItem.itemData.bagInventory.currentWeight += Mathf.RoundToInt(draggedInvItem.itemData.item.weight * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                    draggedInvItem.parentInvItem.itemData.bagInventory.currentVolume += Mathf.RoundToInt(draggedInvItem.itemData.item.volume * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                    draggedInvItem.parentInvItem.UpdateItemNumberTexts();

                    // If this is a container UI item, remove the item from the appropriate directional list
                    if (draggedInvItem.myInvUI == gm.containerInvUI && draggedInvItem.parentInvItem == null)
                        gm.containerInvUI.GetItemsListFromActiveDirection().Remove(draggedInvItem.itemData);
                }
                else
                {
                    draggedInvItem.canDragToCurrentLocation = false;
                    // Debug.Log("Not enough room in " + lastActiveItem.parentInvItem.itemData.itemName);
                }
            }
            // If we drag into an open bag from below and we already know we can't place the item in the bag
            else if (lastActiveItem != null && lastActiveItem != draggedInvItem && lastActiveItem.parentInvItem != null && draggedInvItem.parentInvItem != lastActiveItem.parentInvItem)
            {
                draggedInvItem.canDragToCurrentLocation = false;
                // Debug.Log("Can't place here");
            }
            else
            {
                draggedInvItem.canDragToCurrentLocation = true;
                draggedInvItem.originalSiblingIndex = draggedInvItem.transform.GetSiblingIndex();

                // Rearrange the inventory list if it has one
                if (draggedInvItem.myInventory != null)
                {
                    int index = draggedInvItem.myInventory.items.IndexOf(draggedInvItem.itemData);
                    if (index > 0) // If this is not the first item in the list
                    {
                        draggedInvItem.myInventory.items.RemoveAt(index);
                        draggedInvItem.myInventory.items.Insert(index - 1, draggedInvItem.itemData);
                    }
                }

                // If this is a container UI item, rearrange the appropriate containerUI's directional items list
                if (draggedInvItem.myInvUI == gm.containerInvUI && draggedInvItem.parentInvItem == null)
                {
                    List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromActiveDirection();
                    int index = itemsList.IndexOf(draggedInvItem.itemData);
                    if (index > 0) // If this is not the first item in the list
                    {
                        itemsList.RemoveAt(index);
                        itemsList.Insert(index - 1, draggedInvItem.itemData);
                    }
                }
            }
        }
        // Drag the InventoryItem down (if the mouse position is below the draggedItem's original position)
        else if (draggedInvItem.transform.GetSiblingIndex() != draggedInvItem.myInvUI.inventoryItemObjectPool.pooledInventoryItems.Count - 1 && draggedInvItem.myInvUI == activeInvUI && activeInvItem != draggedInvItem 
            && Input.mousePosition.y < draggedInvItem.transform.position.y - 16)
        {
            // Set the new sibling index of the item
            draggedInvItem.transform.SetSiblingIndex(draggedInvItem.transform.GetSiblingIndex() + 1);

            // If the item is in a bag and we drag it below the last item in the bag, remove it from the bag
            if (draggedInvItem.parentInvItem != null && lastActiveItem != null && lastActiveItem != draggedInvItem
                && draggedInvItem.parentInvItem.itemData.bagInventory.items.Contains(lastActiveItem.itemData) == false)
            {
                draggedInvItem.canDragToCurrentLocation = true;
                draggedInvItem.originalSiblingIndex = draggedInvItem.transform.GetSiblingIndex();

                // The item is no longer in the bag, so remove it from the bag
                int indexOfBag = draggedInvItem.parentInvItem.myInventory.items.IndexOf(draggedInvItem.parentInvItem.itemData);
                draggedInvItem.myInventory = draggedInvItem.parentInvItem.myInventory;
                draggedInvItem.myInventory.items.Insert(indexOfBag + 2, draggedInvItem.itemData);
                draggedInvItem.parentInvItem.itemData.bagInventory.items.Remove(draggedInvItem.itemData);
                draggedInvItem.itemData.transform.SetParent(draggedInvItem.myInventory.itemsParent);
                draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Remove(draggedInvItem);

                // Update weight/volume for the bag
                draggedInvItem.parentInvItem.itemData.bagInventory.currentWeight -= Mathf.RoundToInt(draggedInvItem.itemData.item.weight * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                draggedInvItem.parentInvItem.itemData.bagInventory.currentVolume -= Mathf.RoundToInt(draggedInvItem.itemData.item.volume * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                draggedInvItem.parentInvItem.UpdateItemNumberTexts();

                // If there's now nothing left in the bag, contract the disclosure widget
                if (draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Count == 0)
                    draggedInvItem.parentInvItem.disclosureWidget.ContractDisclosureWidget();

                draggedInvItem.parentInvItem = null;
                draggedInvItem.isItemInsideBag = false;

                // If this is a container UI item, rearrange the appropriate containerUI's directional items list
                if (draggedInvItem.myInvUI == gm.containerInvUI && draggedInvItem.parentInvItem == null)
                    gm.containerInvUI.GetItemsListFromActiveDirection().Insert(indexOfBag + 2, draggedInvItem.itemData);
            }
            // If we drag into an open bag from above, try adding the item to the bag
            else if (lastActiveItem != null && lastActiveItem != draggedInvItem && (lastActiveItem.itemData.item.IsBag() || lastActiveItem.itemData.item.itemType == ItemType.PortableContainer)
                && lastActiveItem.disclosureWidget.isExpanded && draggedInvItem.itemData.item.IsBag() == false
                && (draggedInvItem.itemData.item.itemType != ItemType.PortableContainer || lastActiveItem.itemData.item.IsBag()))
            {
                if (lastActiveItem.itemData.bagInventory.HasRoomInInventory(draggedInvItem.itemData, draggedInvItem.itemData.currentStackSize))
                {
                    draggedInvItem.canDragToCurrentLocation = true;
                    draggedInvItem.originalSiblingIndex = draggedInvItem.transform.GetSiblingIndex();

                    draggedInvItem.myInventory.items.Remove(draggedInvItem.itemData);

                    // If the item was inside another bag, remove it from the bag...and if there's now nothing left in the bag, contract the bag's disclosure widget
                    if (draggedInvItem.parentInvItem != null)
                    {
                        draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Remove(draggedInvItem);
                        if (draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Count == 0)
                            draggedInvItem.parentInvItem.disclosureWidget.ContractDisclosureWidget();

                        // Update weight/volume for the bag the item came from
                        draggedInvItem.parentInvItem.itemData.bagInventory.currentWeight -= Mathf.RoundToInt(draggedInvItem.itemData.item.weight * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                        draggedInvItem.parentInvItem.itemData.bagInventory.currentVolume -= Mathf.RoundToInt(draggedInvItem.itemData.item.volume * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                        draggedInvItem.parentInvItem.UpdateItemNumberTexts();
                    }
                    else
                    {
                        draggedInvItem.isItemInsideBag = true;

                        // If the item is in a container, remove it from the appropriate directional list
                        if (draggedInvItem.myInvUI == gm.containerInvUI)
                            gm.containerInvUI.GetItemsListFromActiveDirection().Remove(draggedInvItem.itemData);
                    }

                    draggedInvItem.myInventory = lastActiveItem.itemData.bagInventory;
                    draggedInvItem.myInventory.items.Insert(0, draggedInvItem.itemData);
                    draggedInvItem.parentInvItem = lastActiveItem;
                    draggedInvItem.parentInvItem.disclosureWidget.expandedItems.Add(draggedInvItem);
                    draggedInvItem.itemData.transform.SetParent(draggedInvItem.myInventory.itemsParent);

                    // Update weight/volume for the bag the item is going into
                    draggedInvItem.myInventory.currentWeight += Mathf.RoundToInt(draggedInvItem.itemData.item.weight * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                    draggedInvItem.myInventory.currentVolume += Mathf.RoundToInt(draggedInvItem.itemData.item.volume * draggedInvItem.itemData.currentStackSize * 100f) / 100f;
                    draggedInvItem.parentInvItem.UpdateItemNumberTexts();
                }
                else
                {
                    draggedInvItem.canDragToCurrentLocation = false;
                    // Debug.Log("Not enough room in " + lastActiveItem.itemData.itemName);
                }
            }
            // If we drag into an open bag from above and we already know we can't place the item in the bag
            else if (lastActiveItem != null && lastActiveItem != draggedInvItem 
                && ((lastActiveItem.parentInvItem != null && draggedInvItem.parentInvItem != lastActiveItem.parentInvItem) || (lastActiveItem.itemData.item.IsBag() && lastActiveItem.disclosureWidget.isExpanded)))
            {
                draggedInvItem.canDragToCurrentLocation = false;
                // Debug.Log("Can't place here");
            }
            else
            {
                draggedInvItem.canDragToCurrentLocation = true;
                draggedInvItem.originalSiblingIndex = draggedInvItem.transform.GetSiblingIndex();

                // Rearrange the inventory list if it has one
                if (draggedInvItem.myInventory != null)
                {
                    int index = draggedInvItem.myInventory.items.IndexOf(draggedInvItem.itemData);
                    if (index < activeInvItemCount - 1)
                    {
                        draggedInvItem.myInventory.items.RemoveAt(index);
                        draggedInvItem.myInventory.items.Insert(index + 1, draggedInvItem.itemData);
                    }
                }

                // If this is a container UI item, rearrange the appropriate containerUI's directional items list
                if (draggedInvItem.myInvUI == gm.containerInvUI && draggedInvItem.parentInvItem == null)
                {
                    List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromActiveDirection();
                    int index = itemsList.IndexOf(draggedInvItem.itemData);
                    if (index < activeInvItemCount - 1)
                    {
                        itemsList.RemoveAt(index);
                        itemsList.Insert(index + 1, draggedInvItem.itemData);
                    }
                }
            }
        }
    }

    public bool UIMenuActive()
    {
        if (gm.playerInvUI.inventoryParent.activeSelf || gm.containerInvUI.inventoryParent.activeSelf)
            return true;

        return false;
    }

    public void ToggleInventory()
    {
        if (gm.containerInvUI.isActive && gm.playerInvUI.isActive == false)
            gm.playerInvUI.ToggleInventoryMenu();
        else if (gm.containerInvUI.isActive == false && gm.playerInvUI.isActive)
            gm.containerInvUI.ToggleInventoryMenu();
        else
        {
            gm.playerInvUI.ToggleInventoryMenu();
            gm.containerInvUI.ToggleInventoryMenu();
        }
    }

    public void DisableInventoryMenus()
    {
        if (gm.playerInvUI.isActive)
            gm.playerInvUI.ToggleInventoryMenu();

        if (gm.containerInvUI.isActive)
            gm.containerInvUI.ToggleInventoryMenu();
    }

    public void DisableInventoryUIComponents()
    {
        // Disable the Context Menu
        if (gm.contextMenu.isActive)
            gm.contextMenu.DisableContextMenu();

        // Disable the Stack Size Selector
        if (gm.stackSizeSelector.isActive)
            gm.stackSizeSelector.HideStackSizeSelector();

        // gm.tooltipManager.HideAllTooltips();
    }

    public bool IsRoomOnGround(ItemData itemDataComingFrom, List<ItemData> itemsListAddingTo, Vector2 groundPosition)
    {
        if (itemDataComingFrom.item.IsBag())
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(groundPosition, Vector2.zero, 1f, dropBagObstacleMask);
            if (hits.Length == 0 && gm.containerInvUI.emptyTileMaxVolume - gm.containerInvUI.GetTotalVolume(itemsListAddingTo) - itemDataComingFrom.bagInventory.currentVolume - itemDataComingFrom.item.volume >= 0)
                return true;
        }
        else if (gm.containerInvUI.emptyTileMaxVolume - gm.containerInvUI.GetTotalVolume(itemsListAddingTo) - itemDataComingFrom.item.volume >= 0 
            && (groundPosition != (Vector2)gm.playerManager.transform.position || gm.containerInvUI.playerPositionInventory == null))
            return true;

        Debug.Log("There's not enough room on the ground to place the " + itemDataComingFrom.itemName);
        return false;
    }

    public void Reset()
    {
        invItemsDragging.Clear();
        selectedItems.Clear();
        activeGhostInvItems.Clear();
        firstSelectedItem = null;
        activeInvItem = null;
        dragTimer = 0;
        activeInvItemCount = 0;
    }

    public void ShowHiddenItems()
    {
        for (int i = 0; i < invItemsDragging.Count; i++)
        {
            invItemsDragging[i].Show();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [HideInInspector] public ContainerSideBarButton activeContainerSideBarButton;
    [HideInInspector] public PlayerInventorySidebarButton activePlayerInvSideBarButton;
    [HideInInspector] public InventoryItem activeInvItem;
    InventoryItem invItemDragging;

    GameManager gm;

    float dragTimer;
    float minDragTime = 0.15f;
    int activeInvItemCount;
    bool onSelectionCooldown;

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
    }

    void Update()
    {
        // Toggle the Player's Inventory
        if (GameControls.gamePlayActions.playerInventory.WasPressed)
        {
            if (gm.containerInvUI.inventoryParent.activeSelf && gm.playerInvUI.inventoryParent.activeSelf == false)
                gm.playerInvUI.ToggleInventoryMenu();
            else if (gm.containerInvUI.inventoryParent.activeSelf == false && gm.playerInvUI.inventoryParent.activeSelf)
                gm.containerInvUI.ToggleInventoryMenu();
            else
            {
                gm.playerInvUI.ToggleInventoryMenu();
                gm.containerInvUI.ToggleInventoryMenu();
            }
        }

        // Disable Inventory menus if they are open and tab is pressed
        if (GameControls.gamePlayActions.tab.WasPressed)
            DisableMenus();

        // Take everything from an open container
        if (GameControls.gamePlayActions.menuContainerTakeAll.WasPressed && gm.containerInvUI.inventoryParent.activeSelf)
            gm.containerInvUI.TakeAll();

        // Use Item
        if (GameControls.gamePlayActions.menuUseItem.WasPressed)
        {
            if (activeInvItem != null)
                activeInvItem.UseItem();

            if (gm.contextMenu.isActive)
                gm.contextMenu.DisableContextMenu();

            if (gm.stackSizeSelector.isActive)
                gm.stackSizeSelector.HideStackSizeSelector();
        }
        
        if (GameControls.gamePlayActions.menuSelect.WasReleased)
        {
            // Transfer Item
            if (dragTimer < minDragTime && activeInvItem != null)
            {
                // Don't transfer unless the context menu is closed, to prevent misclicks
                if (gm.contextMenu.isActive == false)
                    activeInvItem.TransferItem();
                else
                    gm.contextMenu.DisableContextMenu();

                if (gm.stackSizeSelector.isActive)
                    gm.stackSizeSelector.HideStackSizeSelector();
            }
            else if (invItemDragging != null)
            {
                int startingItemCount = invItemDragging.itemData.currentStackSize;

                // If we drag and drop an item onto a container sidebar button, place the item in the corresponding inventory/ground space
                if (activeContainerSideBarButton != null)
                {
                    // Get the side bar button's corresponding inventory, if it has one
                    Inventory inv = activeContainerSideBarButton.GetInventory();
                    
                    // If putting in a container
                    if (inv != null)
                    {
                        // Add the item to the corresponding inventory
                        inv.Add(invItemDragging, invItemDragging.itemData, invItemDragging.itemData.currentStackSize, invItemDragging.myInventory);

                        // If we're taking the item from an inventory, remove the item
                        if (invItemDragging.myInventory != null)
                            invItemDragging.myInventory.Remove(invItemDragging.itemData, invItemDragging.itemData.currentStackSize, invItemDragging);
                        else if (invItemDragging.myEquipmentManager != null)
                        {
                            invItemDragging.itemData.currentStackSize = startingItemCount;
                            invItemDragging.myEquipmentManager.Unequip(invItemDragging.myEquipmentManager.GetEquipmentSlotFromItemData(invItemDragging.itemData), false);
                        }
                        else
                            // Clear out the InventoryItem we were dragging
                            invItemDragging.ClearItem();

                        // If the active container sidebar button's items are active in the menu
                        if (gm.containerInvUI.activeDirection == activeContainerSideBarButton.directionFromPlayer)
                            gm.containerInvUI.UpdateUINumbers();
                    }
                    else // If putting on the ground
                    {
                        // Drop the item
                        gm.dropItemController.DropItem(gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromDirection(activeContainerSideBarButton.directionFromPlayer), invItemDragging.itemData, invItemDragging.itemData.currentStackSize);

                        // If we're taking the item from an inventory, remove the item
                        if (invItemDragging.myInventory != null)
                            invItemDragging.myInventory.Remove(invItemDragging.itemData, invItemDragging.itemData.currentStackSize, invItemDragging);
                        else if (invItemDragging.myEquipmentManager != null)
                        {
                            invItemDragging.itemData.currentStackSize = startingItemCount;
                            invItemDragging.myEquipmentManager.Unequip(invItemDragging.myEquipmentManager.GetEquipmentSlotFromItemData(invItemDragging.itemData), false);
                        }
                        else
                            // Clear out the InventoryItem we were dragging
                            invItemDragging.ClearItem();
                        
                        // If the active container sidebar button's items are active in the menu
                        if (gm.containerInvUI.activeDirection == activeContainerSideBarButton.directionFromPlayer)
                            gm.containerInvUI.UpdateUINumbers();
                    }
                }
                // If we drag and drop an item onto a player inv sidebar button, place the item in the corresponding inventory
                else if (activePlayerInvSideBarButton != null && activePlayerInvSideBarButton.playerInventoryType != PlayerInventoryType.EquippedItems 
                    && (activePlayerInvSideBarButton.playerInventoryType != PlayerInventoryType.Keys || invItemDragging.itemData.item.IsKey()))
                {
                    // Get the side bar button's corresponding inventory
                    Inventory inv = activePlayerInvSideBarButton.GetInventory();
                    
                    // Add the item to the corresponding inventory
                    inv.Add(invItemDragging, invItemDragging.itemData, invItemDragging.itemData.currentStackSize, invItemDragging.myInventory);

                    // If we're taking the item from an inventory, remove the item
                    if (invItemDragging.myInventory != null)
                        invItemDragging.myInventory.Remove(invItemDragging.itemData, invItemDragging.itemData.currentStackSize, invItemDragging);
                    else if (invItemDragging.myEquipmentManager != null)
                    {
                        invItemDragging.itemData.currentStackSize = startingItemCount;
                        invItemDragging.myEquipmentManager.Unequip(invItemDragging.myEquipmentManager.GetEquipmentSlotFromItemData(invItemDragging.itemData), false);
                    }
                    else
                        // Clear out the InventoryItem we were dragging
                        invItemDragging.ClearItem();

                    // If the active player inv sidebar button's items are active in the menu
                    if (gm.playerInvUI.activeInventory == inv)
                        gm.playerInvUI.UpdateUINumbers();
                }
            }

            // Reset these variables:
            invItemDragging = null;
            dragTimer = 0;
            activeInvItemCount = 0;
        }

        // Drag Item
        if (GameControls.gamePlayActions.menuSelect.IsPressed && activeInvItem != null)
            DragAndDrop_DragItem();

        // If we have an active Inventory Item and we click outside of the Inventory screen, drop the Item
        if (GameControls.gamePlayActions.menuSelect.WasReleased && activeInvItem == null)
        {
            // TODO
        }

        // Build the context menu
        if (GameControls.gamePlayActions.menuContext.WasPressed)
        {
            if (gm.contextMenu.isActive == false && activeInvItem != null && activeInvItem.itemData != null)
            {
                if (gm.contextMenu.isActive && gm.contextMenu.activeInvItem != activeInvItem)
                    gm.contextMenu.DisableContextMenu();

                gm.contextMenu.BuildContextMenu(activeInvItem);
            }
            else if (gm.contextMenu.isActive)
                gm.contextMenu.DisableContextMenu();

            if (gm.stackSizeSelector.isActive)
                gm.stackSizeSelector.HideStackSizeSelector();
        }
        
        // Disable the context menu
        if (GameControls.gamePlayActions.menuSelect.WasPressed && gm.contextMenu.isActive && ((activeInvItem != null && activeInvItem.itemData == null) || activeInvItem == null))
        {
            StartCoroutine(gm.contextMenu.DelayDisableContextMenu());
        }

        // Split stack
        if (GameControls.gamePlayActions.menuSelect.WasPressed && GameControls.gamePlayActions.leftShift.IsPressed)
        {
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
    }

    public void SetSelectedItem()
    {
        if (GameControls.gamePlayActions.leftShift.IsPressed == false && onSelectionCooldown == false)
        {
            // TODO

            // Disable the Context Menu
            if (gm.contextMenu.isActive)
                gm.contextMenu.DisableContextMenu();

            // Disable the Stack Size Selector
            if (gm.stackSizeSelector.isActive)
                gm.stackSizeSelector.HideStackSizeSelector();
        }
    }

    public void DeselectItem()
    {
        // TODO
    }

    void DragAndDrop_DragItem()
    {
        dragTimer += Time.deltaTime;

        if (invItemDragging == null)
            invItemDragging = activeInvItem;

        if (dragTimer >= minDragTime)
        {
            // Calculate the activeInvItemCount if we haven't done so already
            if (activeInvItemCount == 0)
            {
                for (int i = 0; i < invItemDragging.transform.parent.childCount; i++)
                {
                    if (invItemDragging.transform.parent.GetChild(i).gameObject.activeSelf)
                        activeInvItemCount++;
                }
            }

            // Drag the InventoryItem up
            if (invItemDragging.transform.GetSiblingIndex() != 0 && Input.mousePosition.y > invItemDragging.transform.position.y + 16)
            {
                // Set the new sibling index of the item
                invItemDragging.transform.SetSiblingIndex(invItemDragging.transform.GetSiblingIndex() - 1);

                // Rearrange the inventory list if it has one
                if (invItemDragging.myInventory != null)
                {
                    int index = invItemDragging.myInventory.items.IndexOf(invItemDragging.itemData);
                    if (index > 0)
                    {
                        invItemDragging.myInventory.items.RemoveAt(index);
                        invItemDragging.myInventory.items.Insert(index - 1, invItemDragging.itemData);
                    }
                }

                // If this is a container UI item, rearrange the directional items list and the InventoryItem object pool lists
                if (invItemDragging.myEquipmentManager == null && (invItemDragging.myInventory == null || (invItemDragging.myInventory != null && invItemDragging.myInventory.myInventoryUI == gm.containerInvUI)))
                {
                    List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromActiveDirection();
                    int index = itemsList.IndexOf(invItemDragging.itemData);
                    if (index > 0)
                    {
                        itemsList.RemoveAt(index);
                        itemsList.Insert(index - 1, invItemDragging.itemData);
                    }
                }
            }
            // Drag the InventoryItem down
            else if (invItemDragging.transform.GetSiblingIndex() != activeInvItemCount - 1 && Input.mousePosition.y < invItemDragging.transform.position.y - 16)
            {
                // Set the new sibling index of the item
                invItemDragging.transform.SetSiblingIndex(invItemDragging.transform.GetSiblingIndex() + 1);

                // Rearrange the inventory list if it has one
                if (invItemDragging.myInventory != null)
                {
                    int index = invItemDragging.myInventory.items.IndexOf(invItemDragging.itemData);
                    if (index < activeInvItemCount - 1)
                    {
                        invItemDragging.myInventory.items.RemoveAt(index);
                        invItemDragging.myInventory.items.Insert(index + 1, invItemDragging.itemData);
                    }
                }

                // If this is a container UI item, rearrange the InventoryItem object pool lists
                if (invItemDragging.myEquipmentManager == null && (invItemDragging.myInventory == null || (invItemDragging.myInventory != null && invItemDragging.myInventory.myInventoryUI == gm.containerInvUI)))
                {
                    List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromActiveDirection();
                    int index = itemsList.IndexOf(invItemDragging.itemData);
                    if (index < activeInvItemCount - 1)
                    {
                        itemsList.RemoveAt(index);
                        itemsList.Insert(index + 1, invItemDragging.itemData);
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

    IEnumerator SelectionCooldown()
    {
        onSelectionCooldown = true;
        yield return new WaitForSeconds(0.1f);
        onSelectionCooldown = false;
    }

    public void DisableMenus()
    {
        if (gm.playerInvUI.inventoryParent.activeSelf)
            gm.playerInvUI.ToggleInventoryMenu();

        if (gm.containerInvUI.inventoryParent.activeSelf)
            gm.containerInvUI.ToggleInventoryMenu();
    }
}

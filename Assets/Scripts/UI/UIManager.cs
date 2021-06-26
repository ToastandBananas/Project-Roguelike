using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [HideInInspector] public InventoryItem activeInventorySlot;

    [HideInInspector] public ContextMenu contextMenu;
    [HideInInspector] public StackSizeSelector stackSizeSelector;
    [HideInInspector] public TooltipManager tooltipManager;

    [HideInInspector] public bool equippedItemsInventoryActive;

    DropItemController dropItemController;
    PlayerManager playerManager;
    PlayerInventory playerInventory;
    ContainerInventoryUI containerInvUI;

    bool onSelectionCooldown;

    public static UIManager instance;

    void Awake()
    {
        #region Singleton
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
        #endregion
    }

    void Start()
    {
        dropItemController = DropItemController.instance;
        playerManager = PlayerManager.instance;
        playerInventory = PlayerInventory.instance;
        containerInvUI = ContainerInventoryUI.instance;
        contextMenu = ContextMenu.instance;
        stackSizeSelector = StackSizeSelector.instance;
        tooltipManager = TooltipManager.instance;
    }

    void Update()
    {
        // Toggle the Player's Inventory
        if (GameControls.gamePlayActions.playerInventory.WasPressed)
        {
            playerInventory.myInventoryUI.ToggleInventoryMenu();
            containerInvUI.ToggleInventoryMenu();
        }

        // Disable Inventory menus if they are open and tab is pressed
        if (GameControls.gamePlayActions.tab.WasPressed)
            DisableMenus();

        // Take everything from an open container
        if (GameControls.gamePlayActions.menuContainerTakeAll.WasPressed && containerInvUI.inventoryParent.activeSelf)
        {
            // TODO
        }

        // Use Item
        if (GameControls.gamePlayActions.menuUseItem.WasPressed)
        {
            if (activeInventorySlot != null)
                activeInventorySlot.UseItem();

            if (contextMenu.isActive)
                contextMenu.DisableContextMenu();

            if (stackSizeSelector.isActive)
                stackSizeSelector.HideStackSizeSelector();
        }

        // If we have an active Inventory Item and we click outside of the Inventory screen, drop the Item
        if (GameControls.gamePlayActions.menuSelect.WasPressed && activeInventorySlot == null)
        {
            // TODO
        }

        // Build the context menu
        if (GameControls.gamePlayActions.menuContext.WasPressed)
        {
            if (contextMenu.isActive == false && activeInventorySlot != null && activeInventorySlot.itemData != null)
            {
                if (contextMenu.isActive && contextMenu.activeInventorySlot != activeInventorySlot)
                    contextMenu.DisableContextMenu();

                contextMenu.BuildContextMenu(activeInventorySlot);
            }
            else if (contextMenu.isActive)
                contextMenu.DisableContextMenu();

            if (stackSizeSelector.isActive)
                stackSizeSelector.HideStackSizeSelector();
        }
        
        // Disable the context menu
        if (GameControls.gamePlayActions.menuSelect.WasPressed && contextMenu.isActive && ((activeInventorySlot != null && activeInventorySlot.itemData == null) || activeInventorySlot == null))
        {
            StartCoroutine(contextMenu.DelayDisableContextMenu());
        }

        // Split stack
        if (GameControls.gamePlayActions.menuSelect.WasPressed && GameControls.gamePlayActions.leftShift.IsPressed)
        {
            if (activeInventorySlot != null && activeInventorySlot.itemData != null && activeInventorySlot.currentStackSize > 1 && activeInventorySlot != stackSizeSelector.selectedInventorySlot)
            {
                // Show the Stack Size Selector
                stackSizeSelector.ShowStackSizeSelector(activeInventorySlot);
            }
            else if (stackSizeSelector.isActive && (activeInventorySlot == null || activeInventorySlot.itemData == null || activeInventorySlot.currentStackSize == 1))
            {
                // Hide the Stack Size Selector
                stackSizeSelector.HideStackSizeSelector();
            }
        }
    }

    public void SetSelectedItem()
    {
        if (GameControls.gamePlayActions.leftShift.IsPressed == false && onSelectionCooldown == false)
        {
            // TODO

            // Disable the Context Menu
            if (contextMenu.isActive)
                contextMenu.DisableContextMenu();

            // Disable the Stack Size Selector
            if (stackSizeSelector.isActive)
                stackSizeSelector.HideStackSizeSelector();
        }
    }

    public void DeselectItem()
    {
        // TODO
    }

    public bool UIMenuActive()
    {
        if (playerInventory.myInventoryUI.inventoryParent.activeSelf)
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
        if (playerInventory.myInventoryUI.inventoryParent.activeSelf)
        {
            playerInventory.myInventoryUI.ToggleInventoryMenu();
        }

        if (containerInvUI.inventoryParent.activeSelf)
            containerInvUI.ToggleInventoryMenu();
    }
}

using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [HideInInspector] public InventoryItem activeInvItem;

    [HideInInspector] public ContextMenu contextMenu;
    [HideInInspector] public StackSizeSelector stackSizeSelector;
    [HideInInspector] public TooltipManager tooltipManager;

    [HideInInspector] public bool equippedItemsInventoryActive;

    DropItemController dropItemController;
    PlayerManager playerManager;
    PlayerInventoryUI playerInvUI;
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
        playerInvUI = PlayerInventoryUI.instance;
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
            playerInvUI.ToggleInventoryMenu();
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
            if (activeInvItem != null)
                activeInvItem.UseItem();

            if (contextMenu.isActive)
                contextMenu.DisableContextMenu();

            if (stackSizeSelector.isActive)
                stackSizeSelector.HideStackSizeSelector();
        }

        // Transfer Item
        if (GameControls.gamePlayActions.menuSelect.WasReleased && activeInvItem != null)
        {
            activeInvItem.TransferItem();

            if (contextMenu.isActive)
                contextMenu.DisableContextMenu();

            if (stackSizeSelector.isActive)
                stackSizeSelector.HideStackSizeSelector();
        }

        // If we have an active Inventory Item and we click outside of the Inventory screen, drop the Item
        if (GameControls.gamePlayActions.menuSelect.WasReleased && activeInvItem == null)
        {
            // TODO
        }

        // Build the context menu
        if (GameControls.gamePlayActions.menuContext.WasPressed)
        {
            if (contextMenu.isActive == false && activeInvItem != null && activeInvItem.itemData != null)
            {
                if (contextMenu.isActive && contextMenu.activeInvItem != activeInvItem)
                    contextMenu.DisableContextMenu();

                contextMenu.BuildContextMenu(activeInvItem);
            }
            else if (contextMenu.isActive)
                contextMenu.DisableContextMenu();

            if (stackSizeSelector.isActive)
                stackSizeSelector.HideStackSizeSelector();
        }
        
        // Disable the context menu
        if (GameControls.gamePlayActions.menuSelect.WasPressed && contextMenu.isActive && ((activeInvItem != null && activeInvItem.itemData == null) || activeInvItem == null))
        {
            StartCoroutine(contextMenu.DelayDisableContextMenu());
        }

        // Split stack
        if (GameControls.gamePlayActions.menuSelect.WasPressed && GameControls.gamePlayActions.leftShift.IsPressed)
        {
            if (activeInvItem != null && activeInvItem.itemData != null && activeInvItem.itemData.currentStackSize > 1 && activeInvItem != stackSizeSelector.selectedInventorySlot)
            {
                // Show the Stack Size Selector
                stackSizeSelector.ShowStackSizeSelector(activeInvItem);
            }
            else if (stackSizeSelector.isActive && (activeInvItem == null || activeInvItem.itemData == null || activeInvItem.itemData.currentStackSize == 1))
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
        if (playerInvUI.inventoryParent.activeSelf)
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
        if (playerInvUI.inventoryParent.activeSelf)
            playerInvUI.ToggleInventoryMenu();

        if (containerInvUI.inventoryParent.activeSelf)
            containerInvUI.ToggleInventoryMenu();
    }
}

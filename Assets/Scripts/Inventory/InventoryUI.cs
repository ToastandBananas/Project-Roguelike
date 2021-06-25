using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI inventoryNameText;
    public Transform slotsParent;
    public GameObject inventoryParent;

    [HideInInspector] public Inventory inventory;
    [HideInInspector] public InventorySlot[] slots;
    [HideInInspector] public PlayerManager playerManager;

    DropItemController dropItemController;
    UIManager uiManager;

    public virtual void Start()
    {
        dropItemController = DropItemController.instance;
        uiManager = UIManager.instance;

        AddCallbacks();

        UpdateVisibleSlots();

        if (inventoryParent.activeSelf)
            inventoryParent.SetActive(false);

        playerManager = PlayerManager.instance;
    }

    void AddItem(ItemData itemData, int itemCount)
    {
        int itemCountRemaining = itemCount;
        // TODO
    }

    void RemoveItem(ItemData itemData, int itemCount, InventorySlot inventorySlot)
    {
        int itemCountRemaining = itemCount;
        // TODO
    }

    public void ToggleInventoryMenu()
    {
        inventoryParent.SetActive(!inventoryParent.activeSelf);

        // If the Inventory was menu was closed
        if (inventoryParent.activeSelf == false)
        {
            // Close the Context Menu
            //if (uiManager.contextMenu.isActive)
                //uiManager.contextMenu.DisableContextMenu();

            // Close the Stack Size Selector
            //if (uiManager.stackSizeSelector.isActive)
                //uiManager.stackSizeSelector.HideStackSizeSelector();

            //uiManager.tooltipManager.HideAllTooltips();
        }
    }

    public bool IsRoomInInventory(ItemData itemData, int itemCount)
    {
        /*if (item.maxStackSize == 1)
        {
            for (int i = 0; i < inventory.space; i++)
            {
                if (slots[i].gameObject.activeSelf && slots[i].IsEmpty())
                    return true;
            }
        }
        else
        {
            int remainingItemCount = itemCount;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == null)
                    return true;
                else if (item.maxStackSize > 1 && slots[i].item == item)
                    remainingItemCount -= item.maxStackSize - slots[i].currentStackSize;
            }

            if (remainingItemCount <= 0)
                return true;
        }*/
        
        // TODO

        Debug.Log("Not enough room in Inventory...");
        return false;
    }

    public void UpdateVisibleSlots()
    {
        if (inventory != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < inventory.maxWeight)
                    slots[i].gameObject.SetActive(true);
                else
                    slots[i].gameObject.SetActive(false);
            }
        }
    }

    public void AddCallbacks()
    {
        if (inventory != null && inventory.invUICallbacksAdded == false)
        {
            inventory.onItemAddedCallback += AddItem;
            inventory.onItemRemovedCallback += RemoveItem;
            inventory.invUICallbacksAdded = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenu : MonoBehaviour
{
    public ContextMenuButton[] buttons;

    [HideInInspector] public InventoryItem contextActiveInvItem;
    [HideInInspector] public bool isActive;

    Canvas canvas;
    GameManager gm;

    readonly float buttonHeight = 32f;
    readonly float buttonWidth = 110f;

    #region Singleton
    public static ContextMenu instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogError("More than one instance of ContextMenu. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }

        canvas = GetComponentInParent<Canvas>();
        gm = GameManager.instance;
    }

    public void BuildContextMenu(InventoryItem invItem)
    {
        isActive = true;
        contextActiveInvItem = invItem;

        if (contextActiveInvItem.myEquipmentManager != null)
        {
            CreateUnequipButton();

            if (gm.containerInvUI.activeInventory != null)
                CreateTransferButton();
            else
                CreateDropItemButton();
        }
        else if (gm.uiManager.activeInvItem != null)
        {
            if (gm.uiManager.activeInvItem.itemData.item.isUsable)
                CreateUseItemButton();

            if (gm.uiManager.activeInvItem.itemData.currentStackSize > 1)
                CreateSplitStackButton();

            // If selecting the player's inventory or equipment menu
            if ((contextActiveInvItem.myInventory != null && contextActiveInvItem.myInventory.myInvUI == gm.playerInvUI) || contextActiveInvItem.myEquipmentManager != null
                || (contextActiveInvItem.parentInvItem != null && contextActiveInvItem.parentInvItem.myInvUI == gm.playerInvUI))
            {
                if (gm.containerInvUI.activeInventory != null)
                    CreateTransferButton();
                else
                    CreateDropItemButton();
            }
            else
                CreateTransferButton();
        }

        float xPosAddon = 0;
        float yPosAddon = 0;

        // Get the desired position:
        // If the mouse position is too close to the bottom of the screen
        if (Input.mousePosition.y <= 190f)
            yPosAddon = GetActiveButtonCount() * buttonHeight;

        // If the mouse position is too far to the right of the screen
        if (Input.mousePosition.x >= 1780f)
            xPosAddon = -buttonWidth;

        transform.position = Input.mousePosition + new Vector3(xPosAddon, yPosAddon);
    }

    void CreateUseItemButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);
        
        if (gm.uiManager.activeInvItem.itemData.item.IsEquipment() == false && gm.uiManager.activeInvItem.itemData.item.IsConsumable() == false)
            contextButton.textMesh.text = "Use";
        else if (gm.uiManager.activeInvItem.itemData.item.IsConsumable())
        {
            Consumable consumable = (Consumable)gm.uiManager.activeInvItem.itemData.item;
            if (consumable.consumableType == ConsumableType.Food)
                contextButton.textMesh.text = "Eat";
            else
                contextButton.textMesh.text = "Drink";
        }
        else
            contextButton.textMesh.text = "Equip";

        contextButton.button.onClick.AddListener(UseItem);
    }

    void UseItem()
    {
        contextActiveInvItem.UseItem();
        DisableContextMenu();
    }

    void CreateUnequipButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        contextButton.textMesh.text = "Unequip";

        contextButton.button.onClick.AddListener(Unequip);
    }

    void Unequip()
    {
        Equipment equipment = (Equipment)contextActiveInvItem.itemData.item;
        gm.playerManager.equipmentManager.Unequip(equipment.equipmentSlot, true, true);
        DisableContextMenu();
    }

    void CreateTransferButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        if ((contextActiveInvItem.myInventory != null && contextActiveInvItem.myInventory.myInvUI == gm.playerInvUI) || contextActiveInvItem.myEquipmentManager != null
            || (contextActiveInvItem.parentInvItem != null && contextActiveInvItem.parentInvItem.myInvUI == gm.playerInvUI))
            contextButton.textMesh.text = "Transfer";
        else
            contextButton.textMesh.text = "Take";

        contextButton.button.onClick.AddListener(TransferItem);
    }

    void TransferItem()
    {
        contextActiveInvItem.TransferItem();
        DisableContextMenu();
    }

    void CreateSplitStackButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        contextButton.textMesh.text = "Split Stack";

        contextButton.button.onClick.AddListener(SplitStack);
    }

    void SplitStack()
    {
        gm.stackSizeSelector.ShowStackSizeSelector(contextActiveInvItem);
        DisableContextMenu();
    }

    void CreateDropItemButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        contextButton.textMesh.text = "Drop";

        contextButton.button.onClick.AddListener(DropItem);
    }

    void DropItem()
    {
        List<ItemData> itemsListAddingTo = gm.containerInvUI.GetItemsListFromActiveDirection();
        Vector3 dropPos = gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection();

        // Make sure there's room on the ground first
        if (gm.uiManager.IsRoomOnGround(contextActiveInvItem.itemData, itemsListAddingTo, dropPos))
        {
            gm.dropItemController.DropItem(dropPos, contextActiveInvItem.itemData, contextActiveInvItem.itemData.currentStackSize, contextActiveInvItem.myInventory, contextActiveInvItem);
            gm.containerInvUI.AddItemToActiveDirectionList(contextActiveInvItem.itemData);

            if (contextActiveInvItem.myEquipmentManager != null)
            {
                Equipment equipment = (Equipment)contextActiveInvItem.itemData.item;
                gm.playerManager.equipmentManager.Unequip(equipment.equipmentSlot, false, false);

                gm.playerInvUI.UpdateUI();
            }
            else
            {
                InventoryItem contextParentInvItem = null;
                if (contextActiveInvItem.parentInvItem != null)
                {
                    contextParentInvItem = contextActiveInvItem.parentInvItem;
                    contextParentInvItem.itemData.bagInventory.SubtractItemsWeightAndVolumeFromInventory(contextActiveInvItem.itemData, contextParentInvItem.itemData.bagInventory, contextActiveInvItem, contextActiveInvItem.itemData.currentStackSize, true);

                    if (contextActiveInvItem.myInvUI == gm.playerInvUI)
                        gm.playerInvUI.UpdateUI();
                    else
                        gm.containerInvUI.UpdateUI();
                }
                else if (contextActiveInvItem.myInventory != null && contextActiveInvItem.itemData.item.IsBag() == false)
                {
                    contextActiveInvItem.myInventory.currentWeight -= Mathf.RoundToInt(contextActiveInvItem.itemData.item.weight * contextActiveInvItem.itemData.currentStackSize * 100f) / 100f;
                    contextActiveInvItem.myInventory.currentVolume -= Mathf.RoundToInt(contextActiveInvItem.itemData.item.volume * contextActiveInvItem.itemData.currentStackSize * 100f) / 100f;

                    if (contextActiveInvItem.itemData.item.itemType == ItemType.Container)
                    {
                        for (int i = 0; i < contextActiveInvItem.itemData.bagInventory.items.Count; i++)
                        {
                            contextActiveInvItem.myInventory.currentWeight -= Mathf.RoundToInt(contextActiveInvItem.itemData.bagInventory.items[i].item.weight * contextActiveInvItem.itemData.bagInventory.items[i].currentStackSize * 100f) / 100f;
                            contextActiveInvItem.myInventory.currentVolume -= Mathf.RoundToInt(contextActiveInvItem.itemData.bagInventory.items[i].item.volume * contextActiveInvItem.itemData.bagInventory.items[i].currentStackSize * 100f) / 100f;
                        }
                    }

                    contextActiveInvItem.myInventory.myInvUI.UpdateUI();
                }
                else
                    gm.containerInvUI.UpdateUI();
                
                contextActiveInvItem.ClearItem();

                if (contextParentInvItem != null)
                    contextParentInvItem.UpdateItemNumberTexts();
            }
        }

        DisableContextMenu();
    }

    ContextMenuButton GetNextInactiveButton()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.activeSelf == false)
                return buttons[i];
        }

        Debug.LogError("Not enough Context Menu Buttons. Fix me!");
        return null;
    }

    public void DisableContextMenu()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].button.onClick.RemoveAllListeners();
            buttons[i].gameObject.SetActive(false);
        }

        isActive = false;
        contextActiveInvItem = null;
        gm.uiManager.activeContextMenuButton = null;
    }

    public IEnumerator DelayDisableContextMenu()
    {
        yield return new WaitForSeconds(0.1f);
        if (isActive)
            DisableContextMenu();
    }

    int GetActiveButtonCount()
    {
        int activeCount = 0;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.activeSelf)
                activeCount++;
        }

        return activeCount;
    }
}

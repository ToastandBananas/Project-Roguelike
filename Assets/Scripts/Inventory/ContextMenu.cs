using System.Collections;
using UnityEngine;

public class ContextMenu : MonoBehaviour
{
    public ContextMenuButton[] buttons;

    [HideInInspector] public InventoryItem activeInvItem;
    [HideInInspector] public bool isActive;

    Canvas canvas;
    GameManager gm;

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
        activeInvItem = invItem;

        if (activeInvItem.myEquipmentManager != null)
        {
            CreateUnequipButton();
            CreateTransferButton();
            CreateDropItemButton();
        }
        else if (gm.uiManager.activeInvItem != null)// && gm.uiManager.activeInvItem != activeInvItem)
        {
            if (gm.uiManager.activeInvItem.itemData.item.isUsable)
                CreateUseItemButton();

            if (gm.uiManager.activeInvItem.itemData.currentStackSize > 1)
                CreateSplitStackButton();

            CreateTransferButton();

            if ((activeInvItem.myInventory != null && activeInvItem.myInventory.myInventoryUI == gm.playerInvUI) || activeInvItem.myEquipmentManager != null)
                CreateDropItemButton();
        }

        // Get the desired position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out Vector2 pos);
        transform.position = canvas.transform.TransformPoint(pos) + new Vector3(1, -3f, 0);

        // If this slot is on the very bottom of the screen
        if (pos.y < -420f)
            transform.position += new Vector3(0, 4f, 0);
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
        activeInvItem.UseItem();
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
        Equipment equipment = (Equipment)activeInvItem.itemData.item;
        gm.playerManager.equipmentManager.Unequip(equipment.equipmentSlot, true);
    }

    void CreateTransferButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        if ((activeInvItem.myInventory != null && activeInvItem.myInventory.myInventoryUI == gm.playerInvUI) || activeInvItem.myEquipmentManager != null)
            contextButton.textMesh.text = "Transfer";
        else
            contextButton.textMesh.text = "Take";

        contextButton.button.onClick.AddListener(TransferItem);
    }

    void TransferItem()
    {
        activeInvItem.TransferItem();
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
        gm.stackSizeSelector.ShowStackSizeSelector(activeInvItem);
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
        gm.dropItemController.DropItem(gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection(), activeInvItem.itemData, activeInvItem.itemData.currentStackSize);
        gm.containerInvUI.AddItemToList(activeInvItem.itemData);

        if (activeInvItem.myEquipmentManager != null)
            Unequip();
        else
            activeInvItem.ClearItem();

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
        activeInvItem = null;
    }

    public IEnumerator DelayDisableContextMenu()
    {
        yield return new WaitForSeconds(0.1f);
        if (isActive)
            DisableContextMenu();
    }
}

using System.Collections;
using UnityEngine;

public class ContextMenu : MonoBehaviour
{
    public bool isActive;
    public InventoryItem activeInvItem;

    public ContextMenuButton[] buttons;

    Canvas canvas;
    DropItemController dropItemController;
    PlayerManager playerManager;
    StackSizeSelector stackSizeSelector;
    UIManager uiManager;

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
        dropItemController = DropItemController.instance;
        playerManager = PlayerManager.instance;
        stackSizeSelector = StackSizeSelector.instance;
        uiManager = UIManager.instance;
    }

    public void BuildContextMenu(InventoryItem invSlot)
    {
        isActive = true;

        if (uiManager.equippedItemsInventoryActive)
        {
            CreateUnequipButton();
            CreateDropItemButton();
        }
        else if (uiManager.activeInvItem != null && uiManager.activeInvItem != activeInvItem)
        {
            if (uiManager.activeInvItem.itemData.item.isUsable)
                CreateUseItemButton();

            if (uiManager.activeInvItem.itemData.currentStackSize > 1)
                CreateSplitStackButton();

            CreateDropItemButton();
        }
        
        activeInvItem = invSlot;

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
        
        if (uiManager.activeInvItem.itemData.item.IsEquipment() == false)
            contextButton.textMesh.text = "Use";
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
        playerManager.equipmentManager.Unequip(equipment.equipmentSlot, true);
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
        stackSizeSelector.ShowStackSizeSelector(activeInvItem);

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
        // TODO

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

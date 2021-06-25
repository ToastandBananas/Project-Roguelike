using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public ItemData itemData;

    [HideInInspector] public Image slotImage;
    [HideInInspector] public RectTransform rectTransform;

    [HideInInspector] public UIManager uiManager;

    public virtual void Init()
    {
        slotImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        uiManager = UIManager.instance;
    }
    
    public virtual void AddItem(ItemData newItemData)
    {
        itemData = newItemData;
    }

    public virtual void ClearSlot()
    {
        itemData.ClearData();
    }

    public bool IsEmpty()
    {
        if (itemData == null)
            return true;

        return false;
    }

    public void SelectItem()
    {
        // TODO
    }

    public virtual void PlaceSelectedItem()
    {
        // This is just meant to be overridden
    }

    public virtual bool IsInventorySlot()
    {
        return false;
    }

    public virtual bool IsEquipSlot()
    {
        return false;
    }
}

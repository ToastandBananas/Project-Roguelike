using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemBase : MonoBehaviour
{
    public ItemData itemData;

    [HideInInspector] public Image slotImage;
    [HideInInspector] public RectTransform rectTransform;

    [HideInInspector] public UIManager uiManager;

    [HideInInspector] public TextMeshProUGUI itemNameText;
    [HideInInspector] public TextMeshProUGUI itemAmountText;
    [HideInInspector] public TextMeshProUGUI itemTypeText;
    [HideInInspector] public TextMeshProUGUI itemWeightText;
    [HideInInspector] public TextMeshProUGUI itemVolumeText;

    public virtual void Init()
    {
        slotImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        itemData = GetComponent<ItemData>();

        uiManager = UIManager.instance;

        itemNameText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        itemAmountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        itemTypeText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        itemWeightText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        itemVolumeText = transform.GetChild(4).GetComponent<TextMeshProUGUI>();
    }
    
    public virtual void AddItem(ItemData newItemData)
    {
        itemData = newItemData;
    }

    public virtual void ClearItem()
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

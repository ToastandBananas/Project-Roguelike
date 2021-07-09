using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DisclosureWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public InventoryItem myInvItem;

    [Header("UI")]
    public Image image;
    public Button button;
    public Sprite rightArrowSprite, downArrowSprite;

    [HideInInspector] public List<InventoryItem> expandedItems = new List<InventoryItem>();

    [HideInInspector] public bool isExpanded, isEnabled, isSelected;

    void Awake()
    {
        isExpanded = false;
        isEnabled = false;
        isSelected = false;
    }

    public void ExpandDisclosureWidget()
    {
        if (myInvItem.itemData.bagInventory.items.Count > 0)
        {
            isExpanded = true;
            image.sprite = downArrowSprite;

            for (int i = myInvItem.itemData.bagInventory.items.Count - 1; i >= 0; i--)
            {
                myInvItem.myInvUI.ShowNewBagItem(myInvItem.itemData.bagInventory.items[i], myInvItem);
            }
        }
        else
            Debug.Log("You look inside the " + myInvItem.itemData.itemName + " and it's empty.");
    }

    public void ContractDisclosureWidget()
    {
        isExpanded = false;
        image.sprite = rightArrowSprite;
        
        for (int i = 0; i < expandedItems.Count; i++)
        {
            expandedItems[i].CollapseItem();
        }

        expandedItems.Clear();
    }

    public void ToggleDisclosureWidget()
    {
        if (expandedItems.Count > 0 || isExpanded)
            ContractDisclosureWidget();
        else
            ExpandDisclosureWidget();
    }

    public void EnableDisclosureWidget()
    {
        isEnabled = true;
        image.enabled = true;
        image.sprite = rightArrowSprite;
        button.enabled = true;
    }

    public void DisableDisclosureWidget()
    {
        isEnabled = false;
        image.enabled = false;
        button.enabled = false;
        isSelected = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isSelected = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isSelected = false;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StackSizeSelector : MonoBehaviour
{
    public GameObject uiParent;
    public Button leftArrowButton, rightArrowButton, submitButton;
    public TMP_InputField inputField;

    [HideInInspector] public InventoryItem selectedInvItem;

    [HideInInspector] public bool isActive;

    GameManager gm;

    int currentValue;
    int maxValue;

    public static StackSizeSelector instance;

    void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogError("More than one instance of StackSizeSelector. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
        #endregion

        if (uiParent.activeSelf)
            HideStackSizeSelector();
    }

    void Start()
    {
        gm = GameManager.instance;
        inputField.selectionColor = new Color(0, 0, 0, 0);
    }

    void Update()
    {
        if (isActive)
        {
            if (GameControls.gamePlayActions.enter.WasPressed)
                Submit();

            if (GameControls.gamePlayActions.menuLeft.WasPressed)
                SubtractFromCurrentValue();

            if (GameControls.gamePlayActions.menuRight.WasPressed)
                AddToCurrentValue();
        }
    }

    public void AddToCurrentValue()
    {
        if (currentValue < maxValue)
        {
            currentValue++;
            inputField.text = currentValue.ToString();
        }
    }

    public void SubtractFromCurrentValue()
    {
        if (currentValue > 0)
        {
            currentValue--;
            inputField.text = currentValue.ToString();
        }
    }

    public void SetCurrentValue()
    {
        int.TryParse(inputField.text, out currentValue);

        if (currentValue > maxValue)
        {
            currentValue = maxValue;
            inputField.text = currentValue.ToString();
        }
        else if (currentValue < 0)
        {
            currentValue = 0;
            inputField.text = currentValue.ToString();
        }
    }
    
    public void Submit()
    {
        if (selectedInvItem != null && currentValue > 0)
        {
            selectedInvItem.itemData.currentStackSize -= currentValue;
            selectedInvItem.UpdateItemTexts();

            ItemData newItemData = null;
            if (selectedInvItem.myInventory != null) // If the item is in an inventory
            {
                newItemData = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();
                newItemData.transform.SetParent(selectedInvItem.myInventory.itemsParent);
                newItemData.gameObject.SetActive(true);

                selectedInvItem.myInventory.items.Add(newItemData);
                if (selectedInvItem.myInventory.myInventoryUI == gm.containerInvUI)
                    gm.containerInvUI.AddItemToList(newItemData);

                #if UNITY_EDITOR
                    newItemData.name = selectedInvItem.itemData.itemName;
                #endif
            }
            else // If the item is on the ground
            {
                ItemPickup newItemPickup = gm.objectPoolManager.pickupsPool.GetPooledItemPickup();
                newItemData = newItemPickup.itemData;
                newItemPickup.spriteRenderer.sprite = selectedInvItem.itemData.item.pickupSprite;
                newItemPickup.transform.position = gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection();
                newItemPickup.gameObject.SetActive(true);
                gm.containerInvUI.AddItemToList(newItemData);

                #if UNITY_EDITOR
                    newItemPickup.name = selectedInvItem.itemData.itemName;
                #endif
            }

            selectedInvItem.itemData.TransferData(selectedInvItem.itemData, newItemData);
            newItemData.currentStackSize = currentValue;

            InventoryItem newInvItem = null;
            if ((selectedInvItem.myInventory != null && selectedInvItem.myInventory.myInventoryUI == gm.containerInvUI) || (selectedInvItem.myInventory == null && selectedInvItem.myEquipmentManager == null))
                newInvItem = gm.containerInvUI.ShowNewInventoryItem(newItemData);
            else
                newInvItem = gm.playerInvUI.ShowNewInventoryItem(newItemData);
            
            //newInvItem.UpdateItemTexts();

            if (selectedInvItem.itemData.currentStackSize > 0)
                selectedInvItem.UpdateItemTexts();
            else
                selectedInvItem.ClearItem();
        }

        HideStackSizeSelector();
    }

    public void ShowStackSizeSelector(InventoryItem inventorySlot)
    {
        uiParent.SetActive(true);
        isActive = true;

        inputField.Select();

        selectedInvItem = inventorySlot;
        maxValue = inventorySlot.itemData.currentStackSize - 1;
        currentValue = 1;
        inputField.text = currentValue.ToString();

        transform.position = inventorySlot.transform.position;
    }

    public void HideStackSizeSelector()
    {
        uiParent.SetActive(false);
        isActive = false;
        Reset();
    }

    public IEnumerator DelayHideStackSizeSelector()
    {
        yield return new WaitForSeconds(0.1f);
        if (isActive)
            HideStackSizeSelector();
    }

    void Reset()
    {
        currentValue = 1;
        inputField.text = currentValue.ToString();
        selectedInvItem = null;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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
    
    // Split the stack based off of the currentValue shown in the stack size selector's in game UI
    public void Submit()
    {
        if (selectedInvItem != null && currentValue > 0)
        {
            selectedInvItem.itemData.currentStackSize -= currentValue;
            selectedInvItem.UpdateItemNumberTexts();

            ItemData newItemData = null;
            if (selectedInvItem.myInventory != null) // If the item is in an inventory
            {
                newItemData = gm.uiManager.CreateNewItemDataChild(selectedInvItem.itemData, selectedInvItem.myInventory, selectedInvItem.myInventory.itemsParent, false); 
                // gm.objectPoolManager.itemDataObjectPool.GetPooledItemData(selectedInvItem.myInventory);
                // newItemData.transform.SetParent(selectedInvItem.myInventory.itemsParent);
                // newItemData.gameObject.SetActive(true);

                if (gm.playerManager.carriedItems.Contains(selectedInvItem.itemData))
                    gm.playerManager.carriedItems.Add(newItemData);
                else
                    selectedInvItem.myInventory.items.Add(newItemData);

                if (selectedInvItem.myInventory.myInvUI == gm.containerInvUI && selectedInvItem.parentInvItem == null)
                    gm.containerInvUI.AddItemToActiveDirectionList(newItemData);

                //#if UNITY_EDITOR
                    //newItemData.name = selectedInvItem.itemData.itemName;
                //#endif
            }
            else // If the item is on the ground
            {
                ItemPickup newItemPickup = gm.objectPoolManager.itemPickupsPool.GetPooledItemPickup();

                newItemData = newItemPickup.itemData;
                gm.dropItemController.SetupItemPickup(newItemPickup, selectedInvItem.itemData, currentValue, gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection());
                gm.containerInvUI.AddItemToActiveDirectionList(newItemData);
                selectedInvItem.itemData.TransferData(selectedInvItem.itemData, newItemData);
            }

            newItemData.currentStackSize = currentValue;

            InventoryItem newInvItem = null;
            if ((selectedInvItem.myInventory != null && selectedInvItem.myInventory.myInvUI == gm.containerInvUI) || (selectedInvItem.myInventory == null && selectedInvItem.myEquipmentManager == null))
            {
                if (selectedInvItem.parentInvItem != null)
                    newInvItem = gm.containerInvUI.ShowNewBagItem(newItemData, selectedInvItem.parentInvItem);
                else
                    newInvItem = gm.containerInvUI.ShowNewInventoryItem(newItemData);

                List<ItemData> directionalItemsList = gm.containerInvUI.GetItemsListFromActiveDirection();
                if (directionalItemsList.Contains(newInvItem.itemData))
                {
                    directionalItemsList.RemoveAt(directionalItemsList.IndexOf(newInvItem.itemData));
                    directionalItemsList.Insert(directionalItemsList.IndexOf(selectedInvItem.itemData) + 1, newInvItem.itemData);
                }
            }
            else
            {
                if (selectedInvItem.parentInvItem != null)
                    newInvItem = gm.playerInvUI.ShowNewBagItem(newItemData, selectedInvItem.parentInvItem);
                else
                    newInvItem = gm.playerInvUI.ShowNewInventoryItem(newItemData);
            }
            
            if (selectedInvItem.myInventory != null)
            {
                newInvItem.transform.SetSiblingIndex(selectedInvItem.transform.GetSiblingIndex());
                if (gm.playerManager.carriedItems.Contains(selectedInvItem.itemData) == false)
                {
                    selectedInvItem.myInventory.items.RemoveAt(selectedInvItem.myInventory.items.IndexOf(newInvItem.itemData));
                    selectedInvItem.myInventory.items.Insert(selectedInvItem.myInventory.items.IndexOf(selectedInvItem.itemData) + 1, newInvItem.itemData);
                }
            }
            else
                newInvItem.transform.SetSiblingIndex(selectedInvItem.transform.GetSiblingIndex() + 1);

            if (selectedInvItem.itemData.currentStackSize > 0)
                selectedInvItem.UpdateItemNumberTexts();
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

        float xPosAddon = 0;
        float yPosAddon = 0;

        if (Input.mousePosition.y < 65f)
            yPosAddon = 20f;

        if (Input.mousePosition.x <= 85)
            xPosAddon = 100f;
        else if (Input.mousePosition.x >= 1830)
            xPosAddon = -100f;

        transform.position = new Vector2(Input.mousePosition.x + xPosAddon, inventorySlot.transform.position.y + yPosAddon);
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

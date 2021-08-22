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
            SplitStack(selectedInvItem, currentValue);

        HideStackSizeSelector();
    }

    public InventoryItem SplitStack(InventoryItem invItem, int newStackSize)
    {
        invItem.itemData.currentStackSize -= newStackSize;
        invItem.UpdateItemNumberTexts();

        ItemData newItemData = null;
        if (invItem.myInventory != null) // If the item is in an inventory
        {
            newItemData = gm.uiManager.CreateNewItemDataChild(invItem.itemData, invItem.myInventory, invItem.myInventory.itemsParent, false);

            if (gm.playerManager.carriedItems.Contains(invItem.itemData))
                gm.playerManager.carriedItems.Add(newItemData);
            else
                invItem.myInventory.items.Add(newItemData);

            if (invItem.myInventory.myInvUI == gm.containerInvUI && invItem.parentInvItem == null)
                gm.containerInvUI.AddItemToActiveDirectionList(newItemData);
        }
        else // If the item is on the ground
        {
            ItemPickup newItemPickup = gm.objectPoolManager.itemPickupsPool.GetPooledItemPickup();

            newItemData = newItemPickup.itemData;
            gm.dropItemController.SetupItemPickup(newItemPickup, invItem.itemData, newStackSize, gm.playerManager.transform.position + gm.dropItemController.GetDropPositionFromActiveDirection());
            gm.containerInvUI.AddItemToActiveDirectionList(newItemData);
            invItem.itemData.TransferData(invItem.itemData, newItemData);
        }

        newItemData.currentStackSize = newStackSize;

        InventoryItem newInvItem = null;
        if ((invItem.myInventory != null && invItem.myInventory.myInvUI == gm.containerInvUI) || (invItem.myInventory == null && invItem.myEquipmentManager == null))
        {
            if (invItem.parentInvItem != null)
                newInvItem = gm.containerInvUI.ShowNewBagItem(newItemData, invItem.parentInvItem);
            else
                newInvItem = gm.containerInvUI.ShowNewInventoryItem(newItemData);

            List<ItemData> directionalItemsList = gm.containerInvUI.GetItemsListFromActiveDirection();
            if (directionalItemsList.Contains(newInvItem.itemData))
            {
                directionalItemsList.RemoveAt(directionalItemsList.IndexOf(newInvItem.itemData));
                directionalItemsList.Insert(directionalItemsList.IndexOf(invItem.itemData) + 1, newInvItem.itemData);
            }
        }
        else
        {
            if (invItem.parentInvItem != null)
                newInvItem = gm.playerInvUI.ShowNewBagItem(newItemData, invItem.parentInvItem);
            else
                newInvItem = gm.playerInvUI.ShowNewInventoryItem(newItemData);
        }

        if (invItem.myInventory != null)
        {
            newInvItem.transform.SetSiblingIndex(invItem.transform.GetSiblingIndex());
            if (gm.playerManager.carriedItems.Contains(invItem.itemData) == false)
            {
                invItem.myInventory.items.RemoveAt(invItem.myInventory.items.IndexOf(newInvItem.itemData));
                invItem.myInventory.items.Insert(invItem.myInventory.items.IndexOf(invItem.itemData) + 1, newInvItem.itemData);
            }
        }
        else
            newInvItem.transform.SetSiblingIndex(invItem.transform.GetSiblingIndex() + 1);

        if (invItem.itemData.currentStackSize > 0)
            invItem.UpdateItemNumberTexts();
        else
            invItem.ClearItem();

        return newInvItem;
    }

    public void ShowStackSizeSelector(InventoryItem invItem)
    {
        uiParent.SetActive(true);
        isActive = true;

        inputField.Select();

        selectedInvItem = invItem;
        maxValue = invItem.itemData.currentStackSize - 1;
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

        transform.position = new Vector2(Input.mousePosition.x + xPosAddon, invItem.transform.position.y + yPosAddon);
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

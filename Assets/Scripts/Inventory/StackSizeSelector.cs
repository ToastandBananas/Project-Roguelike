using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StackSizeSelector : MonoBehaviour
{
    public GameObject uiParent;
    public Button leftArrowButton, rightArrowButton, submitButton;
    public TMP_InputField inputField;

    [HideInInspector] public InventoryItem selectedInventorySlot;

    [HideInInspector] public bool isActive;

    UIManager uiManager;

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
        uiManager = UIManager.instance;
        inputField.selectionColor = new Color(0, 0, 0, 0);
    }

    void Update()
    {
        if (GameControls.gamePlayActions.enter.WasPressed && isActive)
            Submit();
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
        if (selectedInventorySlot != null && currentValue > 0)
        {
            // TODO: Create a new stack

            selectedInventorySlot.itemData.currentStackSize -= currentValue;

            if (selectedInventorySlot.itemData.currentStackSize > 0)
                selectedInventorySlot.UpdateItemTexts();
            else
                selectedInventorySlot.ClearItem();
        }

        HideStackSizeSelector();
    }

    public void ShowStackSizeSelector(InventoryItem inventorySlot)
    {
        uiParent.SetActive(true);
        isActive = true;

        inputField.Select();

        selectedInventorySlot = inventorySlot;
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
        selectedInventorySlot = null;
    }
}

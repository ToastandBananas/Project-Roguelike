using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    public ItemData itemData;
    public int currentStackSize;

    [HideInInspector] public ItemData originItemData;
    [HideInInspector] public Image slotImage;
    [HideInInspector] public RectTransform rectTransform;

    [HideInInspector] public UIManager uiManager;

    [HideInInspector] public TextMeshProUGUI itemNameText;
    [HideInInspector] public TextMeshProUGUI itemAmountText;
    [HideInInspector] public TextMeshProUGUI itemTypeText;
    [HideInInspector] public TextMeshProUGUI itemWeightText;
    [HideInInspector] public TextMeshProUGUI itemVolumeText;

    [HideInInspector] public Inventory myInventory;

    ContainerInventoryUI containerInvUI;
    ObjectPoolManager objectPoolManager;
    PlayerInventoryUI playerInvUI;
    PlayerEquipmentManager playerEquipmentManager;

    public void Init()
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

        containerInvUI = ContainerInventoryUI.instance;
        objectPoolManager = ObjectPoolManager.instance;
        playerInvUI = PlayerInventoryUI.instance;
        playerEquipmentManager = PlayerEquipmentManager.instance;

        itemData = GetComponent<ItemData>();
    }

    public void ClearUI()
    {
        originItemData = null;
        itemData.ClearData();
    }

    public void ClearItem()
    {
        containerInvUI.UpdateUINumbers();

        if (myInventory != null)
            myInventory.items.Remove(originItemData);

        if (originItemData.CompareTag("Item Data Object"))
        {
            originItemData.transform.SetParent(objectPoolManager.itemDataObjectPool.transform);
            if (objectPoolManager.itemDataObjectPool.pooledObjects.Contains(originItemData.gameObject) == false)
            {
                objectPoolManager.itemDataObjectPool.pooledObjects.Add(originItemData.gameObject);
                objectPoolManager.itemDataObjectPool.pooledItemDatas.Add(originItemData);
            }
        }

        originItemData.ClearData();
        originItemData.gameObject.SetActive(false);

        ClearUI();
        gameObject.SetActive(false);
    }

    public bool IsEmpty()
    {
        if (itemData == null)
            return true;

        return false;
    }

    public void UseItem(int amountToUse = 1)
    {
        if (itemData != null)
        {
            if (myInventory.HasEnough(itemData, amountToUse))
                itemData.item.Use(playerEquipmentManager, myInventory, this, amountToUse);
            else
                Debug.Log("You don't have enough " + itemData.name);
        }
    }

    public void SelectItem()
    {
        // TODO
    }

    public void PlaceSelectedItem()
    {
        // TODO
    }

    public virtual bool IsInventorySlot()
    {
        return false;
    }

    public virtual bool IsEquipSlot()
    {
        return false;
    }

    public void UpdateStackSizeText()
    {
        // TODO
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1) // Single click
        {
            
        }
        else if (eventData.clickCount == 2) // Double click
        {
            if (myInventory == null || myInventory.myInventoryUI == containerInvUI) // If we're taking this item from a container or the ground
            {
                if (playerInvUI.activeInventory != null && playerInvUI.activeInventory != playerInvUI.keysInventory // We don't want to add items directly to the keys inv (unless it's a key) or to the current equipment inv
                    || (playerInvUI.activeInventory == playerInvUI.keysInventory && itemData.item.itemType == ItemType.Key)) // If the item is a key and the key inventory is active
                {
                    if (playerInvUI.activeInventory.Add(itemData, itemData.currentStackSize)) // If there's room in the inventory
                    {
                        ClearItem();
                    }
                    else // If there wasn't enough room in the inventory, try adding 1 at a time, until we can't fit anymore
                    {
                        if (itemData.item.maxStackSize > 1)
                            AddItemToInventory_OneAtATime(playerInvUI.activeInventory, itemData);

                        // Now try to fit the item in other bags (if there are any)
                        if (itemData.currentStackSize > 0 && playerInvUI.bag1Active && playerInvUI.activeInventory != playerInvUI.bag1Inventory)
                            AddItemToInventory_OneAtATime(playerInvUI.bag1Inventory, itemData);
                        
                        if (itemData.currentStackSize > 0 && playerInvUI.bag2Active && playerInvUI.activeInventory != playerInvUI.bag2Inventory)
                            AddItemToInventory_OneAtATime(playerInvUI.bag2Inventory, itemData);
                        
                        if (itemData.currentStackSize > 0 && playerInvUI.bag3Active && playerInvUI.activeInventory != playerInvUI.bag3Inventory)
                            AddItemToInventory_OneAtATime(playerInvUI.bag3Inventory, itemData);
                        
                        if (itemData.currentStackSize > 0 && playerInvUI.bag4Active && playerInvUI.activeInventory != playerInvUI.bag4Inventory)
                            AddItemToInventory_OneAtATime(playerInvUI.bag4Inventory, itemData);
                        
                        if (itemData.currentStackSize > 0 && playerInvUI.bag5Active && playerInvUI.activeInventory != playerInvUI.bag5Inventory)
                            AddItemToInventory_OneAtATime(playerInvUI.bag5Inventory, itemData);

                        // Now try to fit the item in the player's personal inventory
                        if (itemData.currentStackSize > 0 && playerInvUI.activeInventory != playerInvUI.personalInventory)
                            AddItemToInventory_OneAtATime(playerInvUI.personalInventory, itemData);
                    }
                }
                else // Otherwise, add the item to the first available bag, or the personal inventory if there's no bag or no room in any of the bags
                {
                    if (itemData.currentStackSize > 0 && playerInvUI.bag1Active)
                        AddItemToInventory_OneAtATime(playerInvUI.bag1Inventory, itemData);

                    if (itemData.currentStackSize > 0 && playerInvUI.bag2Active)
                        AddItemToInventory_OneAtATime(playerInvUI.bag2Inventory, itemData);

                    if (itemData.currentStackSize > 0 && playerInvUI.bag3Active)
                        AddItemToInventory_OneAtATime(playerInvUI.bag3Inventory, itemData);

                    if (itemData.currentStackSize > 0 && playerInvUI.bag4Active)
                        AddItemToInventory_OneAtATime(playerInvUI.bag4Inventory, itemData);

                    if (itemData.currentStackSize > 0 && playerInvUI.bag5Active)
                        AddItemToInventory_OneAtATime(playerInvUI.bag5Inventory, itemData);

                    // Now try to fit the item in the player's personal inventory
                    if (itemData.currentStackSize > 0)
                        AddItemToInventory_OneAtATime(playerInvUI.personalInventory, itemData);
                }

                playerInvUI.UpdateUINumbers();
            }
            else // If we're taking this item from the player's inventory
            {
                // TODO: Place item in the inventory...or if not searching a container, then put item on ground
                if (containerInvUI.activeInventory != null) // If we're trying to place the item in a container
                {
                    if (containerInvUI.activeInventory.Add(itemData, itemData.currentStackSize)) // Try adding the item's entire stack
                        ClearItem();
                    else if (itemData.currentStackSize > 1) // If there wasn't room for all of the items, try adding them one at a time
                        AddItemToInventory_OneAtATime(containerInvUI.activeInventory, itemData);
                }
                else // If we're trying to place the item on the ground
                {
                    Debug.Log("Placing on ground");
                }

                playerInvUI.UpdateUINumbers();
            }
        }
    }

    void AddItemToInventory_OneAtATime(Inventory inventory, ItemData itemData)
    {
        for (int i = 0; i < itemData.currentStackSize; i++)
        {
            if (inventory.Add(itemData, 1))
            {
                if (itemData.currentStackSize == 0)
                    ClearItem();
            }
            else
            {
                break; // If there's no longer any room, break out of the loop
            }
        }
    }
}

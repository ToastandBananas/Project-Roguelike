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
    DropItemController dropItemController;
    ObjectPoolManager objectPoolManager;
    PlayerInventoryUI playerInvUI;
    PlayerEquipmentManager playerEquipmentManager;
    PlayerManager playerManager;

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
        dropItemController = DropItemController.instance;
        objectPoolManager = ObjectPoolManager.instance;
        playerInvUI = PlayerInventoryUI.instance;
        playerEquipmentManager = PlayerEquipmentManager.instance;
        playerManager = PlayerManager.instance;

        itemData = GetComponent<ItemData>();
    }

    public void ClearUI()
    {
        originItemData = null;
        itemData.ClearData();
    }

    public void ClearItem()
    {
        containerInvUI.RemoveItemFromList(originItemData);

        if (myInventory != null)
            myInventory.items.Remove(originItemData);

        containerInvUI.UpdateUINumbers();

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
                    if (playerInvUI.activeInventory.Add(itemData, itemData.currentStackSize, myInventory)) // If there's room in the inventory
                    {
                        ClearItem();
                    }
                    else // If there wasn't enough room in the inventory, try adding 1 at a time, until we can't fit anymore
                    {
                        if (itemData.item.maxStackSize > 1)
                            AddItemToInventory_OneAtATime(myInventory, playerInvUI.activeInventory, itemData);

                        // Now try to fit the item in other bags (if there are any)
                        if (itemData.currentStackSize > 0 && playerInvUI.bag1Active && playerInvUI.activeInventory != playerInvUI.bag1Inventory)
                            AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag1Inventory, itemData);
                        
                        if (itemData.currentStackSize > 0 && playerInvUI.bag2Active && playerInvUI.activeInventory != playerInvUI.bag2Inventory)
                            AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag2Inventory, itemData);
                        
                        if (itemData.currentStackSize > 0 && playerInvUI.bag3Active && playerInvUI.activeInventory != playerInvUI.bag3Inventory)
                            AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag3Inventory, itemData);
                        
                        if (itemData.currentStackSize > 0 && playerInvUI.bag4Active && playerInvUI.activeInventory != playerInvUI.bag4Inventory)
                            AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag4Inventory, itemData);
                        
                        if (itemData.currentStackSize > 0 && playerInvUI.bag5Active && playerInvUI.activeInventory != playerInvUI.bag5Inventory)
                            AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag5Inventory, itemData);

                        // Now try to fit the item in the player's personal inventory
                        if (itemData.currentStackSize > 0 && playerInvUI.activeInventory != playerInvUI.personalInventory)
                            AddItemToInventory_OneAtATime(myInventory, playerInvUI.personalInventory, itemData);
                    }
                }
                else // Otherwise, add the item to the first available bag, or the personal inventory if there's no bag or no room in any of the bags
                {
                    if (itemData.currentStackSize > 0 && playerInvUI.bag1Active)
                        AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag1Inventory, itemData);

                    if (itemData.currentStackSize > 0 && playerInvUI.bag2Active)
                        AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag2Inventory, itemData);

                    if (itemData.currentStackSize > 0 && playerInvUI.bag3Active)
                        AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag3Inventory, itemData);

                    if (itemData.currentStackSize > 0 && playerInvUI.bag4Active)
                        AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag4Inventory, itemData);

                    if (itemData.currentStackSize > 0 && playerInvUI.bag5Active)
                        AddItemToInventory_OneAtATime(myInventory, playerInvUI.bag5Inventory, itemData);

                    // Now try to fit the item in the player's personal inventory
                    if (itemData.currentStackSize > 0)
                        AddItemToInventory_OneAtATime(myInventory, playerInvUI.personalInventory, itemData);
                }

                playerInvUI.UpdateUINumbers();
            }
            else // If we're taking this item from the player's inventory
            {
                if (containerInvUI.activeInventory != null) // If we're trying to place the item in a container
                {
                    if (containerInvUI.activeInventory.Add(itemData, itemData.currentStackSize, myInventory)) // Try adding the item's entire stack
                        ClearItem();
                    else if (itemData.currentStackSize > 1) // If there wasn't room for all of the items, try adding them one at a time
                        AddItemToInventory_OneAtATime(myInventory, containerInvUI.activeInventory, itemData);
                }
                else // If we're trying to place the item on the ground
                {
                    ItemPickup newItemPickup = dropItemController.DropItem(playerManager.transform.position, itemData, itemData.currentStackSize);
                    containerInvUI.playerPositionItems.Add(newItemPickup.itemData);

                    if (containerInvUI.activeDirection == Direction.Center)
                        containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);

                    playerInvUI.activeInventory.items.Remove(itemData);
                    playerInvUI.activeInventory.currentWeight -= itemData.item.weight * itemData.currentStackSize;
                    playerInvUI.activeInventory.currentWeight = Mathf.RoundToInt(playerInvUI.activeInventory.currentWeight * 100f) / 100f;
                    playerInvUI.activeInventory.currentVolume -= itemData.item.volume * itemData.currentStackSize;
                    playerInvUI.activeInventory.currentVolume = Mathf.RoundToInt(playerInvUI.activeInventory.currentVolume * 100f) / 100f;

                    ClearItem();
                }

                playerInvUI.UpdateUINumbers();
            }
        }
    }

    void AddItemToInventory_OneAtATime(Inventory invComingFrom, Inventory invAddingTo, ItemData itemData)
    {
        for (int i = 0; i < itemData.currentStackSize; i++)
        {
            if (invAddingTo.Add(itemData, 1, invComingFrom))
            {
                if (itemData.currentStackSize == 0)
                    ClearItem();
            }
            else
            {
                containerInvUI.UpdateUINumbers();
                break; // If there's no longer any room, break out of the loop
            }
        }
    }
}

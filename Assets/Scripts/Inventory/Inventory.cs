using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public delegate void OnItemAdded(ItemData itemData, int itemCount);
    public OnItemAdded onItemAddedCallback;

    public delegate void OnItemRemoved(ItemData itemData, int itemCount, InventoryItem inventorySlot);
    public OnItemRemoved onItemRemovedCallback;

    public float maxWeight = 20;
    public float maxVolume = 20;

    public List<ItemData> items = new List<ItemData>();

    [HideInInspector] public Container container;
    [HideInInspector] public InventoryUI myInventoryUI;
    [HideInInspector] public GameObject inventoryOwner;

    [HideInInspector] public bool hasBeenInitialized;

    Transform itemsParent;

    public virtual void Start()
    {
        Init();
    }

    public void Init()
    {
        if (hasBeenInitialized == false)
        {
            inventoryOwner = gameObject;
            itemsParent = transform.Find("Items");
            TryGetComponent(out container);

            for (int i = 0; i < itemsParent.childCount; i++)
            {
                items.Add(itemsParent.GetChild(i).GetComponent<ItemData>());
            }

            if (items.Count > 0)
            {
                // Clamp itemCount values if we accidentally set them over the Item's maxStackSize
                for (int i = 0; i < items.Count; i++)
                {
                    // TODO
                }
            }

            hasBeenInitialized = true;
        }
    }

    public bool Add(ItemData itemData, int itemCount)
    {
        bool hasRoom = false;

        if (inventoryOwner.CompareTag("NPC"))
            hasRoom = true;
        else
            hasRoom = myInventoryUI.IsRoomInInventory(itemData, itemCount);

        if (hasRoom == false)
        {
            Debug.Log("Not enough room in Inventory...");
            return false;
        }

        if (onItemAddedCallback != null)
            onItemAddedCallback.Invoke(itemData, itemCount);
        else if (inventoryOwner.CompareTag("NPC"))
        {
            // NPC's won't have an active InventoryUI, so we will just directly add it to their items/itemCounts lists
            items.Add(itemData);
        }

        return true;
    }

    public void Remove(ItemData itemData, int itemCount, InventoryItem inventorySlot)
    {
        if (onItemRemovedCallback != null)
            onItemRemovedCallback.Invoke(itemData, itemCount, inventorySlot);
    }

    public bool HasEnough(ItemData itemData, int itemAmountRequired)
    {
        int itemCountInInventory = 0;

        // Add up how much of this Item that we have in our Inventory
        for (int i = 0; i < myInventoryUI.inventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            if (myInventoryUI.inventoryItemObjectPool.pooledInventoryItems[i].itemData.item == itemData.item)
                itemCountInInventory += myInventoryUI.inventoryItemObjectPool.pooledInventoryItems[i].currentStackSize;
        }

        if (itemCountInInventory >= itemAmountRequired)
            return true;

        return false;
    }
}

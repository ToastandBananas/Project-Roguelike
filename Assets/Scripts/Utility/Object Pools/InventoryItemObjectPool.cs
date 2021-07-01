using System.Collections.Generic;
using UnityEngine;

public class InventoryItemObjectPool : ObjectPool
{
    [HideInInspector] public List<InventoryItem> pooledInventoryItems = new List<InventoryItem>();
    [HideInInspector] public List<InventoryItem> activePooledInventoryItems = new List<InventoryItem>();

    public override void Start()
    {
        Init();
    }

    public override void Init()
    {
        if (hasBeenInitialized == false)
        {
            // Add existing objects to the list
            for (int i = 0; i < transform.childCount; i++)
            {
                InventoryItem existingInvItem = transform.GetChild(i).GetComponent<InventoryItem>();
                pooledInventoryItems.Add(existingInvItem);
                pooledObjects.Add(existingInvItem.gameObject);
                existingInvItem.Init();
            }

            // Create and add new objects to the list
            InventoryItem invItem;
            for (int i = 0; i < amountToPool; i++)
            {
                invItem = Instantiate(objectToPool.GetComponent<InventoryItem>());
                invItem.transform.SetParent(transform);
                invItem.gameObject.SetActive(false);
                pooledInventoryItems.Add(invItem);
                pooledObjects.Add(invItem.gameObject);
                invItem.Init();
            }

            hasBeenInitialized = true;
        }
    }

    public InventoryItem GetPooledInventoryItem()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (pooledInventoryItems[i].itemData == null)
            {
                pooledInventoryItems[i].transform.SetAsLastSibling();
                activePooledInventoryItems.Add(pooledInventoryItems[i]);
                return pooledInventoryItems[i];
            }
        }

        InventoryItem invItem = Instantiate(objectToPool).GetComponent<InventoryItem>();
        invItem.transform.SetParent(transform);
        pooledInventoryItems.Add(invItem);
        pooledObjects.Add(invItem.gameObject);
        activePooledInventoryItems.Add(invItem);
        return invItem;
    }
}

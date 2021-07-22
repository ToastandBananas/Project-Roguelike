using System.Collections.Generic;
using UnityEngine;

public class ItemPickupObjectPool : ObjectPool
{
    [HideInInspector] public List<ItemPickup> pooledItemPickups = new List<ItemPickup>();

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
                ItemPickup existingItemPickup = transform.GetChild(i).GetComponent<ItemPickup>();
                pooledItemPickups.Add(existingItemPickup);
                pooledObjects.Add(existingItemPickup.gameObject);
            }

            // Create and add new objects to the list
            ItemPickup itemPickup;
            for (int i = 0; i < amountToPool; i++)
            {
                itemPickup = Instantiate(objectToPool.GetComponent<ItemPickup>());
                itemPickup.transform.SetParent(transform);
                itemPickup.gameObject.SetActive(false);
                pooledItemPickups.Add(itemPickup);
                pooledObjects.Add(itemPickup.gameObject);
            }

            hasBeenInitialized = true;
        }
    }

    public ItemPickup GetPooledItemPickup()
    {
        for (int i = 0; i < pooledItemPickups.Count; i++)
        {
            if (pooledItemPickups[i].gameObject.activeSelf == false)
                return pooledItemPickups[i];
        }

        ItemPickup invItem = Instantiate(objectToPool).GetComponent<ItemPickup>();
        invItem.transform.SetParent(transform);
        pooledItemPickups.Add(invItem);
        pooledObjects.Add(invItem.gameObject);
        return invItem;
    }
}

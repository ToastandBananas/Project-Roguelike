using System.Collections.Generic;
using UnityEngine;

public class ItemDataObjectPool : ObjectPool
{
    [HideInInspector] public List<ItemData> pooledItemDatas = new List<ItemData>();

    public override void Start()
    {
        Init();
    }

    public override void Init()
    {
        if (hasBeenInitialized == false)
        {
            // Create and add new objects to the list
            ItemData itemData;
            for (int i = 0; i < amountToPool; i++)
            {
                itemData = Instantiate(objectToPool.GetComponent<ItemData>());
                itemData.transform.SetParent(transform);
                pooledItemDatas.Add(itemData);
                pooledObjects.Add(itemData.gameObject);

                if (itemData.CompareTag("Item Data Container Object"))
                {
                    itemData.bagInventory = itemData.GetComponent<Inventory>();
                    itemData.bagInventory.Init();
                }

                itemData.gameObject.SetActive(false);
            }

            hasBeenInitialized = true;
        }
    }

    public ItemData GetPooledItemData(Inventory inventoryAddingTo)
    {
        for (int i = 0; i < pooledItemDatas.Count; i++)
        {
            if (pooledItemDatas[i].gameObject.activeInHierarchy == false && pooledItemDatas[i].item == null)
                return pooledItemDatas[i];
        }

        ItemData itemData = Instantiate(objectToPool).GetComponent<ItemData>();
        itemData.transform.SetParent(transform);
        pooledItemDatas.Add(itemData);
        pooledObjects.Add(itemData.gameObject);

        if (itemData.CompareTag("Item Data Container Object"))
        {
            itemData.bagInventory = itemData.GetComponent<Inventory>();
            itemData.bagInventory.Init();
        }

        if (inventoryAddingTo != null)
            itemData.parentInventory = inventoryAddingTo;

        return itemData;
    }
}

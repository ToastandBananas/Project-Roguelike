using System.Collections.Generic;
using UnityEngine;

public class ItemDataObjectPool : ObjectPool
{
    [HideInInspector] public List<ItemData> pooledItemDatas = new List<ItemData>();

    public override void Awake()
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
                ItemData existingItemData = transform.GetChild(i).GetComponent<ItemData>();
                pooledItemDatas.Add(existingItemData);
                pooledObjects.Add(existingItemData.gameObject);

                if (existingItemData.CompareTag("Item Data Container Object"))
                    existingItemData.GetComponent<Inventory>().Init();
            }

            // Create and add new objects to the list
            ItemData itemData;
            for (int i = 0; i < amountToPool; i++)
            {
                itemData = Instantiate(objectToPool.GetComponent<ItemData>());
                itemData.transform.SetParent(transform);
                itemData.gameObject.SetActive(false);
                pooledItemDatas.Add(itemData);
                pooledObjects.Add(itemData.gameObject);

                if (itemData.CompareTag("Item Data Container Object"))
                    itemData.GetComponent<Inventory>().Init();
            }

            hasBeenInitialized = true;
        }
    }

    public ItemData GetPooledItemData()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (pooledObjects[i].activeInHierarchy == false)
                return pooledItemDatas[i];
        }

        ItemData itemData = Instantiate(objectToPool).GetComponent<ItemData>();
        itemData.transform.SetParent(transform);
        pooledItemDatas.Add(itemData);
        pooledObjects.Add(itemData.gameObject);

        if (itemData.CompareTag("Item Data Container Object"))
            itemData.GetComponent<Inventory>().Init();

        return itemData;
    }
}

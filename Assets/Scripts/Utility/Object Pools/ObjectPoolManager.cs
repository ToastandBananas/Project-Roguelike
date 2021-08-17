using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public AddItemEffectObjectPool addItemEffectObjectPool;
    public ContextButtonObjectPool contextButtonObjectPool;
    public InjuryTextButtonObjectPool injuryTextButtonObjectPool;
    public InventoryItemObjectPool containerInventoryItemObjectPool;
    public InventoryItemObjectPool playerInventoryItemObjectPool;
    public InventoryItemObjectPool ghostImageInventoryItemObjectPool;
    public ItemPickupObjectPool itemPickupsPool;
    public ItemPickupObjectPool bagPickupsPool;
    public ItemPickupObjectPool portableContainerPickupsPool;
    public ItemDataObjectPool itemDataObjectPool;
    public ItemDataObjectPool itemDataContainerObjectPool;
    public TextPopupObjectPool textPopupObjectPool;

    #region Singleton
    public static ObjectPoolManager instance;

    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of ObjectPoolManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    public ItemPickup GetItemPickupFromPool(Item item)
    {
        ItemPickup newItemPickup = null;
        if (item.IsBag())
            newItemPickup = bagPickupsPool.GetPooledItemPickup();
        else if (item.IsPortableContainer())
            newItemPickup = portableContainerPickupsPool.GetPooledItemPickup();
        else
            newItemPickup = itemPickupsPool.GetPooledItemPickup();

        return newItemPickup;
    }

    public ItemData GetItemDataFromPool(Item item, Inventory inventoryAddingTo)
    {
        if (item.IsBag() || item.IsPortableContainer())
            return itemDataContainerObjectPool.GetPooledItemData(inventoryAddingTo);
        else
            return itemDataObjectPool.GetPooledItemData(inventoryAddingTo);
    }
}

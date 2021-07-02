using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public AddItemEffectObjectPool addItemEffectObjectPool;
    public InventoryItemObjectPool containerInventoryItemObjectPool;
    public InventoryItemObjectPool playerInventoryItemObjectPool;
    public InventoryItemObjectPool ghostImageInventoryItemObjectPool;
    public ItemPickupObjectPool pickupsPool;
    public ItemDataObjectPool itemDataObjectPool;

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
}

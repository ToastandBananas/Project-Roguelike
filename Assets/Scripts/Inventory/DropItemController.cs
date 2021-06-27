using UnityEngine;

public class DropItemController : MonoBehaviour
{
    ItemPickupObjectPool itemPickupObjectPool;

    LayerMask obstacleMask;

    public static DropItemController instance;

    void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of DropItemController. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
        #endregion
        
        obstacleMask = LayerMask.GetMask("Objects", "Walls", "Interactable Objects");
    }

    void Start()
    {
        itemPickupObjectPool = ObjectPoolManager.instance.pickupsPool;
    }

    public ItemPickup DropItem(Vector3 dropPosition, ItemData itemData, int amountToDrop)
    {
        return Drop(dropPosition, itemData, amountToDrop);
    }

    public void DropEquipment(EquipmentManager equipmentManager, EquipmentSlot equipmentSlot, Vector3 dropPosition, ItemData itemData, int amountToDrop)
    {
        if (itemData != null)
        {
            Drop(dropPosition, itemData, amountToDrop);

            if (equipmentManager != null)
                equipmentManager.Unequip(equipmentSlot, false);
        }
    }

    ItemPickup Drop(Vector3 dropPosition, ItemData itemData, int amountToDrop)
    {
        ItemPickup newItemPickup = itemPickupObjectPool.GetPooledObject().GetComponent<ItemPickup>();
        SetupItemPickup(newItemPickup, itemData, amountToDrop, dropPosition);
        return newItemPickup;
    }

    void SetupItemPickup(ItemPickup newItemPickup, ItemData itemData, int amountToDrop, Vector3 dropPosition)
    {
        newItemPickup.gameObject.SetActive(true);

        newItemPickup.transform.position = dropPosition;

        if (itemData.item.pickupSprite != null)
            newItemPickup.spriteRenderer.sprite = itemData.item.pickupSprite;
        else
            newItemPickup.spriteRenderer.sprite = itemData.item.defaultSprite;

        itemData.TransferData(itemData, newItemPickup.itemData);
        newItemPickup.itemCount = amountToDrop;
        newItemPickup.interactionTransform = newItemPickup.transform;

        #if UNITY_EDITOR
            newItemPickup.name = itemData.name + " Pickup";
        #endif
    }
}

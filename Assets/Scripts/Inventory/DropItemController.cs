using System.Collections;
using UnityEngine;

public class DropItemController : MonoBehaviour
{
    ObjectPool itemPickupObjectPool;
    ObjectPoolManager objectPoolManager;

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
        
        obstacleMask = LayerMask.GetMask("Objects", "Walls", "DeepWater");
    }

    void Start()
    {
        objectPoolManager = ObjectPoolManager.instance;
        itemPickupObjectPool = objectPoolManager.pickupsPool;
    }

    public void DropItem(Vector3 dropPosition, ItemData itemData, int amountToDrop, bool shouldDisableAutoPickup, bool shouldDropIndividually, bool shouldRotate, bool tossInAir)
    {
        if (itemData != null)
            Drop(dropPosition, itemData, amountToDrop, shouldDisableAutoPickup, shouldDropIndividually, shouldRotate, tossInAir);
    }

    public void DropEquipment(EquipmentManager equipmentManager, EquipmentSlot equipmentSlot, Vector3 dropPosition, ItemData itemData, int amountToDrop, bool shouldDisableAutoPickup, bool shouldDropIndividually, bool shouldRotate, bool tossInAir)
    {
        if (itemData != null)
        {
            Drop(dropPosition, itemData, amountToDrop, shouldDisableAutoPickup, shouldDropIndividually, shouldRotate, tossInAir);

            if (equipmentManager != null)
                equipmentManager.Unequip(equipmentSlot, false);
        }
    }

    void Drop(Vector3 dropPosition, ItemData itemData, int amountToDrop, bool shouldDisableAutoPickup, bool shouldDropIndividually, bool shouldRotate, bool tossInAir)
    {
        if (shouldDropIndividually)
        {
            for (int i = 0; i < amountToDrop; i++)
            {
                ItemPickup newItemPickup = itemPickupObjectPool.GetPooledObject().GetComponent<ItemPickup>();
               
                SetupItemPickup(newItemPickup, itemData, 1, dropPosition);
            }
        }
        else
        {
            ItemPickup newItemPickup = itemPickupObjectPool.GetPooledObject().GetComponent<ItemPickup>();

            SetupItemPickup(newItemPickup, itemData, amountToDrop, dropPosition);
        }
    }

    void SetupItemPickup(ItemPickup newItemPickup, ItemData itemData, int amountToDrop, Vector3 dropPosition)
    {
        newItemPickup.gameObject.SetActive(true);

        newItemPickup.transform.position = dropPosition;

        if (itemData.item.pickupSprite != null)
            newItemPickup.spriteRenderer.sprite = itemData.item.pickupSprite;
        else
            newItemPickup.spriteRenderer.sprite = itemData.item.defaultSprite;

        newItemPickup.interactRadius = itemData.item.pickupRadius;

        newItemPickup.itemData = itemData;
        newItemPickup.itemCount = amountToDrop;
        newItemPickup.interactionTransform = newItemPickup.transform;

        #if UNITY_EDITOR
            newItemPickup.name = itemData.name + " Pickup";
        #endif
    }
}

using UnityEngine;

public class DropItemController : MonoBehaviour
{
    GameManager gm;
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
        gm = GameManager.instance;
        itemPickupObjectPool = ObjectPoolManager.instance.pickupsPool;
    }

    public void DropItem(Vector3 dropPosition, ItemData itemData, int amountToDrop)
    {
        ItemPickup newItemPickup = itemPickupObjectPool.GetPooledObject().GetComponent<ItemPickup>();
        SetupItemPickup(newItemPickup, itemData, amountToDrop, dropPosition);

        if (dropPosition == gm.playerManager.transform.position + GetDropPositionFromActiveDirection())
        {
            gm.containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);
            gm.containerInvUI.AddItemToList(newItemPickup.itemData);
            gm.containerInvUI.UpdateUINumbers();
        }
    }

    public void DropEquipment(EquipmentManager equipmentManager, EquipmentSlot equipmentSlot, Vector3 dropPosition, ItemData itemData, int amountToDrop)
    {
        if (itemData != null)
        {
            DropItem(dropPosition, itemData, amountToDrop);

            if (equipmentManager != null)
                equipmentManager.Unequip(equipmentSlot, false);
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

        itemData.TransferData(itemData, newItemPickup.itemData);
        newItemPickup.itemCount = amountToDrop;
        newItemPickup.interactionTransform = newItemPickup.transform;

        #if UNITY_EDITOR
            newItemPickup.name = itemData.name + " Pickup";
        #endif
    }

    public Vector3 GetDropPositionFromActiveDirection()
    {
        switch (gm.containerInvUI.activeDirection)
        {
            case Direction.Center:
                return Vector3.zero;
            case Direction.Up:
                return new Vector3(0, 1);
            case Direction.Down:
                return new Vector3(0, -1);
            case Direction.Left:
                return new Vector3(-1, 0);
            case Direction.Right:
                return new Vector3(1, 0);
            case Direction.UpLeft:
                return new Vector3(-1, 1);
            case Direction.UpRight:
                return new Vector3(1, 1);
            case Direction.DownLeft:
                return new Vector3(-1, -1);
            case Direction.DownRight:
                return new Vector3(1, -1);
            default:
                return Vector3.zero;
        }
    }
}

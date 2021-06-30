using System.Collections.Generic;
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
        else
        {
            List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromDirection(GetDirectionFromDropPosition(dropPosition));
            itemsList.Add(newItemPickup.itemData);
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

    public Vector3 GetDropPositionFromDirection(Direction direction)
    {
        switch (direction)
        {
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

    public Direction GetDirectionFromDropPosition(Vector3 dropPosition)
    {
        if (dropPosition == gm.playerManager.transform.position)
            return Direction.Center;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(0, 1))
            return Direction.Up;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(0, -1))
            return Direction.Down;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(-1, 0))
            return Direction.Left;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(1, 0))
            return Direction.Right;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(-1, 1))
            return Direction.UpLeft;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(1, 1))
            return Direction.UpRight;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(-1, -1))
            return Direction.DownLeft;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(1, -1))
            return Direction.DownRight;
        else
            return Direction.Center;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class DropItemController : MonoBehaviour
{
    GameManager gm;

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
    }

    public void DropItem(Vector3 dropPosition, ItemData itemData, int amountToDrop)
    {
        ItemPickup newItemPickup = null;
        if (itemData.item.IsBag())
        {
            newItemPickup = gm.objectPoolManager.bagPickupsPool.GetPooledItemPickup();
            Direction direction = GetDirectionFromDropPosition(dropPosition);
            gm.containerInvUI.AssignInventory(direction, newItemPickup.inventory);
            Bag bag = (Bag)itemData.item;
            gm.containerInvUI.GetSideBarButtonFromDirection(direction).icon.sprite = bag.sidebarSprite;
        }
        else
            newItemPickup = gm.objectPoolManager.itemPickupsPool.GetPooledItemPickup();

        SetupItemPickup(newItemPickup, itemData, amountToDrop, dropPosition);

        if (dropPosition == gm.playerManager.transform.position + GetDropPositionFromActiveDirection())
        {
            StartCoroutine(gm.containerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.GetSideBarButtonFromDirection(gm.containerInvUI.activeDirection), null));
            gm.containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);
            gm.containerInvUI.AddItemToList(newItemPickup.itemData);
            gm.containerInvUI.UpdateUINumbers();
        }
        else if (dropPosition == gm.playerManager.transform.position)
        {
            StartCoroutine(gm.containerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.playerPositionSideBarButton, null));

            if (gm.containerInvUI.activeDirection == Direction.Center)
                gm.containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);

            gm.containerInvUI.AddItemToListFromDirection(newItemPickup.itemData, Direction.Center);

            if (gm.containerInvUI.activeDirection == Direction.Center)
                gm.containerInvUI.UpdateUINumbers();
        }
        else
        {
            Direction direction = GetDirectionFromDropPosition(dropPosition);
            StartCoroutine(gm.containerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.GetSideBarButtonFromDirection(direction), null));
            List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromDirection(direction);
            itemsList.Add(newItemPickup.itemData);
        }
    }

    void SetupItemPickup(ItemPickup newItemPickup, ItemData itemData, int amountToDrop, Vector3 dropPosition)
    {
        newItemPickup.gameObject.SetActive(true);
        newItemPickup.transform.position = dropPosition;
        newItemPickup.spriteRenderer.sprite = itemData.item.pickupSprite;

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
            case Direction.North:
                return new Vector3(0, 1);
            case Direction.South:
                return new Vector3(0, -1);
            case Direction.West:
                return new Vector3(-1, 0);
            case Direction.East:
                return new Vector3(1, 0);
            case Direction.Northwest:
                return new Vector3(-1, 1);
            case Direction.Northeast:
                return new Vector3(1, 1);
            case Direction.Southwest:
                return new Vector3(-1, -1);
            case Direction.Southeast:
                return new Vector3(1, -1);
            default:
                return Vector3.zero;
        }
    }

    public Vector3 GetDropPositionFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector3(0, 1);
            case Direction.South:
                return new Vector3(0, -1);
            case Direction.West:
                return new Vector3(-1, 0);
            case Direction.East:
                return new Vector3(1, 0);
            case Direction.Northwest:
                return new Vector3(-1, 1);
            case Direction.Northeast:
                return new Vector3(1, 1);
            case Direction.Southwest:
                return new Vector3(-1, -1);
            case Direction.Southeast:
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
            return Direction.North;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(0, -1))
            return Direction.South;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(-1, 0))
            return Direction.West;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(1, 0))
            return Direction.East;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(-1, 1))
            return Direction.Northwest;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(1, 1))
            return Direction.Northeast;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(-1, -1))
            return Direction.Southwest;
        else if (dropPosition == gm.playerManager.transform.position + new Vector3(1, -1))
            return Direction.Southeast;
        else
            return Direction.Center;
    }
}

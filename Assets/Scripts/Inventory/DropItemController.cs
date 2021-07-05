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

    public void DropItem(Vector3 dropPosition, ItemData itemData, int amountToDrop, Inventory invComingFrom)
    {
        ItemPickup newItemPickup = null;
        Direction dropDirection = GetDirectionFromDropPosition(dropPosition);

        if (itemData.item.IsBag())
        {
            // If the item we're dropping is a bag, create a new bag pickup
            newItemPickup = gm.objectPoolManager.bagPickupsPool.GetPooledItemPickup();

            // Find our drop direction and assign the bag's inventory to our directional inventory
            gm.containerInvUI.AssignDirectionalInventory(dropDirection, newItemPickup.inventory);

            if (gm.containerInvUI.activeDirection == dropDirection)
                gm.containerInvUI.activeInventory = newItemPickup.inventory;
        }
        else // Otherwise, if the item is not a bag, create a normal item pickup
            newItemPickup = gm.objectPoolManager.itemPickupsPool.GetPooledItemPickup();

        // Activate the pickup, transfer data to it and set its sprite, position, item count and interaction transform
        SetupItemPickup(newItemPickup, itemData, amountToDrop, dropPosition);

        if (itemData.item.IsBag())
        {
            Inventory bagInv = null;
            if (itemData.transform.parent.parent != null && itemData.transform.parent.parent.name == "Equipped Items") // If we're dropping this item straight from our equipped items menu
                bagInv = gm.playerInvUI.GetInventoryFromBagEquipSlot(itemData);
            else
                bagInv = itemData.bagInventory;

            // If the item we're dropping is a bag, add new ItemData Objects to the items parent of the dropped bag and transfer data to them
            for (int i = 0; i < bagInv.items.Count; i++)
            {
                ItemData newItemDataObject = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();
                newItemDataObject.transform.SetParent(newItemPickup.itemData.bagInventory.itemsParent);
                newItemDataObject.gameObject.SetActive(true);
                bagInv.items[i].TransferData(bagInv.items[i], newItemDataObject);

                // Populate the new bag's inventory, but make sure it's not already in the items list (because of the Inventory's Init method, which populates this list)
                if (newItemPickup.itemData.bagInventory.items.Contains(newItemDataObject) == false)
                    newItemPickup.itemData.bagInventory.items.Add(newItemDataObject);

                #if UNITY_EDITOR
                    newItemDataObject.name = newItemDataObject.itemName;
                #endif
            }

            // Set the weight and volume of the "new" bag
            newItemPickup.itemData.bagInventory.GetCurrentWeightAndVolume();

            // If the bag is coming from an Inventory (and not from the ground), subtract the bag's weight/volume, including the items inside it
            if (invComingFrom != null)
            {
                if (gm.contextMenu.contextActiveInvItem != null && itemData == gm.contextMenu.contextActiveInvItem.itemData) // If the item was dropped using the context menu
                    invComingFrom.SubtractItemsWeightAndVolumeFromInventory(itemData, invComingFrom, 1, true);
                else if (itemData.CompareTag("Item Pickup") == false)
                    invComingFrom.SubtractItemsWeightAndVolumeFromInventory(itemData, invComingFrom, 1, true);

                bagInv.ResetWeightAndVolume(); // Reset the bag's inventory

                invComingFrom.myInventoryUI.UpdateUI();
            }
            else // If the bag is coming from the ground
                gm.containerInvUI.RemoveBagFromGround();

            for (int i = 0; i < bagInv.items.Count; i++)
            {
                // Return the item we took out of the "old" bag back to it's object pool
                bagInv.items[i].ReturnToItemDataObjectPool();
            }

            // Clear out the items list of the "old" bag
            bagInv.items.Clear();
        }

        // If the drop position is our active direction
        if (dropPosition == gm.playerManager.transform.position + GetDropPositionFromActiveDirection())
        {
            // Play the add item effect
            StartCoroutine(gm.containerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.GetSideBarButtonFromDirection(gm.containerInvUI.activeDirection), null));

            // Show the item in our container inventory UI
            gm.containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);

            // Add the item to our active direction's items list and update the container UI numbers
            gm.containerInvUI.AddItemToActiveDirectionList(newItemPickup.itemData);
            gm.containerInvUI.UpdateUI();
        }
        else if (dropPosition == gm.playerManager.transform.position) // If the drop position is the player's position
        {
            // Play the add item effect
            StartCoroutine(gm.containerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.playerPositionSideBarButton, null));

            // If our active direction is the centered, show the item in our container inventory UI
            if (gm.containerInvUI.activeDirection == Direction.Center)
                gm.containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);

            // Add the item to the player position items list
            gm.containerInvUI.AddItemToListFromDirection(newItemPickup.itemData, Direction.Center);

            // If our active direction is the centered, update the container UI numbers
            if (gm.containerInvUI.activeDirection == Direction.Center)
                gm.containerInvUI.UpdateUI();
        }
        else // If the drop position is not our active direction or the player's position
        {
            // Play the add item effect
            StartCoroutine(gm.containerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.GetSideBarButtonFromDirection(dropDirection), null));

            // Add the item to the appropriate directional list
            List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromDirection(dropDirection);
            itemsList.Add(newItemPickup.itemData);
        }

        // If the item we're dropping is a bag, set the container sidebar icon to our bag icon
        if (itemData.item.IsBag())
        {
            Bag bag = (Bag)itemData.item;
            gm.containerInvUI.GetSideBarButtonFromDirection(dropDirection).icon.sprite = bag.sidebarSprite;
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
            newItemPickup.name = itemData.name;
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

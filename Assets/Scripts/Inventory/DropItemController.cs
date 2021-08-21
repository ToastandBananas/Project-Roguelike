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

    public void ForceDropNearest(CharacterManager characterManager, ItemData itemData, int amountToDrop, Inventory invComingFrom, InventoryItem invItemComingFrom)
    {
        if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.Center), characterManager.transform.position))
            DropItem(characterManager, characterManager.transform.position, itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.North), characterManager.transform.position + new Vector3(0, 1)))
            DropItem(characterManager, characterManager.transform.position + new Vector3(0, 1), itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.East), characterManager.transform.position + new Vector3(1, 0)))
            DropItem(characterManager, characterManager.transform.position + new Vector3(1, 0), itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.South), characterManager.transform.position + new Vector3(0, -1)))
            DropItem(characterManager, characterManager.transform.position + new Vector3(0, -1), itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.West), characterManager.transform.position + new Vector3(-1, 0)))
            DropItem(characterManager, characterManager.transform.position + new Vector3(-1, 0), itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.Northwest), characterManager.transform.position + new Vector3(-1, 1)))
            DropItem(characterManager, characterManager.transform.position + new Vector3(-1, 1), itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.Northeast), characterManager.transform.position + new Vector3(1, 1)))
            DropItem(characterManager, characterManager.transform.position + new Vector3(1, 1), itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.Southwest), characterManager.transform.position + new Vector3(-1, -1)))
            DropItem(characterManager, characterManager.transform.position + new Vector3(-1, -1), itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else if (gm.uiManager.IsRoomOnGround(itemData, gm.containerInvUI.GetItemsListFromDirection(Direction.Southeast), characterManager.transform.position + new Vector3(1, -1)))
            DropItem(characterManager, characterManager.transform.position + new Vector3(1, -1), itemData, amountToDrop, invComingFrom, invItemComingFrom);
        else
            DropItem(characterManager, characterManager.transform.position, itemData, amountToDrop, invComingFrom, invItemComingFrom); // Drop at the character's position as a last resort
    }

    public void DropItem(CharacterManager characterDropping, Vector3 dropPosition, ItemData itemData, int amountToDrop, Inventory invComingFrom, InventoryItem invItemComingFrom)
    {
        ItemPickup newItemPickup = null;
        Direction dropDirection = GetDirectionFromDropPosition(dropPosition);

        // If this item is carried, remove it from the character's carried items
        if (characterDropping != null)
            characterDropping.RemoveCarriedItem(itemData, itemData.currentStackSize);

        if (itemData.item.IsBag())
        {
            // If the item we're dropping is a bag, create a new bag pickup
            newItemPickup = gm.objectPoolManager.bagPickupsPool.GetPooledItemPickup();
            newItemPickup.itemData.bagInventory.items.Clear();

            // Find our drop direction and assign the bag's inventory to our directional inventory
            gm.containerInvUI.AssignDirectionalInventory(dropDirection, newItemPickup.itemData.bagInventory);

            if (gm.containerInvUI.activeDirection == dropDirection)
                gm.containerInvUI.activeInventory = newItemPickup.itemData.bagInventory;
        }
        else if (itemData.item.IsPortableContainer())
        {
            // If the item we're dropping is a portable container, create a new portable container pickup
            newItemPickup = gm.objectPoolManager.portableContainerPickupsPool.GetPooledItemPickup();
            newItemPickup.itemData.bagInventory.items.Clear();
        }
        else // Otherwise, if the item is not a bag, create a normal item pickup
            newItemPickup = gm.objectPoolManager.itemPickupsPool.GetPooledItemPickup();

        // Activate the pickup, transfer data to it and set its sprite, position, item count and interaction transform
        SetupItemPickup(newItemPickup, itemData, amountToDrop, dropPosition);

        if (itemData.item.IsBag() || itemData.item.IsPortableContainer())
        {
            Inventory bagInv = null;
            if (itemData.transform.parent.parent != null && itemData.transform.parent.parent.name == "Equipped Items") // If we're dropping this item straight from our equipped items menu
                bagInv = gm.playerInvUI.GetInventoryFromBagEquipSlot(itemData);
            else
                bagInv = itemData.bagInventory;

            // If the item we're dropping is a bag (or portable container), add new ItemData Objects to the items parent of the dropped bag and transfer data to them
            for (int i = 0; i < bagInv.items.Count; i++)
            {
                gm.uiManager.CreateNewItemDataChild(bagInv.items[i], newItemPickup.itemData.bagInventory, newItemPickup.itemData.bagInventory.itemsParent, true);
            }

            // Set the weight and volume of the "new" bag
            newItemPickup.itemData.bagInventory.UpdateCurrentWeightAndVolume();

            // If the bag is coming from an Inventory (and not from the ground)
            if (invComingFrom != null)
                bagInv.ResetWeightAndVolume(); // Reset the bag's inventory
            else // If the bag is coming from the ground
                gm.containerInvUI.RemoveBagFromGround(itemData.bagInventory);

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

            // If the ground items list is active
            if (gm.containerInvUI.activeInventory == null)
            {
                // Show the item in our container inventory UI
                gm.containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);

                // Add the item to our active direction's items list and update the container UI numbers
                gm.containerInvUI.AddItemToActiveDirectionList(newItemPickup.itemData);
                gm.containerInvUI.UpdateUI();
            }
        }
        else if (dropPosition == gm.playerManager.transform.position) // If the drop position is the player's position
        {
            // Play the add item effect
            StartCoroutine(gm.containerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.playerPositionSideBarButton, null));

            // If the ground items list is active
            if (gm.containerInvUI.activeInventory == null)
            {
                // If our active direction is centered, show the item in our container inventory UI
                if (gm.containerInvUI.activeDirection == Direction.Center)
                    gm.containerInvUI.ShowNewInventoryItem(newItemPickup.itemData);

                // Add the item to the player position items list
                gm.containerInvUI.AddItemToDirectionalListFromDirection(newItemPickup.itemData, Direction.Center);

                // If our active direction is the centered, update the container UI numbers
                if (gm.containerInvUI.activeDirection == Direction.Center)
                    gm.containerInvUI.UpdateUI();
            }
        }
        else // If the drop position is not our active direction or the player's position
        {
            // Play the add item effect
            StartCoroutine(gm.containerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, gm.containerInvUI.GetSideBarButtonFromDirection(dropDirection), null));

            // If there's no inventories (such as a bag) on this drop space
            if (gm.containerInvUI.GetInventoriesListFromDirection(dropDirection).Count == 0)
            {
                // Add the item to the appropriate directional list
                List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromDirection(dropDirection);

                if (newItemPickup.itemData.item.IsBag())
                {
                    itemsList.Clear();
                    gm.containerInvUI.AssignDirectionalInventory(dropDirection, newItemPickup.itemData.bagInventory);
                }

                itemsList.Add(newItemPickup.itemData);
            }
        }

        // If the item we're dropping is a bag, set our active inventory to the bag's inventory, add the bag to the appropriate lists and setup the UI
        if (itemData.item.IsBag())
        {
            gm.containerInvUI.GetInventoriesListFromDirection(dropDirection).Add(newItemPickup.itemData.bagInventory);

            Bag bag = (Bag)itemData.item;
            gm.containerInvUI.GetSideBarButtonFromDirection(dropDirection).icon.sprite = bag.sidebarSprite;
            gm.containerInvUI.AssignDirectionalInventory(dropDirection, newItemPickup.itemData.bagInventory);

            List<ItemData> itemsList = gm.containerInvUI.GetItemsListFromDirection(dropDirection);
            itemsList.Clear();
            itemsList.Add(newItemPickup.itemData);

            if (dropDirection == gm.containerInvUI.activeDirection)
            {
                gm.containerInvUI.activeInventory = newItemPickup.itemData.bagInventory;
                gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.GetItemsListFromActiveDirection(), gm.containerInvUI.activeDirection);
            }
        }
        else // If the item we're dropping is not a bag, assign the item to the appropriate lists
        {
            gm.containerInvUI.AddItemToGroundItemsListFromDirection(newItemPickup.itemData, dropDirection);
            if (gm.containerInvUI.activeInventory == null && gm.containerInvUI.GetItemsListFromDirection(dropDirection).Contains(newItemPickup.itemData) == false)
                gm.containerInvUI.AddItemToDirectionalListFromDirection(newItemPickup.itemData, dropDirection);
        }

        itemData.currentStackSize = 0;
        if (invItemComingFrom != null)
            invItemComingFrom.UpdateInventoryWeightAndVolume();

        if (invComingFrom == null || invComingFrom.CompareTag("Object") == false)
            gm.flavorText.WriteLine_DropItem(itemData, amountToDrop);
    }

    public void SetupItemPickup(ItemPickup newItemPickup, ItemData itemData, int amountToDrop, Vector3 dropPosition)
    {
        newItemPickup.gameObject.SetActive(true);
        newItemPickup.transform.position = dropPosition;
        newItemPickup.transform.position = Utilities.ClampedPosition(newItemPickup.transform.position);

        itemData.TransferData(itemData, newItemPickup.itemData);
        newItemPickup.spriteRenderer.sprite = itemData.item.pickupSprite;
        newItemPickup.itemCount = amountToDrop;
        newItemPickup.interactionTransform = newItemPickup.transform;

        if (itemData.item.IsBag())
        {
            Bag bag = (Bag)itemData.item;
            newItemPickup.itemData.bagInventory.container.sidebarSpriteClosed = bag.sidebarSprite;
            bag.SetupBagInventory(newItemPickup.itemData.bagInventory);
        }
        else if (itemData.item.IsPortableContainer())
        {
            PortableContainer portableContainer = (PortableContainer)itemData.item;
            portableContainer.SetupPortableContainerInventory(newItemPickup.itemData.bagInventory);
        }

        #if UNITY_EDITOR
            newItemPickup.name = itemData.name;
        #endif

        GameTiles.AddItemData(newItemPickup.itemData, newItemPickup.transform.position);
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
        Vector3 clampedDropPos = new Vector3(Mathf.RoundToInt(dropPosition.x), Mathf.RoundToInt(dropPosition.y));

        if (clampedDropPos == gm.playerManager.transform.position)
            return Direction.Center;
        else if (clampedDropPos == gm.playerManager.transform.position + new Vector3(0, 1))
            return Direction.North;
        else if (clampedDropPos == gm.playerManager.transform.position + new Vector3(0, -1))
            return Direction.South;
        else if (clampedDropPos == gm.playerManager.transform.position + new Vector3(-1, 0))
            return Direction.West;
        else if (clampedDropPos == gm.playerManager.transform.position + new Vector3(1, 0))
            return Direction.East;
        else if (clampedDropPos == gm.playerManager.transform.position + new Vector3(-1, 1))
            return Direction.Northwest;
        else if (clampedDropPos == gm.playerManager.transform.position + new Vector3(1, 1))
            return Direction.Northeast;
        else if (clampedDropPos == gm.playerManager.transform.position + new Vector3(-1, -1))
            return Direction.Southwest;
        else if (clampedDropPos == gm.playerManager.transform.position + new Vector3(1, -1))
            return Direction.Southeast;
        else
            return Direction.Center;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerInventoryUI : InventoryUI
{
    [Header("Floor Icon")]
    public Sprite floorIconSprite;

    [Header("Side Bar Buttons")]
    public ContainerSideBarButton playerPositionSideBarButton;
    public ContainerSideBarButton upSideBarButton, downSideBarButton, leftSideBarButton, rightSideBarButton, upLeftSideBarButton, upRightSideBarButton, downLeftSideBarButton, downRightSideBarButton;

    [Header("Max Ground Volume")]
    public float emptyTileMaxVolume = 1000f;

    public List<ItemData> playerPositionItems = new List<ItemData>();
    public List<ItemData> leftItems = new List<ItemData>();
    public List<ItemData> rightItems = new List<ItemData>();
    public List<ItemData> upItems = new List<ItemData>();
    public List<ItemData> downItems = new List<ItemData>();
    public List<ItemData> upLeftItems = new List<ItemData>();
    public List<ItemData> upRightItems = new List<ItemData>();
    public List<ItemData> downLeftItems = new List<ItemData>();
    public List<ItemData> downRightItems = new List<ItemData>();

    [HideInInspector] public Inventory leftInventory, rightInventory, upInventory, downInventory, upLeftInventory, upRightInventory, downLeftInventory, downRightInventory;

    [HideInInspector] public Direction activeDirection;

    LayerMask interactableMask;

    #region Singleton
    public static ContainerInventoryUI instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of ContainerInventoryUI. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    public override void Start()
    {
        base.Start();

        interactableMask = LayerMask.GetMask("Interactable", "Interactable Objects");

        inventoryItemObjectPool.Init();
        GetItemsAroundPlayer();
        PopulateInventoryUI(playerPositionItems, Direction.Center);
    }

    // This method runs when the user clicks on a container side bar icon
    public void PopulateInventoryUI(List<ItemData> itemsList, Direction direction)
    {
        activeInventory = null;
        ClearInventoryUI();
        activeDirection = direction;

        for (int i = 0; i < itemsList.Count; i++)
        {
            InventoryItem invItem = ShowNewInventoryItem(itemsList[i]);
            AssignInventoryToInventoryItem(invItem, direction);
        }

        // Set container open icon sprite (when applicable) and header/volume/weight text
        SetUpInventoryUI(direction);
    }

    public void GetItemsAroundPlayer()
    {
        ClearAllLists();

        GetItemsAtPosition(gm.playerManager.transform.position, playerPositionItems, Direction.Center);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(-1, 0), leftItems, Direction.Left);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(1, 0), rightItems, Direction.Right);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(0, 1), upItems, Direction.Up);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(0, -1), downItems, Direction.Down);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(-1, 1), upLeftItems, Direction.UpLeft);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(1, 1), upRightItems, Direction.UpRight);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(-1, -1), downLeftItems, Direction.DownLeft);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(1, -1), downRightItems, Direction.DownRight);
    }

    void GetItemsAtPosition(Vector2 position, List<ItemData> itemsList, Direction direction)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero, 1, interactableMask);

        SetSideBarIcon_Floor(direction);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null)
            {
                if (hits[i].collider.CompareTag("NPC") == false && hits[i].collider.TryGetComponent(out Inventory inventory) != false)
                {
                    // Get each item in the container's inventory
                    for (int j = 0; j < inventory.items.Count; j++)
                    {
                        itemsList.Add(inventory.items[j]);
                        SetSideBarIcon_Container(direction, inventory);
                    }

                    AssignInventory(direction, inventory);
                }
                else
                {
                    // Get each item on the ground
                    itemsList.Add(hits[i].collider.GetComponent<ItemData>());
                }
            }
        }
    }

    public void TakeAll()
    {
        if (gm.playerInvUI.inventoryParent.activeSelf == false)
            gm.playerInvUI.ToggleInventoryMenu();
        else if (gm.playerInvUI.background.activeSelf == false)
            gm.playerInvUI.ToggleMinimization();

        for (int i = 0; i < gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            if (gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems[i].gameObject.activeSelf)
                gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems[i].TransferItem();
        }
    }

    void AssignInventory(Direction direction, Inventory inventory)
    {
        switch (direction)
        {
            case Direction.Center:
                break;
            case Direction.Up:
                if (inventory != null) upInventory = inventory;
                break;
            case Direction.Down:
                if (inventory != null) downInventory = inventory;
                break;
            case Direction.Left:
                if (inventory != null) leftInventory = inventory;
                break;
            case Direction.Right:
                if (inventory != null) rightInventory = inventory;
                break;
            case Direction.UpLeft:
                if (inventory != null) upLeftInventory = inventory;
                break;
            case Direction.UpRight:
                if (inventory != null) upRightInventory = inventory;
                break;
            case Direction.DownLeft:
                if (inventory != null) downLeftInventory = inventory;
                break;
            case Direction.DownRight:
                if (inventory != null) downRightInventory = inventory;
                break;
            default:
                break;
        }
    }

    void AssignInventoryToInventoryItem(InventoryItem invItem, Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                invItem.myInventory = null;
                break;
            case Direction.Up:
                invItem.myInventory = upInventory;
                break;
            case Direction.Down:
                invItem.myInventory = downInventory;
                break;
            case Direction.Left:
                invItem.myInventory = leftInventory;
                break;
            case Direction.Right:
                invItem.myInventory = rightInventory;
                break;
            case Direction.UpLeft:
                invItem.myInventory = upLeftInventory;
                break;
            case Direction.UpRight:
                invItem.myInventory = upRightInventory;
                break;
            case Direction.DownLeft:
                invItem.myInventory = downLeftInventory;
                break;
            case Direction.DownRight:
                invItem.myInventory = downRightInventory;
                break;
            default:
                break;
        }
    }

    void SetSideBarIcon_Container(Direction direction, Inventory inventory)
    {
        switch (direction)
        {
            case Direction.Center:
                playerPositionSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Up:
                upSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Down:
                downSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Left:
                leftSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Right:
                rightSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.UpLeft:
                upLeftSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.UpRight:
                upRightSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.DownLeft:
                downLeftSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.DownRight:
                downRightSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            default:
                break;
        }
    }

    void SetSideBarIcon_Floor(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                playerPositionSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.Up:
                upSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.Down:
                downSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.Left:
                leftSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.Right:
                rightSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.UpLeft:
                upLeftSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.UpRight:
                upRightSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.DownLeft:
                downLeftSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.DownRight:
                downRightSideBarButton.icon.sprite = floorIconSprite;
                break;
            default:
                break;
        }
    }

    void SetUpInventoryUI(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                inventoryNameText.text = "Items Under Self";
                weightText.text = GetTotalWeight(playerPositionItems).ToString();
                volumeText.text = GetTotalVolume(playerPositionItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                break;
            case Direction.Up:
                if (upInventory != null)
                    SetupContainerUI(upInventory, upSideBarButton.icon, upItems);
                else
                {
                    inventoryNameText.text = "Items Above Self";
                    weightText.text = GetTotalWeight(upItems).ToString();
                    volumeText.text = GetTotalVolume(upItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Down:
                if (downInventory != null)
                    SetupContainerUI(downInventory, downSideBarButton.icon, downItems);
                else
                {
                    inventoryNameText.text = "Items Below Self";
                    weightText.text = GetTotalWeight(downItems).ToString();
                    volumeText.text = GetTotalVolume(downItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Left:
                if (leftInventory != null)
                    SetupContainerUI(leftInventory, leftSideBarButton.icon, leftItems);
                else
                {
                    inventoryNameText.text = "Items Left of Self";
                    weightText.text = GetTotalWeight(leftItems).ToString();
                    volumeText.text = GetTotalVolume(leftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Right:
                if (rightInventory != null)
                    SetupContainerUI(rightInventory, rightSideBarButton.icon, rightItems);
                else
                {
                    inventoryNameText.text = "Items Right of Self";
                    weightText.text = GetTotalWeight(rightItems).ToString();
                    volumeText.text = GetTotalVolume(rightItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.UpLeft:
                if (upLeftInventory != null)
                    SetupContainerUI(upLeftInventory, upLeftSideBarButton.icon, upLeftItems);
                else
                {
                    inventoryNameText.text = "Items Above and Left of Self";
                    weightText.text = GetTotalWeight(upLeftItems).ToString();
                    volumeText.text = GetTotalVolume(upLeftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.UpRight:
                if (upRightInventory != null)
                    SetupContainerUI(upRightInventory, upRightSideBarButton.icon, upRightItems);
                else
                {
                    inventoryNameText.text = "Items Above and Right of Self";
                    weightText.text = GetTotalWeight(upRightItems).ToString();
                    volumeText.text = GetTotalVolume(upRightItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.DownLeft:
                if (downLeftInventory != null)
                    SetupContainerUI(downLeftInventory, downLeftSideBarButton.icon, downLeftItems);
                else
                {
                    inventoryNameText.text = "Items Below and Left of Self";
                    weightText.text = GetTotalWeight(downLeftItems).ToString();
                    volumeText.text = GetTotalVolume(downLeftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.DownRight:
                if (downRightInventory != null)
                    SetupContainerUI(downRightInventory, downRightSideBarButton.icon, downRightItems);
                else
                {
                    inventoryNameText.text = "Items Below and Right Self";
                    weightText.text = GetTotalWeight(downRightItems).ToString();
                    volumeText.text = GetTotalVolume(downRightItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            default:
                break;
        }
    }

    public List<ItemData> GetItemsListFromActiveDirection()
    {
        switch (activeDirection)
        {
            case Direction.Center:
                return playerPositionItems;
            case Direction.Up:
                return upItems;
            case Direction.Down:
                return downItems;
            case Direction.Left:
                return leftItems;
            case Direction.Right:
                return rightItems;
            case Direction.UpLeft:
                return upLeftItems;
            case Direction.UpRight:
                return upRightItems;
            case Direction.DownLeft:
                return downLeftItems;
            case Direction.DownRight:
                return downRightItems;
            default:
                return null;
        }
    }

    public List<ItemData> GetItemsListFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                return playerPositionItems;
            case Direction.Up:
                return upItems;
            case Direction.Down:
                return downItems;
            case Direction.Left:
                return leftItems;
            case Direction.Right:
                return rightItems;
            case Direction.UpLeft:
                return upLeftItems;
            case Direction.UpRight:
                return upRightItems;
            case Direction.DownLeft:
                return downLeftItems;
            case Direction.DownRight:
                return downRightItems;
            default:
                return null;
        }
    }

    void SetupContainerUI(Inventory inventory, Image sideBarIcon, List<ItemData> itemsList)
    {
        inventoryNameText.text = inventory.container.name + " Inventory";
        sideBarIcon.sprite = GetContainerIcon(inventory, true);
        inventory.container.spriteRenderer.sprite = GetContainerIcon(inventory, true);
        activeInventory = inventory;
        weightText.text = GetTotalWeight(itemsList).ToString() + "/" + inventory.maxWeight.ToString();
        volumeText.text = GetTotalVolume(itemsList).ToString() + "/" + inventory.maxVolume.ToString();
    }

    Sprite GetContainerIcon(Inventory inventory, bool containerIsActive)
    {
        if (containerIsActive)
        {
            if (inventory.container.sidebarSpriteOpen != null)
                return inventory.container.sidebarSpriteOpen;
        }

        return inventory.container.sidebarSpriteClosed;
    }

    public void ResetContainerIcons(Direction newDirection)
    {
        gm.uiManager.activeInvItem = null;
        upSideBarButton.ResetDirectionIconColor();
        downSideBarButton.ResetDirectionIconColor();
        leftSideBarButton.ResetDirectionIconColor();
        rightSideBarButton.ResetDirectionIconColor();
        upLeftSideBarButton.ResetDirectionIconColor();
        upRightSideBarButton.ResetDirectionIconColor();
        downLeftSideBarButton.ResetDirectionIconColor();
        downRightSideBarButton.ResetDirectionIconColor();
        if (activeDirection != Direction.Center || newDirection != Direction.Center)
            playerPositionSideBarButton.ResetDirectionIconColor();

        if (upInventory != null)
        {
            upInventory.container.spriteRenderer.sprite = GetContainerIcon(upInventory, false);
            upSideBarButton.icon.sprite = GetContainerIcon(upInventory, false);
        }

        if (downInventory != null)
        {
            downInventory.container.spriteRenderer.sprite = GetContainerIcon(downInventory, false);
            downSideBarButton.icon.sprite = GetContainerIcon(downInventory, false);
        }

        if (leftInventory != null)
        {
            leftInventory.container.spriteRenderer.sprite = GetContainerIcon(leftInventory, false);
            leftSideBarButton.icon.sprite = GetContainerIcon(leftInventory, false);
        }

        if (rightInventory != null)
        {
            rightInventory.container.spriteRenderer.sprite = GetContainerIcon(rightInventory, false);
            rightSideBarButton.icon.sprite = GetContainerIcon(rightInventory, false);
        }

        if (upLeftInventory != null)
        {
            upLeftInventory.container.spriteRenderer.sprite = GetContainerIcon(upLeftInventory, false);
            upLeftSideBarButton.icon.sprite = GetContainerIcon(upLeftInventory, false);
        }

        if (upRightInventory != null)
        {
            upRightInventory.container.spriteRenderer.sprite = GetContainerIcon(upRightInventory, false);
            upRightSideBarButton.icon.sprite = GetContainerIcon(upRightInventory, false);
        }

        if (downLeftInventory != null)
        {
            downLeftInventory.container.spriteRenderer.sprite = GetContainerIcon(downLeftInventory, false);
            downLeftSideBarButton.icon.sprite = GetContainerIcon(downLeftInventory, false);
        }

        if (downRightInventory != null)
        {
            downRightInventory.container.spriteRenderer.sprite = GetContainerIcon(downRightInventory, false);
            downRightSideBarButton.icon.sprite = GetContainerIcon(downRightInventory, false);
        }
    }

    public override void UpdateUINumbers()
    {
        if (activeInventory != null)
        {
            weightText.text = (Mathf.RoundToInt(activeInventory.currentWeight * 100f) / 100f).ToString() + "/" + activeInventory.maxWeight.ToString();
            volumeText.text = (Mathf.RoundToInt(activeInventory.currentVolume * 100f) / 100f).ToString() + "/" + activeInventory.maxVolume.ToString();
        }
        else
        {
            switch (activeDirection)
            {
                case Direction.Center:
                    weightText.text = GetTotalWeight(playerPositionItems).ToString();
                    volumeText.text = GetTotalVolume(playerPositionItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.Up:
                    weightText.text = GetTotalWeight(upItems).ToString();
                    volumeText.text = GetTotalVolume(upItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.Down:
                    weightText.text = GetTotalWeight(downItems).ToString();
                    volumeText.text = GetTotalVolume(downItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.Left:
                    weightText.text = GetTotalWeight(leftItems).ToString();
                    volumeText.text = GetTotalVolume(leftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.Right:
                    weightText.text = GetTotalWeight(rightItems).ToString();
                    volumeText.text = GetTotalVolume(rightItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.UpLeft:
                    weightText.text = GetTotalWeight(upLeftItems).ToString();
                    volumeText.text = GetTotalVolume(upLeftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.UpRight:
                    weightText.text = GetTotalWeight(upRightItems).ToString();
                    volumeText.text = GetTotalVolume(upRightItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.DownLeft:
                    weightText.text = GetTotalWeight(downLeftItems).ToString();
                    volumeText.text = GetTotalVolume(downLeftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.DownRight:
                    weightText.text = GetTotalWeight(downRightItems).ToString();
                    volumeText.text = GetTotalVolume(downRightItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                default:
                    break;
            }
        }
    }

    public void AddItemToList(ItemData itemDataToAdd)
    {
        switch (activeDirection)
        {
            case Direction.Center:
                playerPositionItems.Add(itemDataToAdd);
                break;
            case Direction.Up:
                upItems.Add(itemDataToAdd);
                break;
            case Direction.Down:
                downItems.Add(itemDataToAdd);
                break;
            case Direction.Left:
                leftItems.Add(itemDataToAdd);
                break;
            case Direction.Right:
                rightItems.Add(itemDataToAdd);
                break;
            case Direction.UpLeft:
                upLeftItems.Add(itemDataToAdd);
                break;
            case Direction.UpRight:
                upRightItems.Add(itemDataToAdd);
                break;
            case Direction.DownLeft:
                downLeftItems.Add(itemDataToAdd);
                break;
            case Direction.DownRight:
                downRightItems.Add(itemDataToAdd);
                break;
            default:
                break;
        }
    }

    public void AddItemToListFromDirection(ItemData itemDataToAdd, Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                playerPositionItems.Add(itemDataToAdd);
                break;
            case Direction.Up:
                upItems.Add(itemDataToAdd);
                break;
            case Direction.Down:
                downItems.Add(itemDataToAdd);
                break;
            case Direction.Left:
                leftItems.Add(itemDataToAdd);
                break;
            case Direction.Right:
                rightItems.Add(itemDataToAdd);
                break;
            case Direction.UpLeft:
                upLeftItems.Add(itemDataToAdd);
                break;
            case Direction.UpRight:
                upRightItems.Add(itemDataToAdd);
                break;
            case Direction.DownLeft:
                downLeftItems.Add(itemDataToAdd);
                break;
            case Direction.DownRight:
                downRightItems.Add(itemDataToAdd);
                break;
            default:
                break;
        }
    }

    public void RemoveItemFromList(ItemData itemDataToRemove)
    {
        switch (activeDirection)
        {
            case Direction.Center:
                if (playerPositionItems.Contains(itemDataToRemove))
                    playerPositionItems.Remove(itemDataToRemove);
                break;
            case Direction.Up:
                if (upItems.Contains(itemDataToRemove))
                    upItems.Remove(itemDataToRemove);
                break;
            case Direction.Down:
                if (downItems.Contains(itemDataToRemove))
                    downItems.Remove(itemDataToRemove);
                break;
            case Direction.Left:
                if (leftItems.Contains(itemDataToRemove))
                    leftItems.Remove(itemDataToRemove);
                break;
            case Direction.Right:
                if (rightItems.Contains(itemDataToRemove))
                    rightItems.Remove(itemDataToRemove);
                break;
            case Direction.UpLeft:
                if (upLeftItems.Contains(itemDataToRemove))
                    upLeftItems.Remove(itemDataToRemove);
                break;
            case Direction.UpRight:
                if (upRightItems.Contains(itemDataToRemove))
                    upRightItems.Remove(itemDataToRemove);
                break;
            case Direction.DownLeft:
                if (downLeftItems.Contains(itemDataToRemove))
                    downLeftItems.Remove(itemDataToRemove);
                break;
            case Direction.DownRight:
                if (downRightItems.Contains(itemDataToRemove))
                    downRightItems.Remove(itemDataToRemove);
                break;
            default:
                break;
        }
    }

    void ClearAllLists()
    {
        playerPositionItems.Clear();
        leftItems.Clear();
        rightItems.Clear();
        upItems.Clear();
        downItems.Clear();
        upLeftItems.Clear();
        upRightItems.Clear();
        downLeftItems.Clear();
        downRightItems.Clear();

        upInventory = null;
        downInventory = null;
        leftInventory = null;
        rightInventory = null;
        upLeftInventory = null;
        upRightInventory = null;
        downLeftInventory = null;
        downRightInventory = null;
    }
}

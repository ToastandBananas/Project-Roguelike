using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerInventoryUI : InventoryUI
{
    [Header("Floor Icon")]
    public Sprite floorIconSprite;

    [Header("Side Bar Buttons")]
    public Image playerPositionSideBarIcon;
    public Image upSideBarIcon, downSideBarIcon, leftSideBarIcon, rightSideBarIcon, upLeftSideBarIcon, upRightSideBarIcon, downLeftSideBarIcon, downRightSideBarIcon;

    [HideInInspector] public List<ItemData> playerPositionItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> leftItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> rightItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> upItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> downItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> upLeftItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> upRightItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> downLeftItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> downRightItems = new List<ItemData>();

    [HideInInspector] Inventory leftInventory, rightInventory, upInventory, downInventory, upLeftInventory, upRightInventory, downLeftInventory, downRightInventory;

    LayerMask interactableMask;
    float emptyTileMaxVolume = 1000f;

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
        ClearInventoryUI();

        for (int i = 0; i < itemsList.Count; i++)
        {
            InventoryItem invItem = inventoryItemObjectPool.GetPooledInventoryItem();
            itemsList[i].TransferData(itemsList[i], invItem.itemData);
            invItem.itemNameText.text = invItem.itemData.itemName;
            invItem.itemAmountText.text = invItem.itemData.currentStackSize.ToString();
            invItem.itemTypeText.text = invItem.itemData.item.itemType.ToString();
            invItem.itemWeightText.text = (invItem.itemData.item.weight * invItem.itemData.currentStackSize).ToString();
            invItem.itemVolumeText.text = (invItem.itemData.item.volume * invItem.itemData.currentStackSize).ToString();
            invItem.gameObject.SetActive(true);
        }

        // Set container open icon sprite (when applicable) and header/volume/weight text
        SetUpInventoryUI(direction);
    }

    public void GetItemsAroundPlayer()
    {
        ClearAllLists();

        GetItemsAtPosition(playerManager.transform.position, playerPositionItems, Direction.Center);
        GetItemsAtPosition(playerManager.transform.position + new Vector3(-1, 0), leftItems, Direction.Left);
        GetItemsAtPosition(playerManager.transform.position + new Vector3(1, 0), rightItems, Direction.Right);
        GetItemsAtPosition(playerManager.transform.position + new Vector3(0, 1), upItems, Direction.Up);
        GetItemsAtPosition(playerManager.transform.position + new Vector3(0, -1), downItems, Direction.Down);
        GetItemsAtPosition(playerManager.transform.position + new Vector3(-1, 1), upLeftItems, Direction.UpLeft);
        GetItemsAtPosition(playerManager.transform.position + new Vector3(1, 1), upRightItems, Direction.UpRight);
        GetItemsAtPosition(playerManager.transform.position + new Vector3(-1, -1), downLeftItems, Direction.DownLeft);
        GetItemsAtPosition(playerManager.transform.position + new Vector3(1, -1), downRightItems, Direction.DownRight);
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
                        AssignInventory(direction, inventory);
                    }
                }
                else
                {
                    // Get each item on the ground
                    itemsList.Add(hits[i].collider.GetComponent<ItemData>());
                }
            }
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

    void SetSideBarIcon_Container(Direction direction, Inventory inventory)
    {
        switch (direction)
        {
            case Direction.Center:
                playerPositionSideBarIcon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Up:
                upSideBarIcon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Down:
                downSideBarIcon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Left:
                leftSideBarIcon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Right:
                rightSideBarIcon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.UpLeft:
                upLeftSideBarIcon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.UpRight:
                upRightSideBarIcon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.DownLeft:
                downLeftSideBarIcon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.DownRight:
                downRightSideBarIcon.sprite = GetContainerIcon(inventory, false);
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
                playerPositionSideBarIcon.sprite = floorIconSprite;
                break;
            case Direction.Up:
                upSideBarIcon.sprite = floorIconSprite;
                break;
            case Direction.Down:
                downSideBarIcon.sprite = floorIconSprite;
                break;
            case Direction.Left:
                leftSideBarIcon.sprite = floorIconSprite;
                break;
            case Direction.Right:
                rightSideBarIcon.sprite = floorIconSprite;
                break;
            case Direction.UpLeft:
                upLeftSideBarIcon.sprite = floorIconSprite;
                break;
            case Direction.UpRight:
                upRightSideBarIcon.sprite = floorIconSprite;
                break;
            case Direction.DownLeft:
                downLeftSideBarIcon.sprite = floorIconSprite;
                break;
            case Direction.DownRight:
                downRightSideBarIcon.sprite = floorIconSprite;
                break;
            default:
                break;
        }
    }

    void SetUpInventoryUI(Direction direction)
    {
        ResetContainerIcons();

        switch (direction)
        {
            case Direction.Center:
                inventoryNameText.text = "Items Under Self";
                weightText.text = GetTotalWeight(playerPositionItems).ToString();
                volumeText.text = GetTotalVolume(playerPositionItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                break;
            case Direction.Up:
                if (upInventory != null)
                {
                    inventoryNameText.text = upInventory.container.name + " Inventory";
                    upSideBarIcon.sprite = GetContainerIcon(upInventory, true);
                    upInventory.container.spriteRenderer.sprite = GetContainerIcon(upInventory, true);
                    weightText.text = GetTotalWeight(upItems).ToString() + "/" + upInventory.maxWeight.ToString();
                    volumeText.text = GetTotalVolume(upItems).ToString() + "/" + upInventory.maxVolume.ToString();
                }
                else
                {
                    inventoryNameText.text = "Items Above Self";
                    weightText.text = GetTotalWeight(upItems).ToString();
                    volumeText.text = GetTotalVolume(upItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Down:
                if (downInventory != null)
                {
                    inventoryNameText.text = downInventory.container.name + " Inventory";
                    downSideBarIcon.sprite = GetContainerIcon(downInventory, true);
                    downInventory.container.spriteRenderer.sprite = GetContainerIcon(downInventory, true);
                    weightText.text = GetTotalWeight(downItems).ToString() + "/" + downInventory.maxWeight.ToString();
                    volumeText.text = GetTotalVolume(downItems).ToString() + "/" + downInventory.maxVolume.ToString();
                }
                else
                {
                    inventoryNameText.text = "Items Below Self";
                    weightText.text = GetTotalWeight(downItems).ToString();
                    volumeText.text = GetTotalVolume(downItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Left:
                if (leftInventory != null)
                {
                    inventoryNameText.text = leftInventory.container.name + " Inventory";
                    leftSideBarIcon.sprite = GetContainerIcon(leftInventory, true);
                    leftInventory.container.spriteRenderer.sprite = GetContainerIcon(leftInventory, true);
                    weightText.text = GetTotalWeight(leftItems).ToString() + "/" + leftInventory.maxWeight.ToString();
                    volumeText.text = GetTotalVolume(leftItems).ToString() + "/" + leftInventory.maxVolume.ToString();
                }
                else
                {
                    inventoryNameText.text = "Items Left of Self";
                    weightText.text = GetTotalWeight(leftItems).ToString();
                    volumeText.text = GetTotalVolume(leftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Right:
                if (rightInventory != null)
                {
                    inventoryNameText.text = rightInventory.container.name + " Inventory";
                    rightSideBarIcon.sprite = GetContainerIcon(rightInventory, true);
                    rightInventory.container.spriteRenderer.sprite = GetContainerIcon(rightInventory, true);
                    weightText.text = GetTotalWeight(rightItems).ToString() + "/" + rightInventory.maxWeight.ToString();
                    volumeText.text = GetTotalVolume(rightItems).ToString() + "/" + rightInventory.maxVolume.ToString();
                }
                else
                {
                    inventoryNameText.text = "Items Right of Self";
                    weightText.text = GetTotalWeight(rightItems).ToString();
                    volumeText.text = GetTotalVolume(rightItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.UpLeft:
                if (upLeftInventory != null)
                {
                    inventoryNameText.text = upLeftInventory.container.name + " Inventory";
                    upLeftSideBarIcon.sprite = GetContainerIcon(upLeftInventory, true);
                    upLeftInventory.container.spriteRenderer.sprite = GetContainerIcon(upLeftInventory, true);
                    weightText.text = GetTotalWeight(upLeftItems).ToString() + "/" + upLeftInventory.maxWeight.ToString();
                    volumeText.text = GetTotalVolume(upLeftItems).ToString() + "/" + upLeftInventory.maxVolume.ToString();
                }
                else
                {
                    inventoryNameText.text = "Items Above and Left of Self";
                    weightText.text = GetTotalWeight(upLeftItems).ToString();
                    volumeText.text = GetTotalVolume(upLeftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.UpRight:
                if (upRightInventory != null)
                {
                    inventoryNameText.text = upRightInventory.container.name + " Inventory";
                    upRightSideBarIcon.sprite = GetContainerIcon(upRightInventory, true);
                    upRightInventory.container.spriteRenderer.sprite = GetContainerIcon(upRightInventory, true);
                    weightText.text = GetTotalWeight(upRightItems).ToString() + "/" + upRightInventory.maxWeight.ToString();
                    volumeText.text = GetTotalVolume(upRightItems).ToString() + "/" + upRightInventory.maxVolume.ToString();
                }
                else
                {
                    inventoryNameText.text = "Items Above and Right of Self";
                    weightText.text = GetTotalWeight(upRightItems).ToString();
                    volumeText.text = GetTotalVolume(upRightItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.DownLeft:
                if (downLeftInventory != null)
                {
                    inventoryNameText.text = downLeftInventory.container.name + " Inventory";
                    downLeftSideBarIcon.sprite = GetContainerIcon(downLeftInventory, true);
                    downLeftInventory.container.spriteRenderer.sprite = GetContainerIcon(downLeftInventory, true);
                    weightText.text = GetTotalWeight(downLeftItems).ToString() + "/" + downLeftInventory.maxWeight.ToString();
                    volumeText.text = GetTotalVolume(downLeftItems).ToString() + "/" + downLeftInventory.maxVolume.ToString();
                }
                else
                {
                    inventoryNameText.text = "Items Below and Left of Self";
                    weightText.text = GetTotalWeight(downLeftItems).ToString();
                    volumeText.text = GetTotalVolume(downLeftItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.DownRight:
                if (downRightInventory != null)
                {
                    inventoryNameText.text = downRightInventory.container.name + " Inventory";
                    downRightSideBarIcon.sprite = GetContainerIcon(downRightInventory, true);
                    downRightInventory.container.spriteRenderer.sprite = GetContainerIcon(downRightInventory, true);
                    weightText.text = GetTotalWeight(downRightItems).ToString() + "/" + downRightInventory.maxWeight.ToString();
                    volumeText.text = GetTotalVolume(downRightItems).ToString() + "/" + downRightInventory.maxVolume.ToString();
                }
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

    Sprite GetContainerIcon(Inventory inventory, bool containerIsActive)
    {
        if (containerIsActive)
        {
            if (inventory.container.sidebarSpriteOpen != null)
                return inventory.container.sidebarSpriteOpen;
        }

        return inventory.container.sidebarSpriteClosed;
    }

    void ResetContainerIcons()
    {
        if (upInventory != null) upInventory.container.spriteRenderer.sprite = GetContainerIcon(upInventory, false);
        if (downInventory != null) downInventory.container.spriteRenderer.sprite = GetContainerIcon(downInventory, false);
        if (leftInventory != null) leftInventory.container.spriteRenderer.sprite = GetContainerIcon(leftInventory, false);
        if (rightInventory != null) rightInventory.container.spriteRenderer.sprite = GetContainerIcon(rightInventory, false);
        if (upLeftInventory != null) upLeftInventory.container.spriteRenderer.sprite = GetContainerIcon(upLeftInventory, false);
        if (upRightInventory != null) upRightInventory.container.spriteRenderer.sprite = GetContainerIcon(upRightInventory, false);
        if (downLeftInventory != null) downLeftInventory.container.spriteRenderer.sprite = GetContainerIcon(downLeftInventory, false);
        if (downRightInventory != null) downRightInventory.container.spriteRenderer.sprite = GetContainerIcon(downRightInventory, false);
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

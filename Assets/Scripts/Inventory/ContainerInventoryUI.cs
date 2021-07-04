using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerInventoryUI : InventoryUI
{
    [Header("Floor Icon")]
    public Sprite floorIconSprite;

    [Header("Side Bar Buttons")]
    public ContainerSideBarButton playerPositionSideBarButton;
    public ContainerSideBarButton northSideBarButton, southSideBarButton, westSideBarButton, eastSideBarButton, northwestSideBarButton, northeastSideBarButton, southwestSideBarButton, southeastSideBarButton;

    [Header("Max Ground Volume")]
    public float emptyTileMaxVolume = 1000f;

    [HideInInspector] public List<ItemData> playerPositionItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> northItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> southItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> westItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> eastItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> northwestItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> northeastItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> southwestItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> southeastItems = new List<ItemData>();

    public Inventory playerPositionInventory, northInventory, southInventory, westInventory, eastInventory, northwestInventory, northeastInventory, southwestInventory, southeastInventory;

    [HideInInspector] public Direction activeDirection;
    [HideInInspector] public ContainerSideBarButton activeContainerSideBarButton;

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

        for (int i = 0; i < inventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            inventoryItemObjectPool.pooledInventoryItems[i].myInvUI = this;
        }

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
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(-1, 0), westItems, Direction.West);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(1, 0), eastItems, Direction.East);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(0, 1), northItems, Direction.North);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(0, -1), southItems, Direction.South);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(-1, 1), northwestItems, Direction.Northwest);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(1, 1), northeastItems, Direction.Northeast);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(-1, -1), southwestItems, Direction.Southwest);
        GetItemsAtPosition(gm.playerManager.transform.position + new Vector3(1, -1), southeastItems, Direction.Southeast);
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
                    if (hits[i].collider.TryGetComponent(out ItemData itemData))
                    {
                        itemsList.Add(itemData);
                    }
                    else
                    {
                        // Get each item in the container's inventory
                        for (int j = 0; j < inventory.items.Count; j++)
                        {
                            itemsList.Add(inventory.items[j]);
                        }
                    }

                    SetSideBarIcon_Container(direction, inventory);
                    AssignDirectionalInventory(direction, inventory);
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

    public void AssignDirectionalInventory(Direction direction, Inventory inventory)
    {
        switch (direction)
        {
            case Direction.Center:
                if (inventory != null) playerPositionInventory = inventory;
                break;
            case Direction.North:
                if (inventory != null) northInventory = inventory;
                break;
            case Direction.South:
                if (inventory != null) southInventory = inventory;
                break;
            case Direction.West:
                if (inventory != null) westInventory = inventory;
                break;
            case Direction.East:
                if (inventory != null) eastInventory = inventory;
                break;
            case Direction.Northwest:
                if (inventory != null) northwestInventory = inventory;
                break;
            case Direction.Northeast:
                if (inventory != null) northeastInventory = inventory;
                break;
            case Direction.Southwest:
                if (inventory != null) southwestInventory = inventory;
                break;
            case Direction.Southeast:
                if (inventory != null) southeastInventory = inventory;
                break;
            default:
                break;
        }
    }

    public void RemoveDirectionalInventory(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                playerPositionInventory = null;
                break;
            case Direction.North:
                northInventory = null;
                break;
            case Direction.South:
                southInventory = null;
                break;
            case Direction.West:
                westInventory = null;
                break;
            case Direction.East:
                eastInventory = null;
                break;
            case Direction.Northwest:
                northwestInventory = null;
                break;
            case Direction.Northeast:
                northeastInventory = null;
                break;
            case Direction.Southwest:
                southwestInventory = null;
                break;
            case Direction.Southeast:
                southeastInventory = null;
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
                invItem.myInventory = playerPositionInventory;
                break;
            case Direction.North:
                invItem.myInventory = northInventory;
                break;
            case Direction.South:
                invItem.myInventory = southInventory;
                break;
            case Direction.West:
                invItem.myInventory = westInventory;
                break;
            case Direction.East:
                invItem.myInventory = eastInventory;
                break;
            case Direction.Northwest:
                invItem.myInventory = northwestInventory;
                break;
            case Direction.Northeast:
                invItem.myInventory = northeastInventory;
                break;
            case Direction.Southwest:
                invItem.myInventory = southwestInventory;
                break;
            case Direction.Southeast:
                invItem.myInventory = southeastInventory;
                break;
            default:
                break;
        }
    }

    public void SetSideBarIcon_Container(Direction direction, Inventory inventory)
    {
        switch (direction)
        {
            case Direction.Center:
                playerPositionSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.North:
                northSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.South:
                southSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.West:
                westSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.East:
                eastSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Northwest:
                northwestSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Northeast:
                northeastSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Southwest:
                southwestSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            case Direction.Southeast:
                southeastSideBarButton.icon.sprite = GetContainerIcon(inventory, false);
                break;
            default:
                break;
        }
    }

    public void SetSideBarIcon_Floor(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                playerPositionSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.North:
                northSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.South:
                southSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.West:
                westSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.East:
                eastSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.Northwest:
                northwestSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.Northeast:
                northeastSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.Southwest:
                southwestSideBarButton.icon.sprite = floorIconSprite;
                break;
            case Direction.Southeast:
                southeastSideBarButton.icon.sprite = floorIconSprite;
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
                activeContainerSideBarButton = playerPositionSideBarButton;
                if (playerPositionInventory != null)
                    SetupContainerUI(playerPositionInventory, playerPositionSideBarButton.icon, playerPositionItems);
                else
                {
                    inventoryNameText.text = "Items Under Self";
                    weightText.text = GetTotalWeight(playerPositionItems).ToString();
                    volumeText.text = GetTotalVolume(playerPositionItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.North:
                activeContainerSideBarButton = northSideBarButton;
                if (northInventory != null)
                    SetupContainerUI(northInventory, northSideBarButton.icon, northItems);
                else
                {
                    inventoryNameText.text = "Items Above Self";
                    weightText.text = GetTotalWeight(northItems).ToString();
                    volumeText.text = GetTotalVolume(northItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.South:
                activeContainerSideBarButton = southSideBarButton;
                if (southInventory != null)
                    SetupContainerUI(southInventory, southSideBarButton.icon, southItems);
                else
                {
                    inventoryNameText.text = "Items Below Self";
                    weightText.text = GetTotalWeight(southItems).ToString();
                    volumeText.text = GetTotalVolume(southItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.West:
                activeContainerSideBarButton = westSideBarButton;
                if (westInventory != null)
                    SetupContainerUI(westInventory, westSideBarButton.icon, westItems);
                else
                {
                    inventoryNameText.text = "Items Left of Self";
                    weightText.text = GetTotalWeight(westItems).ToString();
                    volumeText.text = GetTotalVolume(westItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.East:
                activeContainerSideBarButton = eastSideBarButton;
                if (eastInventory != null)
                    SetupContainerUI(eastInventory, eastSideBarButton.icon, eastItems);
                else
                {
                    inventoryNameText.text = "Items Right of Self";
                    weightText.text = GetTotalWeight(eastItems).ToString();
                    volumeText.text = GetTotalVolume(eastItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Northwest:
                activeContainerSideBarButton = northwestSideBarButton;
                if (northwestInventory != null)
                    SetupContainerUI(northwestInventory, northwestSideBarButton.icon, northwestItems);
                else
                {
                    inventoryNameText.text = "Items Above and Left of Self";
                    weightText.text = GetTotalWeight(northwestItems).ToString();
                    volumeText.text = GetTotalVolume(northwestItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Northeast:
                activeContainerSideBarButton = northeastSideBarButton;
                if (northeastInventory != null)
                    SetupContainerUI(northeastInventory, northeastSideBarButton.icon, northeastItems);
                else
                {
                    inventoryNameText.text = "Items Above and Right of Self";
                    weightText.text = GetTotalWeight(northeastItems).ToString();
                    volumeText.text = GetTotalVolume(northeastItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Southwest:
                activeContainerSideBarButton = southwestSideBarButton;
                if (southwestInventory != null)
                    SetupContainerUI(southwestInventory, southwestSideBarButton.icon, southwestItems);
                else
                {
                    inventoryNameText.text = "Items Below and Left of Self";
                    weightText.text = GetTotalWeight(southwestItems).ToString();
                    volumeText.text = GetTotalVolume(southwestItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                }
                break;
            case Direction.Southeast:
                activeContainerSideBarButton = southeastSideBarButton;
                if (southeastInventory != null)
                    SetupContainerUI(southeastInventory, southeastSideBarButton.icon, southeastItems);
                else
                {
                    inventoryNameText.text = "Items Below and Right Self";
                    weightText.text = GetTotalWeight(southeastItems).ToString();
                    volumeText.text = GetTotalVolume(southeastItems).ToString() + "/" + emptyTileMaxVolume.ToString();
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
            case Direction.North:
                return northItems;
            case Direction.South:
                return southItems;
            case Direction.West:
                return westItems;
            case Direction.East:
                return eastItems;
            case Direction.Northwest:
                return northwestItems;
            case Direction.Northeast:
                return northeastItems;
            case Direction.Southwest:
                return southwestItems;
            case Direction.Southeast:
                return southeastItems;
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
            case Direction.North:
                return northItems;
            case Direction.South:
                return southItems;
            case Direction.West:
                return westItems;
            case Direction.East:
                return eastItems;
            case Direction.Northwest:
                return northwestItems;
            case Direction.Northeast:
                return northeastItems;
            case Direction.Southwest:
                return southwestItems;
            case Direction.Southeast:
                return southeastItems;
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
        northSideBarButton.ResetDirectionIconColor();
        southSideBarButton.ResetDirectionIconColor();
        westSideBarButton.ResetDirectionIconColor();
        eastSideBarButton.ResetDirectionIconColor();
        northwestSideBarButton.ResetDirectionIconColor();
        northeastSideBarButton.ResetDirectionIconColor();
        southwestSideBarButton.ResetDirectionIconColor();
        southeastSideBarButton.ResetDirectionIconColor();
        if (activeDirection != Direction.Center || newDirection != Direction.Center)
            playerPositionSideBarButton.ResetDirectionIconColor();

        if (playerPositionInventory != null)
        {
            playerPositionInventory.container.spriteRenderer.sprite = GetContainerIcon(playerPositionInventory, false);
            playerPositionSideBarButton.icon.sprite = GetContainerIcon(playerPositionInventory, false);
        }

        if (northInventory != null)
        {
            northInventory.container.spriteRenderer.sprite = GetContainerIcon(northInventory, false);
            northSideBarButton.icon.sprite = GetContainerIcon(northInventory, false);
        }

        if (southInventory != null)
        {
            southInventory.container.spriteRenderer.sprite = GetContainerIcon(southInventory, false);
            southSideBarButton.icon.sprite = GetContainerIcon(southInventory, false);
        }

        if (westInventory != null)
        {
            westInventory.container.spriteRenderer.sprite = GetContainerIcon(westInventory, false);
            westSideBarButton.icon.sprite = GetContainerIcon(westInventory, false);
        }

        if (eastInventory != null)
        {
            eastInventory.container.spriteRenderer.sprite = GetContainerIcon(eastInventory, false);
            eastSideBarButton.icon.sprite = GetContainerIcon(eastInventory, false);
        }

        if (northwestInventory != null)
        {
            northwestInventory.container.spriteRenderer.sprite = GetContainerIcon(northwestInventory, false);
            northwestSideBarButton.icon.sprite = GetContainerIcon(northwestInventory, false);
        }

        if (northeastInventory != null)
        {
            northeastInventory.container.spriteRenderer.sprite = GetContainerIcon(northeastInventory, false);
            northeastSideBarButton.icon.sprite = GetContainerIcon(northeastInventory, false);
        }

        if (southwestInventory != null)
        {
            southwestInventory.container.spriteRenderer.sprite = GetContainerIcon(southwestInventory, false);
            southwestSideBarButton.icon.sprite = GetContainerIcon(southwestInventory, false);
        }

        if (southeastInventory != null)
        {
            southeastInventory.container.spriteRenderer.sprite = GetContainerIcon(southeastInventory, false);
            southeastSideBarButton.icon.sprite = GetContainerIcon(southeastInventory, false);
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
                case Direction.North:
                    weightText.text = GetTotalWeight(northItems).ToString();
                    volumeText.text = GetTotalVolume(northItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.South:
                    weightText.text = GetTotalWeight(southItems).ToString();
                    volumeText.text = GetTotalVolume(southItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.West:
                    weightText.text = GetTotalWeight(westItems).ToString();
                    volumeText.text = GetTotalVolume(westItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.East:
                    weightText.text = GetTotalWeight(eastItems).ToString();
                    volumeText.text = GetTotalVolume(eastItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.Northwest:
                    weightText.text = GetTotalWeight(northwestItems).ToString();
                    volumeText.text = GetTotalVolume(northwestItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.Northeast:
                    weightText.text = GetTotalWeight(northeastItems).ToString();
                    volumeText.text = GetTotalVolume(northeastItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.Southwest:
                    weightText.text = GetTotalWeight(southwestItems).ToString();
                    volumeText.text = GetTotalVolume(southwestItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                case Direction.Southeast:
                    weightText.text = GetTotalWeight(southeastItems).ToString();
                    volumeText.text = GetTotalVolume(southeastItems).ToString() + "/" + emptyTileMaxVolume.ToString();
                    break;
                default:
                    break;
            }
        }
    }

    public void AddItemToActiveDirectionList(ItemData itemDataToAdd)
    {
        switch (activeDirection)
        {
            case Direction.Center:
                playerPositionItems.Add(itemDataToAdd);
                break;
            case Direction.North:
                northItems.Add(itemDataToAdd);
                break;
            case Direction.South:
                southItems.Add(itemDataToAdd);
                break;
            case Direction.West:
                westItems.Add(itemDataToAdd);
                break;
            case Direction.East:
                eastItems.Add(itemDataToAdd);
                break;
            case Direction.Northwest:
                northwestItems.Add(itemDataToAdd);
                break;
            case Direction.Northeast:
                northeastItems.Add(itemDataToAdd);
                break;
            case Direction.Southwest:
                southwestItems.Add(itemDataToAdd);
                break;
            case Direction.Southeast:
                southeastItems.Add(itemDataToAdd);
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
            case Direction.North:
                northItems.Add(itemDataToAdd);
                break;
            case Direction.South:
                southItems.Add(itemDataToAdd);
                break;
            case Direction.West:
                westItems.Add(itemDataToAdd);
                break;
            case Direction.East:
                eastItems.Add(itemDataToAdd);
                break;
            case Direction.Northwest:
                northwestItems.Add(itemDataToAdd);
                break;
            case Direction.Northeast:
                northeastItems.Add(itemDataToAdd);
                break;
            case Direction.Southwest:
                southwestItems.Add(itemDataToAdd);
                break;
            case Direction.Southeast:
                southeastItems.Add(itemDataToAdd);
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
            case Direction.North:
                if (northItems.Contains(itemDataToRemove))
                    northItems.Remove(itemDataToRemove);
                break;
            case Direction.South:
                if (southItems.Contains(itemDataToRemove))
                    southItems.Remove(itemDataToRemove);
                break;
            case Direction.West:
                if (westItems.Contains(itemDataToRemove))
                    westItems.Remove(itemDataToRemove);
                break;
            case Direction.East:
                if (eastItems.Contains(itemDataToRemove))
                    eastItems.Remove(itemDataToRemove);
                break;
            case Direction.Northwest:
                if (northwestItems.Contains(itemDataToRemove))
                    northwestItems.Remove(itemDataToRemove);
                break;
            case Direction.Northeast:
                if (northeastItems.Contains(itemDataToRemove))
                    northeastItems.Remove(itemDataToRemove);
                break;
            case Direction.Southwest:
                if (southwestItems.Contains(itemDataToRemove))
                    southwestItems.Remove(itemDataToRemove);
                break;
            case Direction.Southeast:
                if (southeastItems.Contains(itemDataToRemove))
                    southeastItems.Remove(itemDataToRemove);
                break;
            default:
                break;
        }
    }

    public ContainerSideBarButton GetSideBarButtonFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                return playerPositionSideBarButton;
            case Direction.North:
                return northSideBarButton;
            case Direction.South:
                return southSideBarButton;
            case Direction.West:
                return westSideBarButton;
            case Direction.East:
                return eastSideBarButton;
            case Direction.Northwest:
                return northwestSideBarButton;
            case Direction.Northeast:
                return northeastSideBarButton;
            case Direction.Southwest:
                return southwestSideBarButton;
            case Direction.Southeast:
                return southeastSideBarButton;
            default:
                return null;
        }
    }

    public void RemoveBagFromGround()
    {
        SetSideBarIcon_Floor(activeDirection);
        activeInventory = null;
        RemoveDirectionalInventory(activeDirection);
    }

    void ClearAllLists()
    {
        playerPositionItems.Clear();
        westItems.Clear();
        eastItems.Clear();
        northItems.Clear();
        southItems.Clear();
        northwestItems.Clear();
        northeastItems.Clear();
        southwestItems.Clear();
        southeastItems.Clear();

        playerPositionInventory = null;
        northInventory = null;
        southInventory = null;
        westInventory = null;
        eastInventory = null;
        northwestInventory = null;
        northeastInventory = null;
        southwestInventory = null;
        southeastInventory = null;
    }
}

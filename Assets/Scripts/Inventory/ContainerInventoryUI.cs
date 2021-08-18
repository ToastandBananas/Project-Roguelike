using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerInventoryUI : InventoryUI
{
    [Header("Floor Icon")]
    public Sprite floorIconSprite;

    [Header("Side Bar Buttons")]
    public InventoryCycler inventoryCycler;
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

    [HideInInspector] public List<ItemData> playerPositionGroundItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> northGroundItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> southGroundItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> westGroundItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> eastGroundItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> northwestGroundItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> northeastGroundItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> southwestGroundItems = new List<ItemData>();
    [HideInInspector] public List<ItemData> southeastGroundItems = new List<ItemData>();

    [HideInInspector] public Inventory playerPositionInventory, northInventory, southInventory, westInventory, eastInventory, northwestInventory, northeastInventory, southwestInventory, southeastInventory;

    [HideInInspector] public List<Inventory> playerPositionInventories = new List<Inventory>();
    [HideInInspector] public List<Inventory> northInventories = new List<Inventory>();
    [HideInInspector] public List<Inventory> southInventories = new List<Inventory>();
    [HideInInspector] public List<Inventory> westInventories = new List<Inventory>();
    [HideInInspector] public List<Inventory> eastInventories = new List<Inventory>();
    [HideInInspector] public List<Inventory> northwestInventories = new List<Inventory>();
    [HideInInspector] public List<Inventory> northeastInventories = new List<Inventory>();
    [HideInInspector] public List<Inventory> southwestInventories = new List<Inventory>();
    [HideInInspector] public List<Inventory> southeastInventories = new List<Inventory>();

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
        inventoryCycler.Init();

        for (int i = 0; i < inventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            inventoryItemObjectPool.pooledInventoryItems[i].myInvUI = this;
        }

        GetItemsAroundPlayer();
        PopulateInventoryUI(playerPositionItems, Direction.Center);
    }

    public IEnumerator DelayPopulateInventoryUI(List<ItemData> itemsList, Direction direction)
    {
        yield return null;
        PopulateInventoryUI(itemsList, direction);
    }

    // This method runs when the user clicks on a container side bar icon
    public void PopulateInventoryUI(List<ItemData> itemsList, Direction direction)
    {
        activeInventory = null;
        ClearInventoryUI();
        activeDirection = direction;

        // Set container open icon sprite (when applicable) and header/volume/weight text
        SetUpInventoryUI(direction);

        // Setup the inventory cycler 
        if (gm.containerInvUI.GetInventoriesListFromDirection(gm.containerInvUI.activeDirection).Count > 0 && gm.containerInvUI.GetInventoriesListFromDirection(gm.containerInvUI.activeDirection)[0].CompareTag("Object") == false)
            gm.containerInvUI.inventoryCycler.Show();
        else
            gm.containerInvUI.inventoryCycler.Hide();

        for (int i = 0; i < itemsList.Count; i++)
        {
            InventoryItem invItem = ShowNewInventoryItem(itemsList[i]);
            AssignInventoryToInventoryItem(invItem, direction);
            
            // If this is a bag on the ground, just automatically expand its disclosure widget to show the items inside the bag
            if (invItem.itemData.IsPickup() && invItem.itemData.item.IsBag())
                invItem.disclosureWidget.ExpandDisclosureWidget();
        }
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
            if (hits[i].collider != null && hits[i].collider.CompareTag("NPC") == false)
            {
                if (hits[i].collider.TryGetComponent(out Inventory inventory))
                {
                    if ((hits[i].collider.TryGetComponent(out ItemData itemData) && itemData.item.IsBag()) || itemData == null)
                        GetInventoriesListFromDirection(direction).Add(inventory);

                    if (GetDirectionalInventory(direction) == null)
                    {
                        itemsList.Clear();
                        if (itemData == null || itemData.item.IsBag())
                        {
                            SetSideBarIcon_Container(direction, inventory);
                            AssignDirectionalInventory(direction, inventory);
                        }

                        if (itemData != null)
                            itemsList.Add(itemData);
                        else
                        {
                            // Get each item in the container's inventory
                            for (int j = 0; j < inventory.items.Count; j++)
                            {
                                itemsList.Add(inventory.items[j]);
                            }
                        }
                    }
                }
                else if (hits[i].collider.TryGetComponent(out ItemData itemData))
                {
                    if (GetDirectionalInventory(direction) == null)
                    {
                        // Get each item on the ground
                        itemsList.Add(itemData);
                        GetGroundItemsListFromDirection(direction).Add(itemData);
                    }
                    else
                        GetGroundItemsListFromDirection(direction).Add(itemData);
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

    public void SetUpInventoryUI(Direction direction)
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
                    volumeText.text = GetTotalVolume(playerPositionItems).ToString();
                }
                break;
            case Direction.North:
                activeContainerSideBarButton = northSideBarButton;
                if (northInventory != null)
                    SetupContainerUI(northInventory, northSideBarButton.icon, northItems);
                else
                {
                    inventoryNameText.text = "Items North of Self";
                    weightText.text = GetTotalWeight(northItems).ToString();
                    volumeText.text = GetTotalVolume(northItems).ToString();
                }
                break;
            case Direction.South:
                activeContainerSideBarButton = southSideBarButton;
                if (southInventory != null)
                    SetupContainerUI(southInventory, southSideBarButton.icon, southItems);
                else
                {
                    inventoryNameText.text = "Items South of Self";
                    weightText.text = GetTotalWeight(southItems).ToString();
                    volumeText.text = GetTotalVolume(southItems).ToString();
                }
                break;
            case Direction.West:
                activeContainerSideBarButton = westSideBarButton;
                if (westInventory != null)
                    SetupContainerUI(westInventory, westSideBarButton.icon, westItems);
                else
                {
                    inventoryNameText.text = "Items West of Self";
                    weightText.text = GetTotalWeight(westItems).ToString();
                    volumeText.text = GetTotalVolume(westItems).ToString();
                }
                break;
            case Direction.East:
                activeContainerSideBarButton = eastSideBarButton;
                if (eastInventory != null)
                    SetupContainerUI(eastInventory, eastSideBarButton.icon, eastItems);
                else
                {
                    inventoryNameText.text = "Items East of Self";
                    weightText.text = GetTotalWeight(eastItems).ToString();
                    volumeText.text = GetTotalVolume(eastItems).ToString();
                }
                break;
            case Direction.Northwest:
                activeContainerSideBarButton = northwestSideBarButton;
                if (northwestInventory != null)
                    SetupContainerUI(northwestInventory, northwestSideBarButton.icon, northwestItems);
                else
                {
                    inventoryNameText.text = "Items Northwest of Self";
                    weightText.text = GetTotalWeight(northwestItems).ToString();
                    volumeText.text = GetTotalVolume(northwestItems).ToString();
                }
                break;
            case Direction.Northeast:
                activeContainerSideBarButton = northeastSideBarButton;
                if (northeastInventory != null)
                    SetupContainerUI(northeastInventory, northeastSideBarButton.icon, northeastItems);
                else
                {
                    inventoryNameText.text = "Items Northeast of Self";
                    weightText.text = GetTotalWeight(northeastItems).ToString();
                    volumeText.text = GetTotalVolume(northeastItems).ToString();
                }
                break;
            case Direction.Southwest:
                activeContainerSideBarButton = southwestSideBarButton;
                if (southwestInventory != null)
                    SetupContainerUI(southwestInventory, southwestSideBarButton.icon, southwestItems);
                else
                {
                    inventoryNameText.text = "Items Southwest of Self";
                    weightText.text = GetTotalWeight(southwestItems).ToString();
                    volumeText.text = GetTotalVolume(southwestItems).ToString();
                }
                break;
            case Direction.Southeast:
                activeContainerSideBarButton = southeastSideBarButton;
                if (southeastInventory != null)
                    SetupContainerUI(southeastInventory, southeastSideBarButton.icon, southeastItems);
                else
                {
                    inventoryNameText.text = "Items Southeast of Self";
                    weightText.text = GetTotalWeight(southeastItems).ToString();
                    volumeText.text = GetTotalVolume(southeastItems).ToString();
                }
                break;
            default:
                break;
        }

        // Setup the scrollbar
        if (inventoryItemObjectPool.activePooledInventoryItems.Count > maxInvItems)
        {
            scrollbar.value = 1;
            invItemsParentRectTransform.offsetMin = new Vector2(invItemsParentRectTransform.offsetMin.x, (inventoryItemObjectPool.activePooledInventoryItems.Count - maxInvItems) * -invItemHeight);
        }
    }

    public List<ItemData> GetItemsListFromActiveDirection()
    {
        return GetItemsListFromDirection(activeDirection);
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
        if (inventory.container != null)
        {
            inventoryNameText.text = inventory.container.name + " Inventory";
            inventory.container.spriteRenderer.sprite = GetContainerIcon(inventory, true);
        }
        else if (inventory.CompareTag("Dead Body"))
            inventoryNameText.text = inventory.gameObject.name + "'s Inventory";
        else
            inventoryNameText.text = "Inventory";

        sideBarIcon.sprite = GetContainerIcon(inventory, true);
        activeInventory = inventory;
        activeInventory.myInvUI = this;

        if (inventory.CompareTag("Dead Body"))
        {
            weightText.text = GetTotalWeight(itemsList).ToString();
            volumeText.text = GetTotalVolume(itemsList).ToString();
        }
        else
        {
            weightText.text = GetTotalWeight(itemsList).ToString() + "/" + inventory.maxWeight.ToString();
            volumeText.text = GetTotalVolume(itemsList).ToString() + "/" + inventory.maxVolume.ToString();
        }
    }

    Sprite GetContainerIcon(Inventory inventory, bool containerIsActive)
    {
        if (inventory.container == null)
        {
            if (inventory.inventoryOwner != null)
                return inventory.inventoryOwner.spriteManager.deathSprite;
            else
                return floorIconSprite;
        }

        if (containerIsActive)
        {
            if (inventory.container.sidebarSpriteOpen != null)
                return inventory.container.sidebarSpriteOpen;
        }

        if (inventory.container.sidebarSpriteClosed != null)
            return inventory.container.sidebarSpriteClosed;
        else
            return floorIconSprite;
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
            if (playerPositionInventory.container != null)
                playerPositionInventory.container.spriteRenderer.sprite = GetContainerIcon(playerPositionInventory, false);

            playerPositionSideBarButton.icon.sprite = GetContainerIcon(playerPositionInventory, false);
        }

        if (northInventory != null)
        {
            if (northInventory.container != null)
                northInventory.container.spriteRenderer.sprite = GetContainerIcon(northInventory, false);

            northSideBarButton.icon.sprite = GetContainerIcon(northInventory, false);
        }

        if (southInventory != null)
        {
            if (southInventory.container != null)
                southInventory.container.spriteRenderer.sprite = GetContainerIcon(southInventory, false);

            southSideBarButton.icon.sprite = GetContainerIcon(southInventory, false);
        }

        if (westInventory != null)
        {
            if (westInventory.container != null)
                westInventory.container.spriteRenderer.sprite = GetContainerIcon(westInventory, false);

            westSideBarButton.icon.sprite = GetContainerIcon(westInventory, false);
        }

        if (eastInventory != null)
        {
            if (eastInventory.container != null)
                eastInventory.container.spriteRenderer.sprite = GetContainerIcon(eastInventory, false);

            eastSideBarButton.icon.sprite = GetContainerIcon(eastInventory, false);
        }

        if (northwestInventory != null)
        {
            if (northwestInventory.container != null)
                northwestInventory.container.spriteRenderer.sprite = GetContainerIcon(northwestInventory, false);

            northwestSideBarButton.icon.sprite = GetContainerIcon(northwestInventory, false);
        }

        if (northeastInventory != null)
        {
            if (northeastInventory.container != null)
                northeastInventory.container.spriteRenderer.sprite = GetContainerIcon(northeastInventory, false);

            northeastSideBarButton.icon.sprite = GetContainerIcon(northeastInventory, false);
        }

        if (southwestInventory != null)
        {
            if (southwestInventory.container != null)
                southwestInventory.container.spriteRenderer.sprite = GetContainerIcon(southwestInventory, false);

            southwestSideBarButton.icon.sprite = GetContainerIcon(southwestInventory, false);
        }

        if (southeastInventory != null)
        {
            if (southeastInventory.container != null)
                southeastInventory.container.spriteRenderer.sprite = GetContainerIcon(southeastInventory, false);

            southeastSideBarButton.icon.sprite = GetContainerIcon(southeastInventory, false);
        }
    }

    public override void UpdateUI()
    {
        if (activeInventory != null)
        {
            if (activeInventory.container != null)
                inventoryNameText.text = activeInventory.container.name + " Inventory";
            else if (activeInventory.CompareTag("Dead Body"))
                inventoryNameText.text = activeInventory.gameObject.name + "'s Inventory";
            else
                inventoryNameText.text = "Inventory";
            
            if (activeInventory.CompareTag("Dead Body"))
            {
                weightText.text = (Mathf.RoundToInt(activeInventory.currentWeight * 100f) / 100f).ToString();
                volumeText.text = (Mathf.RoundToInt(activeInventory.currentVolume * 100f) / 100f).ToString();
            }
            else
            {
                weightText.text = (Mathf.RoundToInt(activeInventory.currentWeight * 100f) / 100f).ToString() + "/" + activeInventory.maxWeight.ToString();
                volumeText.text = (Mathf.RoundToInt(activeInventory.currentVolume * 100f) / 100f).ToString() + "/" + activeInventory.maxVolume.ToString();
            }
        }
        else
        {
            switch (activeDirection)
            {
                case Direction.Center:
                    inventoryNameText.text = "Items Under Self";
                    weightText.text = GetTotalWeight(playerPositionItems).ToString();
                    volumeText.text = GetTotalVolume(playerPositionItems).ToString();
                    break;
                case Direction.North:
                    inventoryNameText.text = "Items North of Self";
                    weightText.text = GetTotalWeight(northItems).ToString();
                    volumeText.text = GetTotalVolume(northItems).ToString();
                    break;
                case Direction.South:
                    inventoryNameText.text = "Items South of Self";
                    weightText.text = GetTotalWeight(southItems).ToString();
                    volumeText.text = GetTotalVolume(southItems).ToString();
                    break;
                case Direction.West:
                    inventoryNameText.text = "Items West of Self";
                    weightText.text = GetTotalWeight(westItems).ToString();
                    volumeText.text = GetTotalVolume(westItems).ToString();
                    break;
                case Direction.East:
                    inventoryNameText.text = "Items East of Self";
                    weightText.text = GetTotalWeight(eastItems).ToString();
                    volumeText.text = GetTotalVolume(eastItems).ToString();
                    break;
                case Direction.Northwest:
                    inventoryNameText.text = "Items Northwest of Self";
                    weightText.text = GetTotalWeight(northwestItems).ToString();
                    volumeText.text = GetTotalVolume(northwestItems).ToString();
                    break;
                case Direction.Northeast:
                    inventoryNameText.text = "Items Northeast of Self";
                    weightText.text = GetTotalWeight(northeastItems).ToString();
                    volumeText.text = GetTotalVolume(northeastItems).ToString();
                    break;
                case Direction.Southwest:
                    inventoryNameText.text = "Items Southwest of Self";
                    weightText.text = GetTotalWeight(southwestItems).ToString();
                    volumeText.text = GetTotalVolume(southwestItems).ToString();
                    break;
                case Direction.Southeast:
                    inventoryNameText.text = "Items Southeast of Self";
                    weightText.text = GetTotalWeight(southeastItems).ToString();
                    volumeText.text = GetTotalVolume(southeastItems).ToString();
                    break;
                default:
                    break;
            }
        }
    }

    public void AddItemToActiveDirectionList(ItemData itemDataToAdd)
    {
        AddItemToDirectionalListFromDirection(itemDataToAdd, activeDirection);
    }

    public void AddItemToDirectionalListFromDirection(ItemData itemDataToAdd, Direction direction)
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

    public void RemoveItemFromActiveDirectionalList(ItemData itemDataToRemove)
    {
        switch (activeDirection)
        {
            case Direction.Center:
                if (playerPositionItems.Contains(itemDataToRemove))
                    playerPositionItems.Remove(itemDataToRemove);
                if (playerPositionGroundItems.Contains(itemDataToRemove))
                    playerPositionGroundItems.Remove(itemDataToRemove);
                break;
            case Direction.North:
                if (northItems.Contains(itemDataToRemove))
                    northItems.Remove(itemDataToRemove);
                if (northGroundItems.Contains(itemDataToRemove))
                    northGroundItems.Remove(itemDataToRemove);
                break;
            case Direction.South:
                if (southItems.Contains(itemDataToRemove))
                    southItems.Remove(itemDataToRemove);
                if (southGroundItems.Contains(itemDataToRemove))
                    southGroundItems.Remove(itemDataToRemove);
                break;
            case Direction.West:
                if (westItems.Contains(itemDataToRemove))
                    westItems.Remove(itemDataToRemove);
                if (westGroundItems.Contains(itemDataToRemove))
                    westGroundItems.Remove(itemDataToRemove);
                break;
            case Direction.East:
                if (eastItems.Contains(itemDataToRemove))
                    eastItems.Remove(itemDataToRemove);
                if (eastGroundItems.Contains(itemDataToRemove))
                    eastGroundItems.Remove(itemDataToRemove);
                break;
            case Direction.Northwest:
                if (northwestItems.Contains(itemDataToRemove))
                    northwestItems.Remove(itemDataToRemove);
                if (northwestGroundItems.Contains(itemDataToRemove))
                    northwestGroundItems.Remove(itemDataToRemove);
                break;
            case Direction.Northeast:
                if (northeastItems.Contains(itemDataToRemove))
                    northeastItems.Remove(itemDataToRemove);
                if (northeastGroundItems.Contains(itemDataToRemove))
                    northeastGroundItems.Remove(itemDataToRemove);
                break;
            case Direction.Southwest:
                if (southwestItems.Contains(itemDataToRemove))
                    southwestItems.Remove(itemDataToRemove);
                if (southwestGroundItems.Contains(itemDataToRemove))
                    southwestGroundItems.Remove(itemDataToRemove);
                break;
            case Direction.Southeast:
                if (southeastItems.Contains(itemDataToRemove))
                    southeastItems.Remove(itemDataToRemove);
                if (southeastGroundItems.Contains(itemDataToRemove))
                    southeastGroundItems.Remove(itemDataToRemove);
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

    public List<Inventory> GetInventoriesListFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                return playerPositionInventories;
            case Direction.North:
                return northInventories;
            case Direction.South:
                return southInventories;
            case Direction.West:
                return westInventories;
            case Direction.East:
                return eastInventories;
            case Direction.Northwest:
                return northwestInventories;
            case Direction.Northeast:
                return northeastInventories;
            case Direction.Southwest:
                return southwestInventories;
            case Direction.Southeast:
                return southeastInventories;
            default:
                return null;
        }
    }

    public Inventory GetDirectionalInventory(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                return playerPositionInventory;
            case Direction.North:
                return northInventory;
            case Direction.South:
                return southInventory;
            case Direction.West:
                return westInventory;
            case Direction.East:
                return eastInventory;
            case Direction.Northwest:
                return northwestInventory;
            case Direction.Northeast:
                return northeastInventory;
            case Direction.Southwest:
                return southwestInventory;
            case Direction.Southeast:
                return southeastInventory;
            default:
                return null;
        }
    }

    public List<ItemData> GetGroundItemsListFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                return playerPositionGroundItems;
            case Direction.North:
                return northGroundItems;
            case Direction.South:
                return southGroundItems;
            case Direction.West:
                return westGroundItems;
            case Direction.East:
                return eastGroundItems;
            case Direction.Northwest:
                return northwestGroundItems;
            case Direction.Northeast:
                return northeastGroundItems;
            case Direction.Southwest:
                return southwestGroundItems;
            case Direction.Southeast:
                return southeastGroundItems;
            default:
                return null;
        }
    }

    public void AddItemToGroundItemsListFromDirection(ItemData itemData, Direction direction)
    {
        switch (direction)
        {
            case Direction.Center:
                playerPositionGroundItems.Add(itemData);
                break;
            case Direction.North:
                northGroundItems.Add(itemData);
                break;
            case Direction.South:
                southGroundItems.Add(itemData);
                break;
            case Direction.West:
                westGroundItems.Add(itemData);
                break;
            case Direction.East:
                eastGroundItems.Add(itemData);
                break;
            case Direction.Northwest:
                northwestGroundItems.Add(itemData);
                break;
            case Direction.Northeast:
                northeastGroundItems.Add(itemData);
                break;
            case Direction.Southwest:
                southwestGroundItems.Add(itemData);
                break;
            case Direction.Southeast:
                southeastGroundItems.Add(itemData);
                break;
            default:
                break;
        }
    }

    public void OnPlayerMoved()
    {
        ResetContainerIcons(Direction.Center);
        GetItemsAroundPlayer();
        PopulateInventoryUI(gm.containerInvUI.playerPositionItems, Direction.Center);
        playerPositionSideBarButton.HighlightDirectionIcon();
    }

    public void RemoveBagFromGround(Inventory bagsInventory)
    {
        List<Inventory> inventoriesList = GetInventoriesListFromDirection(activeDirection);
        inventoriesList.Remove(bagsInventory);

        if (inventoriesList.Count > 0)
        {
            activeInventory = inventoriesList[0];
            AssignDirectionalInventory(activeDirection, inventoriesList[0]);
            PopulateDirectionalItemsList(activeInventory, activeDirection);
            StartCoroutine(DelayPopulateInventoryUI(GetItemsListFromActiveDirection(), activeDirection));
        }
        else
        {
            activeInventory = null;
            RemoveDirectionalInventory(activeDirection);
            inventoryCycler.Hide();
            PopulateDirectionalItemsList(GetGroundItemsListFromDirection(activeDirection), activeDirection);
            StartCoroutine(DelayPopulateInventoryUI(GetItemsListFromActiveDirection(), activeDirection));
            GetSideBarButtonFromDirection(activeDirection).icon.sprite = floorIconSprite;
        }
    }

    public void PopulateDirectionalItemsList(Inventory inventory, Direction direction)
    {
        List<ItemData> directionalItemsList = GetItemsListFromDirection(direction);
        directionalItemsList.Clear();

        if (inventory.CompareTag("Item Pickup"))
            directionalItemsList.Add(inventory.myItemData);
        else
        {
            for (int i = 0; i < inventory.items.Count; i++)
            {
                directionalItemsList.Add(inventory.items[i]);
            }
        }
    }

    public void PopulateDirectionalItemsList(List<ItemData> groundItemsList, Direction direction)
    {
        List<ItemData> directionalItemsList = GetItemsListFromDirection(direction);
        directionalItemsList.Clear();

        for (int i = 0; i < groundItemsList.Count; i++)
        {
            directionalItemsList.Add(groundItemsList[i]);
        }
    }

    public float GetTotalVolumeForTile(Direction direction)
    {
        float totalVolume = 0;
        
        totalVolume += GetTotalVolume(gm.containerInvUI.GetGroundItemsListFromDirection(direction));

        List<Inventory> inventoriesList = gm.containerInvUI.GetInventoriesListFromDirection(direction);
        for (int i = 0; i < inventoriesList.Count; i++)
        {
            totalVolume += GetTotalVolume(inventoriesList[i].items);
        }
        
        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
    }

    void ClearAllLists()
    {
        playerPositionItems.Clear();
        northItems.Clear();
        southItems.Clear();
        westItems.Clear();
        eastItems.Clear();
        northwestItems.Clear();
        northeastItems.Clear();
        southwestItems.Clear();
        southeastItems.Clear();

        playerPositionGroundItems.Clear();
        northGroundItems.Clear();
        southGroundItems.Clear();
        westGroundItems.Clear();
        eastGroundItems.Clear();
        northwestGroundItems.Clear();
        northeastGroundItems.Clear();
        southwestGroundItems.Clear();
        southeastGroundItems.Clear();

        playerPositionInventories.Clear();
        northInventories.Clear();
        southInventories.Clear();
        westInventories.Clear();
        eastInventories.Clear();
        northwestInventories.Clear();
        northeastInventories.Clear();
        southwestInventories.Clear();
        southeastInventories.Clear();

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

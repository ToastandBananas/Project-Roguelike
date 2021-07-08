using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventoryUI : InventoryUI
{
    public TextMeshProUGUI totalWeightText;
    public TextMeshProUGUI totalVolumeText;

    [Header("Inventories")]
    public Inventory personalInventory;
    public Inventory backpackInventory, leftHipPouchInventory, rightHipPouchInventory, quiverInventory, keysInventory;
    
    public bool backpackEquipped, leftHipPouchEquipped, rightHipPouchEquipped, quiverEquipped;

    [Header("Side Bar Buttons")]
    public PlayerInventorySidebarButton personalInventorySideBarButton;
    public PlayerInventorySidebarButton backpackSidebarButton, leftHipPouchSidebarButton, rightHipPouchSidebarButton, quiverSidebarButton, keysSideBarButton, equipmentSideBarButton;

    [HideInInspector] public PlayerInventorySidebarButton activePlayerInvSideBarButton;

    #region Singleton
    public static PlayerInventoryUI instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of PlayerInventoryUI. Fix me!");
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

        inventoryItemObjectPool.Init();

        for (int i = 0; i < inventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            inventoryItemObjectPool.pooledInventoryItems[i].myInvUI = this;
        }

        InitInventories();

        personalInventory.maxWeight = gm.playerManager.playerStats.maxPersonalInvWeight.GetValue();
        personalInventory.maxVolume = gm.playerManager.playerStats.maxPersonalInvVolume.GetValue();

        PopulateInventoryUI(personalInventory.items, PlayerInventoryType.Personal);
    }

    // This method runs when the user clicks on a container side bar icon
    public void PopulateInventoryUI(List<ItemData> itemsList, PlayerInventoryType playerInvType)
    {
        ClearInventoryUI();

        for (int i = 0; i < itemsList.Count; i++)
        {
            InventoryItem invItem = ShowNewInventoryItem(itemsList[i]);
            AssignInventoryOrEquipmentManagerToInventoryItem(invItem, playerInvType);
        }

        // Set header/volume/weight text
        SetUpInventoryUI(playerInvType);
    }

    public void PopulateInventoryUI(ItemData[] currentEquipment, PlayerInventoryType playerInvType)
    {
        ClearInventoryUI();

        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
            {
                InventoryItem invItem = ShowNewInventoryItem(currentEquipment[i]);
                AssignInventoryOrEquipmentManagerToInventoryItem(invItem, playerInvType);
            }
        }

        // Set container open icon sprite (when applicable) and header/volume/weight text
        SetUpInventoryUI(playerInvType);
    }

    public void TransferAll()
    {
        if (gm.containerInvUI.inventoryParent.activeSelf == false)
            gm.containerInvUI.ToggleInventoryMenu();
        else if (gm.containerInvUI.background.activeSelf == false)
            gm.containerInvUI.ToggleMinimization();

        for (int i = 0; i < gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            if (gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems[i].gameObject.activeSelf)
                gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems[i].TransferItem();
        }
    }

    void SetUpInventoryUI(PlayerInventoryType playerInvType)
    {
        switch (playerInvType)
        {
            case PlayerInventoryType.Personal:
                inventoryNameText.text = "Personal Inventory";
                weightText.text = GetTotalWeight(personalInventory.items).ToString() + "/" + gm.playerManager.playerStats.maxPersonalInvWeight.GetValue().ToString();
                volumeText.text = GetTotalVolume(personalInventory.items).ToString() + "/" + gm.playerManager.playerStats.maxPersonalInvVolume.GetValue().ToString();
                activeInventory = personalInventory;
                activePlayerInvSideBarButton = personalInventorySideBarButton;
                break;
            case PlayerInventoryType.Backpack:
                inventoryNameText.text = "Backpack Inventory";
                weightText.text = GetTotalWeight(backpackInventory.items).ToString() + "/" + backpackInventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(backpackInventory.items).ToString() + "/" + backpackInventory.maxVolume.ToString();
                activeInventory = backpackInventory;
                activePlayerInvSideBarButton = backpackSidebarButton;
                break;
            case PlayerInventoryType.LeftHipPouch:
                inventoryNameText.text = "Left Hip Pouch Inventory";
                weightText.text = GetTotalWeight(leftHipPouchInventory.items).ToString() + "/" + leftHipPouchInventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(leftHipPouchInventory.items).ToString() + "/" + leftHipPouchInventory.maxVolume.ToString();
                activeInventory = leftHipPouchInventory;
                activePlayerInvSideBarButton = leftHipPouchSidebarButton;
                break;
            case PlayerInventoryType.RightHipPouch:
                inventoryNameText.text = "Right Hip Pouch Inventory";
                weightText.text = GetTotalWeight(rightHipPouchInventory.items).ToString() + "/" + rightHipPouchInventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(rightHipPouchInventory.items).ToString() + "/" + rightHipPouchInventory.maxVolume.ToString();
                activeInventory = rightHipPouchInventory;
                activePlayerInvSideBarButton = rightHipPouchSidebarButton;
                break;
            case PlayerInventoryType.Quiver:
                inventoryNameText.text = "Quiver Inventory";
                weightText.text = GetTotalWeight(quiverInventory.items).ToString() + "/" + quiverInventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(quiverInventory.items).ToString() + "/" + quiverInventory.maxVolume.ToString();
                activeInventory = quiverInventory;
                activePlayerInvSideBarButton = quiverSidebarButton;
                break;
            case PlayerInventoryType.Keys:
                inventoryNameText.text = "Keys";
                weightText.text = GetTotalWeight(keysInventory.items).ToString();
                volumeText.text = GetTotalVolume(keysInventory.items).ToString();
                activeInventory = keysInventory;
                activePlayerInvSideBarButton = keysSideBarButton;
                break;
            case PlayerInventoryType.EquippedItems:
                inventoryNameText.text = "Equipped Items";
                weightText.text = GetTotalWeight(gm.playerManager.equipmentManager.currentEquipment).ToString();
                volumeText.text = GetTotalVolume(gm.playerManager.equipmentManager.currentEquipment).ToString();
                activeInventory = null;
                activePlayerInvSideBarButton = equipmentSideBarButton;
                break;
            default:
                break;
        }

        totalWeightText.text = GetTotalCarriedWeight().ToString();
        totalVolumeText.text = GetTotalCarriedVolume().ToString();

        // Setup the scrollbar
        if (inventoryItemObjectPool.activePooledInventoryItems.Count > maxInvItems)
        {
            scrollbar.value = 1;
            invItemsParentRectTransform.offsetMin = new Vector2(invItemsParentRectTransform.offsetMin.x, (inventoryItemObjectPool.activePooledInventoryItems.Count - maxInvItems) * -invItemHeight);
        }
    }

    public PlayerInventorySidebarButton GetPlayerInvSidebarButtonFromActiveInv()
    {
        if (activeInventory == personalInventory)
            return personalInventorySideBarButton;
        else if (activeInventory == backpackInventory)
            return backpackSidebarButton;
        else if (activeInventory == leftHipPouchInventory)
            return leftHipPouchSidebarButton;
        else if (activeInventory == rightHipPouchInventory)
            return rightHipPouchSidebarButton;
        else
            return null;
    }

    void AssignInventoryOrEquipmentManagerToInventoryItem(InventoryItem invItem, PlayerInventoryType playerInvType)
    {
        switch (playerInvType)
        {
            case PlayerInventoryType.Personal:
                invItem.myEquipmentManager = null;
                invItem.myInventory = personalInventory;
                break;
            case PlayerInventoryType.Backpack:
                invItem.myEquipmentManager = null;
                invItem.myInventory = backpackInventory;
                break;
            case PlayerInventoryType.LeftHipPouch:
                invItem.myEquipmentManager = null;
                invItem.myInventory = leftHipPouchInventory;
                break;
            case PlayerInventoryType.RightHipPouch:
                invItem.myEquipmentManager = null;
                invItem.myInventory = rightHipPouchInventory;
                break;
            case PlayerInventoryType.Quiver:
                invItem.myEquipmentManager = null;
                invItem.myInventory = quiverInventory;
                break;
            case PlayerInventoryType.Keys:
                invItem.myEquipmentManager = null;
                invItem.myInventory = keysInventory;
                break;
            case PlayerInventoryType.EquippedItems:
                invItem.myEquipmentManager = gm.playerManager.playerEquipmentManager;
                invItem.myInventory = null;
                break;
            default:
                break;
        }
    }

    public Inventory GetInventoryFromBagEquipSlot(ItemData bagItemData)
    {
        Bag bag = (Bag)bagItemData.item;
        switch (bag.equipmentSlot)
        {
            case EquipmentSlot.Quiver:
                return quiverInventory;
            case EquipmentSlot.Backpack:
                return backpackInventory;
            case EquipmentSlot.LeftHipPouch:
                return leftHipPouchInventory;
            case EquipmentSlot.RightHipPouch:
                return rightHipPouchInventory;
            default:
                return null;
        }
    }

    public void EquipBag(ItemData bagItemData)
    {
        Bag bag = (Bag)bagItemData.item;
        switch (bag.equipmentSlot)
        {
            case EquipmentSlot.Quiver:
                quiverSidebarButton.ShowSideBarButton(bag);
                break;
            case EquipmentSlot.Backpack:
                backpackSidebarButton.ShowSideBarButton(bag);
                break;
            case EquipmentSlot.LeftHipPouch:
                leftHipPouchSidebarButton.ShowSideBarButton(bag);
                break;
            case EquipmentSlot.RightHipPouch:
                rightHipPouchSidebarButton.ShowSideBarButton(bag);
                break;
            default:
                break;
        }
        
        Inventory bagInv = GetInventoryFromBagEquipSlot(bagItemData);
        bagInv.maxWeight = bag.maxWeight;
        bagInv.maxVolume = bag.maxVolume;
        bagInv.singleItemVolumeLimit = bag.singleItemVolumeLimit;
    }

    public void UnequipBag(Bag bag, Inventory bagInv)
    {
        switch (bag.equipmentSlot)
        {
            case EquipmentSlot.Quiver:
                quiverSidebarButton.HideSideBarButton();
                break;
            case EquipmentSlot.Backpack:
                backpackSidebarButton.HideSideBarButton();
                break;
            case EquipmentSlot.LeftHipPouch:
                leftHipPouchSidebarButton.HideSideBarButton();
                break;
            case EquipmentSlot.RightHipPouch:
                rightHipPouchSidebarButton.HideSideBarButton();
                break;
            default:
                break;
        }
        
        bagInv.ResetWeightAndVolume();
        bagInv.items.Clear();
    }

    public void TemporarilyDisableBag(ItemData bagItemData)
    {
        Bag bag = (Bag)bagItemData.item;
        switch (bag.equipmentSlot)
        {
            case EquipmentSlot.Quiver:
                quiverEquipped = false;
                break;
            case EquipmentSlot.Backpack:
                backpackEquipped = false;
                break;
            case EquipmentSlot.LeftHipPouch:
                leftHipPouchEquipped = false;
                break;
            case EquipmentSlot.RightHipPouch:
                rightHipPouchEquipped = false;
                break;
            default:
                break;
        }
    }

    public void ReenableBag(ItemData bagItemData)
    {
        Bag bag = (Bag)bagItemData.item;
        switch (bag.equipmentSlot)
        {
            case EquipmentSlot.Quiver:
                quiverEquipped = true;
                break;
            case EquipmentSlot.Backpack:
                backpackEquipped = true;
                break;
            case EquipmentSlot.LeftHipPouch:
                leftHipPouchEquipped = true;
                break;
            case EquipmentSlot.RightHipPouch:
                rightHipPouchEquipped = true;
                break;
            default:
                break;
        }
    }

    public override void UpdateUI()
    {
        if (activeInventory != null)
        {
            if (activeInventory == keysInventory)
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
            weightText.text = GetTotalWeight(gm.playerManager.playerEquipmentManager.currentEquipment).ToString();
            volumeText.text = GetTotalVolume(gm.playerManager.playerEquipmentManager.currentEquipment).ToString();
        }

        totalWeightText.text = GetTotalCarriedWeight().ToString();
        totalVolumeText.text = GetTotalCarriedVolume().ToString();
    }

    float GetTotalCarriedWeight()
    {
        float totalWeight = 0;
        totalWeight += personalInventory.currentWeight;

        if (backpackInventory != null)
            totalWeight += backpackInventory.currentWeight;

        if (leftHipPouchInventory != null)
            totalWeight += leftHipPouchInventory.currentWeight;

        if (rightHipPouchInventory != null)
            totalWeight += rightHipPouchInventory.currentWeight;

        if (quiverInventory != null)
            totalWeight += quiverInventory.currentWeight;

        if (keysInventory != null)
            totalWeight += keysInventory.currentWeight;
        
        totalWeight += gm.playerManager.equipmentManager.currentWeight;

        return Mathf.RoundToInt(totalWeight * 100f) / 100f;
    }

    float GetTotalCarriedVolume()
    {
        float totalVolume = 0;
        totalVolume += personalInventory.currentVolume;

        if (backpackInventory != null)
            totalVolume += backpackInventory.currentVolume;

        if (leftHipPouchInventory != null)
            totalVolume += leftHipPouchInventory.currentVolume;

        if (rightHipPouchInventory != null)
            totalVolume += rightHipPouchInventory.currentVolume;

        if (quiverInventory != null)
            totalVolume += quiverInventory.currentVolume;

        if (keysInventory != null)
            totalVolume += keysInventory.currentVolume;

        totalVolume += gm.playerManager.equipmentManager.currentVolume;

        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
    }

    public bool ItemIsInABag(ItemData itemData)
    {
        if ((backpackEquipped && backpackInventory.items.Contains(itemData)) || (leftHipPouchEquipped && leftHipPouchInventory.items.Contains(itemData))
            || (rightHipPouchEquipped && rightHipPouchInventory.items.Contains(itemData)) || (quiverEquipped && quiverInventory.items.Contains(itemData)))
            return true;

        return false;
    }

    void InitInventories()
    {
        personalInventory.Init();
        backpackInventory.Init();
        leftHipPouchInventory.Init();
        rightHipPouchInventory.Init();
        quiverInventory.Init();
        keysInventory.Init();
    }
}

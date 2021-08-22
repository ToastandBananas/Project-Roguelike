using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventoryUI : InventoryUI
{
    public TextMeshProUGUI totalWeightText;
    public TextMeshProUGUI totalVolumeText;

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

        PopulateInventoryUI(gm.playerManager.personalInventory.items, PlayerInventoryType.Personal);
    }

    // This method runs when the user clicks on an inventory side bar icon
    public void PopulateInventoryUI(List<ItemData> itemsList, PlayerInventoryType playerInvType)
    {
        ClearInventoryUI();

        for (int i = 0; i < itemsList.Count; i++)
        {
            InventoryItem invItem = ShowNewInventoryItem(itemsList[i]);
            AssignInventoryOrEquipmentManagerToInventoryItem(invItem, playerInvType);
        }

        if (playerInvType == PlayerInventoryType.Personal)
        {
            for (int i = 0; i < gm.playerManager.carriedItems.Count; i++)
            {
                InventoryItem invItem = ShowNewInventoryItem(gm.playerManager.carriedItems[i]);
                AssignInventoryOrEquipmentManagerToInventoryItem(invItem, playerInvType);
            }
        }

        // Set header/volume/weight text
        SetUpInventoryUI(playerInvType);
    }

    // This method runs when the user clicks on the equipment manager side bar icon
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
                if (gm.playerManager.carriedItems.Count == 0)
                    weightText.text = GetTotalWeight(gm.playerManager.personalInventory.items).ToString();
                else
                    weightText.text = GetTotalWeight(gm.playerManager.personalInventory.items).ToString() + " (+" + GetTotalWeight(gm.playerManager.carriedItems) + ")";

                if (gm.playerManager.carriedItems.Count == 0)
                    volumeText.text = GetTotalVolume(gm.playerManager.personalInventory.items).ToString() + "/" + gm.playerManager.personalInventory.maxVolume.ToString();
                else
                    volumeText.text = GetTotalVolume(gm.playerManager.personalInventory.items).ToString() + "/" + gm.playerManager.personalInventory.maxVolume.ToString() 
                        + " (+" + GetTotalVolume(gm.playerManager.carriedItems) + ")";

                activeInventory = gm.playerManager.personalInventory;
                activePlayerInvSideBarButton = personalInventorySideBarButton;
                break;
            case PlayerInventoryType.Backpack:
                activeInventory = gm.playerManager.backpackInventory;
                activePlayerInvSideBarButton = backpackSidebarButton;
                break;
            case PlayerInventoryType.LeftHipPouch:
                activeInventory = gm.playerManager.leftHipPouchInventory;
                activePlayerInvSideBarButton = leftHipPouchSidebarButton;
                break;
            case PlayerInventoryType.RightHipPouch:
                activeInventory = gm.playerManager.rightHipPouchInventory;
                activePlayerInvSideBarButton = rightHipPouchSidebarButton;
                break;
            case PlayerInventoryType.Quiver:
                activeInventory = gm.playerManager.quiverInventory;
                activePlayerInvSideBarButton = quiverSidebarButton;
                break;
            case PlayerInventoryType.Keys:
                inventoryNameText.text = "Keys";
                weightText.text = GetTotalWeight(gm.playerManager.keysInventory.items).ToString();
                volumeText.text = GetTotalVolume(gm.playerManager.keysInventory.items).ToString();
                activeInventory = gm.playerManager.keysInventory;
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

        if (activeInventory != null)
        {
            activeInventory.myInvUI = this;

            if (playerInvType != PlayerInventoryType.Personal && playerInvType != PlayerInventoryType.Keys)
            {
                inventoryNameText.text = activeInventory.myItemData.GetItemName(1);
                weightText.text = GetTotalWeight(activeInventory.items).ToString() + "/" + activeInventory.maxWeight.ToString();
                volumeText.text = GetTotalVolume(activeInventory.items).ToString() + "/" + activeInventory.maxVolume.ToString();
            }
        }

        totalWeightText.text = GetTotalCarriedWeight().ToString();
        totalVolumeText.text = GetTotalCarriedVolume().ToString();

        // Setup the scrollbar
        if (inventoryItemObjectPool.activePooledInventoryItems.Count > MaxInvItems())
        {
            scrollbar.value = 1;
            invItemsParentRectTransform.offsetMin = new Vector2(invItemsParentRectTransform.offsetMin.x, (inventoryItemObjectPool.activePooledInventoryItems.Count - MaxInvItems()) * -InvItemHeight());
        }
    }

    public PlayerInventorySidebarButton GetPlayerInvSidebarButtonFromActiveInv()
    {
        if (activeInventory == gm.playerManager.personalInventory)
            return personalInventorySideBarButton;
        else if (activeInventory == gm.playerManager.backpackInventory)
            return backpackSidebarButton;
        else if (activeInventory == gm.playerManager.leftHipPouchInventory)
            return leftHipPouchSidebarButton;
        else if (activeInventory == gm.playerManager.rightHipPouchInventory)
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
                invItem.myInventory = gm.playerManager.personalInventory;
                break;
            case PlayerInventoryType.Backpack:
                invItem.myEquipmentManager = null;
                invItem.myInventory = gm.playerManager.backpackInventory;
                break;
            case PlayerInventoryType.LeftHipPouch:
                invItem.myEquipmentManager = null;
                invItem.myInventory = gm.playerManager.leftHipPouchInventory;
                break;
            case PlayerInventoryType.RightHipPouch:
                invItem.myEquipmentManager = null;
                invItem.myInventory = gm.playerManager.rightHipPouchInventory;
                break;
            case PlayerInventoryType.Quiver:
                invItem.myEquipmentManager = null;
                invItem.myInventory = gm.playerManager.quiverInventory;
                break;
            case PlayerInventoryType.Keys:
                invItem.myEquipmentManager = null;
                invItem.myInventory = gm.playerManager.keysInventory;
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
                return gm.playerManager.quiverInventory;
            case EquipmentSlot.Backpack:
                return gm.playerManager.backpackInventory;
            case EquipmentSlot.LeftHipPouch:
                return gm.playerManager.leftHipPouchInventory;
            case EquipmentSlot.RightHipPouch:
                return gm.playerManager.rightHipPouchInventory;
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

    public void TemporarilyDisableBag(CharacterManager characterManager, ItemData bagItemData)
    {
        Bag bag = (Bag)bagItemData.item;
        switch (bag.equipmentSlot)
        {
            case EquipmentSlot.Backpack:
                characterManager.backpackInventory = null;
                break;
            case EquipmentSlot.LeftHipPouch:
                characterManager.leftHipPouchInventory = null;
                break;
            case EquipmentSlot.RightHipPouch:
                characterManager.rightHipPouchInventory = null;
                break;
            case EquipmentSlot.Quiver:
                characterManager.quiverInventory = null;
                break;
            default:
                break;
        }
    }

    public void ReenableBag(CharacterManager characterManager, ItemData bagItemData)
    {
        Bag bag = (Bag)bagItemData.item;
        switch (bag.equipmentSlot)
        {
            case EquipmentSlot.Backpack:
                characterManager.backpackInventory = bagItemData.bagInventory;
                break;
            case EquipmentSlot.LeftHipPouch:
                characterManager.leftHipPouchInventory = bagItemData.bagInventory;
                break;
            case EquipmentSlot.RightHipPouch:
                characterManager.rightHipPouchInventory = bagItemData.bagInventory;
                break;
            case EquipmentSlot.Quiver:
                characterManager.quiverInventory = bagItemData.bagInventory;
                break;
            default:
                break;
        }
    }

    public override void UpdateUI()
    {
        if (activeInventory != null)
        {
            if (activeInventory == gm.playerManager.personalInventory)
            {
                if (gm.playerManager.carriedItems.Count == 0)
                    weightText.text = (Mathf.RoundToInt(activeInventory.currentWeight * 100f) / 100f).ToString();
                else
                    weightText.text = (Mathf.RoundToInt(activeInventory.currentWeight * 100f) / 100f).ToString() + " (" + GetTotalWeight(gm.playerManager.carriedItems) + ")";

                if (gm.playerManager.carriedItems.Count == 0)
                    volumeText.text = (Mathf.RoundToInt(activeInventory.currentVolume * 100f) / 100f).ToString() + "/" + activeInventory.maxVolume.ToString();
                else
                    volumeText.text = (Mathf.RoundToInt(activeInventory.currentVolume * 100f) / 100f).ToString() + "/" + activeInventory.maxVolume.ToString()
                        + " (" + GetTotalVolume(gm.playerManager.carriedItems) + ")";
            }
            else if (activeInventory == gm.playerManager.keysInventory)
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
        totalWeight += gm.playerManager.personalInventory.currentWeight;

        if (gm.playerManager.backpackInventory != null)
            totalWeight += gm.playerManager.backpackInventory.currentWeight;

        if (gm.playerManager.leftHipPouchInventory != null)
            totalWeight += gm.playerManager.leftHipPouchInventory.currentWeight;

        if (gm.playerManager.rightHipPouchInventory != null)
            totalWeight += gm.playerManager.rightHipPouchInventory.currentWeight;

        if (gm.playerManager.quiverInventory != null)
            totalWeight += gm.playerManager.quiverInventory.currentWeight;

        if (gm.playerManager.keysInventory != null)
            totalWeight += gm.playerManager.keysInventory.currentWeight;
        
        totalWeight += gm.playerManager.equipmentManager.currentWeight;

        return Mathf.RoundToInt(totalWeight * 100f) / 100f;
    }

    float GetTotalCarriedVolume()
    {
        float totalVolume = 0;
        totalVolume += gm.playerManager.personalInventory.currentVolume;

        if (gm.playerManager.backpackInventory != null)
            totalVolume += gm.playerManager.backpackInventory.currentVolume;

        if (gm.playerManager.leftHipPouchInventory != null)
            totalVolume += gm.playerManager.leftHipPouchInventory.currentVolume;

        if (gm.playerManager.rightHipPouchInventory != null)
            totalVolume += gm.playerManager.rightHipPouchInventory.currentVolume;

        if (gm.playerManager.quiverInventory != null)
            totalVolume += gm.playerManager.quiverInventory.currentVolume;

        if (gm.playerManager.keysInventory != null)
            totalVolume += gm.playerManager.keysInventory.currentVolume;

        totalVolume += gm.playerManager.equipmentManager.currentVolume;

        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
    }

    public bool ItemIsInABag(ItemData itemData)
    {
        if ((gm.playerManager.backpackInventory != null && gm.playerManager.backpackInventory.items.Contains(itemData)) || (gm.playerManager.leftHipPouchInventory != null && gm.playerManager.leftHipPouchInventory.items.Contains(itemData))
            || (gm.playerManager.rightHipPouchInventory != null && gm.playerManager.rightHipPouchInventory.items.Contains(itemData)) || (gm.playerManager.quiverInventory != null && gm.playerManager.quiverInventory.items.Contains(itemData)))
            return true;
        return false;
    }
}

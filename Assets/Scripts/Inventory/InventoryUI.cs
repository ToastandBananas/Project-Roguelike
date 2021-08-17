using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Transform slotsParent;
    public GameObject inventoryParent, background, sideBarParent, minimizeButtonText;
    public RectTransform invItemsParentRectTransform;
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    public InventoryItemObjectPool inventoryItemObjectPool;

    [Header("Texts")]
    public TextMeshProUGUI inventoryNameText;
    public TextMeshProUGUI weightText, volumeText;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public Inventory activeInventory;
    [HideInInspector] public bool isActive, isMinimized, scrollbarSelected;

    [HideInInspector] public int maxInvItems = 15;
    [HideInInspector] public int invItemHeight = 32;

    int addItemEffectsPlayCount;

    public virtual void Start()
    {
        gm = GameManager.instance;

        if (inventoryParent.activeSelf)
            inventoryParent.SetActive(false);
    }

    public void ClearInventoryUI()
    {
        for (int i = 0; i < inventoryItemObjectPool.pooledInventoryItems.Count; i++)
        {
            if (inventoryItemObjectPool.pooledInventoryItems[i].gameObject.activeSelf)
            {
                inventoryItemObjectPool.pooledInventoryItems[i].gameObject.SetActive(false);
                inventoryItemObjectPool.pooledInventoryItems[i].ResetInvItem();
            }
        }

        inventoryItemObjectPool.activePooledInventoryItems.Clear();
        ResetInventoryItemsParentHeight();
    }

    public InventoryItem ShowNewInventoryItem(ItemData newItemData)
    {
        InventoryItem invItem = inventoryItemObjectPool.GetPooledInventoryItem();
        invItem.itemData = newItemData;
        invItem.UpdateAllItemTexts();
        invItem.myInventory = activeInventory;
        invItem.myInvUI = this;

        if (activeInventory == null && this == gm.playerInvUI)
        {
            invItem.myEquipmentManager = gm.playerManager.playerEquipmentManager;
            invItem.transform.SetSiblingIndex((int)invItem.myEquipmentManager.GetEquipmentSlotFromItemData(invItem.itemData));
        }
        
        if (activeInventory != null)
            activeInventory.myInvUI = this;

        if (invItem.itemData.bagInventory != null)
            invItem.itemData.bagInventory.myInvUI = this;

        invItem.gameObject.SetActive(true);
        invItem.originalSiblingIndex = invItem.transform.GetSiblingIndex();

        if (newItemData.item.IsBag())
        {
            Bag bag = (Bag)newItemData.item;
            invItem.itemData.bagInventory.maxWeight = bag.maxWeight;
            invItem.itemData.bagInventory.maxVolume = bag.maxVolume;
            invItem.itemData.bagInventory.singleItemVolumeLimit = bag.singleItemVolumeLimit;
        }
        else if (newItemData.item.IsPortableContainer())
        {
            PortableContainer portableContainer = (PortableContainer)newItemData.item;
            invItem.itemData.bagInventory.maxWeight = portableContainer.maxWeight;
            invItem.itemData.bagInventory.maxVolume = portableContainer.maxVolume;
            invItem.itemData.bagInventory.singleItemVolumeLimit = portableContainer.singleItemVolumeLimit;
        }

        if (inventoryItemObjectPool.activePooledInventoryItems.Count > maxInvItems)
            EditInventoryItemsParentHeight(-invItemHeight);

        if ((newItemData.item.itemType == ItemType.Bag || newItemData.item.itemType == ItemType.Container) && gm.playerManager.playerEquipmentManager.ItemIsEquipped(newItemData) == false && invItem.disclosureWidget != null)
            invItem.disclosureWidget.EnableDisclosureWidget();

        return invItem;
    }

    public InventoryItem ShowNewBagItem(ItemData newItemData, InventoryItem bagInvItem)
    {
        InventoryItem invItem = ShowNewInventoryItem(newItemData);
        if (bagInvItem.disclosureWidget.isExpanded)
            bagInvItem.disclosureWidget.expandedItems.Add(invItem);
        invItem.backgroundImage.sprite = invItem.blueHighlightedSprite;
        invItem.isItemInsideBag = true;
        invItem.myInventory = bagInvItem.itemData.bagInventory;
        invItem.parentInvItem = bagInvItem;
        invItem.transform.SetSiblingIndex(bagInvItem.transform.GetSiblingIndex() + 1);
        invItem.originalSiblingIndex = invItem.transform.GetSiblingIndex();

        return invItem;
    }

    public virtual void UpdateUI()
    {
        // This is just meant to be overridden
    }

    public void ToggleInventoryMenu()
    {
        inventoryParent.SetActive(!inventoryParent.activeSelf);

        // If the Inventory menu was closed
        if (inventoryParent.activeSelf == false)
        {
            isActive = false;
            isMinimized = false;
            DeselectScrollbar();

            if (gm.uiManager.activeInvUI == this)
            {
                gm.uiManager.ResetSelections();
                gm.uiManager.activeInvUI = null;
            }

            // Close the context menu, stackSizeSelector and any active tooltips
            gm.uiManager.DisableInventoryUIComponents();

            if (gm.containerInvUI == this)
                gm.uiManager.activeContainerSideBarButton = null;
            else if (gm.playerInvUI == this)
                gm.uiManager.activePlayerInvSideBarButton = null;
        }
        else // If the Inventory menu was opened
        {
            isActive = true;
            isMinimized = false;

            if (background.activeSelf == false)
            {
                // Make sure these are active, in case the inventory had been minimized previously
                background.SetActive(true);
                sideBarParent.SetActive(true);
                minimizeButtonText.transform.rotation = Quaternion.Euler(0, 0, 180);
                isActive = true;
                isMinimized = false;
            }
        }
    }

    public void ToggleMinimization()
    {
        background.SetActive(!background.activeSelf);
        sideBarParent.SetActive(!sideBarParent.activeSelf);

        // If the inventory was minimized
        if (background.activeSelf == false)
        {
            DeselectScrollbar();

            isMinimized = true;
            minimizeButtonText.transform.rotation = Quaternion.Euler(Vector3.zero);

            // Close the context menu, stackSizeSelector and any active tooltips
            gm.uiManager.DisableInventoryUIComponents();
        }
        else
        {
            isMinimized = false;
            minimizeButtonText.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    public float GetTotalWeight(List<ItemData> itemsList)
    {
        float totalWeight = 0f;
        for (int i = 0; i < itemsList.Count; i++)
        {
            totalWeight += Mathf.RoundToInt(itemsList[i].item.weight * itemsList[i].currentStackSize * 100f) / 100f;
            if (itemsList[i].item.IsBag() || itemsList[i].item.IsPortableContainer())
            {
                if (itemsList[i].item.IsBag() && itemsList[i].IsPickup())
                    totalWeight -= Mathf.RoundToInt(itemsList[i].item.weight * itemsList[i].currentStackSize * 100f) / 100f;

                for (int j = 0; j < itemsList[i].bagInventory.items.Count; j++)
                {
                    totalWeight += Mathf.RoundToInt(itemsList[i].bagInventory.items[j].item.weight * itemsList[i].bagInventory.items[j].currentStackSize * 100f) / 100f;

                    if (itemsList[i].bagInventory.items[j].item.IsBag() || itemsList[i].bagInventory.items[j].item.IsPortableContainer())
                    {
                        for (int k = 0; k < itemsList[i].bagInventory.items[j].bagInventory.items.Count; k++)
                        {
                            totalWeight += Mathf.RoundToInt(itemsList[i].bagInventory.items[j].bagInventory.items[k].item.weight * itemsList[i].bagInventory.items[j].bagInventory.items[k].currentStackSize * 100f) / 100f;
                        }
                    }
                }
            }
        }

        return Mathf.RoundToInt(totalWeight * 100f) / 100f;
    }

    public float GetTotalWeight(ItemData[] currentEquipment)
    {
        float totalWeight = 0f;
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
                totalWeight += Mathf.RoundToInt(currentEquipment[i].item.weight * currentEquipment[i].currentStackSize * 100f) / 100f;
        }

        return Mathf.RoundToInt(totalWeight * 100f) / 100f;
    }

    public float GetTotalVolume(List<ItemData> itemsList)
    {
        float totalVolume = 0f;
        for (int i = 0; i < itemsList.Count; i++)
        {
            totalVolume += Mathf.RoundToInt(itemsList[i].item.volume * itemsList[i].currentStackSize * 100f) / 100f;
            if (itemsList[i].item.IsBag() || itemsList[i].item.itemType == ItemType.Container)
            {
                if (itemsList[i].item.IsBag() && itemsList[i].IsPickup())
                    totalVolume -= Mathf.RoundToInt(itemsList[i].item.volume * itemsList[i].currentStackSize * 100f) / 100f;

                for (int j = 0; j < itemsList[i].bagInventory.items.Count; j++)
                {
                    totalVolume += Mathf.RoundToInt(itemsList[i].bagInventory.items[j].item.volume * itemsList[i].bagInventory.items[j].currentStackSize * 100f) / 100f;

                    if (itemsList[i].bagInventory.items[j].item.IsBag() || itemsList[i].bagInventory.items[j].item.IsPortableContainer())
                    {
                        for (int k = 0; k < itemsList[i].bagInventory.items[j].bagInventory.items.Count; k++)
                        {
                            totalVolume += Mathf.RoundToInt(itemsList[i].bagInventory.items[j].bagInventory.items[k].item.volume * itemsList[i].bagInventory.items[j].bagInventory.items[k].currentStackSize * 100f) / 100f;
                        }
                    }
                }
            }
        }

        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
    }

    public float GetTotalVolume(ItemData[] currentEquipment)
    {
        float totalVolume = 0f;
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
                totalVolume += Mathf.RoundToInt(currentEquipment[i].item.volume * currentEquipment[i].currentStackSize * 100f) / 100f;
        }

        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
    }

    public IEnumerator PlayAddItemEffect(Sprite itemSprite, ContainerSideBarButton containerSideBarButton, PlayerInventorySidebarButton playerInvSideBarButton)
    {
        addItemEffectsPlayCount++;
        AddItemEffect addItemEffect = gm.objectPoolManager.addItemEffectObjectPool.GetPooledAddItemEffect();
        addItemEffect.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f * (addItemEffectsPlayCount - 1));

        if ((containerSideBarButton != null && containerSideBarButton.gameObject.activeInHierarchy) || (playerInvSideBarButton != null && playerInvSideBarButton.gameObject.activeInHierarchy))
        {
            if (containerSideBarButton != null)
                addItemEffect.DoEffect_Left(itemSprite, containerSideBarButton.transform.position.y);
            else
                addItemEffect.DoEffect_Left(itemSprite, playerInvSideBarButton.transform.position.y);

            yield return new WaitForSeconds(addItemEffect.anim.GetCurrentAnimatorStateInfo(0).length);
            addItemEffectsPlayCount--;
        }
        else
        {
            addItemEffect.gameObject.SetActive(false);
            addItemEffectsPlayCount--;
        }
    }

    public InventoryItem GetBagItemFromInventory(Inventory inventory)
    {
        for (int i = 0; i < inventoryItemObjectPool.activePooledInventoryItems.Count; i++)
        {
            if (inventoryItemObjectPool.activePooledInventoryItems[i].myInventory == inventory)
                return inventoryItemObjectPool.activePooledInventoryItems[i];
        }

        return null;
    }

    public void EditInventoryItemsParentHeight(int amount)
    {
        invItemsParentRectTransform.offsetMin = new Vector2(0, invItemsParentRectTransform.offsetMin.y + amount);
    }

    public void ResetInventoryItemsParentHeight()
    {
        invItemsParentRectTransform.offsetMax = Vector2.zero;
        invItemsParentRectTransform.offsetMin = Vector2.zero;
    }

    public void SelectScrollbar()
    {
        scrollbarSelected = true;
    }

    public void DeselectScrollbar()
    {
        scrollbarSelected = false;
    }
}
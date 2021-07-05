using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform slotsParent;
    public GameObject inventoryParent, background, sideBarParent, minimizeButtonText;
    public InventoryItemObjectPool inventoryItemObjectPool;

    [Header("Texts")]
    public TextMeshProUGUI inventoryNameText;
    public TextMeshProUGUI weightText, volumeText;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public Inventory activeInventory;
    [HideInInspector] public bool isActive, isMinimized;

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
    }

    public InventoryItem ShowNewInventoryItem(ItemData newItemData)
    {
        InventoryItem invItem = inventoryItemObjectPool.GetPooledInventoryItem();
        invItem.itemData = newItemData;
        invItem.UpdateAllItemTexts();
        invItem.myInventory = activeInventory;
        invItem.gameObject.SetActive(true);

        if (newItemData.item.itemType == ItemType.Bag || newItemData.item.itemType == ItemType.PortableContainer)
            invItem.disclosureWidget.enabled = true;

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
            totalWeight += itemsList[i].item.weight * itemsList[i].currentStackSize;
            if (itemsList[i].item.IsBag() || itemsList[i].item.itemType == ItemType.PortableContainer)
            {
                if (itemsList[i].CompareTag("Item Pickup"))
                    totalWeight -= itemsList[i].item.weight * itemsList[i].currentStackSize;

                for (int j = 0; j < itemsList[i].bagInventory.items.Count; j++)
                {
                    totalWeight += itemsList[i].bagInventory.items[j].item.weight * itemsList[i].bagInventory.items[j].currentStackSize;
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
            {
                totalWeight += currentEquipment[i].item.weight * currentEquipment[i].currentStackSize;
                if (currentEquipment[i].item.IsBag())
                {
                    for (int j = 0; j < currentEquipment[i].bagInventory.items.Count; j++)
                    {
                        totalWeight += currentEquipment[i].bagInventory.items[j].item.weight * currentEquipment[i].bagInventory.items[j].currentStackSize;
                    }
                }
            }
        }

        return Mathf.RoundToInt(totalWeight * 100f) / 100f;
    }

    public float GetTotalVolume(List<ItemData> itemsList)
    {
        float totalVolume = 0f;
        for (int i = 0; i < itemsList.Count; i++)
        {
            totalVolume += itemsList[i].item.volume * itemsList[i].currentStackSize;
            if (itemsList[i].item.IsBag() || itemsList[i].item.itemType == ItemType.PortableContainer)
            {
                if (itemsList[i].CompareTag("Item Pickup"))
                    totalVolume -= itemsList[i].item.volume * itemsList[i].currentStackSize;

                for (int j = 0; j < itemsList[i].bagInventory.items.Count; j++)
                {
                    totalVolume += itemsList[i].bagInventory.items[j].item.volume * itemsList[i].bagInventory.items[j].currentStackSize;
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
            {
                totalVolume += currentEquipment[i].item.volume * currentEquipment[i].currentStackSize;
                if (currentEquipment[i].item.IsBag())
                {
                    for (int j = 0; j < currentEquipment[i].bagInventory.items.Count; j++)
                    {
                        totalVolume += currentEquipment[i].bagInventory.items[j].item.volume * currentEquipment[i].bagInventory.items[j].currentStackSize;
                    }
                }
            }
        }

        return Mathf.RoundToInt(totalVolume * 100f) / 100f;
    }

    public InventoryItem GetItemDatasInventoryItem(ItemData itemData)
    {
        if (this == gm.playerInvUI)
        {
            for (int i = 0; i < gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems.Count; i++)
            {
                if (gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems[i].itemData == itemData)
                    return gm.objectPoolManager.playerInventoryItemObjectPool.pooledInventoryItems[i];
            }
        }
        else if (this == gm.containerInvUI)
        {
            for (int i = 0; i < gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems.Count; i++)
            {
                if (gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems[i].itemData == itemData)
                    return gm.objectPoolManager.containerInventoryItemObjectPool.pooledInventoryItems[i];
            }
        }

        return null;
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
                addItemEffect.DoEffect_Right(itemSprite, playerInvSideBarButton.transform.position.y);

            yield return new WaitForSeconds(addItemEffect.anim.GetCurrentAnimatorStateInfo(0).length);
            addItemEffectsPlayCount--;
        }
        else
        {
            addItemEffect.gameObject.SetActive(false);
            addItemEffectsPlayCount--;
        }
    }
}
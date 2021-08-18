using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public InventoryTooltip[] invItemTooltips;

    PlayerEquipmentManager playerEquipmentManager;

    #region Singleton
    public static TooltipManager instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of TooltipManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        playerEquipmentManager = PlayerEquipmentManager.instance;

        HideAllTooltips();
    }

    public void ShowInventoryTooltip(InventoryItem invItem)
    {
        if (invItem != null && invItem.itemData != null)
        {
            Tooltip primaryTooltip = invItemTooltips[0];
            primaryTooltip.BuildTooltip(invItem.itemData);
            primaryTooltip.ShowTooltip(Input.mousePosition, false, true);

            ShowMatchingEquipmentTooltips(invItem);
        }
    }

    void ShowMatchingEquipmentTooltips(InventoryItem invItem)
    {
        if (invItem.itemData.item.IsEquipment())
        {
            Equipment equipment = (Equipment)invItem.itemData.item;
            InventoryTooltip firsMatchingTooltip = invItemTooltips[1];
                
            if (playerEquipmentManager.currentEquipment[(int)equipment.equipmentSlot] != null && playerEquipmentManager.currentEquipment[(int)equipment.equipmentSlot] != invItem.itemData)
            {
                firsMatchingTooltip.BuildTooltip(playerEquipmentManager.currentEquipment[(int)equipment.equipmentSlot]);
                firsMatchingTooltip.ShowTooltip(invItemTooltips[0].rectTransform.position + new Vector3(invItemTooltips[0].rectTransform.sizeDelta.x, invItemTooltips[0].rectTransform.sizeDelta.y), false, false);
            }

            // Weapons can potentially have two tooltips since the player can dual wield
            if (equipment.IsWeapon() || equipment.IsShield())
            {
                InventoryTooltip secondMatchingTooltip = null;
                if (equipment.equipmentSlot == EquipmentSlot.LeftHandItem)
                {
                    if (playerEquipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem] != invItem.itemData && playerEquipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem] != null)
                    {
                        secondMatchingTooltip = invItemTooltips[2];
                        secondMatchingTooltip.BuildTooltip(playerEquipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem]);
                    }
                }
                else
                {
                    if (playerEquipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem] != invItem.itemData && playerEquipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem] != null)
                    {
                        secondMatchingTooltip = invItemTooltips[2];
                        secondMatchingTooltip.BuildTooltip(playerEquipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem]);
                    }
                }

                if (secondMatchingTooltip != null)
                {
                    if (invItemTooltips[1].gameObject.activeSelf)
                    {
                        secondMatchingTooltip.ShowTooltip(invItemTooltips[0].rectTransform.position
                            + new Vector3(invItemTooltips[0].rectTransform.sizeDelta.x + invItemTooltips[1].rectTransform.sizeDelta.x, invItemTooltips[0].rectTransform.sizeDelta.y), false, false);
                    }
                    else
                    {
                        secondMatchingTooltip.ShowTooltip(invItemTooltips[0].rectTransform.position
                            + new Vector3(invItemTooltips[0].rectTransform.sizeDelta.x, invItemTooltips[0].rectTransform.sizeDelta.y), false, false);
                    }
                }
            }
        }
    }
    
    public void HideInventoryTooltip(Tooltip tooltip)
    {
        tooltip.gameObject.SetActive(false);
    }

    public void HideAllTooltips()
    {
        for (int i = 0; i < invItemTooltips.Length; i++)
        {
            HideInventoryTooltip(invItemTooltips[i]);
        }
    }
}

using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public InventoryTooltip invItemTooltip;

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
            invItemTooltip.BuildTooltip(invItem.itemData);
            invItemTooltip.ShowTooltip(Input.mousePosition);
            //ShowMatchingEquipmentTooltip(invItem);
        }
    }

    public void HideInventoryTooltip()
    {
        invItemTooltip.gameObject.SetActive(false);
    }

    /*void ShowMatchingEquipmentTooltip(InventoryItem invItem)
    {
        if (invItem.itemData.item.IsEquipment())
        {
            Equipment equipment = (Equipment)invItem.itemData.item;
            if (playerEquipmentManager.currentEquipment[(int)equipment.equipmentSlot] != null)
            {
                InventoryTooltip tooltip = GetNextAvailableInventoryTooltip();

                if (tooltip != null)
                {
                    tooltip.BuildTooltip(playerEquipmentManager.currentEquipment[(int)equipment.equipmentSlot]);
                    tooltip.rectTransform.localPosition = equipmentTooltipPosition;

                    // Weapons can potentially have two tooltips since the player can dual wield
                    if (equipment.IsWeapon())
                    {
                        if (equipment.equipmentSlot == EquipmentSlot.LeftWeapon)
                        {
                            if (playerEquipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon] != null)
                            {
                                InventoryTooltip secondTooltip = GetNextAvailableInventoryTooltip();

                                if (secondTooltip != null)
                                {
                                    secondTooltip.BuildTooltip(playerEquipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon]);
                                    secondTooltip.rectTransform.localPosition = equipmentTooltipPosition;
                                    tooltip.rectTransform.localPosition = secondaryEquipmentTooltipPosition;
                                }
                            }
                        }
                        else
                        {
                            if (playerEquipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon] != null)
                            {
                                InventoryTooltip secondTooltip = GetNextAvailableInventoryTooltip();

                                if (secondTooltip != null)
                                {
                                    secondTooltip.BuildTooltip(playerEquipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon]);
                                    secondTooltip.rectTransform.localPosition = secondaryEquipmentTooltipPosition;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    InventoryTooltip GetNextAvailableInventoryTooltip()
    {
        for (int i = 0; i < invTooltips.Length; i++)
        {
            if (invTooltips[i].gameObject.activeSelf == false)
                return invTooltips[i];
        }
        
        return null;
    }*/

    public void HideAllTooltips()
    {
        HideInventoryTooltip();
    }
}

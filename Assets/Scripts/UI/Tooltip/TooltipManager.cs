using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public InventoryTooltip[] invTooltips;

    PlayerEquipmentManager playerEquipmentManager;

    Vector3 inventoryTooltipPosition = new Vector3(164f, 185f);
    Vector3 equipmentTooltipPosition = new Vector3(-477f, 185f);
    Vector3 secondaryEquipmentTooltipPosition = new Vector3(-794f, 185f);

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

    public void ShowInventoryTooltip(InventoryItemBase slot)
    {
        if (slot != null && slot.itemData != null)
        {
            InventoryTooltip tooltip = GetNextAvailableInventoryTooltip();

            if (tooltip != null)
            {
                tooltip.BuildTooltip(slot.itemData);

                if (slot.IsInventorySlot())
                {
                    tooltip.rectTransform.localPosition = inventoryTooltipPosition;
                    ShowMatchingEquipmentTooltip(slot);
                }
                else
                    tooltip.rectTransform.localPosition = equipmentTooltipPosition;
            }
        }
    }

    void ShowMatchingEquipmentTooltip(InventoryItemBase slot)
    {
        if (slot.itemData.item.IsEquipment())
        {
            Equipment equipment = (Equipment)slot.itemData.item;
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
    }

    public void HideAllTooltips()
    {
        for (int i = 0; i < invTooltips.Length; i++)
        {
            if (invTooltips[i].gameObject.activeSelf)
                invTooltips[i].HideTooltip();
        }
    }
}

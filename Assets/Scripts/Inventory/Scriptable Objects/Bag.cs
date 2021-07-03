using UnityEngine;

[CreateAssetMenu(fileName = "New Bag", menuName = "Inventory/Bag")]
public class Bag : Equipment
{
    public Sprite sidebarSprite;

    [Header("Inventory")]
    public float maxWeight = 50f;
    public float maxVolume = 20f;

    public override void Use(EquipmentManager equipmentManager, Inventory inventory, InventoryItem invItem, int itemCount)
    {
        // Equip the item
        if (invItem.myEquipmentManager == null)
        {
            SetupBag();
        }

        if (invItem.myInvUI == GameManager.instance.containerInvUI)
            GameManager.instance.containerInvUI.SetSideBarIcon_Floor(GameManager.instance.containerInvUI.activeDirection);

        base.Use(equipmentManager, inventory, invItem, itemCount);
    }

    void SetupBag()
    {
        switch (equipmentSlot)
        {
            case EquipmentSlot.Quiver:
                //GameManager.instance.playerInvUI.quiverSidebarButton.SetupBag(this);
                break;
            case EquipmentSlot.Backpack:
                //GameManager.instance.playerInvUI.backpackSidebarButton.SetupBag(this);
                break;
            case EquipmentSlot.LeftHipPouch:
                //GameManager.instance.playerInvUI.leftHipPouchSidebarButton.SetupBag(this);
                break;
            case EquipmentSlot.RightHipPouch:
                //GameManager.instance.playerInvUI.rightHipPouchSidebarButton.SetupBag(this);
                break;
            default:
                Debug.LogError(name + " does not have it's equipmentSlot set to a proper bag slot. Fix me!");
                break;
        }
    }

    public override bool IsBag()
    {
        return true;
    }
}

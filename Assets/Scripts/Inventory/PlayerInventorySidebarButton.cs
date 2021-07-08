using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PlayerInventoryType { Personal, Backpack, LeftHipPouch, RightHipPouch, Quiver, Keys, EquippedItems }

public class PlayerInventorySidebarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerInventoryType playerInventoryType;
    public Image icon;
    public Inventory inventory;

    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
        inventory = GetComponent<Inventory>();
        
        if (inventory != null)
            inventory.myInventoryUI = gm.playerInvUI;
        
        HideInactiveBags();
    }

    public Inventory GetInventory()
    {
        switch (playerInventoryType)
        {
            case PlayerInventoryType.Personal:
                return gm.playerInvUI.personalInventory;
            case PlayerInventoryType.Backpack:
                return gm.playerInvUI.backpackInventory;
            case PlayerInventoryType.LeftHipPouch:
                return gm.playerInvUI.leftHipPouchInventory;
            case PlayerInventoryType.RightHipPouch:
                return gm.playerInvUI.rightHipPouchInventory;
            case PlayerInventoryType.Quiver:
                return gm.playerInvUI.quiverInventory;
            case PlayerInventoryType.Keys:
                return gm.playerInvUI.keysInventory;
            default:
                return null;
        }
    }

    public void ShowInventoryItems()
    {
        gm.uiManager.activeInvItem = null;

        switch (playerInventoryType)
        {
            case PlayerInventoryType.Personal:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.personalInventory.items, PlayerInventoryType.Personal);
                break;
            case PlayerInventoryType.Backpack:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.backpackInventory.items, PlayerInventoryType.Backpack);
                break;
            case PlayerInventoryType.LeftHipPouch:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.leftHipPouchInventory.items, PlayerInventoryType.LeftHipPouch);
                break;
            case PlayerInventoryType.RightHipPouch:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.rightHipPouchInventory.items, PlayerInventoryType.RightHipPouch);
                break;
            case PlayerInventoryType.Quiver:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.quiverInventory.items, PlayerInventoryType.Quiver);
                break;
            case PlayerInventoryType.Keys:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.keysInventory.items, PlayerInventoryType.Keys);
                break;
            case PlayerInventoryType.EquippedItems:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.gm.playerManager.equipmentManager.currentEquipment, PlayerInventoryType.EquippedItems);
                break;
            default:
                break;
        }
    }

    public void HideSideBarButton()
    {
        gameObject.SetActive(false);

        switch (playerInventoryType)
        {
            case PlayerInventoryType.Backpack:
                gm.playerInvUI.backpackEquipped = false;
                break;
            case PlayerInventoryType.LeftHipPouch:
                gm.playerInvUI.leftHipPouchEquipped = false;
                break;
            case PlayerInventoryType.RightHipPouch:
                gm.playerInvUI.rightHipPouchEquipped = false;
                break;
            case PlayerInventoryType.Quiver:
                gm.playerInvUI.quiverEquipped = false;
                break;
            default:
                break;
        }
    }

    public void ShowSideBarButton(Bag bag)
    {
        gameObject.SetActive(true);

        switch (playerInventoryType)
        {
            case PlayerInventoryType.Backpack:
                gm.playerInvUI.backpackEquipped = true;
                gm.playerInvUI.backpackSidebarButton.icon.sprite = bag.sidebarSprite;
                break;
            case PlayerInventoryType.LeftHipPouch:
                gm.playerInvUI.leftHipPouchEquipped = true;
                gm.playerInvUI.leftHipPouchSidebarButton.icon.sprite = bag.sidebarSprite;
                break;
            case PlayerInventoryType.RightHipPouch:
                gm.playerInvUI.rightHipPouchEquipped = true;
                gm.playerInvUI.rightHipPouchSidebarButton.icon.sprite = bag.sidebarSprite;
                break;
            case PlayerInventoryType.Quiver:
                gm.playerInvUI.quiverEquipped = true;
                gm.playerInvUI.quiverSidebarButton.icon.sprite = bag.sidebarSprite;
                break;
            default:
                break;
        }
    }

    void HideInactiveBags()
    {
        if ((gm.playerInvUI.backpackEquipped == false && playerInventoryType == PlayerInventoryType.Backpack) || (gm.playerInvUI.leftHipPouchEquipped == false && playerInventoryType == PlayerInventoryType.LeftHipPouch)
            || (gm.playerInvUI.rightHipPouchEquipped == false && playerInventoryType == PlayerInventoryType.RightHipPouch) || (gm.playerInvUI.quiverEquipped == false && playerInventoryType == PlayerInventoryType.Quiver))
            HideSideBarButton();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gm.uiManager.activePlayerInvSideBarButton = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gm.uiManager.activePlayerInvSideBarButton == this)
            gm.uiManager.activePlayerInvSideBarButton = null;
    }
}

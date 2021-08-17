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
            inventory.myInvUI = gm.playerInvUI;
        
        HideInactiveBags();
    }

    public Inventory GetInventory()
    {
        switch (playerInventoryType)
        {
            case PlayerInventoryType.Personal:
                return gm.playerManager.personalInventory;
            case PlayerInventoryType.Backpack:
                return gm.playerManager.backpackInventory;
            case PlayerInventoryType.LeftHipPouch:
                return gm.playerManager.leftHipPouchInventory;
            case PlayerInventoryType.RightHipPouch:
                return gm.playerManager.rightHipPouchInventory;
            case PlayerInventoryType.Quiver:
                return gm.playerManager.quiverInventory;
            case PlayerInventoryType.Keys:
                return gm.playerManager.keysInventory;
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
                gm.playerInvUI.PopulateInventoryUI(gm.playerManager.personalInventory.items, PlayerInventoryType.Personal);
                break;
            case PlayerInventoryType.Backpack:
                gm.playerInvUI.PopulateInventoryUI(gm.playerManager.backpackInventory.items, PlayerInventoryType.Backpack);
                break;
            case PlayerInventoryType.LeftHipPouch:
                gm.playerInvUI.PopulateInventoryUI(gm.playerManager.leftHipPouchInventory.items, PlayerInventoryType.LeftHipPouch);
                break;
            case PlayerInventoryType.RightHipPouch:
                gm.playerInvUI.PopulateInventoryUI(gm.playerManager.rightHipPouchInventory.items, PlayerInventoryType.RightHipPouch);
                break;
            case PlayerInventoryType.Quiver:
                gm.playerInvUI.PopulateInventoryUI(gm.playerManager.quiverInventory.items, PlayerInventoryType.Quiver);
                break;
            case PlayerInventoryType.Keys:
                gm.playerInvUI.PopulateInventoryUI(gm.playerManager.keysInventory.items, PlayerInventoryType.Keys);
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

        /*switch (playerInventoryType)
        {
            case PlayerInventoryType.Backpack:
                gm.playerManager.backpackInventory = null;
                break;
            case PlayerInventoryType.LeftHipPouch:
                gm.playerManager.leftHipPouchInventory = null;
                break;
            case PlayerInventoryType.RightHipPouch:
                gm.playerManager.rightHipPouchInventory = null;
                break;
            case PlayerInventoryType.Quiver:
                gm.playerManager.quiverInventory = null;
                break;
            default:
                break;
        }*/
    }

    public void ShowSideBarButton(Bag bag)
    {
        gameObject.SetActive(true);

        switch (playerInventoryType)
        {
            case PlayerInventoryType.Backpack:
                gm.playerInvUI.backpackSidebarButton.icon.sprite = bag.sidebarSprite;
                break;
            case PlayerInventoryType.LeftHipPouch:
                gm.playerInvUI.leftHipPouchSidebarButton.icon.sprite = bag.sidebarSprite;
                break;
            case PlayerInventoryType.RightHipPouch:
                gm.playerInvUI.rightHipPouchSidebarButton.icon.sprite = bag.sidebarSprite;
                break;
            case PlayerInventoryType.Quiver:
                gm.playerInvUI.quiverSidebarButton.icon.sprite = bag.sidebarSprite;
                break;
            default:
                break;
        }
    }

    void HideInactiveBags()
    {
        if ((gm.playerManager.backpackInventory == null && playerInventoryType == PlayerInventoryType.Backpack) || (gm.playerManager.leftHipPouchInventory == null && playerInventoryType == PlayerInventoryType.LeftHipPouch)
            || (gm.playerManager.rightHipPouchInventory == null && playerInventoryType == PlayerInventoryType.RightHipPouch) || (gm.playerManager.quiverInventory == null && playerInventoryType == PlayerInventoryType.Quiver))
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

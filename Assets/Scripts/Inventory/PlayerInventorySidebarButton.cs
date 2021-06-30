using UnityEngine;
using UnityEngine.EventSystems;

public enum PlayerInventoryType { Personal, Bag1, Bag2, Bag3, Bag4, Bag5, Keys, EquippedItems }

public class PlayerInventorySidebarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerInventoryType playerInventoryType;

    GameManager gm;
    Inventory inventory;

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
            case PlayerInventoryType.Bag1:
                return gm.playerInvUI.bag1Inventory;
            case PlayerInventoryType.Bag2:
                return gm.playerInvUI.bag2Inventory;
            case PlayerInventoryType.Bag3:
                return gm.playerInvUI.bag3Inventory;
            case PlayerInventoryType.Bag4:
                return gm.playerInvUI.bag4Inventory;
            case PlayerInventoryType.Bag5:
                return gm.playerInvUI.bag5Inventory;
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
            case PlayerInventoryType.Bag1:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.bag1Inventory.items, PlayerInventoryType.Bag1);
                break;
            case PlayerInventoryType.Bag2:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.bag2Inventory.items, PlayerInventoryType.Bag2);
                break;
            case PlayerInventoryType.Bag3:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.bag3Inventory.items, PlayerInventoryType.Bag3);
                break;
            case PlayerInventoryType.Bag4:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.bag4Inventory.items, PlayerInventoryType.Bag4);
                break;
            case PlayerInventoryType.Bag5:
                gm.playerInvUI.PopulateInventoryUI(gm.playerInvUI.bag5Inventory.items, PlayerInventoryType.Bag5);
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
            case PlayerInventoryType.Bag1:
                gm.playerInvUI.bag1Active = false;
                break;
            case PlayerInventoryType.Bag2:
                gm.playerInvUI.bag2Active = false;
                break;
            case PlayerInventoryType.Bag3:
                gm.playerInvUI.bag3Active = false;
                break;
            case PlayerInventoryType.Bag4:
                gm.playerInvUI.bag4Active = false;
                break;
            case PlayerInventoryType.Bag5:
                gm.playerInvUI.bag5Active = false;
                break;
            default:
                break;
        }
    }

    public void ShowSideBarButton()
    {
        gameObject.SetActive(true);

        switch (playerInventoryType)
        {
            case PlayerInventoryType.Bag1:
                gm.playerInvUI.bag1Active = true;
                break;
            case PlayerInventoryType.Bag2:
                gm.playerInvUI.bag2Active = true;
                break;
            case PlayerInventoryType.Bag3:
                gm.playerInvUI.bag3Active = true;
                break;
            case PlayerInventoryType.Bag4:
                gm.playerInvUI.bag4Active = true;
                break;
            case PlayerInventoryType.Bag5:
                gm.playerInvUI.bag5Active = true;
                break;
            default:
                break;
        }
    }

    void HideInactiveBags()
    {
        if ((gm.playerInvUI.bag1Active == false && playerInventoryType == PlayerInventoryType.Bag1) || (gm.playerInvUI.bag2Active == false && playerInventoryType == PlayerInventoryType.Bag2)
            || (gm.playerInvUI.bag3Active == false && playerInventoryType == PlayerInventoryType.Bag3) || (gm.playerInvUI.bag4Active == false && playerInventoryType == PlayerInventoryType.Bag4)
            || (gm.playerInvUI.bag5Active == false && playerInventoryType == PlayerInventoryType.Bag5))
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

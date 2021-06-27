using UnityEngine;

public enum PlayerInventoryType { Personal, Bag1, Bag2, Bag3, Bag4, Bag5, Keys, EquippedItems }

public class PlayerInventorySidebarButton : MonoBehaviour
{
    public PlayerInventoryType playerInventoryType;

    Inventory inventory;
    PlayerInventoryUI playerInvUI;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        playerInvUI = PlayerInventoryUI.instance;

        inventory.myInventoryUI = playerInvUI;
        
        HideInactiveBags();
    }

    public void ShowInventoryItems()
    {
        switch (playerInventoryType)
        {
            case PlayerInventoryType.Personal:
                playerInvUI.PopulateInventoryUI(playerInvUI.personalInventory.items, PlayerInventoryType.Personal);
                break;
            case PlayerInventoryType.Bag1:
                playerInvUI.PopulateInventoryUI(playerInvUI.bag1Inventory.items, PlayerInventoryType.Bag1);
                break;
            case PlayerInventoryType.Bag2:
                playerInvUI.PopulateInventoryUI(playerInvUI.bag2Inventory.items, PlayerInventoryType.Bag2);
                break;
            case PlayerInventoryType.Bag3:
                playerInvUI.PopulateInventoryUI(playerInvUI.bag3Inventory.items, PlayerInventoryType.Bag3);
                break;
            case PlayerInventoryType.Bag4:
                playerInvUI.PopulateInventoryUI(playerInvUI.bag4Inventory.items, PlayerInventoryType.Bag4);
                break;
            case PlayerInventoryType.Bag5:
                playerInvUI.PopulateInventoryUI(playerInvUI.bag5Inventory.items, PlayerInventoryType.Bag5);
                break;
            case PlayerInventoryType.Keys:
                playerInvUI.PopulateInventoryUI(playerInvUI.keysInventory.items, PlayerInventoryType.Keys);
                break;
            case PlayerInventoryType.EquippedItems:
                playerInvUI.PopulateInventoryUI(playerInvUI.playerManager.equipmentManager.currentEquipment, PlayerInventoryType.EquippedItems);
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
                playerInvUI.bag1Active = false;
                break;
            case PlayerInventoryType.Bag2:
                playerInvUI.bag2Active = false;
                break;
            case PlayerInventoryType.Bag3:
                playerInvUI.bag3Active = false;
                break;
            case PlayerInventoryType.Bag4:
                playerInvUI.bag4Active = false;
                break;
            case PlayerInventoryType.Bag5:
                playerInvUI.bag5Active = false;
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
                playerInvUI.bag1Active = true;
                break;
            case PlayerInventoryType.Bag2:
                playerInvUI.bag2Active = true;
                break;
            case PlayerInventoryType.Bag3:
                playerInvUI.bag3Active = true;
                break;
            case PlayerInventoryType.Bag4:
                playerInvUI.bag4Active = true;
                break;
            case PlayerInventoryType.Bag5:
                playerInvUI.bag5Active = true;
                break;
            default:
                break;
        }
    }

    void HideInactiveBags()
    {
        if ((playerInvUI.bag1Active == false && playerInventoryType == PlayerInventoryType.Bag1) || (playerInvUI.bag2Active == false && playerInventoryType == PlayerInventoryType.Bag2)
            || (playerInvUI.bag3Active == false && playerInventoryType == PlayerInventoryType.Bag3) || (playerInvUI.bag4Active == false && playerInventoryType == PlayerInventoryType.Bag4)
            || (playerInvUI.bag5Active == false && playerInventoryType == PlayerInventoryType.Bag5))
            HideSideBarButton();
    }
}

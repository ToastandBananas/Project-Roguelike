using System.Collections.Generic;
using UnityEngine;

public enum PlayerInventoryType { Personal, Bag1, Bag2, Bag3, Bag4, Bag5, Keys, EquippedItems }

public class PlayerInventorySidebarButton : MonoBehaviour
{
    public PlayerInventoryType playerInventoryType;

    PlayerInventoryUI playerInvUI;

    void Start()
    {
        playerInvUI = PlayerInventoryUI.instance;
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
}

using System.Collections.Generic;
using UnityEngine;

public class InventoryCycler : MonoBehaviour
{
    bool isActive;

    GameManager gm;

    public void Init()
    {
        gm = GameManager.instance;
    }

    public void CycleToNextInventory()
    {
        int currentInventoriesIndex = 0;
        List<Inventory> invList = gm.containerInvUI.GetInventoriesListFromDirection(gm.containerInvUI.activeDirection);
        for (int i = 0; i < invList.Count; i++)
        {
            if (invList[i] == gm.containerInvUI.activeInventory)
            {
                currentInventoriesIndex = i;
                break;
            }
        }

        if (gm.containerInvUI.activeInventory == null)
            gm.containerInvUI.activeInventory = invList[0];
        else if (currentInventoriesIndex == invList.Count - 1)
            gm.containerInvUI.activeInventory = null;
        else
            gm.containerInvUI.activeInventory = invList[currentInventoriesIndex + 1];

        if (gm.containerInvUI.activeInventory != null)
            gm.containerInvUI.AssignDirectionalInventory(gm.containerInvUI.activeDirection, gm.containerInvUI.activeInventory);
        else
            gm.containerInvUI.RemoveDirectionalInventory(gm.containerInvUI.activeDirection);
        
        if (gm.containerInvUI.activeInventory != null)
            gm.containerInvUI.PopulateDirectionalItemsList(gm.containerInvUI.activeInventory, gm.containerInvUI.activeDirection);
        else
        {
            gm.containerInvUI.PopulateDirectionalItemsList(gm.containerInvUI.GetGroundItemsListFromDirection(gm.containerInvUI.activeDirection), gm.containerInvUI.activeDirection);
            gm.containerInvUI.GetSideBarButtonFromDirection(gm.containerInvUI.activeDirection).icon.sprite = gm.containerInvUI.floorIconSprite;
        }
        
        gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.GetItemsListFromActiveDirection(), gm.containerInvUI.activeDirection);
    }

    public void CycleToPreviousInventory()
    {
        int currentInventoriesIndex = 0;
        List<Inventory> invList = gm.containerInvUI.GetInventoriesListFromDirection(gm.containerInvUI.activeDirection);
        for (int i = 0; i < invList.Count; i++)
        {
            if (invList[i] == gm.containerInvUI.activeInventory)
            {
                currentInventoriesIndex = i;
                break;
            }
        }
        
        if (gm.containerInvUI.activeInventory == null)
            gm.containerInvUI.activeInventory = invList[invList.Count - 1];
        else if (currentInventoriesIndex == 0)
            gm.containerInvUI.activeInventory = null;
        else
            gm.containerInvUI.activeInventory = invList[currentInventoriesIndex - 1];

        if (gm.containerInvUI.activeInventory != null)
            gm.containerInvUI.AssignDirectionalInventory(gm.containerInvUI.activeDirection, gm.containerInvUI.activeInventory);
        else
            gm.containerInvUI.RemoveDirectionalInventory(gm.containerInvUI.activeDirection);

        if (gm.containerInvUI.activeInventory != null)
            gm.containerInvUI.PopulateDirectionalItemsList(gm.containerInvUI.activeInventory, gm.containerInvUI.activeDirection);
        else
        {
            gm.containerInvUI.PopulateDirectionalItemsList(gm.containerInvUI.GetGroundItemsListFromDirection(gm.containerInvUI.activeDirection), gm.containerInvUI.activeDirection);
            gm.containerInvUI.GetSideBarButtonFromDirection(gm.containerInvUI.activeDirection).icon.sprite = gm.containerInvUI.floorIconSprite;
        }

        gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.GetItemsListFromActiveDirection(), gm.containerInvUI.activeDirection);
    }

    public void Show()
    {
        if (isActive == false)
        {
            isActive = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        transform.position = gm.containerInvUI.GetSideBarButtonFromDirection(gm.containerInvUI.activeDirection).transform.position + new Vector3(27, -41.5f);
    }

    public void Hide()
    {
        if (isActive)
        {
            isActive = false;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}

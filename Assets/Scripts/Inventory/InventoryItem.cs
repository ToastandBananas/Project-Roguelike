using UnityEngine;

public class InventoryItem : InventoryItemBase
{ 
    public int currentStackSize;
    
    [HideInInspector] public Inventory inventory;

    ContainerInventoryUI containerInvUI;
    PlayerEquipmentManager playerEquipmentManager;

    public override void Init()
    {
        base.Init();

        playerEquipmentManager = PlayerEquipmentManager.instance;
        containerInvUI = ContainerInventoryUI.instance;

        itemData = GetComponent<ItemData>();
        if (transform.parent.parent.parent.name == "Player Inventory")
            inventory = PlayerInventory.instance;

        UpdateStackSizeText();
    }

    public override void AddItem(ItemData newItemData)
    {
        base.AddItem(newItemData);

        UpdateStackSizeText();
    }

    /*public override void ClearItem()
    {
        base.ClearItem();

        currentStackSize = 0;
        
        UpdateStackSizeText();
    }*/

    public void UseItem(int amountToUse = 1)
    {
        if (itemData != null)
        {
            if (inventory.HasEnough(itemData, amountToUse))
                itemData.item.Use(playerEquipmentManager, inventory, this, amountToUse);
            else
                Debug.Log("You don't have enough " + itemData.name);
        }
    }

    public void UpdateStackSizeText()
    {
        // TODO
    }

    public override void PlaceSelectedItem()
    {
        // TODO
    }
}

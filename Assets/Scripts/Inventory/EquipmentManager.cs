using System.Collections;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public delegate void OnWearableChanged(ItemData newItemData, ItemData oldItemData);
    public OnWearableChanged onWearableChanged;

    public delegate void OnWeaponChanged(ItemData newItemData, ItemData oldItemData);
    public OnWeaponChanged onWeaponChanged;

    public Transform itemsParent;
    public ItemData[] currentEquipment;

    public float currentWeight, currentVolume;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;
    [HideInInspector] public bool isPlayer;

    public virtual void Start()
    {
        gm = GameManager.instance;

        if (gameObject.CompareTag("NPC"))
        {
            characterManager = GetComponent<CharacterManager>();
            isPlayer = false;
        }
        else
        {
            characterManager = PlayerManager.instance;
            isPlayer = true;
            itemsParent = gm.playerInvUI.equipmentSideBarButton.transform.GetChild(1);
        }

        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new ItemData[numSlots];
    }

    public IEnumerator UseAPAndSetupEquipment(Equipment equipment, EquipmentSlot equipSlot)
    {
        characterManager.actionQueued = true;

        while (characterManager.isMyTurn == false)
        {
            yield return null;
        }

        if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                if (currentEquipment[(int)equipSlot] != null)
                {
                    // Show the equipment's sprite on the player
                    if (equipment.IsWeapon() == false)
                        SetWearableSprite(equipSlot, equipment);
                    else
                        SetWeaponSprite(equipSlot, equipment);
                }
                else
                {
                    // Hide the equipment's sprite on the player
                    if (equipment.IsWeapon() == false)
                        RemoveWearableSprite(equipSlot);
                    else
                        RemoveWearableSprite(equipSlot);
                }

                characterManager.actionQueued = false;
                characterManager.characterStats.UseAP(characterManager.remainingAPToBeUsed);

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.characterStats.UseAP(characterManager.characterStats.currentAP);
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndSetupEquipment(equipment, equipSlot));
            }
        }
        else
        {
            int remainingAP = characterManager.characterStats.UseAPAndGetRemainder(gm.apManager.GetEquipAPCost(equipment));
            if (remainingAP == 0)
            {
                if (currentEquipment[(int)equipSlot] != null)
                {
                    // Show the equipment's sprite on the player
                    if (equipment.IsWeapon() == false)
                        SetWearableSprite(equipSlot, equipment);
                    else
                        SetWeaponSprite(equipSlot, equipment);
                }
                else
                {
                    // Hide the equipment's sprite on the player
                    if (equipment.IsWeapon() == false)
                        RemoveWearableSprite(equipSlot);
                    else
                        RemoveWearableSprite(equipSlot);
                }

                characterManager.actionQueued = false;
                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.remainingAPToBeUsed = remainingAP;
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndSetupEquipment(equipment, equipSlot));
            }
        }
    }

    public bool Equip(ItemData newItemData, InventoryItem invItemComingFrom, EquipmentSlot equipmentSlot)
    {
        // If the item we're equipping is a bag, make sure it's not inside of another bag. If it is, the player will need to remove it from the bag it's inside before they can equip it. 
        if (newItemData.item.IsBag() && gm.playerInvUI.ItemIsInABag(newItemData))
        {
            Debug.Log("You must remove " + newItemData.itemName + " from your bag before you can equip it.");
            return false;
        }

        ItemData equipmentItemData = null;
        if (newItemData.item.IsBag())
        {
            equipmentItemData = gm.objectPoolManager.itemDataContainerObjectPool.GetPooledItemData();
            equipmentItemData.bagInventory.items.Clear();
        }
        else
            equipmentItemData = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();

        newItemData.TransferData(newItemData, equipmentItemData);
        equipmentItemData.currentStackSize = 1;
        equipmentItemData.transform.SetParent(itemsParent);
        equipmentItemData.gameObject.SetActive(true);

        if (newItemData.item.IsBag())
        {
            Bag bag = (Bag)newItemData.item;
            bag.SetupBagInventory(equipmentItemData.bagInventory);
        }

        #if UNITY_EDITOR
            equipmentItemData.gameObject.name = equipmentItemData.itemName;
        #endif

        if (AssignEquipment(equipmentItemData, equipmentSlot) == false)
        {
            if (newItemData.item.IsBag())
                equipmentItemData.ReturnToItemDataContainerObjectPool();
            else
                equipmentItemData.ReturnToItemDataObjectPool();

            Debug.Log("Can't equip item");
            return false;
        }

        // Adjust the equipment manager's weight and volume
        currentWeight += Mathf.RoundToInt(equipmentItemData.item.weight * 100f) / 100f;
        currentVolume += Mathf.RoundToInt(equipmentItemData.item.volume * 100f) / 100f;

        if (newItemData.item.IsBag())
        {
            // Setup the sidebar for the new bag
            gm.playerInvUI.EquipBag(newItemData);

            Inventory bagsInventory = gm.playerInvUI.GetInventoryFromBagEquipSlot(newItemData);
            Inventory itemDataComingFromsInv = null;
            if (newItemData.CompareTag("Item Pickup")) // If this is a bag we're picking up from the ground
                itemDataComingFromsInv = newItemData.bagInventory;
            else if (invItemComingFrom != null)
                itemDataComingFromsInv = invItemComingFrom.myInventory; // If this bag is inside a container or one of the player's inventories

            for (int i = 0; i < newItemData.bagInventory.items.Count; i++)
            {
                gm.uiManager.CreateNewItemDataChild(newItemData.bagInventory.items[i], bagsInventory, true);
            }

            // Set the weight and volume of the "new" bag
            bagsInventory.UpdateCurrentWeightAndVolume();

            // Reset the old bag's inventory
            newItemData.bagInventory.ResetWeightAndVolume(); 

            for (int i = 0; i < newItemData.bagInventory.items.Count; i++)
            {
                // Return the item we took out of the "old" bag back to it's object pool
                newItemData.bagInventory.items[i].ReturnToItemDataObjectPool();
            }

            // Clear out the items list of the "old" bag
            newItemData.bagInventory.items.Clear();

            if (gm.playerInvUI.activeInventory == bagsInventory)
                gm.playerInvUI.GetPlayerInvSidebarButtonFromActiveInv().ShowInventoryItems();
        }

        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(equipmentItemData.item.pickupSprite, null, gm.playerInvUI.equipmentSideBarButton));

        // If the equipment inventory is active, show the item in the menu
        if (gm.playerInvUI.activeInventory == null && isPlayer)
            gm.playerInvUI.ShowNewInventoryItem(equipmentItemData);

        // Show the equipment's sprite on the player
        /*if (equipmentItemData.item.IsWeapon() == false)
            SetWearableSprite(equipmentSlot, (Equipment)equipmentItemData.item);
        else
            SetWeaponSprite(equipmentSlot, (Equipment)equipmentItemData.item);*/

        // If the item is a bag and it was on the ground, set the sidebar icon to a floor icon
        if (newItemData.item.IsBag() && newItemData.CompareTag("Item Pickup"))
            gm.containerInvUI.SetSideBarIcon_Floor(gm.containerInvUI.activeDirection);

        return true;
    }

    public bool Unequip(EquipmentSlot equipmentSlot, bool shouldAddToInventory, bool canDropItem)
    {
        if (currentEquipment[(int)equipmentSlot] != null)
        {
            ItemData oldItemData = currentEquipment[(int)equipmentSlot];
            InventoryItem invItemComingFrom = gm.playerInvUI.GetItemDatasInventoryItem(oldItemData);
            Inventory invComingFrom = null;
            int stackSize = oldItemData.currentStackSize;
            bool shouldDropItem = false;
            
            // If the item we're unequipping is a bag
            if (oldItemData.item.IsBag())
            {
                // Get the bag's inventory (the one on the sidebar)
                invComingFrom = gm.playerInvUI.GetInventoryFromBagEquipSlot(oldItemData);

                // If we're dropping the bag
                if (shouldAddToInventory == false && canDropItem)
                {
                    // Make sure there's room to drop it (bags cannot be dropped on other items)
                    if (gm.uiManager.IsRoomOnGround(oldItemData, gm.containerInvUI.playerPositionItems, gm.playerManager.transform.position) == false)
                        return false;
                }
                else // If we're trying to add the bag to our inventory before dropping it
                {
                    // Check through each of the player's inventories and see if there's room in any of them...if there is, we can continue on
                    bool hasRoomInInventory = false;
                    if (gm.playerInvUI.backpackEquipped && gm.playerInvUI.backpackInventory != invComingFrom)
                        hasRoomInInventory = gm.playerInvUI.backpackInventory.HasRoomInInventory(oldItemData, 1);

                    if (hasRoomInInventory == false && gm.playerInvUI.leftHipPouchEquipped && gm.playerInvUI.leftHipPouchInventory != invComingFrom)
                        hasRoomInInventory = gm.playerInvUI.leftHipPouchInventory.HasRoomInInventory(oldItemData, 1);

                    if (hasRoomInInventory == false && gm.playerInvUI.rightHipPouchEquipped && gm.playerInvUI.rightHipPouchInventory != invComingFrom)
                        hasRoomInInventory = gm.playerInvUI.rightHipPouchInventory.HasRoomInInventory(oldItemData, 1);

                    if (hasRoomInInventory == false)
                        hasRoomInInventory = gm.playerInvUI.personalInventory.HasRoomInInventory(oldItemData, 1);

                    if (hasRoomInInventory == false) // If there's no room in any of our inventories
                    {
                        // Check if there's room on the ground (bags cannot be dropped on other items)
                        if (gm.uiManager.IsRoomOnGround(oldItemData, gm.containerInvUI.playerPositionItems, gm.playerManager.transform.position) == false)
                            return false;
                    }
                }

                for (int i = 0; i < invComingFrom.items.Count; i++)
                {
                    // Set each item in the inventory as a child of the bag item itself
                    invComingFrom.items[i].transform.SetParent(oldItemData.bagInventory.itemsParent);

                    // Add each item in the inventory to the bag's items list, so that when it's added to another inventory or dropped, the items are accounted for
                    oldItemData.bagInventory.items.Add(invComingFrom.items[i]);
                }
            }

            if (shouldAddToInventory)
            {
                if (isPlayer) // If this is the player's equipment
                {
                    bool itemWasAddedToInv = false;
                    bool addItemEffectPlayed = false;

                    // Try adding to the player's inventory and if it is added, play the add item effect
                    if (gm.playerInvUI.backpackEquipped && gm.playerInvUI.backpackInventory != invComingFrom)
                        itemWasAddedToInv = gm.playerInvUI.backpackInventory.AddItem(invItemComingFrom, oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                    if (itemWasAddedToInv)
                    {
                        invItemComingFrom.UpdateInventoryWeightAndVolume();
                        gm.playerInvUI.backpackInventory.UpdateCurrentWeightAndVolume();
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));
                        addItemEffectPlayed = true;
                    }
                    else if (gm.playerInvUI.leftHipPouchEquipped && gm.playerInvUI.leftHipPouchInventory != invComingFrom)
                        itemWasAddedToInv = gm.playerInvUI.leftHipPouchInventory.AddItem(invItemComingFrom, oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                    if (itemWasAddedToInv && addItemEffectPlayed == false)
                    {
                        invItemComingFrom.UpdateInventoryWeightAndVolume();
                        gm.playerInvUI.leftHipPouchInventory.UpdateCurrentWeightAndVolume();
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));
                        addItemEffectPlayed = true;
                    }
                    else if (itemWasAddedToInv == false && gm.playerInvUI.rightHipPouchEquipped && gm.playerInvUI.rightHipPouchInventory != invComingFrom)
                        itemWasAddedToInv = gm.playerInvUI.rightHipPouchInventory.AddItem(invItemComingFrom, oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                    if (itemWasAddedToInv && addItemEffectPlayed == false)
                    {
                        invItemComingFrom.UpdateInventoryWeightAndVolume();
                        gm.playerInvUI.rightHipPouchInventory.UpdateCurrentWeightAndVolume();
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));
                        addItemEffectPlayed = true;
                    }
                    else if (itemWasAddedToInv == false)
                        itemWasAddedToInv = gm.playerInvUI.personalInventory.AddItem(invItemComingFrom, oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                    if (itemWasAddedToInv && addItemEffectPlayed == false)
                    {
                        invItemComingFrom.UpdateInventoryWeightAndVolume();
                        gm.playerInvUI.personalInventory.UpdateCurrentWeightAndVolume();
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.personalInventorySideBarButton));
                    }
                    else if (itemWasAddedToInv == false) // If we can't add it to the Inventory, drop it, but first we need to run the rest of the code in this method, so we'll just set a bool for now
                        shouldDropItem = true;
                }
                else // If this is an NPC's equipment
                {
                    // Try adding to the NPC's inventory, else drop the item at their feet
                    if (characterManager.inventory.AddItem(null, oldItemData, oldItemData.currentStackSize, invComingFrom, true) == false)
                        shouldDropItem = true;
                    else
                        characterManager.inventory.UpdateCurrentWeightAndVolume();
                }
            }
            else
                shouldDropItem = true;

            bool isRoomOnGround = false;
            if (shouldDropItem && canDropItem)
                isRoomOnGround = gm.uiManager.IsRoomOnGround(oldItemData, gm.containerInvUI.playerPositionItems, gm.playerManager.transform.position);

            if (shouldDropItem && canDropItem && isRoomOnGround == false)
                return false;
            else
            {
                // If this is a Wearable Item, set the scriptableObject to null
                if (oldItemData.item.IsWeapon() == false)
                    RemoveWearableSprite(equipmentSlot);
                else  // If this is a Weapon Item, get rid of the weapon's gameobject
                    RemoveWeaponSprite(equipmentSlot);

                // Adjust the equipment manager's weight and volume
                oldItemData.currentStackSize = stackSize;
                currentWeight -= Mathf.RoundToInt(oldItemData.item.weight * oldItemData.currentStackSize * 100f) / 100f;
                currentVolume -= Mathf.RoundToInt(oldItemData.item.volume * oldItemData.currentStackSize * 100f) / 100f;

                UnassignEquipment(oldItemData, equipmentSlot);

                // If we determined we should drop the item, then drop it
                if (shouldDropItem && canDropItem)
                    gm.dropItemController.DropItem(characterManager.transform.position, oldItemData, oldItemData.currentStackSize, invComingFrom, invItemComingFrom);

                // If the item was a bag, be sure to subtract the weight/volume of the bag's contents
                if (oldItemData.item.IsBag())
                    gm.playerInvUI.UnequipBag((Bag)oldItemData.item, invComingFrom);

                if (invItemComingFrom != null)
                    invItemComingFrom.ClearItem();
                else
                    oldItemData.ReturnToItemDataObjectPool();
            }
        }

        return true;
    }

    public virtual bool AssignEquipment(ItemData newItemData, EquipmentSlot equipmentSlot)
    {
        ItemData oldItemData = null;
        bool canAssignEquipment = true;

        // If there's already an Item in this slot
        if (currentEquipment[(int)equipmentSlot] != null)
        {
            oldItemData = currentEquipment[(int)equipmentSlot];

            // If the item we're equipping is a bag, we need to temporarily disable the corresponding bag inventory so that the bag that we're unequipping doesn't try to place itself in its own inventory. 
            // It will be re-enabled in the Equip method, or below if we weren't able to assign it.
            if (oldItemData.item.IsBag())
                gm.playerInvUI.TemporarilyDisableBag(oldItemData);

            // Try to unequip the old Item
            canAssignEquipment = Unequip(equipmentSlot, true, true);
        }

        if (canAssignEquipment) // If we are able to assign the equipment
        {
            if (onWearableChanged != null && newItemData.item.IsWeapon() == false)
                onWearableChanged.Invoke(newItemData, oldItemData);
            else if (onWeaponChanged != null && newItemData.item.IsWeapon())
                onWeaponChanged.Invoke(newItemData, oldItemData);

            currentEquipment[(int)equipmentSlot] = newItemData;
        }
        else if (oldItemData.item.IsBag()) // If we aren't able to assign the equipment
        {
            gm.playerInvUI.ReenableBag(oldItemData); // Re-enable the bag, since we disabled it earlier

            if (gm.containerInvUI.activeInventory == newItemData.bagInventory)
            {
                // And show the sidebar icon for the bag, since it will have been hidden at this point
                Bag bag = (Bag)newItemData.item;
                gm.containerInvUI.GetSideBarButtonFromDirection(gm.containerInvUI.activeDirection).icon.sprite = bag.sidebarSprite;
            }
        }

        return canAssignEquipment;
    }

    public virtual void UnassignEquipment(ItemData oldItemData, EquipmentSlot equipmentSlot)
    {
        currentEquipment[(int)equipmentSlot] = null;

        if (onWearableChanged != null && oldItemData.item.IsWeapon() == false)
            onWearableChanged.Invoke(null, oldItemData);
        else if (onWeaponChanged != null && oldItemData.item.IsWeapon())
            onWeaponChanged.Invoke(null, oldItemData);
    }

    public void UnequipAll(bool shouldAddEquipmentToInventory)
    {
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
            {
                Equipment equipment = (Equipment)currentEquipment[i].item;
                Unequip(equipment.equipmentSlot, shouldAddEquipmentToInventory, true);
            }
        }
    }

    public EquipmentSlot GetEquipmentSlotFromItemData(ItemData itemData)
    {
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null && currentEquipment[i] == itemData)
                return (EquipmentSlot)i;
        }

        return 0;
    }

    public bool ItemIsEquipped(ItemData itemDataInQuestion)
    {
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] == itemDataInQuestion)
                return true;
        }

        return false;
    }

    void SetWearableSprite(EquipmentSlot wearableSlot, Equipment equipment)
    {
        characterManager.equippedItemsSpriteManager.AssignSprite(wearableSlot, equipment);
    }

    void RemoveWearableSprite(EquipmentSlot wearableSlot)
    {
        characterManager.equippedItemsSpriteManager.RemoveSprite(wearableSlot);
    }

    void SetWeaponSprite(EquipmentSlot weaponSlot, Equipment equipment)
    {
        characterManager.equippedItemsSpriteManager.AssignSprite(weaponSlot, equipment);
    }

    void RemoveWeaponSprite(EquipmentSlot weaponSlot)
    {
        characterManager.equippedItemsSpriteManager.RemoveSprite(weaponSlot);
    }

    public void SheathWeapon(EquipmentSlot weaponSlot)
    {
        // TODO
        Debug.Log("Sheathing weapon");
    }

    public void UnsheathWeapon(EquipmentSlot weaponSlot)
    {
        // TODO
        Debug.Log("Unsheathing weapon");
    }
}

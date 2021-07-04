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

    public virtual void Equip(ItemData newItemData, InventoryItem invItemComingFrom, EquipmentSlot equipmentSlot)
    {
        ItemData equipmentItemData = null;
        if (newItemData.item.IsBag())
            equipmentItemData = gm.objectPoolManager.itemDataContainerObjectPool.GetPooledItemData();
        else
            equipmentItemData = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();

        newItemData.TransferData(newItemData, equipmentItemData);
        equipmentItemData.currentStackSize = 1;
        equipmentItemData.transform.SetParent(itemsParent);
        equipmentItemData.gameObject.SetActive(true);

        #if UNITY_EDITOR
            equipmentItemData.gameObject.name = equipmentItemData.itemName;
        #endif

        // Adjust the equipment manager's weight and volume
        currentWeight += Mathf.RoundToInt(equipmentItemData.item.weight * 100f) / 100f;
        currentVolume += Mathf.RoundToInt(equipmentItemData.item.volume * 100f) / 100f;

        // If the item was a bag, be sure to add the weight/volume of the bag's contents
        if (newItemData.item.IsBag())
        {
            gm.playerInvUI.EquipBag(newItemData);

            Inventory bagsInventory = gm.playerInvUI.GetInventoryFromBagEquipSlot(newItemData);
            Inventory itemDataComingFromsInv = null;
            if (newItemData.CompareTag("Item Pickup")) // If this is a bag we're picking up from the ground
                itemDataComingFromsInv = newItemData.bagInventory;
            else if (invItemComingFrom != null)
                itemDataComingFromsInv = invItemComingFrom.myInventory; // If this bag is inside a container or one of the player's inventories

            for (int i = 0; i < newItemData.bagInventory.items.Count; i++)
            {
                // Add new ItemData Objects to the items parent of the new bag and transfer data to them
                ItemData newItemDataObject = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();
                newItemDataObject.transform.SetParent(bagsInventory.itemsParent);
                newItemDataObject.gameObject.SetActive(true);
                newItemData.bagInventory.items[i].TransferData(newItemData.bagInventory.items[i], newItemDataObject);

                // Populate the new bag's inventory, but make sure it's not already in the items list (because of the Inventory's Init method, which populates this list)
                if (bagsInventory.items.Contains(newItemDataObject) == false)
                    bagsInventory.items.Add(newItemDataObject);

                #if UNITY_EDITOR
                    newItemDataObject.name = newItemDataObject.itemName;
                #endif
            }

            // Set the weight and volume of the "new" bag
            bagsInventory.GetCurrentWeightAndVolume();

            // If the bag is coming from an Inventory or EquipmentManager (and not from the ground), subtract the bag's weight/volume, including the items inside it
            if (newItemData.CompareTag("Item Pickup") == false)
                bagsInventory.SubtractItemsWeightAndVolumeFromInventory(newItemData, invItemComingFrom.myInventory, 1, true);
            else
                newItemData.bagInventory.ResetWeightAndVolume(); // Else if it is a pickup, just set the weight and volume to 0

            for (int i = 0; i < itemDataComingFromsInv.items.Count; i++)
            {
                // Return the item we took out of the "old" bag back to it's object pool
                itemDataComingFromsInv.items[i].ReturnToItemDataObjectPool();
            }

            // Clear out the items list of the "old" bag
            itemDataComingFromsInv.items.Clear();
        }

        AssignEquipment(equipmentItemData, equipmentSlot);

        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(equipmentItemData.item.pickupSprite, null, gm.playerInvUI.equipmentSideBarButton));

        // If the equipment inventory is active, show the item in the menu
        if (gm.playerInvUI.activeInventory == null && isPlayer)
        {
            gm.playerInvUI.ShowNewInventoryItem(equipmentItemData);
            gm.playerInvUI.UpdateUINumbers();
        }

        // If this is a Wearable item, show the equipment's sprite on the player
        if (equipmentItemData.item.IsWeapon() == false)
            SetWearableSprite(equipmentSlot, (Equipment)equipmentItemData.item);
        else
            SetWeaponSprite(equipmentSlot, (Equipment)equipmentItemData.item);
    }

    public virtual void Unequip(EquipmentSlot equipmentSlot, bool shouldAddToInventory)
    {
        if (currentEquipment[(int)equipmentSlot] != null)
        {
            ItemData oldItemData = currentEquipment[(int)equipmentSlot];
            InventoryItem invItemComingFrom = gm.playerInvUI.GetItemDatasInventoryItem(oldItemData);
            int stackSize = oldItemData.currentStackSize;
            bool shouldDropItem = false;

            if (shouldAddToInventory)
            {
                if (isPlayer) // If this is the player's equipment
                {
                    bool itemAddedToInv = false;
                    bool addItemEffectPlayed = false;
                    
                    // Try adding to the player's inventory and if it is added, play the add item effect
                    if (gm.playerInvUI.backpackEquipped)
                        itemAddedToInv = gm.playerInvUI.backpackInventory.Add(invItemComingFrom, oldItemData, oldItemData.currentStackSize, null);
                    
                    if (itemAddedToInv)
                    {
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));
                        addItemEffectPlayed = true;
                    }
                    else if (gm.playerInvUI.leftHipPouchEquipped)
                        itemAddedToInv = gm.playerInvUI.leftHipPouchInventory.Add(invItemComingFrom, oldItemData, oldItemData.currentStackSize, null);

                    if (itemAddedToInv && addItemEffectPlayed == false)
                    {
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));
                        addItemEffectPlayed = true;
                    }
                    else if (itemAddedToInv == false && gm.playerInvUI.rightHipPouchEquipped)
                        itemAddedToInv = gm.playerInvUI.rightHipPouchInventory.Add(invItemComingFrom, oldItemData, oldItemData.currentStackSize, null);

                    if (itemAddedToInv && addItemEffectPlayed == false)
                    {
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));
                        addItemEffectPlayed = true;
                    }
                    else if (itemAddedToInv == false)
                        itemAddedToInv = gm.playerInvUI.personalInventory.Add(invItemComingFrom, oldItemData, oldItemData.currentStackSize, null);
                    
                    if (itemAddedToInv && addItemEffectPlayed == false)
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.personalInventorySideBarButton));
                    else if (itemAddedToInv == false) // If we can't add it to the Inventory, drop it, but first we need to run the rest of the code in this method, so we'll just set a bool for now
                        shouldDropItem = true;
                }
                else // If this is an NPC's equipment
                {
                    // Try adding to the NPC's inventory, else drop the item at their feet
                    if (characterManager.inventory.Add(null, oldItemData, oldItemData.currentStackSize, null) == false)
                        shouldDropItem = true;
                }
            }

            bool isRoomOnGround = true;
            if (shouldDropItem)
                isRoomOnGround = invItemComingFrom.IsRoomOnGround(oldItemData, gm.containerInvUI.playerPositionItems, gm.playerManager.transform.position);

            if (shouldDropItem == false || isRoomOnGround)
            {
                // If this is a Wearable Item, set the scriptableObject to null
                if (oldItemData.item.IsWeapon() == false)
                    RemoveWearableSprite(equipmentSlot);
                else  // If this is a Weapon Item, get rid of the weapon's gameobject
                    RemoveWeaponSprite(equipmentSlot);

                UnassignEquipment(oldItemData, equipmentSlot);
                
                // If we determined we should drop the item, then drop it
                if (shouldDropItem)
                    gm.dropItemController.DropItem(characterManager.transform.position, oldItemData, oldItemData.currentStackSize, null);

                // Adjust the equipment manager's weight and volume
                oldItemData.currentStackSize = stackSize;
                currentWeight -= Mathf.RoundToInt(oldItemData.item.weight * oldItemData.currentStackSize * 100f) / 100f;
                currentVolume -= Mathf.RoundToInt(oldItemData.item.volume * oldItemData.currentStackSize * 100f) / 100f;

                // If the item was a bag, be sure to subtract the weight/volume of the bag's contents
                if (oldItemData.item.IsBag())
                {
                    for (int i = 0; i < oldItemData.bagInventory.items.Count; i++)
                    {
                        currentWeight -= Mathf.RoundToInt(oldItemData.bagInventory.items[i].item.weight * oldItemData.bagInventory.items[i].currentStackSize * 100f) / 100f;
                        currentVolume -= Mathf.RoundToInt(oldItemData.bagInventory.items[i].item.volume * oldItemData.bagInventory.items[i].currentStackSize * 100f) / 100f;
                    }
                }

                if (invItemComingFrom != null)
                    invItemComingFrom.ClearItem();
                else
                    oldItemData.ReturnToItemDataObjectPool();

                if (isPlayer)
                    gm.playerInvUI.UpdateUINumbers();
            }
        }
    }

    public virtual void AssignEquipment(ItemData newItemData, EquipmentSlot equipmentSlot)
    {
        ItemData oldItemData = null;

        // If there's already an Item in this slot
        if (currentEquipment[(int)equipmentSlot] != null)
        {
            oldItemData = currentEquipment[(int)equipmentSlot];
            Unequip(equipmentSlot, true);
        }

        if (onWearableChanged != null && newItemData.item.IsWeapon() == false)
            onWearableChanged.Invoke(newItemData, oldItemData);
        else if (onWeaponChanged != null && newItemData.item.IsWeapon())
            onWeaponChanged.Invoke(newItemData, oldItemData);

        currentEquipment[(int)equipmentSlot] = newItemData;
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
                Unequip(equipment.equipmentSlot, shouldAddEquipmentToInventory);
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

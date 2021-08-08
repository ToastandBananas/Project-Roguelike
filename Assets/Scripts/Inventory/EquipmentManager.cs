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

    [Header("Starting Equipment")]
    public Equipment startingHelmet;
    public Equipment startingCape, startingShirt, startingPants, startingBoots, startingGloves, startingBodyArmor, startingLegArmor, startingLeftWeapon, startingRightWeapon, startingRangedWeapon;
    public Equipment startingQuiver, startingBackpack, startingLeftHipPouch, startingRightHipPouch;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;
    [HideInInspector] public bool isPlayer, isTwoHanding;

    float twoHandedDamageMultiplier = 1.35f;

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
        
        currentEquipment = new ItemData[System.Enum.GetNames(typeof(EquipmentSlot)).Length];

        SetupStartingEquipment();
    }

    public IEnumerator UseAPAndSetupEquipment(Equipment equipment, EquipmentSlot equipSlot, ItemData newItemData, ItemData oldItemData, bool droppingEquipment)
    {
        characterManager.actionQueued = true;

        while (characterManager.isMyTurn == false)
        {
            yield return null;
        }

        if (characterManager.status.isDead)
        {
            characterManager.actionQueued = false;
            characterManager.remainingAPToBeUsed = 0;
            yield break;
        }

        if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                SetEquippedSprite(equipSlot, equipment, droppingEquipment);
                OnEquipmentChanged(oldItemData, newItemData);

                if (newItemData != null)
                    newItemData.ReturnToObjectPool();
                if (oldItemData != null)
                    oldItemData.ReturnToObjectPool();

                characterManager.actionQueued = false;
                characterManager.characterStats.UseAP(characterManager.remainingAPToBeUsed);

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.characterStats.UseAP(characterManager.characterStats.currentAP);
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndSetupEquipment(equipment, equipSlot, newItemData, oldItemData, droppingEquipment));
            }
        }
        else
        {
            float newBagInvWeight = 0;
            float oldBagInvWeight = 0;
            if (newItemData != null && newItemData.item.IsBag())
                newBagInvWeight += newItemData.bagInventory.currentWeight;
            if (oldItemData != null && oldItemData.item.IsBag())
                oldBagInvWeight += oldItemData.bagInventory.currentWeight;
            
            int APCost = 0;
            if (newItemData != null)
                APCost += gm.apManager.GetEquipAPCost(equipment, newBagInvWeight);
            if (oldItemData != null)
                APCost += gm.apManager.GetEquipAPCost(equipment, oldBagInvWeight);
            
            int remainingAP = characterManager.characterStats.UseAPAndGetRemainder(APCost);
            if (remainingAP == 0)
            {
                SetEquippedSprite(equipSlot, equipment, droppingEquipment);
                OnEquipmentChanged(oldItemData, newItemData);

                if (newItemData != null)
                    newItemData.ReturnToObjectPool();
                if (oldItemData != null)
                    oldItemData.ReturnToObjectPool();

                characterManager.actionQueued = false;
                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.remainingAPToBeUsed += remainingAP;
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndSetupEquipment(equipment, equipSlot, newItemData, oldItemData, droppingEquipment));
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

            Debug.Log("Can't equip item because there's nowhere to put your currently equipped item.");
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

        // If the item is a bag and it was on the ground, set the sidebar icon to a floor icon
        if (newItemData.item.IsBag() && newItemData.CompareTag("Item Pickup"))
        {
            gm.containerInvUI.SetSideBarIcon_Floor(gm.containerInvUI.activeDirection);
            gm.containerInvUI.GetInventoriesListFromDirection(gm.containerInvUI.activeDirection).Remove(newItemData.bagInventory);
        }

        gm.flavorText.WriteEquipLine(newItemData, characterManager);

        return true;
    }

    public bool Unequip(EquipmentSlot equipmentSlot, bool shouldAddToInventory, bool canDropItem, bool forceUnequip)
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
                        if (invItemComingFrom != null && invItemComingFrom.myInventory != null)
                            invItemComingFrom.UpdateInventoryWeightAndVolume();

                        gm.playerInvUI.backpackInventory.UpdateCurrentWeightAndVolume();
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));
                        addItemEffectPlayed = true;
                        gm.flavorText.WriteUnquipLine(oldItemData, characterManager);
                    }
                    else if (gm.playerInvUI.leftHipPouchEquipped && gm.playerInvUI.leftHipPouchInventory != invComingFrom)
                        itemWasAddedToInv = gm.playerInvUI.leftHipPouchInventory.AddItem(invItemComingFrom, oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                    if (itemWasAddedToInv && addItemEffectPlayed == false)
                    {
                        if (invItemComingFrom != null && invItemComingFrom.myInventory != null)
                            invItemComingFrom.UpdateInventoryWeightAndVolume();

                        gm.playerInvUI.leftHipPouchInventory.UpdateCurrentWeightAndVolume();
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));
                        addItemEffectPlayed = true;
                        gm.flavorText.WriteUnquipLine(oldItemData, characterManager);
                    }
                    else if (itemWasAddedToInv == false && gm.playerInvUI.rightHipPouchEquipped && gm.playerInvUI.rightHipPouchInventory != invComingFrom)
                        itemWasAddedToInv = gm.playerInvUI.rightHipPouchInventory.AddItem(invItemComingFrom, oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                    if (itemWasAddedToInv && addItemEffectPlayed == false)
                    {
                        if (invItemComingFrom != null && invItemComingFrom.myInventory != null)
                            invItemComingFrom.UpdateInventoryWeightAndVolume();

                        gm.playerInvUI.rightHipPouchInventory.UpdateCurrentWeightAndVolume();
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));
                        addItemEffectPlayed = true;
                        gm.flavorText.WriteUnquipLine(oldItemData, characterManager);
                    }
                    else if (itemWasAddedToInv == false)
                        itemWasAddedToInv = gm.playerInvUI.personalInventory.AddItem(invItemComingFrom, oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                    if (itemWasAddedToInv && addItemEffectPlayed == false)
                    {
                        if (invItemComingFrom != null && invItemComingFrom.myInventory != null)
                            invItemComingFrom.UpdateInventoryWeightAndVolume();

                        gm.playerInvUI.personalInventory.UpdateCurrentWeightAndVolume();
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.personalInventorySideBarButton));
                        gm.flavorText.WriteUnquipLine(oldItemData, characterManager);
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

            if (forceUnequip == false && shouldDropItem && canDropItem && isRoomOnGround == false)
                return false;
            else
            {
                // Adjust the equipment manager's weight and volume
                oldItemData.currentStackSize = stackSize;
                currentWeight -= Mathf.RoundToInt(oldItemData.item.weight * oldItemData.currentStackSize * 100f) / 100f;
                currentVolume -= Mathf.RoundToInt(oldItemData.item.volume * oldItemData.currentStackSize * 100f) / 100f;

                // If we determined we should drop the item, then drop it
                if (forceUnequip && shouldDropItem)
                {
                    gm.flavorText.WriteUnquipLine(oldItemData, characterManager);
                    gm.dropItemController.ForceDropNearest(oldItemData, oldItemData.currentStackSize, invComingFrom, invItemComingFrom);
                }
                else if (shouldDropItem && canDropItem)
                {
                    gm.flavorText.WriteUnquipLine(oldItemData, characterManager);
                    gm.dropItemController.DropItem(characterManager.transform.position, oldItemData, oldItemData.currentStackSize, invComingFrom, invItemComingFrom);
                }

                // If the item was a bag, be sure to subtract the weight/volume of the bag's contents
                if (oldItemData.item.IsBag())
                    gm.playerInvUI.UnequipBag((Bag)oldItemData.item, invComingFrom);

                if (invItemComingFrom != null)
                    invItemComingFrom.ClearItem();
                else
                    oldItemData.ReturnToItemDataObjectPool();
            }
        }

        UnassignEquipment(equipmentSlot);

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
            canAssignEquipment = Unequip(equipmentSlot, true, true, false);
        }

        if (canAssignEquipment) // If we are able to assign the equipment
        {
            // Assign the new equipment to the appropriate slot
            currentEquipment[(int)equipmentSlot] = newItemData;

            // If the new item is a weapon
            if (newItemData.item.IsWeapon() || newItemData.item.IsShield())
            {
                Weapon weapon = null;
                if (newItemData.item.IsWeapon())
                    weapon = (Weapon)newItemData.item;
                
                // If the weapon we're equipping is two-handed and there's a weapon equipped in our off-hand, unequip it
                if (weapon != null && weapon.isTwoHanded && currentEquipment[(int)EquipmentSlot.LeftWeapon] != null)
                    Unequip(EquipmentSlot.LeftWeapon, true, true, true);
                // If we're equipping a weapon to our left hand and we already have a two-handed weapon equipped, unequip it
                else if (TwoHandedWeaponEquipped() && equipmentSlot == EquipmentSlot.LeftWeapon)
                    Unequip(EquipmentSlot.RightWeapon, true, true, true);
            }
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

    public virtual void UnassignEquipment(EquipmentSlot equipmentSlot)
    {
        currentEquipment[(int)equipmentSlot] = null;
    }

    public void OnEquipmentChanged(ItemData oldItemData, ItemData newItemData)
    {
        Equipment equipment = null;
        if (newItemData != null)
            equipment = (Equipment)newItemData.item;
        else
            equipment = (Equipment)oldItemData.item;

        if (onWearableChanged != null && equipment.IsWeapon() == false)
            onWearableChanged.Invoke(newItemData, oldItemData);
        else if (onWeaponChanged != null && equipment.IsWeapon())
            onWeaponChanged.Invoke(newItemData, oldItemData);
    }

    public void UnequipAll(bool shouldAddEquipmentToInventory)
    {
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null)
            {
                Equipment equipment = (Equipment)currentEquipment[i].item;
                Unequip((EquipmentSlot)i, shouldAddEquipmentToInventory, true, false);
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

    public bool TwoHandedWeaponEquipped()
    {
        if (currentEquipment[(int)EquipmentSlot.RightWeapon] != null && currentEquipment[(int)EquipmentSlot.RightWeapon].item.IsWeapon())
        {
            Weapon weapon = (Weapon)currentEquipment[(int)EquipmentSlot.RightWeapon].item;
            if (weapon.isTwoHanded)
                return true;
        }
        else if (currentEquipment[(int)EquipmentSlot.LeftWeapon] != null && currentEquipment[(int)EquipmentSlot.LeftWeapon].item.IsWeapon())
        {
            Weapon weapon = (Weapon)currentEquipment[(int)EquipmentSlot.LeftWeapon].item;
            if (weapon.isTwoHanded)
                return true;
        }

        return false;
    }

    public bool IsDualWielding()
    {
        if (currentEquipment[(int)EquipmentSlot.RightWeapon] != null && currentEquipment[(int)EquipmentSlot.RightWeapon].item.IsWeapon() 
            && currentEquipment[(int)EquipmentSlot.LeftWeapon] != null && currentEquipment[(int)EquipmentSlot.LeftWeapon].item.IsWeapon())
            return true;

        return false;
    }

    public bool RightWeaponEquipped()
    {
        if (currentEquipment[(int)EquipmentSlot.RightWeapon] != null)
            return true;

        return false;
    }

    public bool LeftWeaponEquipped()
    {
        if (currentEquipment[(int)EquipmentSlot.LeftWeapon] != null)
            return true;

        return false;
    }

    public bool MeleeWeaponEquipped()
    {
        if (currentEquipment[(int)EquipmentSlot.LeftWeapon] != null || currentEquipment[(int)EquipmentSlot.RightWeapon] != null)
            return true;

        return false;
    }

    public bool ShieldEquipped()
    {
        if ((currentEquipment[(int)EquipmentSlot.LeftWeapon] != null && currentEquipment[(int)EquipmentSlot.LeftWeapon].item.IsShield())
            || (currentEquipment[(int)EquipmentSlot.RightWeapon] != null && currentEquipment[(int)EquipmentSlot.RightWeapon].item.IsShield()))
            return true;

        return false;
    }

    /// <summary> Returns a melee weapon's slash, blunt, pierce or cleave damage based on the melee attack type. </summary>
    public int GetPhysicalMeleeDamage(ItemData weaponsItemData, MeleeAttackType meleeAttackType, PhysicalDamageType physicalDamageType)
    {
        float damage = 0;
        switch (physicalDamageType)
        {
            case PhysicalDamageType.Slash:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    damage = weaponsItemData.slashDamage_Swipe;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    damage = weaponsItemData.slashDamage_Thrust;
                else if (meleeAttackType == MeleeAttackType.Overhead)
                    damage = weaponsItemData.slashDamage_Overhead;
                break;
            case PhysicalDamageType.Blunt:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    damage = weaponsItemData.bluntDamage_Swipe;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    damage = weaponsItemData.bluntDamage_Thrust;
                else if (meleeAttackType == MeleeAttackType.Overhead)
                    damage = weaponsItemData.bluntDamage_Overhead;
                break;
            case PhysicalDamageType.Pierce:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    damage = weaponsItemData.pierceDamage_Swipe;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    damage = weaponsItemData.pierceDamage_Thrust;
                else if (meleeAttackType == MeleeAttackType.Overhead)
                    damage = weaponsItemData.pierceDamage_Overhead;
                break;
            case PhysicalDamageType.Cleave:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    damage = weaponsItemData.cleaveDamage_Swipe;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    damage = weaponsItemData.cleaveDamage_Thrust;
                else if (meleeAttackType == MeleeAttackType.Overhead)
                    damage = weaponsItemData.cleaveDamage_Overhead;
                break;
            default:
                break;
        }

        if (damage <= 0)
            return 0;

        // Add a little randomization to the damage amount
        damage += Mathf.RoundToInt(Random.Range(damage * -0.2f, damage * 0.2f));

        // If two-handing a one-handed weapon, increase the damage
        if (TwoHandedWeaponEquipped() == false && isTwoHanding)
            damage = Mathf.RoundToInt(damage * twoHandedDamageMultiplier);

        return Mathf.RoundToInt(damage);
    }

    public Weapon GetRightWeapon()
    {
        if (currentEquipment[(int)EquipmentSlot.RightWeapon] == null)
            return null;
        return (Weapon)currentEquipment[(int)EquipmentSlot.RightWeapon].item;
    }

    public Weapon GetLeftWeapon()
    {
        if (currentEquipment[(int)EquipmentSlot.LeftWeapon] == null)
            return null;
        return (Weapon)currentEquipment[(int)EquipmentSlot.LeftWeapon].item;
    }

    public Shield GetRightShield()
    {
        if (currentEquipment[(int)EquipmentSlot.RightWeapon] == null)
            return null;
        return (Shield)currentEquipment[(int)EquipmentSlot.RightWeapon].item;
    }

    public Shield GetLeftShield()
    {
        if (currentEquipment[(int)EquipmentSlot.LeftWeapon] == null)
            return null;
        return (Shield)currentEquipment[(int)EquipmentSlot.LeftWeapon].item;
    }

    public Weapon GetRangedWeapon()
    {
        if (currentEquipment[(int)EquipmentSlot.Ranged] == null)
            return null;
        return (Weapon)currentEquipment[(int)EquipmentSlot.Ranged].item;
    }

    public ItemData GetEquipmentsItemData(EquipmentSlot equipSlot)
    {
        return currentEquipment[(int)equipSlot];
    }

    public Equipment GetEquipment(EquipmentSlot equipSlot)
    {
        return (Equipment)currentEquipment[(int)equipSlot].item;
    }

    void SetEquippedSprite(EquipmentSlot equipSlot, Equipment equipment, bool droppingEquipment)
    {
        // If we equipped an item
        if (droppingEquipment == false && currentEquipment[(int)equipSlot] != null)
        {
            // Show the equipment's sprite on the player
            if (equipment.IsWeapon() == false && equipment.IsShield() == false)
                SetEquipmentSprite(equipSlot, equipment);
            else
            {
                SetEquipmentSprite(equipSlot, equipment);

                Weapon weapon = null;
                if (equipment.IsWeapon())
                    weapon = (Weapon)equipment;

                // If the new weapon is two-handed, remove our left weapon sprite if we have one and set our character's sprites to the two-handed stance
                if (weapon != null && weapon.isTwoHanded)
                {
                    RemoveEquipmentSprite(EquipmentSlot.LeftWeapon);
                    characterManager.humanoidSpriteManager.SetupTwoHandedWeaponStance(this, characterManager);
                }
                // If the new weapon is one-handed and our right weapon is now null, remove our right weapon sprite and set our character's sprites to the one-handed stance
                else if (equipSlot == EquipmentSlot.LeftWeapon && currentEquipment[(int)EquipmentSlot.RightWeapon] == null)
                {
                    RemoveEquipmentSprite(EquipmentSlot.RightWeapon);
                    characterManager.humanoidSpriteManager.SetupOneHandedWeaponStance(this, characterManager);
                }
                // If the new weapon is one-handed, just set our character's sprites to the one-handed stance
                else if (weapon != null && weapon.isTwoHanded == false)
                    characterManager.humanoidSpriteManager.SetupOneHandedWeaponStance(this, characterManager);
            }
        }
        else // If we unequipped an item
        {
            // Hide the equipment's sprite on the player
            if (equipment.IsWeapon() == false && equipment.IsShield() == false)
                RemoveEquipmentSprite(equipSlot);
            else
            {
                Weapon weapon = null;
                if (equipment.IsWeapon())
                    weapon = (Weapon)equipment;

                RemoveEquipmentSprite(equipSlot);

                // If the weapon we're unequipping is two-handed, set our character's sprites back to the one-handed stance
                if (weapon != null && weapon.isTwoHanded)
                    characterManager.humanoidSpriteManager.SetupOneHandedWeaponStance(this, characterManager);
            }
        }
    }

    void SetEquipmentSprite(EquipmentSlot equipSlot, Equipment equipment)
    {
        characterManager.humanoidSpriteManager.AssignSprite(equipSlot, equipment, this);
    }

    void RemoveEquipmentSprite(EquipmentSlot equipSlot)
    {
        characterManager.humanoidSpriteManager.RemoveSprite(equipSlot);
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

    void SetupStartingEquipment()
    {
        EquipNewEquipment(startingHelmet, EquipmentSlot.Helmet);
        EquipNewEquipment(startingCape, EquipmentSlot.Cape);
        EquipNewEquipment(startingShirt, EquipmentSlot.Shirt);
        EquipNewEquipment(startingPants, EquipmentSlot.Pants);
        EquipNewEquipment(startingBoots, EquipmentSlot.Boots);
        EquipNewEquipment(startingGloves, EquipmentSlot.Gloves);
        EquipNewEquipment(startingBodyArmor, EquipmentSlot.BodyArmor);
        EquipNewEquipment(startingLegArmor, EquipmentSlot.LegArmor);
        EquipNewEquipment(startingLeftWeapon, EquipmentSlot.LeftWeapon);
        EquipNewEquipment(startingRightWeapon, EquipmentSlot.RightWeapon);
        EquipNewEquipment(startingRangedWeapon, EquipmentSlot.Ranged);
        EquipNewEquipment(startingQuiver, EquipmentSlot.Quiver);
        EquipNewEquipment(startingBackpack, EquipmentSlot.Backpack);
        EquipNewEquipment(startingLeftHipPouch, EquipmentSlot.LeftHipPouch);
        EquipNewEquipment(startingRightHipPouch, EquipmentSlot.RightHipPouch);
    }

    void EquipNewEquipment(Equipment equipment, EquipmentSlot equipSlot)
    {
        if (equipment != null)
        {
            ItemData newItemData = gm.objectPoolManager.GetItemDataFromPool(equipment);
            newItemData.gameObject.SetActive(true);
            newItemData.transform.SetParent(itemsParent);
            newItemData.item = equipment;
            newItemData.RandomizeData();

            currentEquipment[(int)equipSlot] = newItemData;
            SetEquipmentSprite(equipSlot, equipment);

            #if UNITY_EDITOR
                newItemData.gameObject.name = newItemData.itemName;
            #endif

            if (equipment.IsWeapon() && onWeaponChanged != null)
                onWeaponChanged.Invoke(newItemData, null);
            else if (equipment.IsWearable() && onWearableChanged != null)
                onWearableChanged.Invoke(newItemData, null);
        }
    }
}

using System.Collections;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public delegate void OnWearableChangedDelegate(ItemData newItemData, ItemData oldItemData);
    public OnWearableChangedDelegate onWearableChanged;

    public delegate void OnWeaponChangedDelegate(ItemData newItemData, ItemData oldItemData);
    public OnWeaponChangedDelegate onWeaponChanged;

    public Transform itemsParent;

    [NamedArray(new string[] { "Helmet", "Shirt", "Pants", "Boots", "Gloves", "Body Armor", "Leg Armor", "Left Hand Item", "Right Hand Item", "Ranged", "Quiver", "Backpack", "Left Hip Pouch", "Right Hip Pouch", "Cape" })]
    public ItemData[] currentEquipment = new ItemData[System.Enum.GetNames(typeof(EquipmentSlot)).Length];

    [NamedArray(new string[] { "Helmet", "Shirt", "Pants", "Boots", "Gloves", "Body Armor", "Leg Armor", "Left Hand Item", "Right Hand Item", "Ranged", "Quiver", "Backpack", "Left Hip Pouch", "Right Hip Pouch", "Cape" })]
    public Equipment[] startingEquipment = new Equipment[System.Enum.GetNames(typeof(EquipmentSlot)).Length];
    
    [HideInInspector] public float currentWeight, currentVolume;
    [HideInInspector] public bool isTwoHanding;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;

    float twoHandedDamageMultiplier = 1.35f;

    public virtual void Start()
    {
        gm = GameManager.instance;

        if (currentEquipment.Length != System.Enum.GetNames(typeof(EquipmentSlot)).Length)
            currentEquipment = new ItemData[System.Enum.GetNames(typeof(EquipmentSlot)).Length];

        characterManager.equipmentManager.onWearableChanged += OnWearableChanged;
        characterManager.equipmentManager.onWeaponChanged += OnWeaponChanged;

        SetupStartingEquipment();
    }

    public IEnumerator SetUpEquipment(ItemData newItemData, ItemData oldItemData, Equipment equipment, EquipmentSlot equipSlot, bool droppingEquipment)
    {
        SetEquippedSprite(equipSlot, equipment, droppingEquipment);
        OnEquipmentChanged(oldItemData, newItemData);

        if (gm.playerInvUI.activePlayerInvSideBarButton.playerInventoryType == PlayerInventoryType.EquippedItems)
        {
            if (currentEquipment[(int)EquipmentSlot.RightHandItem] != null)
                currentEquipment[(int)EquipmentSlot.RightHandItem].GetItemDatasInventoryItem().UpdateAllItemTexts();
            if (currentEquipment[(int)EquipmentSlot.LeftHandItem] != null)
                currentEquipment[(int)EquipmentSlot.LeftHandItem].GetItemDatasInventoryItem().UpdateAllItemTexts();
        }

        characterManager.DropAllCarriedItems();

        if (newItemData != null)
        {
            if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                gm.flavorText.WriteLine_Equip(newItemData, characterManager);

            if (characterManager.isNPC == false && newItemData.item.IsWeapon() && (equipSlot == EquipmentSlot.LeftHandItem || equipSlot == EquipmentSlot.RightHandItem))
                gm.flavorText.WriteLine_SwitchStance(characterManager, newItemData, (Weapon)newItemData.item);

            newItemData.ReturnToObjectPool();
        }
        
        if (oldItemData != null)
            oldItemData.ReturnToObjectPool();

        if (characterManager.status.isDead == false)
            characterManager.FinishAction();

        yield return null;
    }

    public bool Equip(ItemData newItemData, InventoryItem invItemComingFrom, EquipmentSlot equipmentSlot)
    {
        // If the item we're equipping is a bag, make sure it's not inside of another bag. If it is, the player will need to remove it from the bag it's inside before they can equip it. 
        if (newItemData.item.IsBag() && gm.playerInvUI.ItemIsInABag(newItemData))
        {
            Debug.Log("You must remove " + newItemData.GetItemName(1) + " from your bag before you can equip it.");
            return false;
        }

        ItemData equipmentItemData = gm.uiManager.CreateNewItemDataChild(newItemData, null, itemsParent, false);

        if (AssignEquipment(equipmentItemData, equipmentSlot) == false)
        {
            equipmentItemData.ReturnToObjectPool();

            if (characterManager.isNPC == false)
                gm.flavorText.WriteLine("You can't equip the <b>" + equipmentItemData.GetItemName(1) + "</b> because there's nowhere to put your currently equipped item.");
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
        if (gm.playerInvUI.activeInventory == null && characterManager.isNPC == false)
            gm.playerInvUI.ShowNewInventoryItem(equipmentItemData);

        // If the item is a bag and it was on the ground, set the sidebar icon to a floor icon
        if (newItemData.item.IsBag() && newItemData.IsPickup())
        {
            gm.containerInvUI.SetSideBarIcon_Floor(gm.containerInvUI.activeDirection);
            gm.containerInvUI.GetInventoriesListFromDirection(gm.containerInvUI.activeDirection).Remove(newItemData.bagInventory);
        }

        // Update the character's total weight/volume
        characterManager.SetTotalCarriedWeightAndVolume();

        return true;
    }

    public bool Unequip(EquipmentSlot equipmentSlot, bool shouldAddToInventory, bool canDropItem, bool forceUnequip)
    {
        if (currentEquipment[(int)equipmentSlot] != null)
        {
            ItemData oldItemData = currentEquipment[(int)equipmentSlot];
            InventoryItem invItemComingFrom = oldItemData.GetItemDatasInventoryItem();
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
                    // Check through each of the character's inventories and see if there's room in any of them...if there is, we can continue on
                    bool hasRoomInInventory = false;
                    if (characterManager.backpackInventory != null && characterManager.backpackInventory != invComingFrom)
                        hasRoomInInventory = characterManager.backpackInventory.HasRoomInInventory(oldItemData, 1);

                    if (hasRoomInInventory == false && characterManager.leftHipPouchInventory != null && characterManager.leftHipPouchInventory != invComingFrom)
                        hasRoomInInventory = characterManager.leftHipPouchInventory.HasRoomInInventory(oldItemData, 1);

                    if (hasRoomInInventory == false && characterManager.rightHipPouchInventory != null && characterManager.rightHipPouchInventory != invComingFrom)
                        hasRoomInInventory = characterManager.rightHipPouchInventory.HasRoomInInventory(oldItemData, 1);

                    if (hasRoomInInventory == false)
                        hasRoomInInventory = characterManager.personalInventory.HasRoomInInventory(oldItemData, 1);

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
                bool itemWasAddedToInv = false;
                bool addItemEffectPlayed = false;

                // Try adding to the player's inventory and if it is added, play the add item effect
                if (characterManager.backpackInventory != null && characterManager.backpackInventory != invComingFrom)
                    itemWasAddedToInv = characterManager.backpackInventory.AddItem(oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                if (itemWasAddedToInv)
                {
                    if (invItemComingFrom != null && invItemComingFrom.myInventory != null)
                        invItemComingFrom.UpdateInventoryWeightAndVolume();

                    characterManager.backpackInventory.UpdateCurrentWeightAndVolume();
                    StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));
                    addItemEffectPlayed = true;

                    if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                        gm.flavorText.WriteLine_Unequip(oldItemData, characterManager);
                }
                else if (characterManager.leftHipPouchInventory != null && characterManager.leftHipPouchInventory != invComingFrom)
                    itemWasAddedToInv = characterManager.leftHipPouchInventory.AddItem(oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                if (itemWasAddedToInv && addItemEffectPlayed == false)
                {
                    if (invItemComingFrom != null && invItemComingFrom.myInventory != null)
                        invItemComingFrom.UpdateInventoryWeightAndVolume();

                    characterManager.leftHipPouchInventory.UpdateCurrentWeightAndVolume();
                    StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));
                    addItemEffectPlayed = true;

                    if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                        gm.flavorText.WriteLine_Unequip(oldItemData, characterManager);
                }
                else if (itemWasAddedToInv == false && characterManager.rightHipPouchInventory != null && characterManager.rightHipPouchInventory != invComingFrom)
                    itemWasAddedToInv = characterManager.rightHipPouchInventory.AddItem(oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                if (itemWasAddedToInv && addItemEffectPlayed == false)
                {
                    if (invItemComingFrom != null && invItemComingFrom.myInventory != null)
                        invItemComingFrom.UpdateInventoryWeightAndVolume();

                    characterManager.rightHipPouchInventory.UpdateCurrentWeightAndVolume();
                    StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));
                    addItemEffectPlayed = true;

                    if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                        gm.flavorText.WriteLine_Unequip(oldItemData, characterManager);
                }
                else if (itemWasAddedToInv == false)
                    itemWasAddedToInv = characterManager.personalInventory.AddItem(oldItemData, oldItemData.currentStackSize, invComingFrom, true);

                if (itemWasAddedToInv && addItemEffectPlayed == false)
                {
                    if (invItemComingFrom != null && invItemComingFrom.myInventory != null)
                        invItemComingFrom.UpdateInventoryWeightAndVolume();

                    characterManager.personalInventory.UpdateCurrentWeightAndVolume();
                    StartCoroutine(gm.playerInvUI.PlayAddItemEffect(oldItemData.item.pickupSprite, null, gm.playerInvUI.personalInventorySideBarButton));

                    if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                        gm.flavorText.WriteLine_Unequip(oldItemData, characterManager);
                }
                else if (itemWasAddedToInv == false) // If we can't add it to the Inventory, drop it, but first we need to run the rest of the code in this method, so we'll just set a bool for now
                    shouldDropItem = true;
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
                    if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                        gm.flavorText.WriteLine_Unequip(oldItemData, characterManager);

                    gm.dropItemController.ForceDropNearest(characterManager, oldItemData, oldItemData.currentStackSize, invComingFrom, invItemComingFrom);
                }
                else if (shouldDropItem && canDropItem)
                {
                    if (gm.playerManager.CanSee(characterManager.spriteRenderer))
                        gm.flavorText.WriteLine_Unequip(oldItemData, characterManager);

                    gm.dropItemController.DropItem(characterManager, characterManager.transform.position, oldItemData, oldItemData.currentStackSize, invComingFrom, invItemComingFrom);
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

        // Update the character's total weight/volume
        characterManager.SetTotalCarriedWeightAndVolume();

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
                gm.playerInvUI.TemporarilyDisableBag(characterManager, oldItemData);

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
                // If we're equipping a weapon to our left hand and we are two-handing, switch to the one-handed stance
                if (equipmentSlot == EquipmentSlot.LeftHandItem && isTwoHanding)
                    characterManager.QueueAction(characterManager.humanoidSpriteManager.SwapStance(this, characterManager, currentEquipment[(int)EquipmentSlot.RightHandItem]), gm.apManager.GetSwapStanceAPCost(characterManager));
            }
            else if (newItemData.item.IsBag())
            {
                // If the new equipment is a bag and this is the player equipping it, then assign the bag's inventory in PlayerInventoryUI
                if (equipmentSlot == EquipmentSlot.Backpack)
                    characterManager.backpackInventory = newItemData.bagInventory;
                else if (equipmentSlot == EquipmentSlot.LeftHipPouch)
                    characterManager.leftHipPouchInventory = newItemData.bagInventory;
                else if (equipmentSlot == EquipmentSlot.RightHipPouch)
                    characterManager.rightHipPouchInventory = newItemData.bagInventory;
                else if (equipmentSlot == EquipmentSlot.Quiver)
                    characterManager.quiverInventory = newItemData.bagInventory;
            }
        }
        else if (oldItemData.item.IsBag()) // If we aren't able to assign the equipment
        {
            gm.playerInvUI.ReenableBag(characterManager, oldItemData); // Re-enable the bag, since we disabled it earlier

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
        
        // If the new equipment is a bag and this is the player equipping it, then assign the bag's inventory in PlayerInventoryUI
        if (equipmentSlot == EquipmentSlot.Backpack)
            characterManager.backpackInventory = null;
        else if (equipmentSlot == EquipmentSlot.LeftHipPouch)
            characterManager.leftHipPouchInventory = null;
        else if (equipmentSlot == EquipmentSlot.RightHipPouch)
            characterManager.rightHipPouchInventory = null;
        else if (equipmentSlot == EquipmentSlot.Quiver)
            characterManager.quiverInventory = null;
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
        if (currentEquipment[(int)EquipmentSlot.RightHandItem] != null && currentEquipment[(int)EquipmentSlot.RightHandItem].item.IsWeapon())
        {
            Weapon weapon = (Weapon)currentEquipment[(int)EquipmentSlot.RightHandItem].item;
            if (weapon.IsTwoHanded(characterManager))
                return true;
        }
        else if (currentEquipment[(int)EquipmentSlot.LeftHandItem] != null && currentEquipment[(int)EquipmentSlot.LeftHandItem].item.IsWeapon())
        {
            Weapon weapon = (Weapon)currentEquipment[(int)EquipmentSlot.LeftHandItem].item;
            if (weapon.IsTwoHanded(characterManager))
                return true;
        }

        return false;
    }

    public bool IsDualWielding()
    {
        if (currentEquipment[(int)EquipmentSlot.RightHandItem] != null && currentEquipment[(int)EquipmentSlot.RightHandItem].item.IsWeapon() 
            && currentEquipment[(int)EquipmentSlot.LeftHandItem] != null && currentEquipment[(int)EquipmentSlot.LeftHandItem].item.IsWeapon())
            return true;

        return false;
    }

    public bool RightHandItemEquipped()
    {
        if (currentEquipment[(int)EquipmentSlot.RightHandItem] != null)
            return true;

        return false;
    }

    public bool LeftHandItemEquipped()
    {
        if (currentEquipment[(int)EquipmentSlot.LeftHandItem] != null)
            return true;

        return false;
    }

    public bool MeleeWeaponEquipped()
    {
        if ((currentEquipment[(int)EquipmentSlot.LeftHandItem] != null && currentEquipment[(int)EquipmentSlot.LeftHandItem].item.IsWeapon())
            || (currentEquipment[(int)EquipmentSlot.RightHandItem] != null && currentEquipment[(int)EquipmentSlot.RightHandItem].item.IsWeapon()))
            return true;

        return false;
    }

    public bool ShieldEquipped()
    {
        if ((currentEquipment[(int)EquipmentSlot.LeftHandItem] != null && currentEquipment[(int)EquipmentSlot.LeftHandItem].item.IsShield())
            || (currentEquipment[(int)EquipmentSlot.RightHandItem] != null && currentEquipment[(int)EquipmentSlot.RightHandItem].item.IsShield()))
            return true;

        return false;
    }

    /// <summary> Returns a melee weapon's slash, blunt, pierce or cleave damage based on the melee attack type. </summary>
    public int GetPhysicalMeleeDamage(ItemData weaponsItemData, MeleeAttackType meleeAttackType, PhysicalDamageType physicalDamageType)
    {
        if (weaponsItemData == null)
            return 0;

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
                else
                    damage = characterManager.characterStats.unarmedDamage.GetValue();
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
        if (currentEquipment[(int)EquipmentSlot.RightHandItem] == null)
            return null;
        return (Weapon)currentEquipment[(int)EquipmentSlot.RightHandItem].item;
    }

    public Weapon GetLeftWeapon()
    {
        if (currentEquipment[(int)EquipmentSlot.LeftHandItem] == null)
            return null;
        return (Weapon)currentEquipment[(int)EquipmentSlot.LeftHandItem].item;
    }

    public Shield GetRightShield()
    {
        if (currentEquipment[(int)EquipmentSlot.RightHandItem] == null)
            return null;
        return (Shield)currentEquipment[(int)EquipmentSlot.RightHandItem].item;
    }

    public Shield GetLeftShield()
    {
        if (currentEquipment[(int)EquipmentSlot.LeftHandItem] == null)
            return null;
        return (Shield)currentEquipment[(int)EquipmentSlot.LeftHandItem].item;
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

                // If the new weapon is two-handed for the character and they don't already have a left weapon/shield equipped
                if (LeftHandItemEquipped() == false && weapon.IsTwoHanded(characterManager))
                {
                    characterManager.humanoidSpriteManager.SetupTwoHandedWeaponStance(this, characterManager);
                }
                // If the new weapon is one-handed and our right weapon is now null, remove our right weapon sprite and set our character's sprites to the one-handed stance
                else if (equipSlot == EquipmentSlot.LeftHandItem && currentEquipment[(int)EquipmentSlot.RightHandItem] == null)
                {
                    RemoveEquipmentSprite(EquipmentSlot.RightHandItem);
                    characterManager.humanoidSpriteManager.SetupOneHandedWeaponStance(this, characterManager);
                }
                // If the new weapon is one-handed, just set our character's sprites to the one-handed stance
                else if (weapon != null && weapon.IsTwoHanded(characterManager) == false)
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
                if (weapon != null && weapon.IsTwoHanded(characterManager))
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

    public IEnumerator SheatheWeapons(bool sheatheLeft, bool sheatheRight)
    {
        bool canSheatheLeft = false;
        if (sheatheLeft && LeftWeaponSheathed() == false && currentEquipment[(int)EquipmentSlot.LeftHandItem] != null)
            canSheatheLeft = true;
        bool canSheatheRight = false;
        if (sheatheRight && RightWeaponSheathed() == false && currentEquipment[(int)EquipmentSlot.RightHandItem] != null)
            canSheatheRight = true;

        if (canSheatheLeft || canSheatheRight)
        {
            if (characterManager.status.isDead) yield break;
            
            if (canSheatheRight)
            {
                Weapon weapon = (Weapon)currentEquipment[(int)EquipmentSlot.RightHandItem].item;
                if (isTwoHanding || weapon.IsTwoHanded(characterManager))
                    characterManager.humanoidSpriteManager.SetupOneHandedWeaponStance(this, characterManager);
                characterManager.humanoidSpriteManager.RemoveSprite(EquipmentSlot.RightHandItem);
                if (characterManager.isNPC == false && gm.playerInvUI.activePlayerInvSideBarButton.playerInventoryType == PlayerInventoryType.EquippedItems)
                    currentEquipment[(int)EquipmentSlot.RightHandItem].GetItemDatasInventoryItem().UpdateAllItemTexts();
            }

            if (canSheatheLeft)
            {
                characterManager.humanoidSpriteManager.RemoveSprite(EquipmentSlot.LeftHandItem);
                if (characterManager.isNPC == false && gm.playerInvUI.activePlayerInvSideBarButton.playerInventoryType == PlayerInventoryType.EquippedItems)
                    currentEquipment[(int)EquipmentSlot.LeftHandItem].GetItemDatasInventoryItem().UpdateAllItemTexts();
            }

            if (characterManager.isNPC == false)
            {
                if (canSheatheLeft && canSheatheRight)
                    gm.flavorText.WriteLine_SheatheWeapon(currentEquipment[(int)EquipmentSlot.LeftHandItem], currentEquipment[(int)EquipmentSlot.RightHandItem]);
                else if (canSheatheLeft)
                    gm.flavorText.WriteLine_SheatheWeapon(currentEquipment[(int)EquipmentSlot.LeftHandItem], null);
                else if (canSheatheRight)
                    gm.flavorText.WriteLine_SheatheWeapon(currentEquipment[(int)EquipmentSlot.RightHandItem], null);
            }
        }

        characterManager.FinishAction();
    }

    public IEnumerator UnsheatheWeapons()
    {
        if (BothWeaponsSheathed() && (currentEquipment[(int)EquipmentSlot.RightHandItem] != null || currentEquipment[(int)EquipmentSlot.LeftHandItem] != null))
        {
            if (characterManager.status.isDead) yield break;

            characterManager.DropAllCarriedItems();

            if (currentEquipment[(int)EquipmentSlot.RightHandItem] != null)
            {
                Weapon weapon = (Weapon)currentEquipment[(int)EquipmentSlot.RightHandItem].item;
                if (weapon.IsTwoHanded(characterManager))
                    characterManager.humanoidSpriteManager.SetupTwoHandedWeaponStance(this, characterManager);
                characterManager.humanoidSpriteManager.AssignSprite(EquipmentSlot.RightHandItem, weapon, this);
                if (characterManager.isNPC == false && gm.playerInvUI.activePlayerInvSideBarButton.playerInventoryType == PlayerInventoryType.EquippedItems)
                    currentEquipment[(int)EquipmentSlot.RightHandItem].GetItemDatasInventoryItem().UpdateAllItemTexts();
            }

            if (currentEquipment[(int)EquipmentSlot.LeftHandItem] != null)
            {
                characterManager.humanoidSpriteManager.SetupOneHandedWeaponStance(this, characterManager);
                characterManager.humanoidSpriteManager.AssignSprite(EquipmentSlot.LeftHandItem, (Equipment)currentEquipment[(int)EquipmentSlot.LeftHandItem].item, this);
                if (characterManager.isNPC == false && gm.playerInvUI.activePlayerInvSideBarButton.playerInventoryType == PlayerInventoryType.EquippedItems)
                    currentEquipment[(int)EquipmentSlot.LeftHandItem].GetItemDatasInventoryItem().UpdateAllItemTexts();
            }
            
            if (characterManager.isNPC == false)
                gm.flavorText.WriteLine_UnheatheWeapon(currentEquipment[(int)EquipmentSlot.LeftHandItem], currentEquipment[(int)EquipmentSlot.RightHandItem]);
        }

        characterManager.FinishAction();
    }

    public bool LeftWeaponSheathed()
    {
        if (currentEquipment[(int)EquipmentSlot.LeftHandItem] != null && characterManager.humanoidSpriteManager.leftHandItem.sprite == null)
            return true;
        return false;
    }

    public bool RightWeaponSheathed()
    {
        if ((currentEquipment[(int)EquipmentSlot.RightHandItem] != null || currentEquipment[(int)EquipmentSlot.Ranged] != null) && characterManager.humanoidSpriteManager.rightHandItem.sprite == null)
            return true;
        return false;
    }

    public bool BothWeaponsSheathed()
    {
        if ((currentEquipment[(int)EquipmentSlot.LeftHandItem] == null || LeftWeaponSheathed()) 
            && ((currentEquipment[(int)EquipmentSlot.RightHandItem] == null && currentEquipment[(int)EquipmentSlot.Ranged] == null) || RightWeaponSheathed()))
            return true;
        return false;
    }

    void SetupStartingEquipment()
    {
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Helmet], EquipmentSlot.Helmet);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Shirt], EquipmentSlot.Shirt);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Pants], EquipmentSlot.Pants);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Boots], EquipmentSlot.Boots);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Gloves], EquipmentSlot.Gloves);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.BodyArmor], EquipmentSlot.BodyArmor);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.LegArmor], EquipmentSlot.LegArmor);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.LeftHandItem], EquipmentSlot.LeftHandItem);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.RightHandItem], EquipmentSlot.RightHandItem);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Ranged], EquipmentSlot.Ranged);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Quiver], EquipmentSlot.Quiver);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Backpack], EquipmentSlot.Backpack);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.LeftHipPouch], EquipmentSlot.LeftHipPouch);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.RightHipPouch], EquipmentSlot.RightHipPouch);
        EquipNewEquipment(startingEquipment[(int)EquipmentSlot.Cape], EquipmentSlot.Cape);

        if (startingEquipment[(int)EquipmentSlot.RightHandItem] != null)
        {
            Weapon weapon = (Weapon)startingEquipment[(int)EquipmentSlot.RightHandItem];
            if (weapon.IsTwoHanded(characterManager))
                characterManager.humanoidSpriteManager.SetupTwoHandedWeaponStance(this, characterManager);
        }
    }

    void EquipNewEquipment(Equipment equipment, EquipmentSlot equipSlot)
    {
        if (equipment != null)
        {
            ItemData newItemData = gm.objectPoolManager.GetItemDataFromPool(equipment, null);
            newItemData.gameObject.SetActive(true);
            newItemData.transform.SetParent(itemsParent);
            newItemData.item = equipment;
            newItemData.RandomizeData();

            currentEquipment[(int)equipSlot] = newItemData;
            SetEquipmentSprite(equipSlot, equipment);

            #if UNITY_EDITOR
                newItemData.gameObject.name = newItemData.GetItemName(newItemData.currentStackSize);
            #endif
            
            if ((equipment.IsWeapon() || equipment.IsWeapon()) && onWeaponChanged != null)
                onWeaponChanged.Invoke(newItemData, null);
            else if (equipment.IsWearable() && onWearableChanged != null)
                onWearableChanged.Invoke(newItemData, null);
        }
    }

    public void OnEquipmentChanged(ItemData oldItemData, ItemData newItemData)
    {
        Equipment equipment = null;
        if (newItemData != null)
            equipment = (Equipment)newItemData.item;
        else
            equipment = (Equipment)oldItemData.item;

        if (onWearableChanged != null && equipment.IsWeapon() == false && equipment.IsShield() == false)
            onWearableChanged.Invoke(newItemData, oldItemData);
        else if (onWeaponChanged != null && (equipment.IsWeapon() || equipment.IsShield()))
            onWeaponChanged.Invoke(newItemData, oldItemData);
    }

    public virtual void OnWearableChanged(ItemData newItemData, ItemData oldItemData)
    {
        if (newItemData != null)
        {
            if (newItemData.item.IsBag() == false)
            {
                Wearable wearable = (Wearable)newItemData.item;

                if (newItemData.pocketsVolume > 0)
                {
                    // Add to personal inventory volume
                    characterManager.characterStats.maxPersonalInvVolume.AddModifier(newItemData.pocketsVolume);
                    characterManager.personalInventory.maxVolume = characterManager.characterStats.maxPersonalInvVolume.GetValue();
                }

                // Add to clothing or armor defense
                #region Defense
                if (wearable.isClothing)
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Clothing.AddModifier(newItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Clothing.AddModifier(newItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Clothing.AddModifier(newItemData.tertiaryDefense);
                    }
                }
                else
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Armor.AddModifier(newItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Armor.AddModifier(newItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Armor.AddModifier(newItemData.tertiaryDefense);
                    }
                }
                #endregion
            }
        }

        if (oldItemData != null)
        {
            if (oldItemData.item.IsBag() == false)
            {
                Wearable wearable = (Wearable)oldItemData.item;

                if (oldItemData.pocketsVolume > 0)
                {
                    // Subtract from personal inventory volume
                    characterManager.characterStats.maxPersonalInvVolume.RemoveModifier(oldItemData.pocketsVolume);
                    characterManager.personalInventory.maxVolume = characterManager.characterStats.maxPersonalInvVolume.GetValue();

                    // If the personal inventory is now over its volume limit, drop items until the volume amount is fine
                    if (characterManager.personalInventory.currentVolume > characterManager.personalInventory.maxVolume)
                    {
                        // Show some flavor text explaining why you're dropping the items
                        if (characterManager.isNPC == false)
                            gm.flavorText.WriteLine_DroppingPersonalItems();

                        characterManager.personalInventory.DropExcessItems();
                    }
                }

                // Subtract from clothing or armor defense
                #region Defense
                if (wearable.isClothing)
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(oldItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(oldItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Clothing.RemoveModifier(oldItemData.tertiaryDefense);
                    }
                }
                else
                {
                    for (int i = 0; i < wearable.primaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.primaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(oldItemData.primaryDefense);
                    }

                    for (int i = 0; i < wearable.secondaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.secondaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(oldItemData.secondaryDefense);
                    }

                    for (int i = 0; i < wearable.tertiaryBodyPartsCovered.Length; i++)
                    {
                        characterManager.status.GetBodyPart(wearable.tertiaryBodyPartsCovered[i]).addedDefense_Armor.RemoveModifier(oldItemData.tertiaryDefense);
                    }
                }
                #endregion
            }
        }
    }

    public virtual void OnWeaponChanged(ItemData newItemData, ItemData oldItemData)
    {

    }
}

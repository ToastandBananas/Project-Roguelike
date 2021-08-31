using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public List<IEnumerator> actions = new List<IEnumerator>();
    public List<int> queuedAP = new List<int>();

    public bool isNamed;

    [HideInInspector] public Alliances alliances;
    [HideInInspector] public SpriteManager spriteManager;
    [HideInInspector] public HumanoidSpriteManager humanoidSpriteManager;
    [HideInInspector] public EquipmentManager equipmentManager;
    [HideInInspector] public Movement movement;
    [HideInInspector] public Attack attack;
    [HideInInspector] public NPCAttack npcAttack;
    [HideInInspector] public NPCMovement npcMovement;
    [HideInInspector] public StateController stateController;
    [HideInInspector] public CharacterStats characterStats;
    [HideInInspector] public Nutrition nutrition;
    [HideInInspector] public Status status;
    [HideInInspector] public Vision vision;

    [HideInInspector] public Inventory personalInventory, backpackInventory, leftHipPouchInventory, rightHipPouchInventory, quiverInventory;
    [HideInInspector] public List<ItemData> carriedItems = new List<ItemData>();
    [HideInInspector] public float leftHandCarryPercent, rightHandCarryPercent;
    [HideInInspector] public float totalCarryWeight, totalCarryVolume;

    [HideInInspector] public CircleCollider2D circleCollider;
    [HideInInspector] public Rigidbody2D rigidBody;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Transform appliedItemsParent;

    [HideInInspector] public bool isNPC { get; private set; }
    [HideInInspector] public bool isMyTurn = false;
    [HideInInspector] public bool isPerformingAction;

    [HideInInspector] public GameManager gm;

    public virtual void Awake()
    {
        if (gameObject.CompareTag("NPC"))
            isNPC = true;

        circleCollider = GetComponent<CircleCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        appliedItemsParent = transform.Find("Applied Items");

        alliances = GetComponent<Alliances>();
        attack = GetComponent<Attack>();
        characterStats = GetComponent<CharacterStats>();
        characterStats.characterManager = this;
        characterStats.spriteRenderer = spriteRenderer;
        movement = GetComponent<Movement>();
        status = GetComponent<Status>();
        status.characterManager = this;
        vision = GetComponentInChildren<Vision>();

        humanoidSpriteManager = transform.GetComponentInChildren<HumanoidSpriteManager>();
        if (humanoidSpriteManager != null)
            spriteManager = humanoidSpriteManager;
        else
            spriteManager = transform.GetComponentInChildren<SpriteManager>();

        if (isNPC)
        {
            npcAttack = (NPCAttack)attack;
            npcMovement = (NPCMovement)movement;
            stateController = GetComponent<StateController>();
        }

        TryGetComponent(out nutrition);
        TryGetComponent(out equipmentManager);
        TryGetComponent(out personalInventory);
        if (equipmentManager != null)
            equipmentManager.characterManager = this;
    }

    public virtual void Start()
    {
        gm = GameManager.instance;

        if (personalInventory != null)
        {
            if (isNPC)
                personalInventory.myInvUI = gm.containerInvUI;
            else
                personalInventory.myInvUI = gm.playerInvUI;
            
            personalInventory.maxVolume = characterStats.maxPersonalInvVolume.GetValue();
        }

        GameTiles.AddCharacter(this, transform.position);
    }

    public bool IsNextToPlayer()
    {
        int distX = Mathf.RoundToInt(Mathf.Abs(transform.position.x - gm.playerManager.transform.position.x));
        int distY = Mathf.RoundToInt(Mathf.Abs(transform.position.y - gm.playerManager.transform.position.y));

        if (distX <= 1 && distY <= 1)
            return true;

        return false;
    }

    #region Inventory
    public IEnumerator CarryItem(ItemData itemData, InventoryItem invItem)
    {
        // Make sure we have room in our hands to carry the item, otherwise yield break out of this method and show some flavor text
        if (HaveRoomInHandsToCarryItem(itemData, itemData.currentStackSize) == false)
        {
            if (isNPC == false)
                gm.flavorText.WriteLine_CantCarryItem(this, itemData, itemData.currentStackSize);
            yield break;
        }

        float carryPercent = itemData.item.GetSizeFactor() * itemData.currentStackSize;

        // Sheathe/stow away weapons or shields if necessary
        if (equipmentManager.isTwoHanding || equipmentManager.TwoHandedWeaponEquipped())
            QueueAction(equipmentManager.SheatheWeapons(false, true), gm.apManager.GetSheatheWeaponAPCost(equipmentManager, false, true));
        else if (leftHandCarryPercent + carryPercent <= 1f)
            QueueAction(equipmentManager.SheatheWeapons(true, false), gm.apManager.GetSheatheWeaponAPCost(equipmentManager, true, false));
        else
            QueueAction(equipmentManager.SheatheWeapons(true, true), gm.apManager.GetSheatheWeaponAPCost(equipmentManager, true, true));

        if (itemData.bagInventory != null)
            gm.apManager.LoseAP(this, gm.apManager.GetTransferItemCost(itemData.item, itemData.currentStackSize, itemData.bagInventory.currentWeight, itemData.bagInventory.currentVolume, false));
        else
            gm.apManager.LoseAP(this, gm.apManager.GetTransferItemCost(itemData.item, itemData.currentStackSize, 0, 0, false));

        // If we can carry the item, add to the handCarryPercent value based off of item size
        if (leftHandCarryPercent + carryPercent <= 1f)
            leftHandCarryPercent += carryPercent;
        else if (rightHandCarryPercent + carryPercent <= 1f)
            rightHandCarryPercent += carryPercent;
        else
        {
            carryPercent -= (1f - leftHandCarryPercent);
            leftHandCarryPercent = 1f;
            rightHandCarryPercent += carryPercent;
        }

        // Add the item to our carriedItems list
        ItemData carriedItemData = gm.uiManager.CreateNewItemDataChild(itemData, null, gm.playerManager.personalInventory.itemsParent, false);
        carriedItems.Add(carriedItemData);
        carriedItemData.parentInventory = personalInventory;
        carriedItemData.parentInventory.inventoryOwner = this;
        
        // Remove the old itemData from its inventory or from the ground
        if (itemData.parentInventory != null)
            itemData.parentInventory.RemoveItem(itemData, itemData.currentStackSize, invItem);
        else
            GameTiles.RemoveItemData(itemData, itemData.transform.position);

        // If the item we're picking up is a bag on the ground, remove it from the ground
        if (itemData.IsPickup() && itemData.item.IsBag())
            gm.containerInvUI.RemoveBagFromGround(itemData.bagInventory);

        // Clear out the item and its InventoryItem
        if (invItem != null)
            invItem.ClearItem();
        else
            itemData.ReturnToObjectPool();

        // If our Personal Inventory is active in the UI, update it to show the new carried item
        if (gm.playerInvUI.activeInventory == gm.playerManager.personalInventory)
            gm.playerInvUI.ShowNewInventoryItem(carriedItemData);

        // Update total weight and volume and the UI
        StartCoroutine(DelaySetTotalCarriedWeightAndVolume());
        gm.containerInvUI.UpdateUI();

        // Show flavor text for picking up and carrying the item
        if (gm.playerManager.CanSee(spriteRenderer))
            gm.flavorText.WriteLine_CarryItem(this, carriedItemData);
    }

    public bool HaveRoomInHandsToCarryItem(ItemData itemData, int itemCount)
    {
        BodyPart leftHand = status.GetBodyPart(BodyPartType.LeftHand);
        BodyPart rightHand = status.GetBodyPart(BodyPartType.RightHand);
        if ((leftHand.isIncapacitated || leftHand.isSevered) && (rightHand.isIncapacitated || rightHand.isSevered))
            return false;
        else if (leftHand.isSevered || leftHand.isIncapacitated)
        {
            if ((float)(itemData.item.GetSizeFactor() * itemCount) <= 1f - rightHandCarryPercent)
                return true;
        }
        else if (rightHand.isSevered || rightHand.isIncapacitated)
        {
            if ((float)(itemData.item.GetSizeFactor() * itemCount) <= 1f - leftHandCarryPercent)
                return true;
        }
        else if ((float)(itemData.item.GetSizeFactor() * itemCount) <= 2f - leftHandCarryPercent - rightHandCarryPercent)
            return true;
        return false;
    }

    public void RemoveCarriedItem(ItemData itemData, int itemCount)
    {
        if (carriedItems.Contains(itemData))
        {
            if (itemData.currentStackSize - itemCount <= 0)
                carriedItems.Remove(itemData);

            // Subtract from carry percentages
            float carryPercent = itemData.item.GetSizeFactor() * itemCount;

            if (rightHandCarryPercent - carryPercent >= 0f)
                rightHandCarryPercent -= carryPercent;
            else
            {
                carryPercent -= rightHandCarryPercent;
                rightHandCarryPercent = 0f;
                leftHandCarryPercent -= carryPercent;
            }

            if (leftHandCarryPercent < 0)
                leftHandCarryPercent = 0;
            if (rightHandCarryPercent < 0)
                rightHandCarryPercent = 0;

            StartCoroutine(DelaySetTotalCarriedWeightAndVolume());
        }
    }

    public void DropAllCarriedItems(ItemData exception = null)
    {
        if (carriedItems.Count > 0)
        {
            for (int i = carriedItems.Count - 1; i >= 0; i--)
            {
                if (carriedItems[i] != exception)
                {
                    ItemData carriedItem = carriedItems[i];
                    gm.dropItemController.ForceDropNearest(this, carriedItem, carriedItem.currentStackSize, null, carriedItem.GetItemDatasInventoryItem());
                    RemoveCarriedItem(carriedItem, carriedItem.currentStackSize);

                    InventoryItem invItem = carriedItem.GetItemDatasInventoryItem();
                    if (invItem != null)
                        invItem.ClearItem();
                    else
                        carriedItem.ReturnToObjectPool();

                    gm.containerInvUI.UpdateUI();
                }
            }
        }
    }

    public bool TryAddingItemToInventory(ItemData itemData, Inventory itemDatasInventory, bool canAddToQuiver)
    {
        bool itemAdded = false;
        if (itemData.item.IsKey() && isNPC == false)
        {
            gm.playerManager.keysInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);
            StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.keysSideBarButton));
            return true;
        }
        else if (itemData.item.itemType == ItemType.Ammo && canAddToQuiver)
            itemAdded = quiverInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.quiverSidebarButton));
            return true;
        }
        else if (backpackInventory != null)
            itemAdded = backpackInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));
            return true;
        }
        else if (leftHipPouchInventory != null)
            itemAdded = leftHipPouchInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));
            return true;
        }
        else if (rightHipPouchInventory != null)
            itemAdded = rightHipPouchInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));
            return true;
        }
        else if (personalInventory != null)
            itemAdded = personalInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.personalInventorySideBarButton));
            return true;
        }
        else if (canAddToQuiver && quiverInventory != null)
            itemAdded = quiverInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.quiverSidebarButton));
            return true;
        }

        return false;
    }

    public void AddItemToOtherBags(ItemData itemToAdd, InventoryItem invItem)
    {
        if (itemToAdd != null)
        {
            if (invItem == null)
                invItem = itemToAdd.GetItemDatasInventoryItem();

            // Cache the item in case the itemData gets cleared out before we can do the add item effect
            Item itemAdding = itemToAdd.item;

            // If the item is ammunition, try adding here first
            if (itemAdding.itemType == ItemType.Ammo && quiverInventory != null && itemToAdd.currentStackSize > 0)
            {
                if (quiverInventory.AddItemToInventory_OneAtATime(this, itemToAdd.parentInventory, itemToAdd, invItem))
                {
                    quiverInventory.UpdateCurrentWeightAndVolume();
                    if (isNPC == false)
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.quiverSidebarButton));
                }
            }

            // Try adding to the active inventory first, if this is the player
            if (isNPC == false && itemToAdd.currentStackSize > 0 && gm.playerInvUI.activeInventory != null && (gm.playerInvUI.activeInventory != gm.playerManager.keysInventory || itemToAdd.item.itemType == ItemType.Key)
                && itemToAdd.item.maxStackSize > 1 && gm.playerInvUI.activeInventory != gm.playerManager.quiverInventory)
            {
                if (gm.playerInvUI.activeInventory.AddItemToInventory_OneAtATime(this, itemToAdd.parentInventory, itemToAdd, invItem))
                {
                    gm.playerInvUI.activeInventory.UpdateCurrentWeightAndVolume();
                    StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.activePlayerInvSideBarButton));
                }
            }

            // Now try to fit the item in other equipped bags
            if (itemToAdd.currentStackSize > 0 && backpackInventory != null && gm.playerInvUI.activeInventory != backpackInventory)
            {
                if (backpackInventory.AddItemToInventory_OneAtATime(this, itemToAdd.parentInventory, itemToAdd, invItem))
                {
                    backpackInventory.UpdateCurrentWeightAndVolume();
                    if (isNPC == false)
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));
                }
            }

            if (itemToAdd.currentStackSize > 0 && leftHipPouchInventory != null && gm.playerInvUI.activeInventory != leftHipPouchInventory)
            {
                if (leftHipPouchInventory.AddItemToInventory_OneAtATime(this, itemToAdd.parentInventory, itemToAdd, invItem))
                {
                    leftHipPouchInventory.UpdateCurrentWeightAndVolume();
                    if (isNPC == false)
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));
                }
            }

            if (itemToAdd.currentStackSize > 0 && rightHipPouchInventory != null && gm.playerInvUI.activeInventory != rightHipPouchInventory)
            {
                if (rightHipPouchInventory.AddItemToInventory_OneAtATime(this, itemToAdd.parentInventory, itemToAdd, invItem))
                {
                    rightHipPouchInventory.UpdateCurrentWeightAndVolume();
                    if (isNPC == false)
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));
                }
            }

            // Now try to fit the item in the player's personal inventory
            if (itemToAdd.currentStackSize > 0 && personalInventory != null && gm.playerInvUI.activeInventory != personalInventory)
            {
                if (personalInventory.AddItemToInventory_OneAtATime(this, itemToAdd.parentInventory, itemToAdd, invItem))
                {
                    gm.playerManager.personalInventory.UpdateCurrentWeightAndVolume();
                    if (isNPC == false)
                        StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemAdding.pickupSprite, null, gm.playerInvUI.personalInventorySideBarButton));
                }
            }

            // If the item is an equippable bag that was on the ground, set the container menu's active inventory to null and setup the sidebar icon
            if (itemToAdd.currentStackSize == 0 && itemAdding.IsBag() && gm.containerInvUI.activeInventory == itemToAdd.bagInventory)
                gm.containerInvUI.RemoveBagFromGround(itemToAdd.bagInventory);
        }
    }

    public List<ItemData> GetMedicalSupplies(MedicalSupplyType medicalSupplyType)
    {
        List<ItemData> medicalItems = new List<ItemData>();
        for (int i = 0; i < personalInventory.items.Count; i++)
        {
            if (personalInventory.items[i].item.IsMedicalSupply())
            {
                MedicalSupply medSupply = (MedicalSupply)personalInventory.items[i].item;
                if (medSupply.medicalSupplyType == medicalSupplyType)
                    medicalItems.Add(personalInventory.items[i]);
            }
            else if (personalInventory.items[i].bagInventory != null)
            {
                for (int j = 0; j < personalInventory.items[i].bagInventory.items.Count; j++)
                {
                    MedicalSupply medSupply = (MedicalSupply)personalInventory.items[i].bagInventory.items[j].item;
                    if (medSupply.medicalSupplyType == medicalSupplyType)
                        medicalItems.Add(personalInventory.items[i].bagInventory.items[j]);
                }
            }
        }

        if (backpackInventory != null)
        {
            for (int i = 0; i < backpackInventory.items.Count; i++)
            {
                if (backpackInventory.items[i].item.IsMedicalSupply())
                {
                    MedicalSupply medSupply = (MedicalSupply)backpackInventory.items[i].item;
                    if (medSupply.medicalSupplyType == medicalSupplyType)
                        medicalItems.Add(backpackInventory.items[i]);
                }
                else if (backpackInventory.items[i].bagInventory != null)
                {
                    for (int j = 0; j < backpackInventory.items[i].bagInventory.items.Count; j++)
                    {
                        MedicalSupply medSupply = (MedicalSupply)backpackInventory.items[i].bagInventory.items[j].item;
                        if (medSupply.medicalSupplyType == medicalSupplyType)
                            medicalItems.Add(backpackInventory.items[i].bagInventory.items[j]);
                    }
                }
            }
        }

        if (leftHipPouchInventory != null)
        {
            for (int i = 0; i < leftHipPouchInventory.items.Count; i++)
            {
                if (leftHipPouchInventory.items[i].item.IsMedicalSupply())
                {
                    MedicalSupply medSupply = (MedicalSupply)leftHipPouchInventory.items[i].item;
                    if (medSupply.medicalSupplyType == medicalSupplyType)
                        medicalItems.Add(leftHipPouchInventory.items[i]);
                }
                else if (leftHipPouchInventory.items[i].bagInventory != null)
                {
                    for (int j = 0; j < leftHipPouchInventory.items[i].bagInventory.items.Count; j++)
                    {
                        MedicalSupply medSupply = (MedicalSupply)leftHipPouchInventory.items[i].bagInventory.items[j].item;
                        if (medSupply.medicalSupplyType == medicalSupplyType)
                            medicalItems.Add(leftHipPouchInventory.items[i].bagInventory.items[j]);
                    }
                }
            }
        }

        if (rightHipPouchInventory != null)
        {
            for (int i = 0; i < rightHipPouchInventory.items.Count; i++)
            {
                if (rightHipPouchInventory.items[i].item.IsMedicalSupply())
                {
                    MedicalSupply medSupply = (MedicalSupply)rightHipPouchInventory.items[i].item;
                    if (medSupply.medicalSupplyType == medicalSupplyType)
                        medicalItems.Add(rightHipPouchInventory.items[i]);
                }
                else if (rightHipPouchInventory.items[i].bagInventory != null)
                {
                    for (int j = 0; j < rightHipPouchInventory.items[i].bagInventory.items.Count; j++)
                    {
                        MedicalSupply medSupply = (MedicalSupply)rightHipPouchInventory.items[i].bagInventory.items[j].item;
                        if (medSupply.medicalSupplyType == medicalSupplyType)
                            medicalItems.Add(rightHipPouchInventory.items[i].bagInventory.items[j]);
                    }
                }
            }
        }

        if (quiverInventory != null)
        {
            for (int i = 0; i < quiverInventory.items.Count; i++)
            {
                if (quiverInventory.items[i].item.IsMedicalSupply())
                {
                    MedicalSupply medSupply = (MedicalSupply)quiverInventory.items[i].item;
                    if (medSupply.medicalSupplyType == medicalSupplyType)
                        medicalItems.Add(quiverInventory.items[i]);
                }
                else if (quiverInventory.items[i].bagInventory != null)
                {
                    for (int j = 0; j < quiverInventory.items[i].bagInventory.items.Count; j++)
                    {
                        MedicalSupply medSupply = (MedicalSupply)quiverInventory.items[i].bagInventory.items[j].item;
                        if (medSupply.medicalSupplyType == medicalSupplyType)
                            medicalItems.Add(quiverInventory.items[i].bagInventory.items[j]);
                    }
                }
            }
        }

        return medicalItems;
    }

    public void SetTotalCarriedWeightAndVolume()
    {
        SetTotalCarriedWeight();
        SetTotalCarriedVolume();
    }

    public IEnumerator DelaySetTotalCarriedWeightAndVolume()
    {
        yield return null;
        SetTotalCarriedWeightAndVolume();
        gm.playerInvUI.UpdateUI();
    }

    void SetTotalCarriedWeight()
    {
        totalCarryWeight = 0;

        if (personalInventory != null)
            totalCarryWeight += personalInventory.currentWeight;

        if (backpackInventory != null)
            totalCarryWeight += backpackInventory.currentWeight;

        if (leftHipPouchInventory != null)
            totalCarryWeight += leftHipPouchInventory.currentWeight;

        if (rightHipPouchInventory != null)
            totalCarryWeight += rightHipPouchInventory.currentWeight;

        if (quiverInventory != null)
            totalCarryWeight += quiverInventory.currentWeight;
        
        if (isNPC == false && gm.playerManager.keysInventory != null)
            totalCarryWeight += gm.playerManager.keysInventory.currentWeight;

        for (int i = 0; i < carriedItems.Count; i++)
        {
            totalCarryWeight += carriedItems[i].item.weight * carriedItems[i].currentStackSize * carriedItems[i].GetPercentRemaining_Decimal();
            if (carriedItems[i].item.IsBag() || carriedItems[i].item.IsPortableContainer())
                totalCarryWeight += gm.playerInvUI.GetTotalWeight(carriedItems[i].bagInventory.items);
        }

        totalCarryWeight += equipmentManager.currentWeight;
        totalCarryWeight = Mathf.RoundToInt(totalCarryWeight * 100f) / 100f;

        if (isNPC == false && IsOverEncumbered())
            gm.flavorText.WriteLine_OverEncumbered();
    }

    void SetTotalCarriedVolume()
    {
        totalCarryVolume = 0;

        if (personalInventory != null)
            totalCarryVolume += personalInventory.currentVolume;

        if (backpackInventory != null)
            totalCarryVolume += backpackInventory.currentVolume;

        if (leftHipPouchInventory != null)
            totalCarryVolume += leftHipPouchInventory.currentVolume;

        if (rightHipPouchInventory != null)
            totalCarryVolume += rightHipPouchInventory.currentVolume;

        if (quiverInventory != null)
            totalCarryVolume += quiverInventory.currentVolume;

        if (isNPC == false && gm.playerManager.keysInventory != null)
            totalCarryVolume += gm.playerManager.keysInventory.currentVolume;

        for (int i = 0; i < carriedItems.Count; i++)
        {
            totalCarryVolume += carriedItems[i].item.volume * carriedItems[i].GetPercentRemaining_Decimal() * carriedItems[i].currentStackSize;
            if (carriedItems[i].item.IsBag() || carriedItems[i].item.IsPortableContainer())
                totalCarryVolume += gm.playerInvUI.GetTotalVolume(carriedItems[i].bagInventory.items);
        }

        totalCarryVolume += equipmentManager.currentVolume;
        totalCarryVolume = Mathf.RoundToInt(totalCarryVolume * 100f) / 100f;
    }

    public bool IsOverEncumbered()
    {
        if (totalCarryWeight > characterStats.GetMaximumWeightCapacity())
            return true;
        return false;
    }

    public bool IsMyInventory(Inventory inv)
    {
        if (inv == personalInventory || inv == backpackInventory || inv == leftHipPouchInventory || inv == rightHipPouchInventory || inv == quiverInventory || (isNPC == false && inv == gm.playerManager.keysInventory))
            return true;
        return false;
    }
    #endregion

    #region Action Queue
    public void TakeTurn()
    {
        if (isMyTurn && status.isDead == false)
        {
            if (characterStats.currentAP <= 0)
                StartCoroutine(gm.turnManager.FinishTurn(this));
            else
            {
                vision.CheckEnemyVisibility();

                if (actions.Count > 0)
                    StartCoroutine(GetNextQueuedAction());
                else if (isNPC)
                    stateController.DoAction();
            }
        }
    }

    public void QueueAction(IEnumerator action, int APCost)
    {
        // if (isNPC) Debug.Log(name + " queued " + action);
        actions.Add(action);
        queuedAP.Add(APCost);
        if (isMyTurn && characterStats.currentAP > 0)
            StartCoroutine(GetNextQueuedAction());

        if (isNPC == false)
            gm.healthDisplay.UpdateLastAPUsed(APCost);
    }

    public IEnumerator GetNextQueuedAction()
    {
        if (status.isDead)
            yield break;

        if (actions.Count > 0 && isPerformingAction == false)
        {
            int APRemainder = characterStats.UseAPAndGetRemainder(queuedAP[0]);
            if (APRemainder <= 0)
            {
                isPerformingAction = true;
                yield return StartCoroutine(actions[0]);
                // if (isNPC == false) Debug.Log("Got next queued action. Actions still queued: " + actions.Count);
            }
            else
            {
                // if (isNPC == false) Debug.Log("Can't do next queued action yet. Remaining AP: " + APRemainder);
                queuedAP[0] = APRemainder;
                StartCoroutine(gm.turnManager.FinishTurn(this));
            }
        }
        else
            yield return null;
    }

    public void FinishAction()
    {
        if (queuedAP.Count > 0)
            queuedAP.Remove(queuedAP[0]);
        if (actions.Count > 0)
            actions.Remove(actions[0]);
        
        isPerformingAction = false;
        
        // If the character has no AP remaining, end their turn
        if (characterStats.currentAP <= 0)
            StartCoroutine(gm.turnManager.FinishTurn(this));
        else if (movement.isMoving == false) // Take another action
            TakeTurn();
    }

    public void ResetActionsQueue()
    {
        actions.Clear();
        queuedAP.Clear();
    }
    #endregion
}
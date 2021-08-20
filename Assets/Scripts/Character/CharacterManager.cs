using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
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
    [HideInInspector] public Status status;
    [HideInInspector] public Vision vision;

    [HideInInspector] public Inventory personalInventory, backpackInventory, leftHipPouchInventory, rightHipPouchInventory, quiverInventory;
    [HideInInspector] public List<ItemData> carriedItems = new List<ItemData>();
    float handCarryPercent;

    [HideInInspector] public CircleCollider2D circleCollider;
    [HideInInspector] public Rigidbody2D rigidBody;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Transform appliedItemsParent;

    [HideInInspector] public bool isNPC { get; private set; }
    [HideInInspector] public bool isMyTurn = false;
    [HideInInspector] public int actionsQueued { get; private set; }
    [HideInInspector] public int currentQueueNumber;

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
        
        TryGetComponent(out equipmentManager);
        TryGetComponent(out personalInventory);
        if (equipmentManager != null)
            equipmentManager.characterManager = this;

        ResetActionsQueue();
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

    public void TakeTurn()
    {
        if (isNPC && characterStats.currentAP > 0 && actionsQueued == 0 && movement.isMoving == false && status.isDead == false)
        {
            vision.CheckEnemyVisibility();
            stateController.DoAction();
        }
    }

    public bool IsNextToPlayer()
    {
        int distX = Mathf.RoundToInt(Mathf.Abs(transform.position.x - gm.playerManager.transform.position.x));
        int distY = Mathf.RoundToInt(Mathf.Abs(transform.position.y - gm.playerManager.transform.position.y));

        if (distX <= 1 && distY <= 1)
            return true;

        return false;
    }

    public void ResetActionsQueue()
    {
        actionsQueued = 0;
        currentQueueNumber = 0;
    }

    public IEnumerator CarryItem(ItemData itemData, InventoryItem invItem)
    {
        // Make sure we have room in our hands to carry the item, otherwise yield break out of this method and show some flavor text
        if (HaveRoomInHandsToCarryItem(itemData, itemData.currentStackSize) == false)
        {
            gm.flavorText.WriteCantCarryItemLine(itemData, itemData.currentStackSize);
            yield break;
        }

        // Sheathe/stow away any weapons or shields
        StartCoroutine(equipmentManager.SheatheWeapons());

        if (itemData.bagInventory != null)
            StartCoroutine(gm.apManager.UseAP(this, gm.apManager.GetTransferItemCost(itemData.item, itemData.currentStackSize, itemData.bagInventory.currentWeight, itemData.bagInventory.currentVolume, false)));
        else
            StartCoroutine(gm.apManager.UseAP(this, gm.apManager.GetTransferItemCost(itemData.item, itemData.currentStackSize, 0, 0, false)));

        int queueNumber = currentQueueNumber + actionsQueued;
        while (queueNumber != currentQueueNumber)
        {
            yield return null;
            if (status.isDead) yield break;
        }

        // If we can carry the item, add to the handCarryPercent value based off of item size
        handCarryPercent += itemData.item.GetSizeFactor() * itemData.currentStackSize;

        // Add the item to our carriedItems list
        ItemData carriedItemData = gm.uiManager.CreateNewItemDataChild(itemData, null, gm.playerManager.personalInventory.itemsParent, false);
        carriedItems.Add(carriedItemData);

        // Remove the old itemData from its inventory or from the ground
        if (itemData.parentInventory != null)
            itemData.parentInventory.RemoveItem(itemData, itemData.currentStackSize, invItem);
        else
            GameTiles.RemoveItemData(itemData, itemData.transform.position);

        // Clear out the item and its InventoryItem
        if (invItem != null)
            invItem.ClearItem();
        else
            itemData.ReturnToObjectPool();

        // If our Personal Inventory is active in the UI, update it to show the new carried item
        if (gm.playerInvUI.activeInventory == gm.playerManager.personalInventory)
        {
            gm.playerInvUI.ShowNewInventoryItem(carriedItemData);
            gm.playerInvUI.UpdateUI();
        }

        // Show flavor text for picking up and carrying the item
        gm.flavorText.WriteCarryItemLine(carriedItemData);
    }

    public bool HaveRoomInHandsToCarryItem(ItemData itemData, int itemCount)
    {
        BodyPart leftHand = status.GetBodyPart(BodyPartType.LeftHand);
        BodyPart rightHand = status.GetBodyPart(BodyPartType.RightHand);
        if ((leftHand.isIncapacitated || leftHand.isSevered) && (rightHand.isIncapacitated || rightHand.isSevered))
            return false;
        else if (leftHand.isSevered || leftHand.isIncapacitated || rightHand.isSevered || rightHand.isIncapacitated)
        {
            if (itemData.item.GetSizeFactor() * itemCount <= 1f - handCarryPercent)
                return true;
        }
        else if (itemData.item.GetSizeFactor() * itemCount <= 2f - handCarryPercent)
            return true;
        return false;
    }

    public void RemoveCarriedItem(ItemData itemData)
    {
        if (carriedItems.Contains(itemData))
        {
            carriedItems.Remove(itemData);
            handCarryPercent -= itemData.item.GetSizeFactor() * itemData.currentStackSize;
        }
    }

    public void DropAllCarriedItems()
    {
        if (carriedItems.Count > 0)
        {
            for (int i = carriedItems.Count - 1; i >= 0; i--)
            {
                ItemData carriedItem = carriedItems[i];
                gm.dropItemController.ForceDropNearest(this, carriedItem, carriedItem.currentStackSize, null, carriedItem.GetItemDatasInventoryItem());
                RemoveCarriedItem(carriedItem);
                InventoryItem invItem = carriedItem.GetItemDatasInventoryItem();
                if (invItem != null)
                    invItem.ClearItem();
                else
                    carriedItem.ReturnToObjectPool();
                gm.containerInvUI.UpdateUI();
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

    public void EditActionsQueued(int amount)
    {
        actionsQueued += amount;
        if (actionsQueued < 0)
            actionsQueued = 0;
    }
}

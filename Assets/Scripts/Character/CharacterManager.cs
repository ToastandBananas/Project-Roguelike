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
        spriteManager = transform.GetComponentInChildren<SpriteManager>();
        humanoidSpriteManager = (HumanoidSpriteManager)spriteManager;
        movement = GetComponent<Movement>();
        status = GetComponent<Status>();
        status.characterManager = this;
        vision = GetComponentInChildren<Vision>();

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

        personalInventory.maxWeight = characterStats.maxPersonalInvWeight.GetValue();
        personalInventory.maxVolume = characterStats.maxPersonalInvVolume.GetValue();

        ResetActionsQueue();
    }

    public virtual void Start()
    {
        gm = GameManager.instance;

        if (isNPC)
            GameTiles.AddNPC(this, transform.position);
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
            itemAdded = gm.playerManager.backpackInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.backpackSidebarButton));
            return true;
        }
        else if (leftHipPouchInventory != null)
            itemAdded = gm.playerManager.leftHipPouchInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.leftHipPouchSidebarButton));
            return true;
        }
        else if (rightHipPouchInventory != null)
            itemAdded = gm.playerManager.rightHipPouchInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

        if (itemAdded)
        {
            if (isNPC == false)
                StartCoroutine(gm.playerInvUI.PlayAddItemEffect(itemData.item.pickupSprite, null, gm.playerInvUI.rightHipPouchSidebarButton));
            return true;
        }
        else if (canAddToQuiver && quiverInventory != null)
            itemAdded = gm.playerManager.quiverInventory.AddItem(itemData, itemData.currentStackSize, itemDatasInventory, true);

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

    void OnMouseEnter()
    {
        if (status.isDead == false)
            gm.tileInfoDisplay.focusedCharacter = this;
    }

    void OnMouseExit()
    {
        if (gm.tileInfoDisplay.focusedCharacter == this && status.isDead == false)
            gm.tileInfoDisplay.focusedCharacter = null;
    }
}

using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Alliances alliances;
    [HideInInspector] public EquippedItemsSpriteManager equippedItemsSpriteManager;
    [HideInInspector] public EquipmentManager equipmentManager;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Movement movement;
    [HideInInspector] public NPCAttack npcAttack;
    [HideInInspector] public NPCMovement npcMovement;
    [HideInInspector] public StateController stateController;
    [HideInInspector] public CharacterStats characterStats;
    [HideInInspector] public Vision vision;

    [HideInInspector] public BoxCollider2D boxCollider;
    [HideInInspector] public Rigidbody2D rigidBody;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    [HideInInspector] public bool actionQueued;
    [HideInInspector] public bool isMyTurn = false;
    public int remainingAPToBeUsed;

    public virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        alliances = GetComponent<Alliances>();
        characterStats = GetComponent<CharacterStats>();
        equippedItemsSpriteManager = transform.GetComponentInChildren<EquippedItemsSpriteManager>();
        movement = GetComponent<Movement>();
        vision = GetComponentInChildren<Vision>();
        
        TryGetComponent(out equipmentManager);
        TryGetComponent(out inventory);
        TryGetComponent(out npcAttack);
        TryGetComponent(out npcMovement);
        TryGetComponent(out stateController);

        actionQueued = false;
    }

    public void TakeTurn()
    {
        if (actionQueued == false && movement.isMoving == false)
        {
            vision.CheckEnemyVisibility();
            stateController.DoAction();
        }
    }

    public virtual void Start()
    {
        
    }
}

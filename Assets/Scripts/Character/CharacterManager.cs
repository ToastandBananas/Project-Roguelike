using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Alliances alliances;
    [HideInInspector] public CharacterSpriteManager characterSpriteManager;
    [HideInInspector] public HumanoidSpriteManager humanoidSpriteManager;
    [HideInInspector] public EquipmentManager equipmentManager;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Movement movement;
    [HideInInspector] public NPCAttack npcAttack;
    [HideInInspector] public NPCMovement npcMovement;
    [HideInInspector] public StateController stateController;
    [HideInInspector] public CharacterStats characterStats;
    [HideInInspector] public Vision vision;

    [HideInInspector] public CircleCollider2D circleCollider;
    [HideInInspector] public Rigidbody2D rigidBody;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    [HideInInspector] public bool isNPC;
    [HideInInspector] public bool actionQueued;
    [HideInInspector] public bool isMyTurn = false;
    public int remainingAPToBeUsed;

    public virtual void Awake()
    {
        if (gameObject.CompareTag("NPC"))
            isNPC = true;

        circleCollider = GetComponent<CircleCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        alliances = GetComponent<Alliances>();
        characterStats = GetComponent<CharacterStats>();
        characterSpriteManager = transform.GetComponentInChildren<CharacterSpriteManager>();
        humanoidSpriteManager = (HumanoidSpriteManager)characterSpriteManager;
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

using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public bool isNamed;

    [HideInInspector] public Alliances alliances;
    [HideInInspector] public SpriteManager characterSpriteManager;
    [HideInInspector] public HumanoidSpriteManager humanoidSpriteManager;
    [HideInInspector] public EquipmentManager equipmentManager;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Movement movement;
    [HideInInspector] public Attack attack;
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
    [HideInInspector] public int remainingAPToBeUsed;

    GameManager gm;

    public virtual void Awake()
    {
        if (gameObject.CompareTag("NPC"))
            isNPC = true;

        circleCollider = GetComponent<CircleCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        alliances = GetComponent<Alliances>();
        attack = GetComponent<Attack>();
        characterStats = GetComponent<CharacterStats>();
        characterSpriteManager = transform.GetComponentInChildren<SpriteManager>();
        humanoidSpriteManager = (HumanoidSpriteManager)characterSpriteManager;
        movement = GetComponent<Movement>();
        vision = GetComponentInChildren<Vision>();

        if (isNPC)
        {
            npcAttack = (NPCAttack)attack;
            npcMovement = (NPCMovement)movement;
            TryGetComponent(out stateController);
        }

        TryGetComponent(out equipmentManager);
        TryGetComponent(out inventory);

        actionQueued = false;
    }

    public virtual void Start()
    {
        gm = GameManager.instance;

        if (isNPC)
            GameTiles.AddNPC(this, transform.position);
    }

    public void TakeTurn()
    {
        if (actionQueued == false && movement.isMoving == false && characterStats.isDeadOrDestroyed == false)
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
}

using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Alliances alliances;
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

    public virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();

        alliances = GetComponent<Alliances>();
        characterStats = GetComponent<CharacterStats>();
        movement = GetComponent<Movement>();
        vision = GetComponentInChildren<Vision>();

        TryGetComponent(out equipmentManager);
        TryGetComponent(out inventory);
        TryGetComponent(out npcAttack);
        TryGetComponent(out npcMovement);
        TryGetComponent(out stateController);
    }

    public virtual void Start()
    {
        
    }
}

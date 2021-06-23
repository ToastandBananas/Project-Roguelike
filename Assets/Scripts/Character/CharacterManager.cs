using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [HideInInspector] public Alliances alliances;
    [HideInInspector] public Movement movement;
    [HideInInspector] public NPCAttack npcAttack;
    [HideInInspector] public NPCMovement npcMovement;
    [HideInInspector] public StateController stateController;
    [HideInInspector] public Vision vision;

    [HideInInspector] public BoxCollider2D boxCollider;
    [HideInInspector] public Rigidbody2D rigidBody;

    public virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();

        alliances = GetComponent<Alliances>();
        movement = GetComponent<Movement>();
        vision = GetComponentInChildren<Vision>();

        TryGetComponent(out npcAttack);
        TryGetComponent(out npcMovement);
        TryGetComponent(out stateController);
    }

    public virtual void Start()
    {
        
    }
}

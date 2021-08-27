using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : CharacterManager
{
    public Inventory keysInventory;

    [HideInInspector] public GameObject playerGameObject;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public PlayerEquipmentManager playerEquipmentManager;
    [HideInInspector] public PlayerMovement playerMovement;
    [HideInInspector] public CircleCollider2D interactionCollider;

    [HideInInspector] public List<Interactable> nearbyInteractables = new List<Interactable>();
    [HideInInspector] public Interactable focusedInteractable;

    public static PlayerManager instance;

    public override void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("There is more than one PlayerManager instance. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
        #endregion

        base.Awake();

        playerGameObject = gameObject;
        playerAttack = GetComponent<PlayerAttack>();
        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();
        interactionCollider = GetComponent<CircleCollider2D>();
    }

    public override void Start()
    {
        base.Start();

        playerEquipmentManager = PlayerEquipmentManager.instance;
        equipmentManager = playerEquipmentManager;

        isMyTurn = true;

        keysInventory.Init();
    }
    
    /// <summary>Determines if the sprite renderer in question is within the main camera's frame, as well as in direct eyesight of the player.</summary>
    public bool CanSee(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == this.spriteRenderer 
            || (spriteRenderer.isVisible && Physics2D.Raycast(transform.position, (spriteRenderer.transform.position - transform.position).normalized, Vector2.Distance(transform.position, spriteRenderer.transform.position), vision.sightObstacleMask) == false))
            return true;
        return false;
    }

    public Interactable GetNearestInteractable()
    {
        Interactable nearestInteractable = null;
        float nearestInteractablePickupDistance = 0;

        for (int i = 0; i < nearbyInteractables.Count; i++)
        {
            if (i == 0)
            {
                nearestInteractable = nearbyInteractables[i];
                nearestInteractablePickupDistance = Vector2.Distance(playerGameObject.transform.position, nearbyInteractables[i].transform.position);
            }
            else
            {
                float dist = Vector2.Distance(playerGameObject.transform.position, nearbyInteractables[i].transform.position);
                if (dist < nearestInteractablePickupDistance)
                {
                    nearestInteractable = nearbyInteractables[i];
                    nearestInteractablePickupDistance = dist;
                }
            }
        }

        return nearestInteractable;
    }
}

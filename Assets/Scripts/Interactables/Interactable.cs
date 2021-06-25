using UnityEngine;

public class Interactable : MonoBehaviour
{
    public float interactRadius = 1f;
    public Transform interactionTransform;

    [HideInInspector] public bool characterInRange;
    [HideInInspector] public bool playerInRange;

    [HideInInspector] public PlayerManager playerManager;
    
    ItemHighlight itemHighlight;

    Transform focusedCharacter;
    bool isFocus, hasInteracted;

    public virtual void Start()
    {
        playerManager = PlayerManager.instance;

        TryGetComponent(out itemHighlight);
    }

    void Update()
    {
        if (isFocus && hasInteracted == false)
        {
            float distance = Vector2.Distance(focusedCharacter.position, interactionTransform.position);
            if (distance <= interactRadius)
            {
                if (focusedCharacter.CompareTag("Player"))
                    Interact(playerManager.equipmentManager, playerManager.inventory, playerManager.playerGameObject.transform);
                else
                {
                    CharacterManager charMananager = focusedCharacter.GetComponent<CharacterManager>();
                    Interact(charMananager.equipmentManager, charMananager.inventory, focusedCharacter);
                    charMananager.stateController.SetToDefaultState(charMananager.npcMovement.shouldFollowLeader);
                }

                hasInteracted = true;
            }
        }
        
        if (GameControls.gamePlayActions.playerInteract.WasPressed && playerInRange && playerManager.focusedInteractable == this && playerManager.playerController.canInteract)
        {
            // Start a single frame cooldown to prevent the player from interacting with multiple objects in a single button press
            playerManager.playerController.StartCoroutine(playerManager.playerController.InteractCooldown());

            Interact(playerManager.equipmentManager, playerManager.inventory, playerManager.playerGameObject.transform);
            hasInteracted = true;
        }
    }

    public virtual void Interact(EquipmentManager equipmentManager, Inventory inventory, Transform whoIsInteracting)
    {
        // Debug.Log("Interacting with " + name);
    }

    public void OnFocused(Transform characterTransform)
    {
        focusedCharacter = characterTransform;
        isFocus = true;
        hasInteracted = false;
    }

    public void OnDefocused()
    {
        focusedCharacter = null;
        isFocus = false;
        hasInteracted = false;
    }

    public virtual void Deactivate()
    {
        characterInRange = false;
        playerInRange = false;
        focusedCharacter = null;
        isFocus = false;
        hasInteracted = false;

        gameObject.SetActive(false);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform == focusedCharacter && collision.isTrigger == false)
            characterInRange = true;

        if (collision.CompareTag("Player") && collision.isTrigger)
        {
            playerInRange = true;

            if (playerManager.nearbyInteractables.Contains(this) == false)
                playerManager.nearbyInteractables.Add(this);

            if (playerManager.focusedInteractable == null)
            {
                playerManager.focusedInteractable = this;

                if (itemHighlight != null)
                    itemHighlight.Highlight();
            }
        }
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform == focusedCharacter && collision.isTrigger == false)
            characterInRange = false;

        if (collision.CompareTag("Player") && collision.isTrigger)
        {
            playerInRange = false;
            UpdateItemPickupFocus();
        }
    }

    public void UpdateItemPickupFocus()
    {
        if (playerManager.nearbyInteractables.Contains(this))
            playerManager.nearbyInteractables.Remove(this);

        if (playerManager.focusedInteractable == this)
        {
            playerManager.focusedInteractable = playerManager.GetNearestInteractable();
            if (playerManager.focusedInteractable != null && playerManager.focusedInteractable.itemHighlight != null)
                playerManager.focusedInteractable.itemHighlight.Highlight();

            if (itemHighlight != null)
                itemHighlight.RemoveHighlight();
        }
    }

}

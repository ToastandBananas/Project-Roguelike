using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public LayerMask interactableMask, obstacleMask;

    [HideInInspector] public Interactable focusInteractable;
    [HideInInspector] public bool canInteract = true;

    Camera mainCamera;
    PlayerManager playerManager;

    void Start()
    {
        mainCamera = Camera.main;
        playerManager = PlayerManager.instance;
    }

    void SetFocus(Interactable newFocus)
    {
        if (newFocus != focusInteractable)
        {
            if (focusInteractable != null)
                focusInteractable.OnDefocused();

            focusInteractable = newFocus;
        }

        newFocus.OnFocused(transform);
    }
    
    void RemoveFocus()
    {
        if (focusInteractable != null)
        {
            focusInteractable.OnDefocused();
            focusInteractable = null;
        }
    }

    public IEnumerator InteractCooldown()
    {
        canInteract = false;
        yield return null;
        canInteract = true;
    }
}

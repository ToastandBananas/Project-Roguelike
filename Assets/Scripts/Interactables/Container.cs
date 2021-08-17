using UnityEngine;

public class Container : Interactable
{
    [Header("Sprites")]
    public Sprite sidebarSpriteClosed;
    public Sprite sidebarSpriteOpen;

    [HideInInspector] public Inventory containerInventory;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    GameManager gm;

    public override void Start()
    {
        base.Start();

        containerInventory = GetComponent<Inventory>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        gm = GameManager.instance;

        containerInventory.myInvUI = gm.containerInvUI;

        if (CompareTag("Object"))
            GameTiles.AddObject(gameObject, transform.position);
    }

    public override void Interact(Inventory inventory, Transform whoIsInteracting)
    {
        base.Interact(inventory, whoIsInteracting);

        // Open the Inventory UIs
        if (gm.containerInvUI.inventoryParent.activeSelf == false)
            gm.containerInvUI.ToggleInventoryMenu();

        if (gm.playerInvUI.inventoryParent.activeSelf == false)
            gm.playerInvUI.ToggleInventoryMenu();
    }
    void OnMouseEnter()
    {
        gm.tileInfoDisplay.focusedObject = gameObject;
    }

    void OnMouseExit()
    {
        if (gm.tileInfoDisplay.focusedObject == gameObject)
            gm.tileInfoDisplay.focusedObject = null;
    }
}

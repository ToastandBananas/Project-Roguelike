using UnityEngine;

public class Container : Interactable
{
    [Header("Sprites")]
    public Sprite sidebarSpriteClosed;
    public Sprite sidebarSpriteOpen;

    [HideInInspector] public Inventory containerInventory;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    ContainerInventoryUI containerInvUI;
    PlayerInventoryUI playerInvUI;
    UIManager uiManager;

    public override void Start()
    {
        base.Start();

        containerInventory = GetComponent<Inventory>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        containerInvUI = ContainerInventoryUI.instance;
        playerInvUI = PlayerInventoryUI.instance;
        uiManager = UIManager.instance;
    }

    public override void Interact(EquipmentManager equipmentManager, Inventory inventory, Transform whoIsInteracting)
    {
        base.Interact(equipmentManager, inventory, whoIsInteracting);

        // Open the Inventory UIs
        if (containerInvUI.inventoryParent.activeSelf == false)
            containerInvUI.ToggleInventoryMenu();

        if (playerInvUI.inventoryParent.activeSelf == false)
            playerInvUI.ToggleInventoryMenu();
    }
}

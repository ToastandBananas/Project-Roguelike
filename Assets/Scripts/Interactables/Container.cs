using UnityEngine;

public class Container : Interactable
{
    [HideInInspector] public Inventory containerInventory;

    ContainerInventoryUI containerInvUI;
    PlayerInventoryUI playerInvUI;
    UIManager uiManager;

    public override void Start()
    {
        base.Start();

        containerInventory = GetComponent<Inventory>();

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
        {
            playerInvUI.ToggleInventoryMenu();
        }

        if (containerInvUI.inventory != containerInventory)
        {
            containerInvUI.AssignInventory(containerInventory);
            containerInvUI.ClearAllSlots();
            containerInvUI.PopulateSlots();
        }
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (collision.CompareTag("Player") && collision.isTrigger && containerInvUI.inventory == containerInventory)
            uiManager.DisableMenus();
    }
}

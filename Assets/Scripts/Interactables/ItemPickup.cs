using UnityEngine;

public class ItemPickup : Interactable
{
    [Header("Item")]
    public ItemData itemData;
    public int itemCount = 1;
    
    [HideInInspector] public Rigidbody2D rigidBody;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Interact(EquipmentManager equipmentManager, Inventory inventory, Transform whoIsInteracting)
    {
        base.Interact(equipmentManager, inventory, whoIsInteracting);

        PickUp(equipmentManager, inventory);
    }

    void PickUp(EquipmentManager equipmentManager, Inventory inventory)
    {
        bool wasPickedUp = false;
        
        if (itemData.item.IsEquipment())
        {
            Equipment equipment = (Equipment)itemData.item;
            if (equipmentManager.currentEquipment[(int)equipment.equipmentSlot] == null)
            {
                equipmentManager.Equip(itemData, equipment.equipmentSlot);
                // TODO: Put in equipment inventory
                wasPickedUp = true;
            }
            else
                wasPickedUp = inventory.Add(itemData, itemCount);
        }
        else
            wasPickedUp = inventory.Add(itemData, itemCount);

        if (wasPickedUp)
        {
            UpdateItemPickupFocus();
            Deactivate();
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();

        itemCount = 1;
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
    }
}

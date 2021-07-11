using UnityEngine;

public class ItemPickup : Interactable
{
    [Header("Item")]
    public int itemCount = 1;
    public bool shouldUseItemCount;

    [HideInInspector] public ItemData itemData;
    [HideInInspector] public Rigidbody2D rigidBody;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemData = GetComponent<ItemData>();
    }

    public override void Start()
    {
        base.Start();

        if (shouldUseItemCount)
            itemData.currentStackSize = itemCount;

        spriteRenderer.sprite = itemData.item.pickupSprite;

        // Make sure the item pickup is properly positioned
        if (transform.position.x % 0.5f != 0)
            transform.position = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, transform.position.y);

        if (transform.position.y % 0.5f != 0)
            transform.position = new Vector3(transform.position.x, Mathf.FloorToInt(transform.position.y) + 0.5f);

        if (itemData.bagInventory != null)
        {
            if (itemData.item.IsBag())
            {
                Bag bag = (Bag)itemData.item;
                itemData.bagInventory.maxWeight = bag.maxWeight;
                itemData.bagInventory.maxVolume = bag.maxVolume;
                itemData.bagInventory.singleItemVolumeLimit = bag.singleItemVolumeLimit;
            }
            else if (itemData.item.IsPortableContainer())
            {
                PortableContainer portableContainer = (PortableContainer)itemData.item;
                itemData.bagInventory.maxWeight = portableContainer.maxWeight;
                itemData.bagInventory.maxVolume = portableContainer.maxVolume;
                itemData.bagInventory.singleItemVolumeLimit = portableContainer.singleItemVolumeLimit;
            }

            StartCoroutine(itemData.ClampItemCounts());
        }
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
                equipmentManager.Equip(itemData, null, equipment.equipmentSlot);
                wasPickedUp = true;
            }
            else
                wasPickedUp = inventory.AddItem(null, itemData, itemCount, null, true);
        }
        else
            wasPickedUp = inventory.AddItem(null, itemData, itemCount, null, true);

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

using UnityEngine;

public class ItemPickup : Interactable
{
    [Header("Item")]
    public int itemCount = 1;
    public bool shouldUseItemCount;

    [HideInInspector] public ItemData itemData;
    [HideInInspector] public Rigidbody2D rigidBody;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    GameManager gm;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemData = GetComponent<ItemData>();
    }

    public override void Start()
    {
        base.Start();

        gm = GameManager.instance;

        if (shouldUseItemCount)
            itemData.currentStackSize = itemCount;

        spriteRenderer.sprite = itemData.item.pickupSprite;

        // Make sure the item pickup is properly positioned
        transform.position = Utilities.ClampedPosition(transform.position);
        
        if (itemData != null)
            gm.gameTiles.AddItemData(itemData, transform.position);

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

    public override void Interact(Inventory inventory, Transform whoIsInteracting)
    {
        base.Interact(inventory, whoIsInteracting);

        PickUp(inventory);
    }

    void PickUp(Inventory inventory)
    {
        bool wasPickedUp = inventory.AddItem(null, itemData, itemCount, null, true);

        if (wasPickedUp)
        {
            gm.gameTiles.RemoveItemData(itemData, transform.position);
            inventory.UpdateCurrentWeightAndVolume();
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

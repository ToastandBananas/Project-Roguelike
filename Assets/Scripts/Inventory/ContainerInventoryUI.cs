using System.Collections.Generic;
using UnityEngine;

public class ContainerInventoryUI : InventoryUI
{
    #region Singleton
    public static ContainerInventoryUI instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of ContainerInventoryUI. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    List<ItemData> playerPositionItems = new List<ItemData>();
    List<ItemData> leftItems = new List<ItemData>();
    List<ItemData> rightItems = new List<ItemData>();
    List<ItemData> upItems = new List<ItemData>();
    List<ItemData> downItems = new List<ItemData>();
    List<ItemData> upLeftItems = new List<ItemData>();
    List<ItemData> upRightItems = new List<ItemData>();
    List<ItemData> downLeftItems = new List<ItemData>();
    List<ItemData> downRightItems = new List<ItemData>();

    LayerMask interactableMask;

    public override void Start()
    {
        base.Start();

        interactableMask = LayerMask.GetMask("Interactable", "Interactable Objects");
    }

    public void AssignInventory(Inventory newInventory)
    {
        inventory = newInventory;
        inventory.myInventoryUI = this;
        AddCallbacks();
    }

    public void PopulateSlots()
    {
        for (int i = 0; i < inventory.items.Count; i++)
        {
            
        }
    }

    public void ClearAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            
        }
    }

    public void GetItemsAroundPlayer()
    {
        RaycastPosition(playerManager.transform.position);
    }

    void RaycastPosition(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, 1, interactableMask);
        if (hit.collider != null)
        {
            if (TryGetComponent(out Inventory inventory) != false)
            {
                // Get each item in the container's inventory

            }
            else
            {
                // Get each item on the ground

            }
        }
    }
}

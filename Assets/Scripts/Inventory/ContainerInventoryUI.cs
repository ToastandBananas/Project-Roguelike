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

    public override void Start()
    {
        base.Start();
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
}

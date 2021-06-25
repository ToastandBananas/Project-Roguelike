using UnityEngine;

public class PlayerInventory : Inventory
{
    public static PlayerInventory instance;
    void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of PlayerInventory. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
        #endregion

        myInventoryUI = GameObject.Find("Player Inventory").GetComponent<InventoryUI>();
        myInventoryUI.inventory = this;
    }

    public override void Start()
    {
        inventoryOwner = PlayerManager.instance.playerGameObject;
    }
}

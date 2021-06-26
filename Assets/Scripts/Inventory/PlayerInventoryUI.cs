using UnityEngine;

public class PlayerInventoryUI : InventoryUI
{
    #region Singleton
    public static PlayerInventoryUI instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of PlayerInventoryUI. Fix me!");
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

        inventoryItemObjectPool.Init();
    }
}

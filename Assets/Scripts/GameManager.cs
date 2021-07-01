using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public ContextMenu contextMenu;
    [HideInInspector] public ContainerInventoryUI containerInvUI;
    [HideInInspector] public PlayerInventoryUI playerInvUI;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public DropItemController dropItemController;
    [HideInInspector] public ObjectPoolManager objectPoolManager;
    [HideInInspector] public StackSizeSelector stackSizeSelector;
    [HideInInspector] public TooltipManager tooltipManager;
    [HideInInspector] public UIManager uiManager;

    #region Singleton
    public static GameManager instance;

    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of GameManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        contextMenu = ContextMenu.instance;
        containerInvUI = ContainerInventoryUI.instance;
        playerInvUI = PlayerInventoryUI.instance;
        playerManager = PlayerManager.instance;
        dropItemController = DropItemController.instance;
        objectPoolManager = ObjectPoolManager.instance;
        stackSizeSelector = StackSizeSelector.instance;
        tooltipManager = TooltipManager.instance;
        uiManager = UIManager.instance;
    }
}

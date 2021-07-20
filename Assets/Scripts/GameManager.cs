using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public APManager apManager;
    [HideInInspector] public ContextMenu contextMenu;
    [HideInInspector] public ContainerInventoryUI containerInvUI;
    [HideInInspector] public GameTiles gameTiles;
    [HideInInspector] public PlayerInventoryUI playerInvUI;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public DropItemController dropItemController;
    [HideInInspector] public ObjectPoolManager objectPoolManager;
    [HideInInspector] public StackSizeSelector stackSizeSelector;
    [HideInInspector] public TooltipManager tooltipManager;
    [HideInInspector] public TurnManager turnManager;
    [HideInInspector] public UIManager uiManager;

    public static GameManager instance;

    void Awake()
    {
        #region Singleton
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
        #endregion

        objectPoolManager = GetComponent<ObjectPoolManager>();
    }

    void Start()
    {
        apManager = APManager.instance;
        contextMenu = ContextMenu.instance;
        containerInvUI = ContainerInventoryUI.instance;
        gameTiles = GameTiles.instance;
        playerInvUI = PlayerInventoryUI.instance;
        playerManager = PlayerManager.instance;
        dropItemController = DropItemController.instance;
        stackSizeSelector = StackSizeSelector.instance;
        tooltipManager = TooltipManager.instance;
        turnManager = TurnManager.instance;
        uiManager = UIManager.instance;
    }
}

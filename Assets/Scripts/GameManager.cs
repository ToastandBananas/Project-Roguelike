using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public APManager apManager;
    [HideInInspector] public ContextMenu contextMenu;
    [HideInInspector] public ContainerInventoryUI containerInvUI;
    [HideInInspector] public GameTiles gameTiles;
    [HideInInspector] public FlavorText flavorText;
    [HideInInspector] public HealthDisplay healthDisplay;
    [HideInInspector] public PlayerInventoryUI playerInvUI;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public DropItemController dropItemController;
    [HideInInspector] public ObjectPoolManager objectPoolManager;
    [HideInInspector] public StackSizeSelector stackSizeSelector;
    [HideInInspector] public TileInfoDisplay tileInfoDisplay;
    [HideInInspector] public TooltipManager tooltipManager;
    [HideInInspector] public HealthSystem healthSystem;
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

    void Update()
    {
        #if UNITY_EDITOR
        //if (Input.GetKeyDown(KeyCode.P))
            //apManager.LoseAP(playerManager, 75);

        //if (Input.GetKeyDown(KeyCode.O))
        #endif

    }

    void Start()
    {
        apManager = APManager.instance;
        contextMenu = ContextMenu.instance;
        containerInvUI = ContainerInventoryUI.instance;
        flavorText = FlavorText.instance;
        gameTiles = GameTiles.instance;
        healthDisplay = HealthDisplay.instance;
        playerInvUI = PlayerInventoryUI.instance;
        playerManager = PlayerManager.instance;
        dropItemController = DropItemController.instance;
        stackSizeSelector = StackSizeSelector.instance;
        tileInfoDisplay = TileInfoDisplay.instance;
        tooltipManager = TooltipManager.instance;
        healthSystem = HealthSystem.instance;
        turnManager = TurnManager.instance;
        uiManager = UIManager.instance;
    }
}

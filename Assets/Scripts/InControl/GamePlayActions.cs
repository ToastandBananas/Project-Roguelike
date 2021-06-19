﻿using InControl;

public class GamePlayActions : PlayerActionSet
{
    public PlayerAction playerInteract;
    public PlayerAction playerSwapWeapon;
    public PlayerAction playerAttack;

    public PlayerAction playerMoveUp, playerMoveDown, playerMoveLeft, playerMoveRight;
    public PlayerAction playerMoveUpLeft, playerMoveUpRight, playerMoveDownLeft, playerMoveDownRight;
    public PlayerTwoAxisAction playerMovementAxis;

    public PlayerAction playerLookUp, playerLookDown, playerLookLeft, playerLookRight;
    public PlayerTwoAxisAction playerLookAxis;

    // UI Actions
    public PlayerAction playerInventory, playerJournal, playerCharacterMenu;
    public PlayerAction menuPause, menuSelect, menuContext, menuDropItem;
    public PlayerAction menuLeft, menuRight, menuUp, menuDown;
    public PlayerAction menuContainerTakeAll, menuTakeItem, menuUseItem;

    // Specific Keys
    public PlayerAction leftCtrl, leftShift, enter, tab;

    public GamePlayActions()
    {
        // Actions
        playerInteract = CreatePlayerAction("PlayerInteract");
        playerSwapWeapon = CreatePlayerAction("PlayerSwapWeapon");
        playerAttack = CreatePlayerAction("PlayerAttack");

        // Movement
        playerMoveUp = CreatePlayerAction("PlayerMoveUp");
        playerMoveDown = CreatePlayerAction("PlayerMoveDown");
        playerMoveLeft = CreatePlayerAction("PlayerMoveLeft");
        playerMoveRight = CreatePlayerAction("PlayerMoveRight");
        playerMoveUpLeft = CreatePlayerAction("PlayerMoveUpLeft");
        playerMoveUpRight = CreatePlayerAction("PlayerMoveUpRight");
        playerMoveDownLeft = CreatePlayerAction("PlayerMoveDownLeft");
        playerMoveDownRight = CreatePlayerAction("PlayerMoveDownRight");
        playerMovementAxis = CreateTwoAxisPlayerAction(playerMoveLeft, playerMoveRight, playerMoveDown, playerMoveUp);

        // Camera Movement
        playerLookUp = CreatePlayerAction("PlayerLookUp");
        playerLookDown = CreatePlayerAction("PlayerLookDown");
        playerLookLeft = CreatePlayerAction("PlayerLookLeft");
        playerLookRight = CreatePlayerAction("PlayerLookRight");
        playerLookAxis = CreateTwoAxisPlayerAction(playerLookLeft, playerLookRight, playerLookDown, playerLookUp);

        // UI Actions
        playerInventory = CreatePlayerAction("PlayerInventory");
        playerJournal = CreatePlayerAction("PlayerJournal");
        playerCharacterMenu = CreatePlayerAction("PlayerCharacterMenu");

        menuPause = CreatePlayerAction("MenuPause");
        menuSelect = CreatePlayerAction("MenuSelect");
        menuContext = CreatePlayerAction("MenuContext");
        menuDropItem = CreatePlayerAction("MenuDropItem");

        menuLeft = CreatePlayerAction("MenuLeft");
        menuRight = CreatePlayerAction("MenuRight");
        menuUp = CreatePlayerAction("MenuUp");
        menuDown = CreatePlayerAction("MenuDown");

        menuContainerTakeAll = CreatePlayerAction("MenuContainerTakeAll");
        menuTakeItem = CreatePlayerAction("MenuTakeItem");
        menuUseItem = CreatePlayerAction("MenuUseItem");

        // Specific Key Actions
        leftCtrl = CreatePlayerAction("LeftCtrl");
        leftShift = CreatePlayerAction("LeftShift");
        enter = CreatePlayerAction("Enter");
        tab = CreatePlayerAction("Tab");
    }
}
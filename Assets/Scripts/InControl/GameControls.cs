using UnityEngine;
using InControl;

public class GameControls : MonoBehaviour
{
    public static GamePlayActions gamePlayActions;

    void Start()
    {
        gamePlayActions = new GamePlayActions();
        BindDefaultControls();
    }

    void BindDefaultControls()
    {
        // Actions
        gamePlayActions.playerInteract.AddDefaultBinding(InputControlType.Action1); // Interact
        gamePlayActions.playerInteract.AddDefaultBinding(Key.Space);

        gamePlayActions.playerSwapWeapon.AddDefaultBinding(InputControlType.Action4); // Swap Weapon
        gamePlayActions.playerSwapWeapon.AddDefaultBinding(Key.LeftAlt);

        gamePlayActions.playerAttack.AddDefaultBinding(InputControlType.LeftTrigger); // Attack
        gamePlayActions.playerAttack.AddDefaultBinding(Key.Space);

        // Movement
        gamePlayActions.playerMoveUp.AddDefaultBinding(InputControlType.LeftStickUp); // Up
        gamePlayActions.playerMoveUp.AddDefaultBinding(Key.W);

        gamePlayActions.playerMoveDown.AddDefaultBinding(InputControlType.LeftStickDown); // Down
        gamePlayActions.playerMoveDown.AddDefaultBinding(Key.S);

        gamePlayActions.playerMoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft); // Left
        gamePlayActions.playerMoveLeft.AddDefaultBinding(Key.A);

        gamePlayActions.playerMoveRight.AddDefaultBinding(InputControlType.LeftStickRight); // Right
        gamePlayActions.playerMoveRight.AddDefaultBinding(Key.D);

        gamePlayActions.playerMoveUpLeft.AddDefaultBinding(Key.Q); // Up-left
        gamePlayActions.playerMoveUpRight.AddDefaultBinding(Key.E); // Up-Right
        gamePlayActions.playerMoveDownLeft.AddDefaultBinding(Key.Z); // Down-left
        gamePlayActions.playerMoveDownRight.AddDefaultBinding(Key.X); // Down-right

        // Camera Movement
        gamePlayActions.playerLookUp.AddDefaultBinding(InputControlType.RightStickUp); // Up
        gamePlayActions.playerLookUp.AddDefaultBinding(Key.UpArrow);

        gamePlayActions.playerLookDown.AddDefaultBinding(InputControlType.RightStickDown); // Down
        gamePlayActions.playerLookUp.AddDefaultBinding(Key.DownArrow);

        gamePlayActions.playerLookLeft.AddDefaultBinding(InputControlType.RightStickLeft); // Left
        gamePlayActions.playerLookUp.AddDefaultBinding(Key.LeftArrow);

        gamePlayActions.playerLookRight.AddDefaultBinding(InputControlType.RightStickRight); // Right
        gamePlayActions.playerLookUp.AddDefaultBinding(Key.RightArrow);

        // UI Actions
        gamePlayActions.playerInventory.AddDefaultBinding(Key.I);
        gamePlayActions.playerInventory.AddDefaultBinding(InputControlType.Pause);
        gamePlayActions.playerInventory.AddDefaultBinding(InputControlType.Options);

        gamePlayActions.playerJournal.AddDefaultBinding(Key.J);
        gamePlayActions.playerJournal.AddDefaultBinding(InputControlType.Select);
        gamePlayActions.playerJournal.AddDefaultBinding(InputControlType.TouchPadButton);

        gamePlayActions.playerCharacterMenu.AddDefaultBinding(Key.C);

        gamePlayActions.menuPause.AddDefaultBinding(Key.Escape);

        gamePlayActions.menuSelect.AddDefaultBinding(Mouse.LeftButton);
        gamePlayActions.menuSelect.AddDefaultBinding(InputControlType.Action1);

        gamePlayActions.menuContext.AddDefaultBinding(Mouse.RightButton);
        gamePlayActions.menuContext.AddDefaultBinding(InputControlType.Action3);

        gamePlayActions.menuDropItem.AddDefaultBinding(InputControlType.Action4);

        gamePlayActions.menuUp.AddDefaultBinding(Key.UpArrow);
        gamePlayActions.menuUp.AddDefaultBinding(InputControlType.DPadUp);

        gamePlayActions.menuDown.AddDefaultBinding(Key.DownArrow);
        gamePlayActions.menuDown.AddDefaultBinding(InputControlType.DPadDown);

        gamePlayActions.menuLeft.AddDefaultBinding(Key.LeftArrow);
        gamePlayActions.menuLeft.AddDefaultBinding(InputControlType.DPadLeft);

        gamePlayActions.menuRight.AddDefaultBinding(Key.RightArrow);
        gamePlayActions.menuRight.AddDefaultBinding(InputControlType.DPadRight);

        gamePlayActions.menuContainerTakeAll.AddDefaultBinding(Key.T);
        gamePlayActions.menuContainerTakeAll.AddDefaultBinding(InputControlType.LeftStickButton);

        gamePlayActions.menuTakeItem.AddDefaultBinding(InputControlType.Action4);

        gamePlayActions.menuUseItem.AddDefaultBinding(Mouse.MiddleButton);
        gamePlayActions.menuUseItem.AddDefaultBinding(InputControlType.RightBumper);

        // Camera Zoom
        gamePlayActions.zoomIn.AddDefaultBinding(Key.Plus);
        gamePlayActions.zoomIn.AddDefaultBinding(Key.Equals);
        gamePlayActions.zoomIn.AddDefaultBinding(Mouse.PositiveScrollWheel);

        gamePlayActions.zoomOut.AddDefaultBinding(Key.Minus);
        gamePlayActions.zoomOut.AddDefaultBinding(Key.Underscore);
        gamePlayActions.zoomOut.AddDefaultBinding(Mouse.NegativeScrollWheel);

        // Specific Key Actions
        gamePlayActions.a.AddDefaultBinding(Key.A);
        gamePlayActions.leftCtrl.AddDefaultBinding(Key.LeftControl);
        gamePlayActions.leftShift.AddDefaultBinding(Key.LeftShift);
        gamePlayActions.leftAlt.AddDefaultBinding(Key.LeftAlt);
        gamePlayActions.enter.AddDefaultBinding(Key.Return);
        gamePlayActions.enter.AddDefaultBinding(Key.PadEnter);
        gamePlayActions.tab.AddDefaultBinding(Key.Tab);
    }
}

using UnityEngine;

public class PlayerMovement : Movement
{
    public override void Start()
    {
        base.Start();
    }
    
    void Update()
    {
        CheckForMovement();
    }

    void CheckForMovement()
    {
        // Do nothing if the Player is still moving
        if (gm.turnManager.isPlayerTurn == false || isMoving || onCooldown || GameControls.gamePlayActions.leftCtrl.IsPressed)
            return;

        Vector2 movementInput = GameControls.gamePlayActions.playerMovementAxis.Value;

        // To store move directions
        float horizontal = movementInput.x;
        float vertical   = movementInput.y;

        if (horizontal > 0.3f || horizontal < -0.3f || vertical > 0.3f || vertical < -0.3f) // Account for stick drift (which is quite common)
        {
            gm.uiManager.DisableInventoryUIComponents();

            // If we're dragging any inventory items, stop dragging them
            if (gm.uiManager.invItemsDragging.Count > 0)
            {
                gm.uiManager.ShowHiddenItems();
                gm.uiManager.ResetandHideGhostItems();
                gm.uiManager.Reset();
            }

            if (horizontal <= 0.3f && horizontal >= -0.3f)
            {
                if (vertical > 0.3f) // Up
                    Move(0, 1, false);
                else if (vertical < -0.3f) // Down
                    Move(0, -1, false);
            }
            else if (vertical <= 0.3f && vertical >= -0.3f)
            {
                if (horizontal < -0.3f) // Left
                    Move(-1, 0, false);
                else if (horizontal > 0.3f) // Right
                    Move(1, 0, false);
            }
            else if (vertical > 0.3f)
            {
                if (horizontal < -0.3f) // Up-left
                    Move(-1, 1, false);
                else if (horizontal > 0.3f) // Up-right
                    Move(1, 1, false);
            }
            else if (vertical < -0.3f)
            {
                if (horizontal < -0.3f) // Down-left
                    Move(-1, -1, false);
                else if (horizontal > 0.3f) // Down-right
                    Move(1, -1, false);
            }

            gm.turnManager.TakePlayersTurn();
        }
        else if (GameControls.gamePlayActions.playerMoveUpLeft.WasPressed) // Up-left
        {
            gm.uiManager.DisableInventoryUIComponents();
            Move(-1, 1, false);
            gm.turnManager.TakePlayersTurn();
        }
        else if (GameControls.gamePlayActions.playerMoveUpRight.WasPressed) // Up-right
        {
            gm.uiManager.DisableInventoryUIComponents();
            Move(1, 1, false);
            gm.turnManager.TakePlayersTurn();
        }
        else if (GameControls.gamePlayActions.playerMoveDownLeft.WasPressed) // Down-left
        {
            gm.uiManager.DisableInventoryUIComponents();
            Move(-1, -1, false);
            gm.turnManager.TakePlayersTurn();
        }
        else if (GameControls.gamePlayActions.playerMoveDownRight.WasPressed) // Down-right
        {
            gm.uiManager.DisableInventoryUIComponents();
            Move(1, -1, false);
            gm.turnManager.TakePlayersTurn();
        }
    }
}

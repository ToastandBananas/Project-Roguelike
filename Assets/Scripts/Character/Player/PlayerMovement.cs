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
        if (turnManager.isPlayerTurn == false || isMoving || onCooldown)
            return;

        Vector2 movementInput = GameControls.gamePlayActions.playerMovementAxis.Value;

        // To store move directions
        float horizontal = movementInput.x;
        float vertical   = movementInput.y;

        if (horizontal > 0.3f || horizontal < -0.3f || vertical > 0.3f || vertical < -0.3f) // Account for stick drift
        {
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

            turnManager.TakePlayersTurn();
        }
        else if (GameControls.gamePlayActions.playerMoveUpLeft.WasPressed) // Up-left
        {
            Move(-1, 1, false);
            turnManager.TakePlayersTurn();
        }
        else if (GameControls.gamePlayActions.playerMoveUpRight.WasPressed) // Up-right
        {
            Move(1, 1, false);
            turnManager.TakePlayersTurn();
        }
        else if (GameControls.gamePlayActions.playerMoveDownLeft.WasPressed) // Down-left
        {
            Move(-1, -1, false);
            turnManager.TakePlayersTurn();
        }
        else if (GameControls.gamePlayActions.playerMoveDownRight.WasPressed) // Down-right
        {
            Move(1, -1, false);
            turnManager.TakePlayersTurn();
        }
    }
}

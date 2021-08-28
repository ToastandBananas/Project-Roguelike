using System.Collections;
using UnityEngine;

public class PlayerMovement : Movement
{
    PlayerManager playerManager;

    public override void Start()
    {
        base.Start();

        playerManager = (PlayerManager)characterManager;
    }
    
    void Update()
    {
        if (GameControls.gamePlayActions.playerRun.WasPressed)
            ToggleRun();

        CheckForMovement();
    }

    void CheckForMovement()
    {
        // Do nothing if the Player is still moving
        if (playerManager.isMyTurn == false || isMoving || characterManager.actions.Count > 0/*characterManager.actionsQueued > 0*/ || GameControls.gamePlayActions.leftCtrl.IsPressed)
            return;

        Vector2 movementInput = GameControls.gamePlayActions.playerMovementAxis.Value;

        // To store move directions
        float horizontal = movementInput.x;
        float vertical   = movementInput.y;

        RaycastHit2D hit;

        if (horizontal > 0.3f || horizontal < -0.3f || vertical > 0.3f || vertical < -0.3f) // Account for stick drift (which is quite common)
        {
            gm.uiManager.DisableInventoryUIComponents();

            // If we're dragging any inventory items, stop dragging them
            if (gm.uiManager.invItemsDragging.Count > 0)
            {
                gm.uiManager.ShowHiddenItems();
                gm.uiManager.ResetandHideGhostItems();
                gm.uiManager.ResetSelections();
            }

            if (horizontal <= 0.3f && horizontal >= -0.3f)
            {
                if (vertical > 0.3f) // Up
                {
                    Rotate(Direction.North, true);

                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(0, 1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    else
                        playerManager.QueueAction(Move(0, 1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(0, 1))));
                }
                else if (vertical < -0.3f) // Down
                {
                    Rotate(Direction.South, true);

                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(0, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    else
                        playerManager.QueueAction(Move(0, -1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(0, -1))));
                }
            }
            else if (vertical <= 0.3f && vertical >= -0.3f)
            {
                if (horizontal < -0.3f) // Left
                {
                    Rotate(Direction.West, true);

                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 0));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    else
                        playerManager.QueueAction(Move(-1, 0), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(-1, 0))));
                }
                else if (horizontal > 0.3f) // Right
                {
                    Rotate(Direction.East, true);

                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 0));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    else
                        playerManager.QueueAction(Move(1, 0), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(1, 0))));
                }
            }
            else if (vertical > 0.3f)
            {
                if (horizontal < -0.3f) // Up-left
                {
                    Rotate(Direction.Northwest, true);

                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    else
                        playerManager.QueueAction(Move(-1, 1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(-1, 1))));
                }
                else if (horizontal > 0.3f) // Up-right
                {
                    Rotate(Direction.Northeast, true);

                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    else
                        playerManager.QueueAction(Move(1, 1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(1, 1))));
                }
            }
            else if (vertical < -0.3f)
            {
                if (horizontal < -0.3f) // Down-left
                {
                    Rotate(Direction.Southwest, true);

                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    else
                        playerManager.QueueAction(Move(-1, -1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(-1, -1))));
                }
                else if (horizontal > 0.3f) // Down-right
                {
                    Rotate(Direction.Southeast, true);

                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    else
                        playerManager.QueueAction(Move(1, -1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(1, -1))));
                }
            }

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveUpLeft.IsPressed) // Up-left
        {
            gm.uiManager.DisableInventoryUIComponents();

            Rotate(Direction.Northwest, true);

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 1));
            if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
            else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                playerManager.playerAttack.DetermineAttack(null, stats);
            else
                playerManager.QueueAction(Move(-1, 1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(-1, 1))));

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveUpRight.IsPressed) // Up-right
        {
            gm.uiManager.DisableInventoryUIComponents();

            Rotate(Direction.Northeast, true);

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 1));
            if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
            else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                playerManager.playerAttack.DetermineAttack(null, stats);
            else
                playerManager.QueueAction(Move(1, 1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(1, 1))));

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveDownLeft.IsPressed) // Down-left
        {
            gm.uiManager.DisableInventoryUIComponents();

            Rotate(Direction.Southwest, true);

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, -1));
            if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
            else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                playerManager.playerAttack.DetermineAttack(null, stats);
            else
                playerManager.QueueAction(Move(-1, -1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(-1, -1))));

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveDownRight.IsPressed) // Down-right
        {
            gm.uiManager.DisableInventoryUIComponents();

            Rotate(Direction.Southeast, true);

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, -1));
            if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
            else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.IsDeadOrDestroyed() == false)
                playerManager.playerAttack.DetermineAttack(null, stats);
            else
                playerManager.QueueAction(Move(1, -1), gm.apManager.GetMovementAPCost(playerManager, IsDiagonal(transform.position + new Vector3(1, -1))));

            StartCoroutine(MovementCooldown(0.25f));
        }
    }

    public IEnumerator Move(int xDir, int yDir)
    {
        if (characterManager.status.isDead) yield break;

        Vector2 startCell = transform.position;
        Vector2 targetCell = startCell + new Vector2(xDir, yDir);

        RaycastHit2D hit = RaycastMovePosition(startCell, targetCell);

        // If the target tile is a walkable tile, the player moves here
        if (hit.collider == null && GameTiles.characters.TryGetValue(targetCell, out CharacterManager character) == false)
        {
            // If over-encumbered, use stamina and stop running
            if (characterManager.IsOverEncumbered())
            {
                if (isRunning) ToggleRun();
                characterManager.status.UseStamina(StaminaCosts.GetOverEncumberedMoveCost(characterManager, IsDiagonal(targetCell)));
            }
            // If running, use stamina (or stop running if not enough stamina)
            else if (isRunning)
            {
                float runStaminaCost = StaminaCosts.GetRunCost(characterManager, IsDiagonal(targetCell));
                if (characterManager.status.HasEnoughStamina(runStaminaCost))
                    characterManager.status.UseStamina(runStaminaCost);
                else
                    ToggleRun();
            }

            GameTiles.RemoveCharacter(transform.position);
            if (targetCell.y == startCell.y)
                StartCoroutine(ArcMovement(targetCell));
            else
                StartCoroutine(SmoothMovement(targetCell));
        }
        else
            StartCoroutine(BlockedMovement(targetCell));
    }

    public RaycastHit2D RaycastMovePosition(Vector2 startCell, Vector2 targetCell)
    {
        return Physics2D.Raycast(targetCell, Vector2.zero, 1, movementObstacleMask);
    }
}

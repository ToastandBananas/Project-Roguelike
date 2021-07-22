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
        CheckForMovement();
    }

    void CheckForMovement()
    {
        // Do nothing if the Player is still moving
        if (gm.turnManager.isPlayersTurn == false || isMoving || onCooldown || characterManager.actionQueued || GameControls.gamePlayActions.leftCtrl.IsPressed)
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
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(0, 1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                        playerManager.playerAttack.DetermineAttack(stats);
                    else
                        StartCoroutine(UseAPAndMove(0, 1));
                }
                else if (vertical < -0.3f) // Down
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(0, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                        playerManager.playerAttack.DetermineAttack(stats);
                    else
                        StartCoroutine(UseAPAndMove(0, -1));
                }
            }
            else if (vertical <= 0.3f && vertical >= -0.3f)
            {
                if (horizontal < -0.3f) // Left
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 0));
                    if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                        playerManager.playerAttack.DetermineAttack(stats);
                    else
                        StartCoroutine(UseAPAndMove(-1, 0));
                }
                else if (horizontal > 0.3f) // Right
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 0));
                    if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                        playerManager.playerAttack.DetermineAttack(stats);
                    else
                        StartCoroutine(UseAPAndMove(1, 0));
                }
            }
            else if (vertical > 0.3f)
            {
                if (horizontal < -0.3f) // Up-left
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                        playerManager.playerAttack.DetermineAttack(stats);
                    else
                        StartCoroutine(UseAPAndMove(-1, 1));
                }
                else if (horizontal > 0.3f) // Up-right
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                        playerManager.playerAttack.DetermineAttack(stats);
                    else
                        StartCoroutine(UseAPAndMove(1, 1));
                }
            }
            else if (vertical < -0.3f)
            {
                if (horizontal < -0.3f) // Down-left
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                        playerManager.playerAttack.DetermineAttack(stats);
                    else
                        StartCoroutine(UseAPAndMove(-1, -1));
                }
                else if (horizontal > 0.3f) // Down-right
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                        playerManager.playerAttack.DetermineAttack(stats);
                    else
                        StartCoroutine(UseAPAndMove(1, -1));
                }
            }

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveUpLeft.IsPressed) // Up-left
        {
            gm.uiManager.DisableInventoryUIComponents();

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 1));
            if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                playerManager.playerAttack.DetermineAttack(stats);
            else
                StartCoroutine(UseAPAndMove(-1, 1));

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveUpRight.IsPressed) // Up-right
        {
            gm.uiManager.DisableInventoryUIComponents();

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 1));
            if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                playerManager.playerAttack.DetermineAttack(stats);
            else
                StartCoroutine(UseAPAndMove(1, 1));

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveDownLeft.IsPressed) // Down-left
        {
            gm.uiManager.DisableInventoryUIComponents();

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, -1));
            if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                playerManager.playerAttack.DetermineAttack(stats);
            else
                StartCoroutine(UseAPAndMove(-1, -1));

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveDownRight.IsPressed) // Down-right
        {
            gm.uiManager.DisableInventoryUIComponents();

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, -1));
            if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDeadOrDestroyed == false)
                playerManager.playerAttack.DetermineAttack(stats);
            else
                StartCoroutine(UseAPAndMove(1, -1));

            StartCoroutine(MovementCooldown(0.25f));
        }
    }

    public void Move(int xDir, int yDir, bool isNPC)
    {
        Vector2 startCell = transform.position;
        Vector2 targetCell = startCell + new Vector2(xDir, yDir);

        RaycastHit2D hit = RaycastMovePosition(startCell, targetCell);

        // If the target tile is a walkable tile, the player moves here
        if (hit.collider == null)
        {
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

    public IEnumerator UseAPAndMove(int xDir, int yDir)
    {
        characterManager.actionQueued = true;

        while (gm.turnManager.isPlayersTurn == false)
        {
            yield return null;
        }

        if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                Move(xDir, yDir, false);
                characterManager.actionQueued = false;
                characterManager.characterStats.UseAP(characterManager.remainingAPToBeUsed);

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.characterStats.UseAP(characterManager.characterStats.currentAP);
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndMove(xDir, yDir));
            }
        }
        else
        {
            int remainingAP = characterManager.characterStats.UseAPAndGetRemainder(gm.apManager.GetMovementAPCost());
            if (remainingAP == 0)
            {
                Move(xDir, yDir, false);
                characterManager.actionQueued = false;

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.remainingAPToBeUsed = remainingAP;
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndMove(xDir, yDir));
            }
        }
    }
}

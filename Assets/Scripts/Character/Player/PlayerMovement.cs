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
        if (playerManager.isMyTurn == false || isMoving || characterManager.actionsQueued > 0 || GameControls.gamePlayActions.leftCtrl.IsPressed)
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
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
                    {
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    }
                    else
                    {
                        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                        StartCoroutine(Move(0, 1));
                    }
                }
                else if (vertical < -0.3f) // Down
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(0, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
                    {
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    }
                    else
                    {
                        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                        StartCoroutine(Move(0, -1));
                    }
                }
            }
            else if (vertical <= 0.3f && vertical >= -0.3f)
            {
                if (horizontal < -0.3f) // Left
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 0));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
                    {
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    }
                    else
                    {
                        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                        StartCoroutine(Move(-1, 0));
                    }
                }
                else if (horizontal > 0.3f) // Right
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 0));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
                    {
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    }
                    else
                    {
                        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                        StartCoroutine(Move(1, 0));
                    }
                }
            }
            else if (vertical > 0.3f)
            {
                if (horizontal < -0.3f) // Up-left
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
                    {
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    }
                    else
                    {
                        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                        StartCoroutine(Move(-1, 1));
                    }
                }
                else if (horizontal > 0.3f) // Up-right
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
                    {
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    }
                    else
                    {
                        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                        StartCoroutine(Move(1, 1));
                    }
                }
            }
            else if (vertical < -0.3f)
            {
                if (horizontal < -0.3f) // Down-left
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
                    {
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    }
                    else
                    {
                        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                        StartCoroutine(Move(-1, -1));
                    }
                }
                else if (horizontal > 0.3f) // Down-right
                {
                    hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, -1));
                    if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                        playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
                    else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
                    {
                        playerManager.playerAttack.DetermineAttack(null, stats);
                    }
                    else
                    {
                        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                        StartCoroutine(Move(1, -1));
                    }
                }
            }

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveUpLeft.IsPressed) // Up-left
        {
            gm.uiManager.DisableInventoryUIComponents();

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, 1));
            if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
            else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
            {
                playerManager.playerAttack.DetermineAttack(null, stats);
            }
            else
            {
                StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                StartCoroutine(Move(-1, 1));
            }

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveUpRight.IsPressed) // Up-right
        {
            gm.uiManager.DisableInventoryUIComponents();

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, 1));
            if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
            else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
            {
                playerManager.playerAttack.DetermineAttack(null, stats);
            }
            else
            {
                StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                StartCoroutine(Move(1, 1));
            }

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveDownLeft.IsPressed) // Down-left
        {
            gm.uiManager.DisableInventoryUIComponents();

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(-1, -1));
            if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
            else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
            {
                playerManager.playerAttack.DetermineAttack(null, stats);
            }
            else
            {
                StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                StartCoroutine(Move(-1, -1));
            }

            StartCoroutine(MovementCooldown(0.25f));
        }
        else if (GameControls.gamePlayActions.playerMoveDownRight.IsPressed) // Down-right
        {
            gm.uiManager.DisableInventoryUIComponents();

            hit = RaycastMovePosition(transform.position, transform.position + new Vector3(1, -1));
            if (hit.collider != null && hit.collider.TryGetComponent(out CharacterManager charManager) && charManager.status.isDead == false)
                playerManager.playerAttack.DetermineAttack(charManager, charManager.characterStats);
            else if (hit.collider != null && hit.collider.TryGetComponent(out Stats stats) && stats.isDestroyed == false)
            {
                playerManager.playerAttack.DetermineAttack(null, stats);
            }
            else
            {
                StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetMovementAPCost()));
                StartCoroutine(Move(1, -1));
            }

            StartCoroutine(MovementCooldown(0.25f));
        }
    }

    public IEnumerator Move(int xDir, int yDir)
    {
        int queueNumber = characterManager.currentQueueNumber + characterManager.actionsQueued;
        while (queueNumber != characterManager.currentQueueNumber)
        {
            yield return null;
            if (characterManager.status.isDead) yield break;
        }

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

    /*public IEnumerator UseAPAndMove(int xDir, int yDir)
    {
        characterManager.actionsQueued = true;

        while (gm.turnManager.IsPlayersTurn() == false)
        {
            yield return null;
        }
        
        if (canMove == false || characterManager.status.isDead)
        {
            characterManager.actionsQueued = false;
            characterManager.remainingAPToBeUsed = 0;
            yield break;
        }

        if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                Move(xDir, yDir, false);
                characterManager.actionsQueued = false;
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
                characterManager.actionsQueued = false;

                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
            }
            else
            {
                characterManager.remainingAPToBeUsed += remainingAP;
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndMove(xDir, yDir));
            }
        }
    }*/
}

using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [HideInInspector] public bool isMoving, onCooldown;

    [Header("Obstacle Mask")]
    public LayerMask movementObstacleMask;

    [HideInInspector] public GameManager gm;

    float moveTime = 0.2f;

    public virtual void Start()
    {
        gm = GameManager.instance;
    }

    public void Move(int xDir, int yDir, bool isNPC)
    {
        Vector2 startCell = transform.position;
        Vector2 targetCell = startCell + new Vector2(xDir, yDir);

        RaycastHit2D hit = Physics2D.Raycast(targetCell, Vector2.zero, 1, movementObstacleMask);
        
        // If the target tile is a walkable tile, the player moves here
        if (hit.collider == null)
            StartCoroutine(SmoothMovement(targetCell, isNPC));
        else
            StartCoroutine(BlockedMovement(targetCell));
    }

    public void FaceForward(Vector2 targetPos)
    {
        if (transform.position.x > targetPos.x)
            transform.localScale = new Vector3(-1, 1);
        else
            transform.localScale = Vector3.one;
    }

    // Move Animation
    public virtual IEnumerator SmoothMovement(Vector2 endPos, bool isNPC)
    {
        isMoving = true;
        FaceForward(endPos);

        // Set pathfinding grid graph tags for current and end position nodes
        gm.gameTiles.SetTagForNode(gm.gameTiles.gridGraph.GetNearest(transform.position).node);
        gm.gameTiles.gridGraph.GetNearest(endPos).node.Tag = 31; // Character tag

        float sqrRemainingDistance = ((Vector2)transform.position - endPos).sqrMagnitude;

        float finalMoveTime = moveTime;
        if (IsDiagonal(endPos))
            finalMoveTime = moveTime / 1.414214f; // 1.414214 is the length of a diagonal movement

        float inverseMoveTime = 1 / finalMoveTime;

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, endPos, inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;
            sqrRemainingDistance = ((Vector2)transform.position - endPos).sqrMagnitude;
            yield return null;
        }

        if (isNPC)
            FinishTurn();
        else
        {
            gm.containerInvUI.ResetContainerIcons(Direction.Center);
            gm.containerInvUI.GetItemsAroundPlayer();
            gm.containerInvUI.PopulateInventoryUI(gm.containerInvUI.playerPositionItems, Direction.Center);
            gm.containerInvUI.playerPositionSideBarButton.HighlightDirectionIcon();
        }

        isMoving = false;
    }

    bool IsDiagonal(Vector2 endPos)
    {
        if (transform.position.x != endPos.x && transform.position.y != endPos.y) 
            return true;

        return false;
    }

    public void FinishTurn()
    {
        gm.turnManager.npcsFinishedTakingTurnCount++;
        if (gm.turnManager.npcsFinishedTakingTurnCount == gm.turnManager.npcs.Count)
            gm.turnManager.ReadyPlayersTurn();
    }

    // Blocked Animation
    IEnumerator BlockedMovement(Vector3 end)
    {
        isMoving = true;

        Vector3 originalPos = transform.position;

        end = transform.position + ((end - transform.position) / 3);
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        float inverseMoveTime = (1 / (moveTime * 2));

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }

        sqrRemainingDistance = (transform.position - originalPos).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, originalPos, inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;
            sqrRemainingDistance = (transform.position - originalPos).sqrMagnitude;
            yield return null;
        }

        isMoving = false;
    }

    IEnumerator ActionCooldown(float cooldownTime)
    {
        onCooldown = true;
        while (cooldownTime > 0f)
        {
            cooldownTime -= Time.deltaTime;
            yield return null;
        }
        onCooldown = false;
    }

    bool TargetTileIsWalkable(Vector2 targetCell)
    {
        if (gm.gameTiles.GetCell(gm.gameTiles.groundTilemap, targetCell) != null || gm.gameTiles.GetCell(gm.gameTiles.shallowWaterTilemap, targetCell) != null)
            return true;

        return false;
    }
}

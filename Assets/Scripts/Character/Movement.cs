using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [HideInInspector] public bool isMoving, onCooldown;
    [HideInInspector] public GameTiles gameTiles;
    [HideInInspector] public TurnManager turnManager;

    float moveTime = 0.2f;

    public virtual void Start()
    {
        gameTiles = GameTiles.instance;
        turnManager = TurnManager.instance;
    }

    public void Move(int xDir, int yDir, bool isNPC)
    {
        Vector2 startCell = transform.position;
        Vector2 targetCell = startCell + new Vector2(xDir, yDir);

        // If the target tile is a walkable tile, the player moves here
        if (TargetTileIsWalkable(targetCell))// || targetHasRoadTile) && (targetHasWallTile == false && targetHasObstacleTile == false && targetHasClosedDoorTile == false))
            StartCoroutine(SmoothMovement(targetCell, isNPC));
        else
            StartCoroutine(BlockedMovement(targetCell));
    }

    // Move Animation
    public virtual IEnumerator SmoothMovement(Vector3 end, bool isNPC)
    {
        isMoving = true;

        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        float inverseMoveTime = 1 / moveTime;

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            yield return null;
        }

        if (isNPC)
        {
            turnManager.npcsFinishedTakingTurnCount++;
            if (turnManager.npcsFinishedTakingTurnCount == turnManager.npcs.Count)
                turnManager.ReadyPlayersTurn();
        }

        isMoving = false;
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
        if (gameTiles.GetCell(gameTiles.groundTilemap, targetCell) != null || gameTiles.GetCell(gameTiles.shallowWaterTilemap, targetCell) != null)
            return true;

        return false;
    }
}

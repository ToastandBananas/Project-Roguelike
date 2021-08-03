using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public bool isMoving, onCooldown;

    [Header("Obstacle Mask")]
    public LayerMask movementObstacleMask;

    [Tooltip("Multiplier for the arc height, which is dependent on the distance from the target (set to 0 for no arc)")]
    public float arcMultiplier = 0.1f;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;

    float moveTime = 0.2f;
    float diaganolMoveTime;

    public virtual void Awake()
    {
        diaganolMoveTime = moveTime / 1.414214f; // 1.414214 is the length of a diagonal movement

        characterManager = GetComponent<CharacterManager>();

        // Make sure the character is properly positioned
        transform.position = Utilities.ClampedPosition(transform.position);
    }

    public virtual void Start()
    {
        gm = GameManager.instance;
    }

    public void FaceForward(Vector2 targetPos)
    {
        if (transform.position.x > targetPos.x)
            transform.localScale = new Vector3(-1, 1);
        else
            transform.localScale = Vector3.one;
    }

    public IEnumerator ArcMovement(Vector2 endPos, int possibleMoveCount = 1)
    {
        isMoving = true;
        transform.position = Utilities.ClampedPosition(transform.position);
        FaceForward(endPos);

        // Set pathfinding grid graph tags for current and end position nodes
        gm.gameTiles.SetTagForNode(gm.gameTiles.gridGraph.GetNearest(transform.position).node);
        gm.gameTiles.gridGraph.GetNearest(endPos).node.Tag = 31; // Character tag

        // Cache our start position, which is really the only thing we need
        // (in addition to our current position, and the target).
        Vector3 startPos = transform.position;

        float x0 = startPos.x;
        float x1 = endPos.x;
        float dist = x1 - x0;
        float arcHeight = dist * arcMultiplier;
        if (endPos.x < transform.position.x)
            arcHeight *= -1f;
        float inverseMoveTime;
        if (IsDiagonal(endPos))
            inverseMoveTime = 1 / (diaganolMoveTime / possibleMoveCount);
        else
            inverseMoveTime = 1 / (moveTime / possibleMoveCount);

        while ((Vector2)transform.position != endPos)
        {
            if (dist == 0 || (-0.25f * dist * dist) == 0)
            {
                transform.position = Utilities.ClampedPosition(transform.position);
                break;
            }
            //if (isNPC) Debug.Log("Arc movement: " + endPos);
            // Compute the next position, with arc added in
            float nextX = Mathf.MoveTowards(transform.position.x, x1, inverseMoveTime * Time.deltaTime);
            float baseY = Mathf.Lerp(startPos.y, endPos.y, (nextX - x0) / dist);
            float arc = arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
            Vector2 nextPos = new Vector2(nextX, baseY + arc);

            // Rotate to face the next position, and then move there
            transform.position = nextPos;

            yield return null;
        }

        if (characterManager.isNPC)
            GameTiles.AddNPC(characterManager, transform.position);

        isMoving = false;
        OnFinishedMoving();
    }

    // Move Animation
    public virtual IEnumerator SmoothMovement(Vector2 endPos, int possibleMoveCount = 1)
    {
        isMoving = true;
        transform.position = Utilities.ClampedPosition(transform.position);
        FaceForward(endPos);

        // Set pathfinding grid graph tags for current and end position nodes
        gm.gameTiles.SetTagForNode(gm.gameTiles.gridGraph.GetNearest(transform.position).node);
        gm.gameTiles.gridGraph.GetNearest(endPos).node.Tag = 31; // Character tag

        float inverseMoveTime;
        if (IsDiagonal(endPos))
            inverseMoveTime = 1 / (diaganolMoveTime / possibleMoveCount);
        else
            inverseMoveTime = 1 / (moveTime / possibleMoveCount);

        while ((Vector2)transform.position != endPos)
        {
            // if (isNPC) Debug.Log("Smooth movement: " + endPos);
            Vector2 newPosition = Vector2.MoveTowards(transform.position, endPos, inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;

            yield return null;
        }

        if (characterManager.isNPC)
            GameTiles.AddNPC(characterManager, transform.position);

        isMoving = false;
        OnFinishedMoving();
    }

    public void TeleportToPosition(Vector2 endPos)
    {
        transform.position = endPos;
        if (characterManager.isNPC)
            GameTiles.AddNPC(characterManager, transform.position);

        OnFinishedMoving();
    }

    void OnFinishedMoving()
    {
        if (characterManager.isNPC)
        {
            if (characterManager.actionQueued == false)
            {
                if (characterManager.characterStats.currentAP <= 0)
                    gm.turnManager.FinishTurn(characterManager);
                else
                    characterManager.TakeTurn();
            }
        }
        else
            gm.containerInvUI.OnPlayerMoved();
    }

    bool IsDiagonal(Vector2 endPos)
    {
        if (transform.position.x != endPos.x && transform.position.y != endPos.y) 
            return true;

        return false;
    }

    // Blocked Animation
    public IEnumerator BlockedMovement(Vector3 endPos)
    {
        isMoving = true;
        FaceForward(endPos);

        Vector3 originalPos = transform.position;

        endPos = transform.position + ((endPos - transform.position) / 3);
        float sqrRemainingDistance = (transform.position - endPos).sqrMagnitude;
        float inverseMoveTime = 1 / (moveTime);

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, endPos, inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;
            sqrRemainingDistance = (transform.position - endPos).sqrMagnitude;
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

        if (characterManager.isNPC)
            OnFinishedMoving();
    }

    public IEnumerator MovementCooldown(float cooldownTime)
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

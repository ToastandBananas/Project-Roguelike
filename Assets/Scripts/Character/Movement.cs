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
        characterManager = GetComponent<CharacterManager>();
    }

    public virtual void Start()
    {
        diaganolMoveTime = moveTime / 1.414214f; // 1.414214 is the length of a diagonal movement

        gm = GameManager.instance;

        // Make sure the character is properly positioned
        ClampPosition();
    }

    public void FaceForward(Vector2 targetPos)
    {
        if (transform.position.x > targetPos.x)
            transform.localScale = new Vector3(-1, 1);
        else
            transform.localScale = Vector3.one;
    }

    public IEnumerator ArcMovement(Vector2 endPos, bool isNPC, int possibleMoveCount = 1)
    {
        isMoving = true;
        ClampPosition();
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
                ClampPosition();
                break;
            }

            Debug.Log("Arc movement: " + endPos);
            // Compute the next position, with arc added in
            float nextX = Mathf.MoveTowards(transform.position.x, x1, inverseMoveTime * Time.deltaTime);
            float baseY = Mathf.Lerp(startPos.y, endPos.y, (nextX - x0) / dist);
            float arc = arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
            Vector2 nextPos = new Vector2(nextX, baseY + arc);

            // Rotate to face the next position, and then move there
            transform.position = nextPos;

            yield return null;
        }
        
        OnFinishedMoving(isNPC);
        isMoving = false;
    }

    // Move Animation
    public virtual IEnumerator SmoothMovement(Vector2 endPos, bool isNPC, int possibleMoveCount = 1)
    {
        isMoving = true;
        ClampPosition();
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
            Debug.Log("Smooth movement: " + endPos);
            Vector2 newPosition = Vector2.MoveTowards(transform.position, endPos, inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;

            yield return null;
        }

        OnFinishedMoving(isNPC);
        isMoving = false;
    }

    public void TeleportToPosition(Vector2 endPos, bool isNPC)
    {
        transform.position = endPos;
        OnFinishedMoving(isNPC);
    }

    void OnFinishedMoving(bool isNPC)
    {
        if (isNPC)
        {
            if (characterManager.characterStats.currentAP <= 0)
                gm.turnManager.FinishTurn(characterManager);
            else
                characterManager.TakeTurn();
        }
        else
            gm.containerInvUI.OnPlayerMoved();
    }

    void ClampPosition()
    {
        if (transform.position.x % 1 != 0)
            transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), transform.position.y);

        if (transform.position.y % 1 != 0)
            transform.position = new Vector2(transform.position.x, Mathf.RoundToInt(transform.position.y));
    }

    bool IsDiagonal(Vector2 endPos)
    {
        if (transform.position.x != endPos.x && transform.position.y != endPos.y) 
            return true;

        return false;
    }

    // Blocked Animation
    public IEnumerator BlockedMovement(Vector3 end)
    {
        isMoving = true;

        Vector3 originalPos = transform.position;

        end = transform.position + ((end - transform.position) / 3);
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        float inverseMoveTime = 1 / (moveTime);

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

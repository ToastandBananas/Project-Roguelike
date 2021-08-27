using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Obstacle Mask")]
    public LayerMask movementObstacleMask;

    [Tooltip("Multiplier for the arc height, which is dependent on the distance from the target (set to 0 for no arc)")]
    public float arcMultiplier = 0.1f;

    [HideInInspector] public bool isMoving, canMove;
    [HideInInspector] public Direction directionFacing;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;

    float moveTime = 0.2f;
    float diagonalMoveTime;

    public virtual void Awake()
    {
        diagonalMoveTime = moveTime / 1.414214f; // 1.414214 is the length of a diagonal movement
        canMove = true;

        characterManager = GetComponent<CharacterManager>();

        // Make sure the character is properly positioned
        transform.position = Utilities.ClampedPosition(transform.position);
        
        if (transform.localScale.x == 1)
            directionFacing = Direction.East;
        else
            directionFacing = Direction.West;
    }

    public virtual void Start()
    {
        gm = GameManager.instance;
    }

    public void FaceForward()
    {
        if ((directionFacing == Direction.East || directionFacing == Direction.Northeast || directionFacing == Direction.Southeast) && transform.localScale.x != 1)
            transform.localScale = Vector3.one;
        else if ((directionFacing == Direction.West || directionFacing == Direction.Northwest || directionFacing == Direction.Southwest) && transform.localScale.x != -1)
            transform.localScale = new Vector3(-1, 1);
    }

    public IEnumerator ArcMovement(Vector2 endPos, int possibleMoveCount = 1)
    {
        characterManager.FinishAction();
        if (isMoving)
        {
            characterManager.FinishAction();
            yield break;
        }

        isMoving = true;
        transform.position = Utilities.ClampedPosition(transform.position);

        // Update the character's position within our GameTiles data
        GameTiles.AddCharacter(characterManager, endPos);

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
            inverseMoveTime = 1 / (diagonalMoveTime / possibleMoveCount);
        else
            inverseMoveTime = 1 / (moveTime / possibleMoveCount);

        while ((Vector2)transform.position != endPos)
        {
            if (dist == 0 || (-0.25f * dist * dist) == 0)
            {
                transform.position = Utilities.ClampedPosition(transform.position);
                break;
            }

            // if (characterManager.isNPC) Debug.Log("Arc movement: " + endPos);
            // Compute the next position, with arc added in
            float nextX = Mathf.MoveTowards(transform.position.x, x1, inverseMoveTime * Time.deltaTime);
            float baseY = Mathf.Lerp(startPos.y, endPos.y, (nextX - x0) / dist);
            float arc = arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
            Vector2 nextPos = new Vector2(nextX, baseY + arc);

            // Rotate to face the next position, and then move there
            transform.position = nextPos;

            yield return null;
        }

        isMoving = false;
        OnFinishedMoving();
    }
    
    public IEnumerator SmoothMovement(Vector2 endPos, int possibleMoveCount = 1)
    {
        characterManager.FinishAction();
        if (isMoving)
        {
            characterManager.FinishAction();
            yield break;
        }

        isMoving = true;
        transform.position = Utilities.ClampedPosition(transform.position);

        // Update the character's position within our GameTiles data
        GameTiles.AddCharacter(characterManager, endPos);

        // Set pathfinding grid graph tags for current and end position nodes
        gm.gameTiles.SetTagForNode(gm.gameTiles.gridGraph.GetNearest(transform.position).node);
        gm.gameTiles.gridGraph.GetNearest(endPos).node.Tag = 31; // Character tag

        float inverseMoveTime;
        if (IsDiagonal(endPos))
            inverseMoveTime = 1 / (diagonalMoveTime / possibleMoveCount);
        else
            inverseMoveTime = 1 / (moveTime / possibleMoveCount);

        while ((Vector2)transform.position != endPos)
        {
            // if (characterManager.isNPC) Debug.Log("Smooth movement: " + endPos);
            Vector2 newPosition = Vector2.MoveTowards(transform.position, endPos, inverseMoveTime * Time.deltaTime);
            transform.position = newPosition;

            yield return null;
        }

        isMoving = false;
        OnFinishedMoving();
    }
    
    public IEnumerator BlockedMovement(Vector3 endPos)
    {
        characterManager.FinishAction();
        if (isMoving)
        {
            characterManager.FinishAction();
            yield break;
        }
        
        isMoving = true;
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

        if (characterManager.attack.isAttacking)
            characterManager.attack.isAttacking = false;

        isMoving = false;
        OnFinishedMoving(false);
    }

    public void TeleportToPosition(Vector2 endPos)
    {
        transform.position = endPos;
        GameTiles.AddCharacter(characterManager, endPos);
        characterManager.FinishAction();
        OnFinishedMoving();
    }

    public void OnFinishedMoving(bool updateTiles = true)
    {
        if (characterManager.isNPC)
        {
            characterManager.npcMovement.moveQueued = false;
            if (characterManager.isMyTurn && characterManager.characterStats.currentAP > 0)
                characterManager.TakeTurn();
        }
        else
        {
            characterManager.vision.CheckEnemyVisibility();
            if (updateTiles)
                gm.containerInvUI.OnPlayerMoved();
        }

        //characterManager.FinishAction();
    }

    public void Rotate(Direction targetDirection, bool queuingAction)
    {
        // Debug.Log(characterManager.name + " is about to rotate...");
        GetRotationsSegmentCount(targetDirection, out int segmentCount, out bool clockwise);

        for (int i = 0; i < segmentCount; i++)
        {
            if (queuingAction)
                characterManager.QueueAction(RotateOneSegment(clockwise, queuingAction), gm.apManager.GetRotateAPCost());
            else
                StartCoroutine(RotateOneSegment(clockwise, queuingAction));
        }
    }

    IEnumerator RotateOneSegment(bool clockwise, bool doingAction)
    {
        // Update current direction facing
        if (characterManager.status.isDead) yield break;

        // Debug.Log(directionFacing + " / " + GetRotationsNextDirection(clockwise));
        directionFacing = GetRotationsNextDirection(clockwise);
        FaceForward();

        // Update arrow graphic to point in current direction facing
        characterManager.humanoidSpriteManager.SetFacingArrowDirection(directionFacing);

        if (doingAction)
            characterManager.FinishAction();
    }

    void GetRotationsSegmentCount(Direction targetDirection, out int count, out bool clockwise)
    {
        switch (directionFacing)
        {
            case Direction.North:
                switch (targetDirection)
                {
                    case Direction.South:
                        count = 4;
                        clockwise = true;
                        break;
                    case Direction.West:
                        count = 2;
                        clockwise = false;
                        break;
                    case Direction.East:
                        count = 2;
                        clockwise = true;
                        break;
                    case Direction.Northwest:
                        count = 1;
                        clockwise = false;
                        break;
                    case Direction.Northeast:
                        count = 1;
                        clockwise = true;
                        break;
                    case Direction.Southwest:
                        count = 3;
                        clockwise = false;
                        break;
                    case Direction.Southeast:
                        count = 3;
                        clockwise = true;
                        break;
                    default:
                        count = 0;
                        clockwise = true;
                        break;
                }
                break;
            case Direction.South:
                switch (targetDirection)
                {
                    case Direction.North:
                        count = 4;
                        clockwise = true;
                        break;
                    case Direction.West:
                        count = 2;
                        clockwise = true;
                        break;
                    case Direction.East:
                        count = 2;
                        clockwise = false;
                        break;
                    case Direction.Northwest:
                        count = 3;
                        clockwise = true;
                        break;
                    case Direction.Northeast:
                        count = 3;
                        clockwise = false;
                        break;
                    case Direction.Southwest:
                        count = 1;
                        clockwise = true;
                        break;
                    case Direction.Southeast:
                        count = 1;
                        clockwise = false;
                        break;
                    default:
                        count = 0;
                        clockwise = true;
                        break;
                }
                break;
            case Direction.West:
                switch (targetDirection)
                {
                    case Direction.North:
                        count = 2;
                        clockwise = true;
                        break;
                    case Direction.South:
                        count = 2;
                        clockwise = false;
                        break;
                    case Direction.East:
                        count = 4;
                        clockwise = true;
                        break;
                    case Direction.Northwest:
                        count = 1;
                        clockwise = true;
                        break;
                    case Direction.Northeast:
                        count = 3;
                        clockwise = true;
                        break;
                    case Direction.Southwest:
                        count = 1;
                        clockwise = false;
                        break;
                    case Direction.Southeast:
                        count = 3;
                        clockwise = false;
                        break;
                    default:
                        count = 0;
                        clockwise = true;
                        break;
                }
                break;
            case Direction.East:
                switch (targetDirection)
                {
                    case Direction.North:
                        count = 2;
                        clockwise = false;
                        break;
                    case Direction.South:
                        count = 2;
                        clockwise = true;
                        break;
                    case Direction.West:
                        count = 4;
                        clockwise = true;
                        break;
                    case Direction.Northwest:
                        count = 3;
                        clockwise = false;
                        break;
                    case Direction.Northeast:
                        count = 1;
                        clockwise = false;
                        break;
                    case Direction.Southwest:
                        count = 3;
                        clockwise = true;
                        break;
                    case Direction.Southeast:
                        count = 1;
                        clockwise = true;
                        break;
                    default:
                        count = 0;
                        clockwise = true;
                        break;
                }
                break;
            case Direction.Northwest:
                switch (targetDirection)
                {
                    case Direction.North:
                        count = 1;
                        clockwise = true;
                        break;
                    case Direction.South:
                        count = 3;
                        clockwise = false;
                        break;
                    case Direction.West:
                        count = 1;
                        clockwise = false;
                        break;
                    case Direction.East:
                        count = 3;
                        clockwise = true;
                        break;
                    case Direction.Northeast:
                        count = 2;
                        clockwise = true;
                        break;
                    case Direction.Southwest:
                        count = 2;
                        clockwise = false;
                        break;
                    case Direction.Southeast:
                        count = 4;
                        clockwise = true;
                        break;
                    default:
                        count = 0;
                        clockwise = true;
                        break;
                }
                break;
            case Direction.Northeast:
                switch (targetDirection)
                {
                    case Direction.North:
                        count = 1;
                        clockwise = false;
                        break;
                    case Direction.South:
                        count = 3;
                        clockwise = true;
                        break;
                    case Direction.West:
                        count = 3;
                        clockwise = false;
                        break;
                    case Direction.East:
                        count = 1;
                        clockwise = true;
                        break;
                    case Direction.Northwest:
                        count = 2;
                        clockwise = false;
                        break;
                    case Direction.Southwest:
                        count = 4;
                        clockwise = true;
                        break;
                    case Direction.Southeast:
                        count = 2;
                        clockwise = true;
                        break;
                    default:
                        count = 0;
                        clockwise = true;
                        break;
                }
                break;
            case Direction.Southwest:
                switch (targetDirection)
                {
                    case Direction.North:
                        count = 3;
                        clockwise = true;
                        break;
                    case Direction.South:
                        count = 1;
                        clockwise = false;
                        break;
                    case Direction.West:
                        count = 1;
                        clockwise = true;
                        break;
                    case Direction.East:
                        count = 3;
                        clockwise = false;
                        break;
                    case Direction.Northwest:
                        count = 2;
                        clockwise = true;
                        break;
                    case Direction.Northeast:
                        count = 4;
                        clockwise = true;
                        break;
                    case Direction.Southeast:
                        count = 2;
                        clockwise = false;
                        break;
                    default:
                        count = 0;
                        clockwise = true;
                        break;
                }
                break;
            case Direction.Southeast:
                switch (targetDirection)
                {
                    case Direction.North:
                        count = 3;
                        clockwise = false;
                        break;
                    case Direction.South:
                        count = 1;
                        clockwise = true;
                        break;
                    case Direction.West:
                        count = 3;
                        clockwise = true;
                        break;
                    case Direction.East:
                        count = 1;
                        clockwise = false;
                        break;
                    case Direction.Northwest:
                        count = 4;
                        clockwise = true;
                        break;
                    case Direction.Northeast:
                        count = 2;
                        clockwise = false;
                        break;
                    case Direction.Southwest:
                        count = 2;
                        clockwise = true;
                        break;
                    default:
                        count = 0;
                        clockwise = true;
                        break;
                }
                break;
            default:
                count = 0;
                clockwise = true;
                break;
        }
    }

    Direction GetRotationsNextDirection(bool clockwise)
    {
        switch (directionFacing)
        {
            case Direction.North:
                if (clockwise)
                    return Direction.Northeast;
                else
                    return Direction.Northwest;
            case Direction.South:
                if (clockwise)
                    return Direction.Southwest;
                else
                    return Direction.Southeast;
            case Direction.West:
                if (clockwise)
                    return Direction.Northwest;
                else
                    return Direction.Southwest;
            case Direction.East:
                if (clockwise)
                    return Direction.Southeast;
                else
                    return Direction.Northeast;
            case Direction.Northwest:
                if (clockwise)
                    return Direction.North;
                else
                    return Direction.West;
            case Direction.Northeast:
                if (clockwise)
                    return Direction.East;
                else
                    return Direction.North;
            case Direction.Southwest:
                if (clockwise)
                    return Direction.West;
                else
                    return Direction.South;
            case Direction.Southeast:
                if (clockwise)
                    return Direction.South;
                else
                    return Direction.East;
            default:
                return Direction.East;
        }
    }

    public bool IsBehindCharacter(CharacterManager character)
    {
        switch (character.movement.directionFacing)
        {
            case Direction.North:
                if (transform.position.y < character.transform.position.y)
                    return true;
                break;
            case Direction.South:
                if (transform.position.y > character.transform.position.y)
                    return true;
                break;
            case Direction.West:
                if (transform.position.x > character.transform.position.x)
                    return true;
                break;
            case Direction.East:
                if (transform.position.x < character.transform.position.x)
                    return true;
                break;
            case Direction.Northwest:
                if ((transform.position.x > character.transform.position.x && transform.position.y < character.transform.position.y) 
                    || (transform.position.x == character.transform.position.x && transform.position.y < character.transform.position.y)
                    || (transform.position.x > character.transform.position.x && transform.position.y == character.transform.position.y))
                    return true;
                break;
            case Direction.Northeast:
                if ((transform.position.x < character.transform.position.x && transform.position.y < character.transform.position.y)
                    || (transform.position.x == character.transform.position.x && transform.position.y < character.transform.position.y)
                    || (transform.position.x < character.transform.position.x && transform.position.y == character.transform.position.y))
                    return true;
                break;
            case Direction.Southwest:
                if ((transform.position.x > character.transform.position.x && transform.position.y > character.transform.position.y)
                    || (transform.position.x == character.transform.position.x && transform.position.y > character.transform.position.y)
                    || (transform.position.x > character.transform.position.x && transform.position.y == character.transform.position.y))
                    return true;
                break;
            case Direction.Southeast:
                if ((transform.position.x < character.transform.position.x && transform.position.y > character.transform.position.y)
                    || (transform.position.x == character.transform.position.x && transform.position.y > character.transform.position.y)
                    || (transform.position.x < character.transform.position.x && transform.position.y == character.transform.position.y))
                    return true;
                break;
            default:
                return false;
        }
        return false;
    }

    public bool IsDiagonal(Vector2 endPos)
    {
        if (Mathf.RoundToInt(transform.position.x) != Mathf.RoundToInt(endPos.x) && Mathf.RoundToInt(transform.position.y) != Mathf.RoundToInt(endPos.y)) 
            return true;
        return false;
    }

    public IEnumerator MovementCooldown(float cooldownTime)
    {
        canMove = false;
        yield return new WaitForSeconds(cooldownTime);
        canMove = true;
    }

    bool TargetTileIsWalkable(Vector2 targetCell)
    {
        if (gm.gameTiles.GetCell(gm.gameTiles.groundTilemap, targetCell) != null || gm.gameTiles.GetCell(gm.gameTiles.shallowWaterTilemap, targetCell) != null)
            return true;

        return false;
    }
}

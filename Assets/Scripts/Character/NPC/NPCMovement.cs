using Pathfinding;
using System.Collections;
using UnityEngine;
using CodeMonkey.Utils;

public class NPCMovement : Movement
{
    [Header("Flee State Variables")]
    public LayerMask fleeObstacleMask;
    public float fleeDistance = 20f;
    public bool shouldAlwaysFleeCombat;

    [Header("Follow State Variables")]
    public CharacterManager leader;
    public float startFollowingDistance = 3f;
    public float slowDownDistance = 4f;
    public bool shouldFollowLeader;

    [Header("Patrol State Variables")]
    public Vector2[] patrolPoints;

    [Header("Pursue State Variables")]
    public float maxChaseDistance = 15f;

    [Header("Wandering State Variables")]
    public Vector2 defaultPosition;
    public float minRoamDistance = 5f;
    public float maxRoamDistance = 20f;

    [HideInInspector] public Transform targetFleeingFrom;

    [HideInInspector] public int currentPatrolPointIndex;
    [HideInInspector] public bool initialPatrolPointSet;

    [HideInInspector] public CharacterManager target;
    [HideInInspector] public Vector2 targetPosition;

    [HideInInspector] public bool moveQueued;
    [HideInInspector] public AIPath AIPath;
    [HideInInspector] public AIDestinationSetter AIDestSetter;
    Seeker seeker;

    Vector2 roamPosition;
    Vector3 fleeDestination;
    float distToFleeDestination = 0;

    bool roamPositionSet;
    bool needsFleeDestination = true;

    public override void Awake()
    {
        base.Awake();

        seeker = GetComponent<Seeker>();
        AIDestSetter = GetComponent<AIDestinationSetter>();
        AIPath = GetComponent<AIPath>();
        AIPath.canMove = false;

        if (defaultPosition == Vector2.zero)
            defaultPosition = transform.position;
    }

    public override void Start()
    {
        base.Start();

        gm.turnManager.npcs.Add(characterManager);
    }

    public IEnumerator Move()
    {
        if (moveQueued || isMoving || characterManager.status.isDead) yield break;

        // Update the character's path
        AIPath.SearchPath();

        // Finish searching for a path before moving
        while (AIPath.pathPending) { yield return null; }

        //if (isMoving) yield break;

        Vector2 nextPos = GetNextPosition();
        Rotate(GetNextDirection(nextPos), true);
        characterManager.QueueAction(MoveToNextPointOnPath(), gm.apManager.GetMovementAPCost(IsDiagonal(nextPos)));
    }

    IEnumerator MoveToNextPointOnPath()
    {
        if (characterManager.status.isDead) yield break;

        moveQueued = true;

        // Update the character's path
        AIPath.SearchPath();

        // Finish searching for a path before moving
        while (AIPath.pathPending) { yield return null; }

        // Get the next position and the direction the character needs to face
        Vector2 nextPos = GetNextPosition();

        // Make sure the character is facing the correct direction
        Rotate(GetNextDirection(nextPos), false);

        // Remove the NPC from the tile they were on, if they are moving
        if (nextPos != (Vector2)transform.position)
            GameTiles.RemoveCharacter(transform.position);

        // Possible move count will determine the speed of the character's movement, to prevent gameplay delays
        int possibleMoveCount = Mathf.FloorToInt(characterManager.characterStats.maxAP.GetValue() / gm.apManager.GetMovementAPCost(false));

        // Move
        if (characterManager.spriteRenderer.isVisible == false)
            TeleportToPosition(nextPos);
        else if (transform.position.y == nextPos.y)
            StartCoroutine(ArcMovement(nextPos, possibleMoveCount));
        else
            StartCoroutine(SmoothMovement(nextPos, possibleMoveCount));
    }

    Vector2 GetNextPosition()
    {
        Path path = seeker.GetCurrentPath();
        if (path == null || path.vectorPath.Count <= 1)
            return transform.position;
        else
        {
            Vector3 dir = (path.vectorPath[1] - transform.position).normalized;
            Vector2 nextPos;
            if (dir == new Vector3(0, 1) || dir == new Vector3(0, -1) || dir == new Vector3(-1, 0) || dir == new Vector3(1, 0)) // Up, down, left, or right
                nextPos = transform.position + dir;
            else if (dir.x < 0 && dir.y > 0) // Up-left
                nextPos = transform.position + new Vector3(-1, 1);
            else if (dir.x > 0 && dir.y > 0) // Up-right
                nextPos = transform.position + new Vector3(1, 1);
            else if (dir.x < 0 && dir.y < 0) // Down-left
                nextPos = transform.position + new Vector3(-1, -1);
            else if (dir.x > 0 && dir.y < 0) // Down-right
                nextPos = transform.position + new Vector3(1, -1);
            else
                return transform.position;

            if (gm.gameTiles.gridGraph.GetNearest(nextPos).node.Tag == 31)
                return transform.position;
            else
            {
                // Make sure the NPC isn't trying to move on top of an obstacle, such as an object or wall
                RaycastHit2D hit = Physics2D.Raycast(nextPos, Vector2.zero, 1, movementObstacleMask);
                if (hit.collider == null && GameTiles.characters.TryGetValue(nextPos, out CharacterManager character) == false)
                    return nextPos;
                else
                    return transform.position;
            }
        }
    }

    Direction GetNextDirection(Vector2 nextPos)
    {
        Path path = seeker.GetCurrentPath();
        if (path == null || path.vectorPath.Count <= 1)
        {
            return directionFacing;
        }
        else
        {
            if (nextPos.x == transform.position.x && nextPos.y == transform.position.y + 1)
                return Direction.North;
            else if (nextPos.x == transform.position.x && nextPos.y == transform.position.y - 1)
                return Direction.South;
            else if (nextPos.x == transform.position.x + 1 && nextPos.y == transform.position.y)
                return Direction.East;
            else if (nextPos.x == transform.position.x - 1 && nextPos.y == transform.position.y)
                return Direction.West;
            else if (nextPos.x == transform.position.x - 1 && nextPos.y == transform.position.y + 1)
                return Direction.Northwest;
            else if (nextPos.x == transform.position.x + 1 && nextPos.y == transform.position.y + 1)
                return Direction.Northeast;
            else if (nextPos.x == transform.position.x + 1 && nextPos.y == transform.position.y - 1)
                return Direction.Southeast;
            else if (nextPos.x == transform.position.x - 1 && nextPos.y == transform.position.y - 1)
                return Direction.Southwest;
            else
                return directionFacing;
        }
    }

    #region Set Pathfinding Target
    public void SetTarget(CharacterManager targetsCharManager)
    {
        // Set pathfinding variables
        target = targetsCharManager;
        AIDestSetter.target = targetsCharManager.transform;
        if (characterManager.attack.TargetInAttackRange(targetsCharManager.transform))
            AIDestSetter.moveToTargetPos = false;
    }

    public void SetPathToCurrentTarget()
    {
        if (target != null) SetTarget(target);
    }

    public void SetPathToTargetAndMove()
    {
        SetPathToCurrentTarget();
        if (target != null && isMoving == false && characterManager.characterStats.currentAP > 0)
            StartCoroutine(Move());
        else if (isMoving == false)
            StartCoroutine(gm.turnManager.FinishTurn(characterManager));
    }

    // Used for waypoint and wandering movement (or any other time assigning a target transform is impossible)
    public void SetTargetPosition(Vector2 targetPosition)
    {
        // Set pathfinding variables
        AIDestSetter.targetPos = targetPosition;
        this.targetPosition = targetPosition;
        AIDestSetter.moveToTargetPos = true;
    }
    #endregion

    #region Follow
    public void FollowTarget(CharacterManager theTarget)
    {
        if (theTarget != null)
        {
            float distToTarget = Vector2.Distance(theTarget.transform.position, transform.position);

            if (distToTarget <= startFollowingDistance)
            {
                StartCoroutine(gm.turnManager.FinishTurn(characterManager));
                return;
            }
            else if (characterManager.npcMovement.AIDestSetter.target != theTarget)
                SetTarget(theTarget);

            if (characterManager.movement.isMoving == false)
                StartCoroutine(Move());
        }
        else
        {
            Debug.LogWarning(name + " doesn't have a target to follow.");
            characterManager.stateController.SetToDefaultState(shouldFollowLeader);
        }
    }

    public void FollowPlayer()
    {
        FollowTarget(PlayerManager.instance);
    }
    #endregion

    #region Flee
    public void Flee(Transform targetToFleeFrom, float fleeDist)
    {
        float distToTarget = Vector2.Distance(targetToFleeFrom.position, transform.position);

        if (distToTarget < fleeDist)
        {
            // If the character is just starting to flee or if they're close to their destination, but are still within range of the targetToFleeFrom
            if (needsFleeDestination || distToFleeDestination < 3f)
            {
                // Move in the opposite direction of the target
                SetFleeDestination(targetToFleeFrom, false);
                SetTargetPosition(fleeDestination);
                needsFleeDestination = false;
            }

            distToFleeDestination = Vector2.Distance(fleeDestination, transform.position);

            // Check if there is a wall in the way
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (fleeDestination - transform.position).normalized, 3f, fleeObstacleMask);

            // If the character is about to run into a wall, choose a new destination
            if (hit.collider != null)
            {
                // Try again with a random direction this time
                SetFleeDestination(targetToFleeFrom, true);
                distToFleeDestination = Vector2.Distance(fleeDestination, transform.position);
                SetTargetPosition(fleeDestination);
            }

            if (characterManager.movement.isMoving == false)
                StartCoroutine(Move());
        }
        else // If the character has reached a safe distance from the targetToFleeFrom, resume its default State
        {
            characterManager.stateController.SetToDefaultState(shouldFollowLeader);
            targetFleeingFrom = null;
            ResetToDefaults();
        }
    }

    void SetFleeDestination(Transform targetToFleeFrom, bool useRandomDirection)
    {
        Vector3 fleeDirection;
        if (useRandomDirection)
            fleeDirection = UtilsClass.GetRandomDir();
        else
            fleeDirection = (transform.position - targetToFleeFrom.position).normalized;

        fleeDestination = transform.position + (fleeDirection * (fleeDistance + 1f));
    }
    #endregion

    #region Patrol
    public void DoPatrol()
    {
        if (patrolPoints.Length > 0)
        {
            if (initialPatrolPointSet == false)
            {
                currentPatrolPointIndex = GetNearestPatrolPointIndex();
                initialPatrolPointSet = true;
            }

            if (Vector2.Distance(patrolPoints[currentPatrolPointIndex], transform.position) <= 0.1f)
            {
                if (currentPatrolPointIndex == patrolPoints.Length - 1)
                    currentPatrolPointIndex = 0;
                else
                    currentPatrolPointIndex++;

                SetTargetPosition(patrolPoints[currentPatrolPointIndex]);
            }
            else if (targetPosition != patrolPoints[currentPatrolPointIndex])
                SetTargetPosition(patrolPoints[currentPatrolPointIndex]);

            if (characterManager.movement.isMoving == false)
                StartCoroutine(Move());
        }
        else
        {
            Debug.LogWarning("No patrol points set for " + name);
            characterManager.stateController.SetToDefaultState(shouldFollowLeader);
        }
    }

    int GetNearestPatrolPointIndex()
    {
        int nearestPatrolPointIndex = 0;
        float nearestPatrolPointDistance = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (i == 0)
                nearestPatrolPointDistance = Vector2.Distance(patrolPoints[i], transform.position);
            else
            {
                float dist = Vector2.Distance(patrolPoints[i], transform.position);
                if (dist < nearestPatrolPointDistance)
                {
                    nearestPatrolPointIndex = i;
                    nearestPatrolPointDistance = dist;
                }
            }
        }

        return nearestPatrolPointIndex;
    }
    #endregion

    #region Pursue
    public void PursueTarget()
    {
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(target.transform.position, transform.position);

            if (distanceToTarget <= characterManager.npcAttack.combatRange && characterManager.stateController.currentState != State.Fight)
            {
                characterManager.npcAttack.targetInCombatRange = true;
                characterManager.npcAttack.currentCombatState = CombatState.MoveInToAttack;
                characterManager.stateController.SetCurrentState(State.Fight);
                characterManager.npcAttack.Fight();
            }
            else
            {
                characterManager.npcAttack.targetInCombatRange = false;
                if (distanceToTarget <= maxChaseDistance)
                {
                    SetPathToTargetAndMove();
                }
                else
                {
                    characterManager.npcAttack.SwitchTarget_Nearest();
                    SetPathToTargetAndMove();
                }

            }
        }
        else if (AIDestSetter.moveToTargetPos)
        {
            if (characterManager.movement.isMoving == false)
                StartCoroutine(Move());
        }
        else
        {
            if (characterManager.vision.knownEnemiesInRange.Count > 0)
            {
                SetTarget(characterManager.vision.GetClosestKnownEnemy());

                if (characterManager.movement.isMoving == false)
                    StartCoroutine(Move());
            }
            else
            {
                StopPathfinding();
                StartCoroutine(gm.turnManager.FinishTurn(characterManager));
                characterManager.npcAttack.targetInCombatRange = false;
                characterManager.stateController.SetToDefaultState(shouldFollowLeader);
            }
        }
    }
    #endregion

    #region Wander
    public void WanderAround()
    {
        if (roamPositionSet == false)
        {
            roamPosition = GetNewRoamingPosition();
            if (RoamingPositionValid())
            {
                SetTargetPosition(roamPosition);

                if (characterManager.movement.isMoving == false)
                    StartCoroutine(Move());
            }
            else
                StartCoroutine(gm.turnManager.FinishTurn(characterManager));
        }
        else if (Vector2.Distance(roamPosition, transform.position) <= 0.1f)
        {
            // Get a new roamPosition when the current one is reached
            roamPositionSet = false;
            StartCoroutine(gm.turnManager.FinishTurn(characterManager));
        }
        else if (characterManager.movement.isMoving == false)
            StartCoroutine(Move());
    }

    bool RoamingPositionValid()
    {
        RaycastHit2D hit = Physics2D.Raycast(roamPosition, Vector2.zero, 5f, movementObstacleMask);
        if (hit.collider != null)
        {
            // Debug.Log("Invalid roamPosition...finding new one now. Collider hit: " + hit.collider);
            roamPositionSet = false;
            return false;
        }

        return true;
    }

    Vector3 GetNewRoamingPosition()
    {
        roamPositionSet = true;
        Vector2 roamPos = defaultPosition + UtilsClass.GetRandomDir() * Random.Range(minRoamDistance, maxRoamDistance);
        roamPos = new Vector2(Mathf.RoundToInt(roamPos.x), Mathf.RoundToInt(roamPos.y));
        return roamPos;
    }
    #endregion

    public void ResetToDefaults()
    {
        roamPositionSet = false;
        needsFleeDestination = true;
        initialPatrolPointSet = false;

        fleeDestination = Vector3.zero;
        distToFleeDestination = 0;
    }

    IEnumerator DelayStopPathfinding(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        StopPathfinding();
    }

    public void StopPathfinding()
    {
        AIDestSetter.target = null;
        AIDestSetter.targetPos = Vector2.zero;
        AIDestSetter.moveToTargetPos = false;
    }
}

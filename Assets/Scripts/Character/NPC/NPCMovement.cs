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
    public Transform leader;
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

    [HideInInspector] public Transform target;
    [HideInInspector] public Vector2 targetPosition;

    [HideInInspector] public CharacterManager characterManager;
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
        characterManager = GetComponent<CharacterManager>();

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

        turnManager.npcs.Add(this);
    }

    public void TakeTurn()
    {
        characterManager.vision.CheckEnemyVisibility();
        characterManager.stateController.DoAction();
    }

    public IEnumerator MoveToNextPointOnPath()
    {
        yield return null;

        AIPath.SearchPath();

        while (AIPath.pathPending)
        {
            // Finish searching for a path before moving
            yield return null;
        }

        StartCoroutine(SmoothMovement(GetNextPosition(), true));
    }

    Vector3 GetNextPosition()
    {
        Path path = seeker.GetCurrentPath();
        if (path == null || path.vectorPath.Count <= 1)
            return transform.position;
        else
        {
            Vector3 dir = (path.vectorPath[1] - transform.position).normalized;
            Vector3 nextPos;
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

            if (gameTiles.gridGraph.GetNearest(nextPos).node.Tag == 31)
                return transform.position;
            else
            {
                // Make sure the NPC isn't trying to move on top of an obstacle, such as an object or wall
                RaycastHit2D hit = Physics2D.Raycast(nextPos, Vector2.zero, 1, movementObstacleMask);
                if (hit.collider == null)
                    return nextPos;
                else
                    return transform.position;
            }
        }
    }

    #region Set Pathfinding Target
    public void SetTarget(Transform targetTransform)
    {
        // Set pathfinding variables
        target = targetTransform;
        AIDestSetter.target = targetTransform;
        AIDestSetter.moveToTargetPos = false;
    }

    public void SetPathToCurrentTarget()
    {
        if (target != null) SetTarget(target);
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
    public void FollowTarget(Transform theTarget)
    {
        if (theTarget != null)
        {
            float distToTarget = Vector2.Distance(theTarget.position, transform.position);

            if (distToTarget <= startFollowingDistance)
            {
                FinishTurn();
                return;
            }
            else if (characterManager.npcMovement.AIDestSetter.target != theTarget)
                SetTarget(theTarget);

            StartCoroutine(MoveToNextPointOnPath());
        }
        else
        {
            Debug.LogWarning(name + " doesn't have a target to follow.");
            characterManager.stateController.SetToDefaultState(shouldFollowLeader);
        }
    }

    public void FollowPlayer()
    {
        FollowTarget(PlayerManager.instance.playerGameObject.transform);
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

            StartCoroutine(MoveToNextPointOnPath());
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

            StartCoroutine(MoveToNextPointOnPath());
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
            float distanceToTarget = Vector2.Distance(target.position, transform.position);

            if (distanceToTarget <= characterManager.npcAttack.combatRange && characterManager.stateController.currentState != State.Fight)
            {
                characterManager.npcAttack.targetInCombatRange = true;
                characterManager.npcAttack.currentCombatState = CombatState.MoveInToAttack;
                characterManager.stateController.SetCurrentState(State.Fight);
                characterManager.npcAttack.Fight();
            }
            else
            {
                if (distanceToTarget <= maxChaseDistance)
                {
                    SetTarget(target);
                    StartCoroutine(MoveToNextPointOnPath());
                }
                else
                {
                    StopPathfinding();
                    FinishTurn();
                }

                characterManager.npcAttack.targetInCombatRange = false;
                characterManager.npcAttack.targetInAttackRange = false;
            }
        }
        else if (AIDestSetter.moveToTargetPos)
        {
            StartCoroutine(MoveToNextPointOnPath());
        }
        else
        {
            if (characterManager.vision.knownEnemiesInRange.Count > 0)
            {
                SetTarget(characterManager.alliances.GetClosestKnownEnemy());
                StartCoroutine(MoveToNextPointOnPath());
            }
            else
            {
                StopPathfinding();
                FinishTurn();
                characterManager.npcAttack.targetInCombatRange = false;
                characterManager.npcAttack.targetInAttackRange = false;
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
                StartCoroutine(MoveToNextPointOnPath());
            }
            else
                FinishTurn();
        }
        else if (Vector2.Distance(roamPosition, transform.position) <= 0.1f)
        {
            // Get a new roamPosition when the current one is reached
            roamPositionSet = false;
            FinishTurn();
        }
        else
            StartCoroutine(MoveToNextPointOnPath());
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
        roamPos = new Vector2(Mathf.RoundToInt(roamPos.x) + 0.5f, Mathf.RoundToInt(roamPos.y) + 0.5f);
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

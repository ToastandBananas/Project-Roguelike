using UnityEngine;

public enum CombatState { MoveInToAttack, CircleOpponent, Ranged }

public class NPCAttack : Attack
{
    [HideInInspector] public CombatState currentCombatState;

    [HideInInspector] public float combatRange;
    [HideInInspector] public bool targetInCombatRange;

    public override void Start()
    {
        base.Start();

        combatRange = characterManager.vision.lookRadius;
    }

    public void Fight()
    {
        if (characterManager.status.isDead)
            return;
        
        // If the NPC has a target
        if (characterManager.npcMovement.target != null)
        {
            float distanceToTarget = Vector2.Distance(characterManager.npcMovement.target.transform.position, transform.position);
            int distX = Mathf.RoundToInt(Mathf.Abs(transform.position.x - characterManager.npcMovement.target.transform.position.x));
            int distY = Mathf.RoundToInt(Mathf.Abs(transform.position.y - characterManager.npcMovement.target.transform.position.y));

            // If the target is too far away for combat, grab the nearest known enemy and pursue them. Otherwise, if there are no known enemies, go back to the default State
            if (distanceToTarget > combatRange)
            {
                targetInCombatRange = false;
                SwitchTarget(characterManager.vision.GetClosestKnownEnemy());

                characterManager.npcMovement.SetPathToCurrentTarget();

                if (characterManager.npcMovement.target != null && characterManager.npcMovement.isMoving == false)
                    StartCoroutine(characterManager.npcMovement.Move());
            }
            // If the target is close enough for combat, move in to attack
            else if (targetInCombatRange == false && distanceToTarget <= combatRange)
            {
                targetInCombatRange = true;
                currentCombatState = CombatState.MoveInToAttack;
                SetMoveToTargetPos(false);
            }

            // This will only run if the Target is not null at this point
            if (currentCombatState == CombatState.Ranged)
                KeepDistanceAndShoot();
            else if (currentCombatState == CombatState.MoveInToAttack)
                MoveInToAttack();
            else if (currentCombatState == CombatState.CircleOpponent)
                CircleOpponent();
        }
        else // If the NPC doesn't have a target, grab the nearest known enemy and pursue them. Otherwise, if there are no known enemies, go back to the default State
        {
            SwitchTarget(characterManager.vision.GetClosestKnownEnemy());

            if (characterManager.npcMovement.target == null)
                StartCoroutine(gm.turnManager.FinishTurn(characterManager));
            else
                Fight();
        }
    }

    public void SwitchTarget(CharacterManager newTarget)
    {
        Debug.Log("Switching target to: " + newTarget);
        if (newTarget == null)
        {
            characterManager.npcMovement.target = null;
            characterManager.stateController.SetToDefaultState(characterManager.npcMovement.shouldFollowLeader);
            StartCoroutine(gm.turnManager.FinishTurn(characterManager));
            return;
        }

        characterManager.npcMovement.SetTarget(newTarget);

        if (characterManager.npcMovement.target == null)
            characterManager.stateController.SetToDefaultState(characterManager.npcMovement.shouldFollowLeader);
        else if (targetInCombatRange == false)
            characterManager.stateController.SetCurrentState(State.MoveToTarget);
        else
            characterManager.stateController.SetCurrentState(State.Fight);

        StartCoroutine(gm.turnManager.FinishTurn(characterManager));
    }

    public void SwitchTarget_Nearest()
    {
        SwitchTarget(characterManager.vision.GetClosestKnownEnemy());
    }

    public void MoveInToAttack()
    {
        // If close enough, do attack animation
        if (TargetInAttackRange(characterManager.npcMovement.target.transform))
        {
            DetermineAttack(characterManager.npcMovement.target, characterManager.npcMovement.target.characterStats);
        }
        else
        {
            characterManager.npcMovement.SetPathToCurrentTarget();

            if (characterManager.movement.isMoving == false)
                StartCoroutine(characterManager.npcMovement.Move());
        }
    }

    public override void DetermineAttack(CharacterManager targetsCharacterManager, Stats targetsStats)
    {
        characterManager.movement.Rotate(GetDirectionToTarget(targetsStats.transform));
        StartRandomMeleeAttack(targetsCharacterManager, targetsStats);
    }

    public Direction GetDirectionToTarget(Transform targetsTransform)
    {
        if (transform.position == targetsTransform.position)
            return Direction.Center;
        else if (transform.position.x == targetsTransform.position.x)
        {
            if (transform.position.y > targetsTransform.position.y)
                return Direction.South;
            else
                return Direction.North;
        }
        else if (transform.position.y == targetsTransform.position.y)
        {
            if (transform.position.x > targetsTransform.position.x)
                return Direction.West;
            else
                return Direction.East;
        }
        else if (transform.position.x < targetsTransform.position.x && transform.position.y > targetsTransform.position.y)
            return Direction.Southeast;
        else if (transform.position.x > targetsTransform.position.x && transform.position.y > targetsTransform.position.y)
            return Direction.Southwest;
        else if (transform.position.x < targetsTransform.position.x && transform.position.y < targetsTransform.position.y)
            return Direction.Northeast;
        else if (transform.position.x > targetsTransform.position.x && transform.position.y < targetsTransform.position.y)
            return Direction.Northwest;
        return Direction.Center;
    }

    void SetMoveToTargetPos(bool moveToTargetPos)
    {
        characterManager.npcMovement.AIDestSetter.moveToTargetPos = moveToTargetPos;
    }

    void CircleOpponent()
    {
        // TODO
    }

    // For ranged combat
    void KeepDistanceAndShoot()
    {
        // TODO
    }
}

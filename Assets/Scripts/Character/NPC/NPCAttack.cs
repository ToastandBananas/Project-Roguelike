using UnityEngine;

public enum CombatState { MoveInToAttack, CircleOpponent, Ranged }

public class NPCAttack : Attack
{
    public float backoffDistance = 1.25f;
    [HideInInspector] public float combatRange;

    public bool targetInCombatRange;
    public bool targetInAttackRange;

    [HideInInspector] public CombatState currentCombatState;

    public override void Start()
    {
        base.Start();

        combatRange = characterManager.vision.lookRadius;
    }

    public void Fight()
    {
        // If the NPC has a target
        if (characterManager.npcMovement.target != null)
        {
            float distanceToTarget = Vector2.Distance(characterManager.npcMovement.target.position, transform.position);
            int distX = Mathf.RoundToInt(Mathf.Abs(transform.position.x - characterManager.npcMovement.target.position.x));
            int distY = Mathf.RoundToInt(Mathf.Abs(transform.position.y - characterManager.npcMovement.target.position.y));

            targetInAttackRange = TargetInAttackRange();

            // If the target is too far away for combat, grab the nearest known enemy and pursue them. Otherwise, if there are no known enemies, go back to the default State
            if (distanceToTarget > combatRange)
            {
                targetInCombatRange = false;
                SwitchTarget(characterManager.alliances.GetClosestKnownEnemy());

                if (characterManager.npcMovement.target == null)
                {
                    characterManager.npcMovement.FinishTurn();
                    return;
                }
                else
                {
                    characterManager.npcMovement.SetPathToCurrentTarget();
                    StartCoroutine(characterManager.npcMovement.MoveToNextPointOnPath());
                }
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
            SwitchTarget(characterManager.alliances.GetClosestKnownEnemy());

            if (characterManager.npcMovement.target == null)
                characterManager.npcMovement.FinishTurn();
            else
                Fight();
        }
    }

    public void SwitchTarget(Transform newTarget)
    {
        Debug.Log("Switching target...");
        characterManager.npcMovement.SetTarget(newTarget);

        if (characterManager.npcMovement.target == null)
            characterManager.stateController.SetToDefaultState(characterManager.npcMovement.shouldFollowLeader);
        else if (targetInCombatRange == false)
            characterManager.stateController.SetCurrentState(State.MoveToTarget);
        else
            characterManager.stateController.SetCurrentState(State.Fight);
    }

    public void MoveInToAttack()
    {
        // If close enough, do attack animation
        if (targetInAttackRange)
        {
            DoAttack();
        }
        else if (targetInAttackRange == false)
        {
            characterManager.npcMovement.SetPathToCurrentTarget();
            StartCoroutine(characterManager.npcMovement.MoveToNextPointOnPath());
        }
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

    public bool TargetInAttackRange()
    {
        int distX = Mathf.RoundToInt(Mathf.Abs(transform.position.x - characterManager.npcMovement.target.position.x));
        int distY = Mathf.RoundToInt(Mathf.Abs(transform.position.y - characterManager.npcMovement.target.position.y));
        
        if (distX <= attackRange && distY <= attackRange)
            return true;

        return false;
    }
}

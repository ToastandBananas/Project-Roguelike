using UnityEngine;

public enum CombatState { MoveInToAttack, CircleOpponent, Ranged }

public class NPCAttack : Attack
{
    [HideInInspector] public float combatRange;
    [HideInInspector] public bool targetInCombatRange;
    [HideInInspector] public bool targetInAttackRange;

    [HideInInspector] public CombatState currentCombatState;

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

            targetInAttackRange = TargetInAttackRange(characterManager.npcMovement.target.transform);

            // If the target is too far away for combat, grab the nearest known enemy and pursue them. Otherwise, if there are no known enemies, go back to the default State
            if (distanceToTarget > combatRange)
            {
                targetInCombatRange = false;
                SwitchTarget(characterManager.alliances.GetClosestKnownEnemy());

                if (characterManager.npcMovement.target == null)
                {
                    gm.turnManager.FinishTurn(characterManager);
                    return;
                }
                else
                {
                    characterManager.npcMovement.SetPathToCurrentTarget();
                    StartCoroutine(characterManager.npcMovement.UseAPAndMove());
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
                gm.turnManager.FinishTurn(characterManager);
            else
                Fight();
        }
    }

    public void SwitchTarget(CharacterManager newTarget)
    {
        Debug.Log("Switching target to: " + newTarget);
        if (newTarget == null)
        {
            characterManager.stateController.SetToDefaultState(characterManager.npcMovement.shouldFollowLeader);
            gm.turnManager.FinishTurn(characterManager);
            return;
        }

        characterManager.npcMovement.SetTarget(newTarget);

        if (characterManager.npcMovement.target == null)
            characterManager.stateController.SetToDefaultState(characterManager.npcMovement.shouldFollowLeader);
        else if (targetInCombatRange == false)
            characterManager.stateController.SetCurrentState(State.MoveToTarget);
        else
            characterManager.stateController.SetCurrentState(State.Fight);

        gm.turnManager.FinishTurn(characterManager);
    }

    public void MoveInToAttack()
    {
        // If close enough, do attack animation
        if (targetInAttackRange)
        {
            DetermineAttack(characterManager.npcMovement.target, characterManager.npcMovement.target.characterStats);
        }
        else if (targetInAttackRange == false)
        {
            characterManager.npcMovement.SetPathToCurrentTarget();
            StartCoroutine(characterManager.npcMovement.UseAPAndMove());
        }
    }

    public override void DetermineAttack(CharacterManager targetsCharacterManager, Stats targetsStats)
    {
        StartRandomMeleeAttack(targetsCharacterManager, targetsStats);
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

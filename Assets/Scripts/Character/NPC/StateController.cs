using UnityEngine;

public enum State { Idle, Patrol, Wander, Follow, MoveToTarget, Fight, Flee, Hunt, FindFood }

public class StateController : MonoBehaviour
{
    public State defaultState = State.Idle;
    public State currentState = State.Idle;

    CharacterManager characterManager;
    GameManager gm;

    void Start()
    {
        characterManager = GetComponent<CharacterManager>();
        gm = GameManager.instance;

        if (currentState == State.Idle)
            SetToDefaultState(characterManager.npcMovement.shouldFollowLeader);
    }

    public void DoAction()
    {
        if (characterManager.actions.Count > 0)
            characterManager.StartCoroutine(characterManager.GetNextQueuedAction());
        else if (characterManager.movement.isMoving == false)
        {
            switch (currentState)
            {
                case State.Idle:
                    StartCoroutine(gm.turnManager.FinishTurn(characterManager));
                    break;
                case State.Patrol:
                    characterManager.npcMovement.DoPatrol();
                    break;
                case State.Wander:
                    characterManager.npcMovement.WanderAround();
                    break;
                case State.Follow:
                    characterManager.npcMovement.FollowTarget(characterManager.npcMovement.leader);
                    break;
                case State.MoveToTarget:
                    characterManager.npcMovement.PursueTarget();
                    break;
                case State.Fight:
                    characterManager.npcAttack.Fight();
                    break;
                case State.Flee:
                    characterManager.npcMovement.Flee(characterManager.npcMovement.targetFleeingFrom, characterManager.npcMovement.fleeDistance);
                    break;
                case State.Hunt:
                    break;
                case State.FindFood:
                    break;
                default:
                    break;
            }
        }
    }

    public void SetCurrentState(State state)
    {
        characterManager.npcMovement.ResetToDefaults();
        currentState = state;
    }

    public void SetToDefaultState(bool shouldFollowLeader)
    {
        characterManager.npcMovement.ResetToDefaults();

        if (shouldFollowLeader && characterManager.npcMovement.leader != null)
            currentState = State.Follow;
        else
            currentState = defaultState;
    }

    public void ChangeDefaultState(State newDefaultState)
    {
        defaultState = newDefaultState;
    }
}

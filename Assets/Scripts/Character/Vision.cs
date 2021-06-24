using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public float lookRadius = 12f;

    public List<Transform> enemiesInRange = new List<Transform>();
    [HideInInspector] public List<Transform> alliesInRange = new List<Transform>();

    public List<Transform> knownEnemiesInRange = new List<Transform>();

    [HideInInspector] public CircleCollider2D visionCollider;

    [HideInInspector] public LayerMask sightObstacleMask;

    CharacterManager characterManager;

    void Awake()
    {
        characterManager = GetComponentInParent<CharacterManager>();

        visionCollider = GetComponent<CircleCollider2D>();
        visionCollider.radius = lookRadius;

        sightObstacleMask = LayerMask.GetMask("Objects", "Walls");
    }

    public void CheckEnemyVisibility()
    {
        foreach (Transform enemy in enemiesInRange)
        {
            Vector2 direction = (enemy.position - transform.position).normalized;
            float rayLength = Vector2.Distance(enemy.position, transform.position);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayLength, sightObstacleMask);

            // If the character has a direct line of sight (meaning no obstacles in the way) to the enemy in question and they weren't visible previously, add them to the knownEnemiesInRange list
            if (hit.collider == null && knownEnemiesInRange.Contains(enemy) == false && IsFacingTransform(enemy))
            {
                knownEnemiesInRange.Add(enemy);

                if (characterManager.npcMovement != null)
                {
                    if (characterManager.npcMovement.shouldAlwaysFleeCombat)
                    {
                        characterManager.npcMovement.targetFleeingFrom = enemy;
                        characterManager.stateController.SetCurrentState(State.Flee);
                    }
                    else if (characterManager.npcMovement.target == null)
                    {
                        characterManager.npcMovement.SetTarget(enemy);
                        characterManager.stateController.SetCurrentState(State.Fight);
                    }
                }
            }
            else if (hit.collider != null && knownEnemiesInRange.Contains(enemy)) // If there's an obstacle in the way and the enemy was visible previously
            {
                knownEnemiesInRange.Remove(enemy);

                if (characterManager.npcMovement != null)
                {
                    if (knownEnemiesInRange.Count > 0)
                    {
                        characterManager.npcAttack.SwitchTarget(characterManager.alliances.GetClosestKnownEnemy());
                        characterManager.npcAttack.MoveInToAttack();
                    }
                    else
                    {
                        Vector2 lastKnownEnemyPosition = enemy.position;
                        characterManager.npcMovement.SetTargetPosition(lastKnownEnemyPosition);
                        characterManager.stateController.SetCurrentState(State.MoveToTarget);
                    }
                }
            }
        }
    }

    public bool IsFacingTransform(Transform targetTransform)
    {
        if ((transform.position.x <= targetTransform.position.x && transform.localScale.x == 1) || (transform.position.x >= targetTransform.position.x && transform.localScale.x == -1))
            return true;

        return false;        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != transform.parent.gameObject && (collision.CompareTag("NPC") || (collision.CompareTag("Player") && transform.parent.gameObject.CompareTag("Player") == false)))
        {
            Alliances npcAlliances = collision.GetComponent<Alliances>();
            if (characterManager.alliances.IsEnemy(npcAlliances.myFaction) && enemiesInRange.Contains(collision.transform) == false)
                enemiesInRange.Add(collision.transform);
            else if (characterManager.alliances.IsAlly(npcAlliances.myFaction) && alliesInRange.Contains(collision.transform) == false)
                alliesInRange.Add(collision.transform);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject != transform.parent.gameObject && (collision.CompareTag("NPC") || (collision.CompareTag("Player") && transform.parent.gameObject.CompareTag("Player") == false)))
        {
            Alliances npcAlliances = collision.GetComponent<Alliances>();
            if (characterManager.alliances.IsEnemy(npcAlliances.myFaction))
            {
                enemiesInRange.Remove(collision.transform);
                if (knownEnemiesInRange.Contains(collision.transform))
                    knownEnemiesInRange.Remove(collision.transform);
            }
            else if (characterManager.alliances.IsAlly(npcAlliances.myFaction))
                alliesInRange.Remove(collision.transform);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}

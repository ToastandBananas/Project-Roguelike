using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public float lookRadius = 12f;

    public List<CharacterManager> enemiesInRange = new List<CharacterManager>();
    [HideInInspector] public List<CharacterManager> alliesInRange = new List<CharacterManager>();

    public List<CharacterManager> knownEnemiesInRange = new List<CharacterManager>();

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
        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            if (enemiesInRange[i].status.isDead)
            {
                enemiesInRange.Remove(characterManager);

                if (knownEnemiesInRange.Contains(characterManager))
                    knownEnemiesInRange.Remove(characterManager);

                if (characterManager.isNPC && characterManager.npcMovement.target == enemiesInRange[i])
                    characterManager.npcAttack.SwitchTarget(GetClosestKnownEnemy());

                continue;
            }

            Vector2 direction = (enemiesInRange[i].transform.position - transform.position).normalized;
            float rayLength = Vector2.Distance(enemiesInRange[i].transform.position, transform.position);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayLength, sightObstacleMask);

            // If the character has a direct line of sight (meaning no obstacles in the way) to the enemy in question and they weren't visible previously, add them to the knownEnemiesInRange list
            if (hit.collider == null && knownEnemiesInRange.Contains(enemiesInRange[i]) == false && IsFacingTransform(enemiesInRange[i].transform))
            {
                knownEnemiesInRange.Add(enemiesInRange[i]);

                if (characterManager.npcMovement != null)
                {
                    if (characterManager.npcMovement.shouldAlwaysFleeCombat)
                    {
                        characterManager.npcMovement.targetFleeingFrom = enemiesInRange[i].transform;
                        characterManager.stateController.SetCurrentState(State.Flee);
                    }
                    else if (characterManager.npcMovement.target == null)
                    {
                        characterManager.npcMovement.SetTarget(enemiesInRange[i]);
                        characterManager.stateController.SetCurrentState(State.Fight);
                    }
                }
            }
            else if (hit.collider != null && knownEnemiesInRange.Contains(enemiesInRange[i])) // If there's an obstacle in the way and the enemy was visible previously
            {
                knownEnemiesInRange.Remove(enemiesInRange[i]);

                if (characterManager.npcMovement != null)
                {
                    if (knownEnemiesInRange.Count > 0)
                    {
                        characterManager.npcAttack.SwitchTarget(GetClosestKnownEnemy());
                        characterManager.npcAttack.MoveInToAttack();
                    }
                    else
                    {
                        Vector2 lastKnownEnemyPosition = enemiesInRange[i].transform.position;
                        characterManager.npcMovement.SetTargetPosition(lastKnownEnemyPosition);
                        characterManager.stateController.SetCurrentState(State.MoveToTarget);
                    }
                }
            }
        }
    }

    public bool IsFacingTransform(Transform targetTransform)
    {
        if ((transform.parent.position.x <= targetTransform.position.x && transform.parent.localScale.x == 1) || (transform.parent.position.x >= targetTransform.position.x && transform.parent.localScale.x == -1))
            return true;
        
        return false;
    }

    public CharacterManager GetClosestKnownEnemy()
    {
        if (characterManager.vision.knownEnemiesInRange.Count > 0)
        {
            CharacterManager closestEnemy = null;
            float distanceToClosestEnemy = 0;

            foreach (CharacterManager enemy in characterManager.vision.knownEnemiesInRange)
            {
                if (closestEnemy == null)
                {
                    closestEnemy = enemy;
                    if (characterManager.vision.knownEnemiesInRange.Count > 1)
                        distanceToClosestEnemy = Vector2.Distance(enemy.transform.position, transform.position);
                }
                else
                {
                    float distanceToEnemy = Vector2.Distance(enemy.transform.position, transform.position);
                    if (distanceToEnemy < distanceToClosestEnemy)
                    {
                        closestEnemy = enemy;
                        distanceToClosestEnemy = distanceToEnemy;
                    }
                }
            }

            return closestEnemy;
        }

        return null;
    }

    public CharacterManager GetClosestAlly()
    {
        if (characterManager.vision.alliesInRange.Count > 0)
        {
            CharacterManager closestAlly = null;
            float distanceToClosestAlly = 0;

            foreach (CharacterManager ally in characterManager.vision.alliesInRange)
            {
                if (closestAlly == null)
                {
                    closestAlly = ally;
                    if (characterManager.vision.alliesInRange.Count > 1)
                        distanceToClosestAlly = Vector2.Distance(ally.transform.position, transform.position);
                }
                else
                {
                    float distanceToAlly = Vector2.Distance(ally.transform.position, transform.position);
                    if (distanceToAlly < distanceToClosestAlly)
                    {
                        closestAlly = ally;
                        distanceToClosestAlly = distanceToAlly;
                    }
                }
            }

            return closestAlly;
        }

        return null;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != transform.parent.gameObject && (collision.CompareTag("NPC") || collision.CompareTag("Player")))
        {
            CharacterManager charManager = collision.GetComponent<CharacterManager>();
            if (characterManager.alliances.IsEnemy(charManager.alliances.myFaction) && enemiesInRange.Contains(charManager) == false)
                enemiesInRange.Add(charManager);
            else if (characterManager.alliances.IsAlly(charManager.alliances.myFaction) && alliesInRange.Contains(charManager) == false)
                alliesInRange.Add(charManager);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject != transform.parent.gameObject && (collision.CompareTag("NPC") || collision.CompareTag("Player")))
        {
            CharacterManager charManager = collision.GetComponent<CharacterManager>();
            if (characterManager.alliances.IsEnemy(charManager.alliances.myFaction))
            {
                enemiesInRange.Remove(charManager);
                if (knownEnemiesInRange.Contains(charManager))
                    knownEnemiesInRange.Remove(charManager);
            }
            else if (characterManager.alliances.IsAlly(charManager.alliances.myFaction))
                alliesInRange.Remove(charManager);
        }
    }

    /*void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }*/
}

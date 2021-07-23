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
        Debug.Log("Checking enemy visibility");
        foreach (CharacterManager enemy in enemiesInRange)
        {
            Vector2 direction = (enemy.transform.position - transform.position).normalized;
            float rayLength = Vector2.Distance(enemy.transform.position, transform.position);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayLength, sightObstacleMask);

            // If the character has a direct line of sight (meaning no obstacles in the way) to the enemy in question and they weren't visible previously, add them to the knownEnemiesInRange list
            if (hit.collider == null && knownEnemiesInRange.Contains(enemy) == false && IsFacingTransform(enemy.transform))
            {
                knownEnemiesInRange.Add(enemy);

                if (characterManager.npcMovement != null)
                {
                    if (characterManager.npcMovement.shouldAlwaysFleeCombat)
                    {
                        characterManager.npcMovement.targetFleeingFrom = enemy.transform;
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
                        Vector2 lastKnownEnemyPosition = enemy.transform.position;
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != transform.parent.gameObject && (collision.CompareTag("NPC") || (collision.CompareTag("Player") && transform.parent.gameObject.CompareTag("Player") == false)))
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
        if (collision.gameObject != transform.parent.gameObject && (collision.CompareTag("NPC") || (collision.CompareTag("Player") && transform.parent.gameObject.CompareTag("Player") == false)))
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

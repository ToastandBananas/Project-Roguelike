using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public float lookRadius = 10f;

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

    void FixedUpdate()
    {
        foreach (Transform enemy in enemiesInRange)
        {
            // If this character hasn't seen or otherwise discovered the enemy yet, but is facing towards them
            if (knownEnemiesInRange.Contains(enemy) == false)// && characterManager.charAnimController.IsFacingTransform(enemy))
            {
                Vector2 direction = (enemy.position - transform.position).normalized;
                float rayLength = Vector2.Distance(enemy.position, transform.position);

                RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, rayLength, sightObstacleMask);

                // If the character has a direct line of sight (meaning no obstacles in the way) to the enemy in question, add them to the knownEnemiesInRange list
                if (raycast.collider == null && knownEnemiesInRange.Contains(enemy) == false)
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
                            characterManager.npcMovement.target = enemy;
                            characterManager.npcMovement.AIDestSetter.target = enemy;
                            characterManager.stateController.SetCurrentState(State.Fight);
                        }
                    }
                }
            }
        }
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
                if (characterManager.npcMovement.target != collision.transform && knownEnemiesInRange.Contains(collision.transform))
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

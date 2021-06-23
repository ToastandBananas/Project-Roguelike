using System.Collections.Generic;
using UnityEngine;

public enum Factions { Neutral, Player, PreyAnimal, PredatorAnimal, Bandits, Undead, Goblins }

public class Alliances : MonoBehaviour
{
    public Factions myFaction;
    public List<Factions> allies;
    public List<Factions> enemies;

    CharacterManager characterManager;

    void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public bool IsAlly(Factions faction)
    {
        if (faction == myFaction || allies.Contains(faction))
            return true;

        return false;
    }

    public bool IsEnemy(Factions faction)
    {
        if (enemies.Contains(faction))
            return true;

        return false;
    }

    public Transform GetClosestKnownEnemy()
    {
        if (characterManager.vision.knownEnemiesInRange.Count > 0)
        {
            Transform closestEnemy = null;
            float distanceToClosestEnemy = 0;

            foreach (Transform enemy in characterManager.vision.knownEnemiesInRange)
            {
                if (closestEnemy == null)
                {
                    closestEnemy = enemy;
                    if (characterManager.vision.knownEnemiesInRange.Count > 1)
                        distanceToClosestEnemy = Vector2.Distance(enemy.position, transform.position);
                }
                else
                {
                    float distanceToEnemy = Vector2.Distance(enemy.position, transform.position);
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

    public Transform GetClosestAlly()
    {
        if (characterManager.vision.alliesInRange.Count > 0)
        {
            Transform closestAlly = null;
            float distanceToClosestAlly = 0;

            foreach (Transform ally in characterManager.vision.alliesInRange)
            {
                if (closestAlly == null)
                {
                    closestAlly = ally;
                    if (characterManager.vision.alliesInRange.Count > 1)
                        distanceToClosestAlly = Vector2.Distance(ally.position, transform.position);
                }
                else
                {
                    float distanceToAlly = Vector2.Distance(ally.position, transform.position);
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
}

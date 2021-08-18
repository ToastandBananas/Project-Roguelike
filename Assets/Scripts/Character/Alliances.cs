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
}

using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<NPCMovement> npcs = new List<NPCMovement>();
    public int npcsFinishedTakingTurnCount;

    public bool isPlayerTurn = true;

    #region Singleton
    public static TurnManager instance;

    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of TurnManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    public void TakePlayersTurn()
    {
        isPlayerTurn = false;
        npcsFinishedTakingTurnCount = 0;

        DoAllNPCsTurns();
    }

    public void ReadyPlayersTurn()
    {
        isPlayerTurn = true;
    }

    public void TakeNPCTurn(NPCMovement npcMovment)
    {
        npcMovment.MoveToNextPointOnPath();
    }

    void DoAllNPCsTurns()
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            TakeNPCTurn(npcs[i]);
        }
    }
}

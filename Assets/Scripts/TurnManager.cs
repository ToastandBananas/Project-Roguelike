using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [HideInInspector] public List<NPCMovement> npcs = new List<NPCMovement>();
    [HideInInspector] public int npcsFinishedTakingTurnCount;

    [HideInInspector] public bool isPlayersTurn = true;

    GameManager gm;

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

    void Start()
    {
        gm = GameManager.instance;
    }

    public void FinishPlayersTurn()
    {
        isPlayersTurn = false;
        npcsFinishedTakingTurnCount = 0;

        DoAllNPCsTurns();
    }

    public void ReadyPlayersTurn()
    {
        isPlayersTurn = true;
        gm.playerManager.playerStats.ReplenishAP();
    }

    public void TakeNPCTurn(NPCMovement npcMovment)
    {
        npcMovment.TakeTurn();
    }

    void DoAllNPCsTurns()
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            TakeNPCTurn(npcs[i]);
        }
    }
}

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [HideInInspector] public List<CharacterManager> npcs = new List<CharacterManager>();
    [HideInInspector] public int npcsFinishedTakingTurnCount;

    public bool isPlayersTurn = true;

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
        isPlayersTurn = true;
    }

    public void FinishTurn(CharacterManager characterManager)
    {
        if (characterManager == gm.playerManager)
            FinishPlayersTurn();
        else
            FinishNPCsTurn(characterManager);
    }

    void FinishPlayersTurn()
    {
        isPlayersTurn = false;
        gm.playerManager.isMyTurn = false;
        npcsFinishedTakingTurnCount = 0;
        
        gm.playerManager.playerStats.ReplenishAP();

        DoAllNPCsTurns();
    }

    public void ReadyPlayersTurn()
    {
        isPlayersTurn = true;
        gm.playerManager.isMyTurn = true;
    }

    void FinishNPCsTurn(CharacterManager npcsCharManager)
    {
        npcsCharManager.isMyTurn = false;
        npcsCharManager.characterStats.ReplenishAP();
        gm.turnManager.npcsFinishedTakingTurnCount++;

        if (gm.turnManager.npcsFinishedTakingTurnCount == gm.turnManager.npcs.Count)
            gm.turnManager.ReadyPlayersTurn();
    }

    public void TakeNPCTurn(CharacterManager charManager)
    {
        charManager.isMyTurn = true;
        charManager.TakeTurn();
    }

    void DoAllNPCsTurns()
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            TakeNPCTurn(npcs[i]);
        }
    }
}

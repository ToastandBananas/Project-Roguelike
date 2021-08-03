using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [HideInInspector] public List<CharacterManager> npcs = new List<CharacterManager>();
    [HideInInspector] public int npcsFinishedTakingTurnCount;

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

    public void FinishTurn(CharacterManager characterManager)
    {
        if (characterManager == gm.playerManager)
            FinishPlayersTurn();
        else
            FinishNPCsTurn(characterManager);
    }

    void FinishPlayersTurn()
    {
        gm.playerManager.isMyTurn = false;
        npcsFinishedTakingTurnCount = 0;
        
        gm.playerManager.playerStats.ReplenishAP();

        DoAllNPCsTurns();
    }

    public void ReadyPlayersTurn()
    {
        gm.playerManager.isMyTurn = true;
        gm.tileInfoDisplay.DisplayTileInfo();
    }

    void FinishNPCsTurn(CharacterManager npcsCharManager)
    {
        npcsCharManager.isMyTurn = false;
        npcsCharManager.characterStats.ReplenishAP();
        gm.turnManager.npcsFinishedTakingTurnCount++;

        if (gm.turnManager.npcsFinishedTakingTurnCount >= gm.turnManager.npcs.Count)
            gm.turnManager.ReadyPlayersTurn();
    }

    public void TakeNPCTurn(CharacterManager charManager)
    {
        if (gm.playerManager.playerStats.isDeadOrDestroyed == false)
        {
            charManager.isMyTurn = true;
            charManager.TakeTurn();
        }
    }

    void DoAllNPCsTurns()
    {
        if (npcs.Count > 0)
        {
            for (int i = 0; i < npcs.Count; i++)
            {
                TakeNPCTurn(npcs[i]);
            }
        }
        else
            ReadyPlayersTurn();
    }

    public bool IsPlayersTurn()
    {
        return gm.playerManager.isMyTurn;
    }
}

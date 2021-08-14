using System.Collections;
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

    public IEnumerator FinishTurn(CharacterManager characterManager)
    {
        while (characterManager.movement.isMoving) { yield return null; }
        
        if (characterManager.isMyTurn)
        {
            // Debug.Log(characterManager.name + " is finishing their turn");
            if (characterManager == gm.playerManager)
                FinishPlayersTurn();
            else
                FinishNPCsTurn(characterManager);
        }
    }

    void FinishPlayersTurn()
    {
        gm.playerManager.isMyTurn = false;
        npcsFinishedTakingTurnCount = 0;

        DoAllNPCsTurns();
    }

    public void ReadyPlayersTurn()
    {
        gm.playerManager.playerStats.ReplenishAP();
        gm.playerManager.isMyTurn = true;

        gm.tileInfoDisplay.DisplayTileInfo();

        gm.playerManager.status.UpdateBuffs();
        gm.playerManager.status.UpdateInjuries();
        TimeSystem.IncreaseTime();

        gm.healthDisplay.UpdateTooltip();

        gm.playerManager.characterStats.ApplyAPLossBuildup();
    }

    void FinishNPCsTurn(CharacterManager npcsCharManager)
    {
        npcsCharManager.isMyTurn = false;
        gm.turnManager.npcsFinishedTakingTurnCount++;

        if (gm.turnManager.npcsFinishedTakingTurnCount >= gm.turnManager.npcs.Count)
            gm.turnManager.ReadyPlayersTurn();
    }

    public void TakeNPCTurn(CharacterManager charManager)
    {
        if (gm.playerManager.status.isDead == false)
        {
            charManager.characterStats.ReplenishAP();
            charManager.isMyTurn = true;

            charManager.status.UpdateBuffs();
            charManager.status.UpdateInjuries();

            charManager.characterStats.ApplyAPLossBuildup();

            if (charManager.characterStats.currentAP > 0)
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

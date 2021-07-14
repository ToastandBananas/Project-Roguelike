using UnityEngine;

public enum ActionType { Move, Attack, Equip, Unequip }

public class APManager : MonoBehaviour
{
    #region Singleton
    public static APManager instance;

    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of APManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    public int baseMovementCost = 100;

    public int GetAPCost(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Move:
                return GetMovementAPCost();
            case ActionType.Attack:
                return 0;
            case ActionType.Equip:
                return 0;
            case ActionType.Unequip:
                return 0;
            default:
                return 0;
        }
    }

    public int GetMovementAPCost()
    {
        return baseMovementCost;
    }
}

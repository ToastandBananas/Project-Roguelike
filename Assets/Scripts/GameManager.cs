using UnityEngine;

public enum Direction { Center, Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight }

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of GameManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion
}

using UnityEngine;

public class TileInfo : MonoBehaviour
{
    #region
    public static TileInfo instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of TileInfo. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    public void DisplayInfo()
    {

    }
}

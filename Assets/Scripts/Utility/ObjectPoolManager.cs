using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public ObjectPool pickupsPool, treePool, weaponTrailsPool, bloodEffectsPool, woodEffectsPool;

    #region Singleton
    public static ObjectPoolManager instance;

    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of ObjectPoolManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion
}

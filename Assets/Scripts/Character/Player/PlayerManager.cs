using UnityEngine;

public class PlayerManager : CharacterManager
{
    [HideInInspector] public GameObject playerGameObject;
    [HideInInspector] public PlayerMovement playerMovement;

    public static PlayerManager instance;

    public override void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("There is more than one PlayerManager instance. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
        #endregion

        base.Awake();

        playerGameObject = gameObject;
        playerMovement = GetComponent<PlayerMovement>();
    }
}

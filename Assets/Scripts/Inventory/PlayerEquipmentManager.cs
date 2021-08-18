using UnityEngine;

public class PlayerEquipmentManager : EquipmentManager
{
    #region Singleton
    public static PlayerEquipmentManager instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of PlayerEquipmentManager. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    public override void Start()
    {
        base.Start();
    }

    void Update()
    {
        if (GameControls.gamePlayActions.playerSwitchStance.WasPressed)
        {
            if (IsDualWielding() == false && RightHandItemEquipped() && GetRightWeapon().isTwoHanded == false)
                StartCoroutine(characterManager.humanoidSpriteManager.SwapStance(this, characterManager));
        }
    }
}

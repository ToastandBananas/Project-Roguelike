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
            if (LeftHandItemEquipped() == false && RightHandItemEquipped() && RightWeaponSheathed() == false)
                characterManager.QueueAction(characterManager.humanoidSpriteManager.SwapStance(this, characterManager, currentEquipment[(int)EquipmentSlot.RightHandItem]), gm.apManager.GetSwapStanceAPCost(characterManager));
        }

        if (GameControls.gamePlayActions.playerSheatheWeapon.WasPressed)
        {
            if (BothWeaponsSheathed())
                characterManager.QueueAction(UnsheatheWeapons(), gm.apManager.GetUnheatheWeaponAPCost(this));
            else
                characterManager.QueueAction(SheatheWeapons(true, true), gm.apManager.GetSheatheWeaponAPCost(this, true, true));
        }
    }
}
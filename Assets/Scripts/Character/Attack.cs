using System.Collections;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int attackRange = 1;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;
    
    [HideInInspector] public bool canAttack = true;

    public virtual void Start()
    {
        canAttack = true;

        gm = GameManager.instance;
        characterManager = GetComponent<CharacterManager>();
    }

    /// <summary>This function determines what type of attack the character should do.</summary>
    public void DoAttack()
    {
        // TODO: Determine attack based off of held weapons, throwables and spells

        characterManager.npcMovement.FaceForward(characterManager.npcMovement.target.position);

        if (characterManager.npcMovement != null)
            Debug.Log(name + " is attacking " + characterManager.npcMovement.target);

        gm.turnManager.FinishTurn(characterManager);
    }

    public IEnumerator Attack_DualWield()
    {
        Attack_Right();
        yield return new WaitForSeconds(0.25f);
        Attack_Left();
    }

    public void Attack_Left()
    {
        
    }

    public void Attack_Right()
    {
        
    }

    /*public void ShowWeaponTrail(HeldWeapon heldWeapon)
    {
        WeaponTrail weaponTrail = weaponTrailObjectPool.GetPooledObject().GetComponent<WeaponTrail>();
        weaponTrail.gameObject.SetActive(true);
        
        weaponTrail.ShowChopTrail(heldWeapon, characterManager.transform);
    }*/
}

using System;
using System.Text;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FlavorText : MonoBehaviour
{
    public TextMeshProUGUI flavorText;
    public Scrollbar scrollbar;

    [HideInInspector] public RectTransform parentRectTransform;

    StringBuilder stringBuilder = new StringBuilder();

    readonly int maxLines = 50;

    #region
    public static FlavorText instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of FlavorText. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    public void WriteLine(string input)
    {
        int numLines = Convert.ToString(stringBuilder).Split('\n').Length;
        if (numLines > maxLines)
            stringBuilder.Remove(0, Convert.ToString(stringBuilder).Split('\n').FirstOrDefault().Length + 1);

        stringBuilder.Append(input + "\n");
        flavorText.text = Convert.ToString(stringBuilder);

        StartCoroutine(SetupScrollbar());
    }

    public IEnumerator DelayWriteLine(string input)
    {
        yield return null;
        WriteLine(input);
    }

    public void WriteAttackCharacterLine(GeneralAttackType attackType, BodyPart bodyPartHit, CharacterManager attacker, CharacterManager victim, int damage)
    {
        switch (attackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(GetPronoun(attacker, true, false) + "punched " + GetPronoun(victim, false, true) + GetHumanoidBodyPartName(bodyPartHit) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(GetPronoun(attacker, true, false) + GetWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon()) + GetPronoun(victim, false, true) + GetHumanoidBodyPartName(bodyPartHit) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(GetPronoun(attacker, true, false) + GetWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon()) + GetPronoun(victim, false, true) + GetHumanoidBodyPartName(bodyPartHit) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon].itemName) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.Ranged:
                WriteLine(GetPronoun(attacker, true, false) + GetWeaponAttackVerb(attacker.equipmentManager.GetRangedWeapon()) + GetPronoun(victim, false, true) + GetHumanoidBodyPartName(bodyPartHit) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.Ranged].itemName) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.Throwing:
                WriteLine(GetPronoun(attacker, true, false) + GetWeaponAttackVerb(attacker.equipmentManager.GetRangedWeapon()) + GetPronoun(victim, false, true) + GetHumanoidBodyPartName(bodyPartHit) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.Ranged].itemName) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.Magic:
                break;
            default:
                break;
        }
    }

    public void WriteMissedAttackLine(CharacterManager attacker, CharacterManager target)
    {
        WriteLine(GetPronoun(attacker, true, false) + "attacked " + GetPronoun(target, false, false) + "and missed!");
    }

    public void WriteEvadedAttackLine(CharacterManager attacker, CharacterManager target)
    {
        WriteLine(GetPronoun(target, true, false) + "evaded " + GetPronoun(attacker, false, true) + "attack!");
    }

    public void WriteBlockedAttackLine(CharacterManager attacker, CharacterManager target, ItemData weaponOrShieldItemData)
    {
        WriteLine(GetPronoun(target, true, false) + "blocked " + GetPronoun(attacker, false, true) + "attack with " + GetPossessivePronoun(target) + "<b>" + weaponOrShieldItemData.itemName + "</b>!");
    }

    public void WriteConsumeLine(Consumable consumable, CharacterManager characterManager)
    {
        switch (consumable.consumableType)
        {
            case ConsumableType.Food:
                WriteLine(GetPronoun(characterManager, true, false) + "ate " + GetIndefiniteArticle(consumable.name) + ".");
                break;
            case ConsumableType.Drink:
                WriteLine(GetPronoun(characterManager, true, false) + "drank " + GetIndefiniteArticle(consumable.name) + ".");
                break;
            default:
                break;
        }
    }

    public void WriteDropItemLine(ItemData itemDropping, int amountDropping)
    {
        if (amountDropping == 1)
            WriteLine(GetPronoun(null, true, false) + "dropped " + GetIndefiniteArticle(itemDropping.itemName) + ".");
        else
            WriteLine(GetPronoun(null, true, false) + "dropped " + amountDropping + " <b>" + itemDropping.itemName + "s</b>.");
    }

    public void WriteTakeItemLine(ItemData itemTaking, int amountTaking, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        if (inventoryPuttingIn.name == "Keys")
        {
            if (inventoryTakingFrom != null)
                WriteLine(GetPronoun(null, true, false) + "took " + GetIndefiniteArticle(itemTaking.itemName) + " from the <b>" + inventoryTakingFrom.name + "</b> and put it on your <b>key ring</b>.");
            else
                WriteLine(GetPronoun(null, true, false) + "picked up " + GetIndefiniteArticle(itemTaking.itemName) + " and put it on your <b>key ring</b>.");
        }
        else if (amountTaking == 1)
        {
            if (inventoryTakingFrom != null)
                WriteLine(GetPronoun(null, true, false) + "took " + GetIndefiniteArticle(itemTaking.itemName) + " from the <b>" + inventoryTakingFrom.name + "</b> and put it in your <b>" + inventoryPuttingIn.name + "</b>.");
            else
                WriteLine(GetPronoun(null, true, false) + "picked up " + GetIndefiniteArticle(itemTaking.itemName) + " and put it in your <b>" + inventoryPuttingIn.name + "</b>.");
        }
        else
        {
            if (inventoryTakingFrom != null)
                WriteLine(GetPronoun(null, true, false) + "took " + amountTaking + " <b>" + itemTaking.itemName + "s</b> from the <b>" + inventoryTakingFrom.name + "</b> and put them in your <b>" + inventoryPuttingIn.name + "</b>.");
            else
                WriteLine(GetPronoun(null, true, false) + "picked up " + amountTaking + " <b>" + itemTaking.itemName + "s</b>" + " and put them in your <b>" + inventoryPuttingIn.name + "</b>.");
        }
    }

    public void WriteTransferItemLine(ItemData itemTaking, int amountTaking, EquipmentManager equipmentManagerTakingFrom, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        if (amountTaking == 1)
        {
            if (equipmentManagerTakingFrom != null)
                WriteLine(GetPronoun(null, true, false) + "took " + GetIndefiniteArticle(itemTaking.itemName) + " from your <b>" + equipmentManagerTakingFrom.name + "</b> and put it in the <b>" + inventoryPuttingIn.name + "</b>.");
            else
                WriteLine(GetPronoun(null, true, false) + "took " + GetIndefiniteArticle(itemTaking.itemName) + " from your <b>" + inventoryTakingFrom.name + "</b> and put it in the <b>" + inventoryPuttingIn.name + "</b>.");
        }
        else
        {
            if (equipmentManagerTakingFrom != null)
                WriteLine(GetPronoun(null, true, false) + "took " + amountTaking + " <b>" + itemTaking.itemName + "s</b> from your <b>" + equipmentManagerTakingFrom.name + "</b> and put them in the <b>" + inventoryPuttingIn.name + "</b>.");
            else
                WriteLine(GetPronoun(null, true, false) + "took " + amountTaking + " <b>" + itemTaking.itemName + "s</b> from your <b>" + inventoryTakingFrom.name + "</b> and put them in the <b>" + inventoryPuttingIn.name + "</b>.");
        }
    }

    public void WriteEquipLine(ItemData itemEquipping, CharacterManager characterManager)
    {
        WriteLine(GetPronoun(characterManager, true, false) + "equipped the <b>" + itemEquipping.itemName + "</b>.");
    }
    
    public void WriteUnquipLine(ItemData itemUnequipping, CharacterManager characterManager)
    {
        WriteLine(GetPronoun(characterManager, true, false) + "unequipped the <b>" + itemUnequipping.itemName + "</b>.");
    }

    IEnumerator SetupScrollbar()
    {
        yield return null;
        yield return null;
        scrollbar.value = 0;
    }

    public string GetPronoun(CharacterManager characterManager, bool uppercase, bool possessive)
    {
        if (characterManager == null || characterManager.isNPC == false)
        {
            if (uppercase)
            {
                if (possessive)
                    return "<b><i>Your</i></b> ";
                else
                    return "<b><i>You</i></b> ";
            }
            else
            {
                if (possessive)
                    return "<b><i>your</i></b> ";
                else
                    return "<b><i>you</i></b> ";
            }
        }
        else
        {
            if (possessive)
                return "<b><i>" + characterManager.name + "'s</i></b> ";
            else
                return "<b><i>" + characterManager.name + "</i></b> ";
        }
    }

    string GetIndefiniteArticle(string noun)
    {
        if ("aeiouAEIOU".IndexOf(noun.Substring(0, 1)) >= 0)
            return "an <b>" + noun + "</b>";
        else
            return "a <b>" + noun + "</b>";
    }

    string GetPossessivePronoun(CharacterManager characterManager)
    {
        if (characterManager.isNPC)
            return "their ";
        else
            return "your ";
    }

    string GetWeaponAttackVerb(Weapon weapon)
    {
        switch (weapon.weaponType)
        {
            case WeaponType.Sword:
                return "slashed ";
            case WeaponType.Dagger:
                return "stabbed ";
            case WeaponType.Axe:
                return "cleaved ";
            case WeaponType.Club:
                return "clubbed ";
            case WeaponType.Mace:
                return "clobbered ";
            case WeaponType.Hammer:
                return "hammered ";
            case WeaponType.Flail:
                return "hit ";
            case WeaponType.Staff:
                return "hit ";
            case WeaponType.Spear:
                return "stabbed ";
            case WeaponType.BluntPolearm:
                return "cleaved ";
            case WeaponType.Sling:
                return "shot ";
            case WeaponType.Bow:
                return "shot ";
            case WeaponType.Crossbow:
                return "shot ";
            case WeaponType.ThrowingKnife:
                return "threw and stuck ";
            default:
                return "hit ";
        }
    }

    string GetHumanoidBodyPartName(BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Torso:
                return "<b>torso</b>";
            case BodyPart.Head:
                return "<b>head</b>";
            case BodyPart.LeftArm:
                return "<b>left arm</b>";
            case BodyPart.RightArm:
                return "<b>right arm</b>";
            case BodyPart.LeftLeg:
                return "<b>left leg</b>"; ;
            case BodyPart.RightLeg:
                return "<b>right leg</b>"; ;
            case BodyPart.LeftHand:
                return "<b>left hand</b>"; ;
            case BodyPart.RightHand:
                return "<b>right hand</b>"; ;
            case BodyPart.LeftFoot:
                return "<b>left foot</b>"; ;
            case BodyPart.RightFoot:
                return "<b>right foot</b>"; ;
            default:
                return "";
        }
    }
}

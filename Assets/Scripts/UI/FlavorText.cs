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

    public void WriteMeleeAttackCharacterLine(CharacterManager attacker, CharacterManager target, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, BodyPart bodyPartHit, int damage)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(GetPronoun(attacker, true, false) + "punched " + GetPronoun(target, false, true) + GetHumanoidBodyPartName(bodyPartHit) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + GetPronoun(target, false, true) + GetHumanoidBodyPartName(bodyPartHit) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + GetPronoun(target, false, true) + GetHumanoidBodyPartName(bodyPartHit) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon].itemName) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WritePenetrateArmorAndClothingLine_Melee(CharacterManager attacker, CharacterManager target, Wearable armor, Wearable clothing, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, PhysicalDamageType physicalDamageType, BodyPart bodyPartHit, int damage)
    {
        WriteLine(GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + GetPronoun(target, false, true) + GetHumanoidBodyPartName(bodyPartHit) 
            + " with " + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName) + " and " + GetPenetrateArmorVerb(physicalDamageType, armor) 
            + GetPronoun(target, false, true) + "<b>" + armor.name + "</b> and <b>" + clothing.name + "</b> ignoring defenses and causing <b><color=red>" + damage + "</color></b> damage.");
    }

    public void WritePenetrateWearableLine_Melee(CharacterManager attacker, CharacterManager target, Wearable wearable, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, PhysicalDamageType physicalDamageType, BodyPart bodyPartHit, int damage)
    {
        WriteLine(GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + GetPronoun(target, false, true) + GetHumanoidBodyPartName(bodyPartHit)
            + " with " + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName) + " and " + GetPenetrateArmorVerb(physicalDamageType, wearable)
            + GetPronoun(target, false, true) + "<b>" + wearable.name + "</b> ignoring its defenses and causing <b><color=red>" + damage + "</color></b> damage.");
    }

    public void WriteMeleeAttackObjectLine(CharacterManager attacker, Stats targetsStats, MeleeAttackType meleeAttackType, int damage)
    {
        WriteLine(GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + "the " + targetsStats.name
            + " with " + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName) + " for <b><color=red>" + damage + "</color></b> damage.");
    }

    public void WriteAbsorbedMeleeAttackLine(CharacterManager attacker, CharacterManager target, MeleeAttackType meleeAttackType, BodyPart bodyPartHit)
    {
        WriteLine(GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + GetPronoun(target, false, true) + GetHumanoidBodyPartName(bodyPartHit) 
            + " with " + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName) + " but it was absorbed by " + GetPronoun(target, false, true) + "armor.");
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
    
    string GetPenetrateArmorVerb(PhysicalDamageType physicalDamageType, Wearable wearable)
    {
        if (physicalDamageType == PhysicalDamageType.Blunt)
            return "crushed ";
        else if (physicalDamageType == PhysicalDamageType.Pierce)
            return "pierced ";
        else if (physicalDamageType == PhysicalDamageType.Cleave)
        {
            if (wearable.IsWooden())
                return "split ";
            else if (wearable.IsMetallic())
                return "crushed ";
            else
                return "cut through ";
        }
        else if (physicalDamageType == PhysicalDamageType.Slash)
        {
            if (wearable.IsMetallic())
                return "crushed ";
            else
                return "sliced ";
        }

        return "crushed ";
    }

    string GetMeleeWeaponAttackVerb(Weapon weapon, MeleeAttackType meleeAttackType)
    {
        switch (weapon.weaponType)
        {
            case WeaponType.Sword:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "slashed ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "chopped ";
            case WeaponType.Dagger:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "slashed ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhand stabbed ";
            case WeaponType.Axe:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "cleaved ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead cleaved ";
            case WeaponType.SpikedAxe:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "cleaved ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead cleaved ";
            case WeaponType.Club:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "clubbed ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.SpikedClub:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "clubbed ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.Mace:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "clobbered ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.SpikedMace:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "clobbered ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.Hammer:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "hammered ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.SpikedHammer:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "hammered ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.Flail:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "struck ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.SpikedFlail:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "struck ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.Staff:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "hit ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.Spear:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "sliced ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead sliced ";
            case WeaponType.Polearm:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "cleaved ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead cleaved ";
            case WeaponType.BluntPolearm:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "smashed ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
            case WeaponType.Sling:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "smacked ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "uppercut ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead struck ";
            case WeaponType.Bow:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "smacked ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead struck ";
            case WeaponType.Crossbow:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "whacked ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead struck ";
            case WeaponType.ThrowingKnife:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "slashed ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhand stabbed ";
            case WeaponType.ThrowingAxe:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "cleaved ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead cleaved ";
            case WeaponType.ThrowingStar:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "slashed ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "stabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhand stabbed ";
            case WeaponType.ThrowingClub:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return "clubbed ";
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return "jabbed ";
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return "overhead smashed ";
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

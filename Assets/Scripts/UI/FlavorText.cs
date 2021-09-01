using System;
using System.Collections;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlavorText : MonoBehaviour
{
    public TextMeshProUGUI flavorText;
    public Scrollbar scrollbar;

    [HideInInspector] public RectTransform parentRectTransform;

    StringBuilder stringBuilder = new StringBuilder();
    GameManager gm;

    readonly int maxLines = 50;

    #region Singleton
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
        gm = GameManager.instance;
        parentRectTransform = transform.GetComponentInParent<RectTransform>();
    }

    public void WriteLine(string input)
    {
        int numLines = stringBuilder.ToString().Split('\n').Length;
        if (numLines > maxLines)
            stringBuilder.Remove(0, stringBuilder.ToString().Split('\n').FirstOrDefault().Length + 1);

        stringBuilder.Append("- " + input + "\n");
        flavorText.text = stringBuilder.ToString();

        StartCoroutine(SetupScrollbar());
    }

    public IEnumerator DelayWriteLine(string input, int yieldCount = 1)
    {
        for (int i = 0; i < yieldCount; i++) { yield return null; }
        WriteLine(input);
    }

    #region General
    public void WriteLine_SeeNPC(CharacterManager npc)
    {
        if (npc.isNamed)
            WriteLine("<b>You</b> see <b>" + npc.name + "</b>.");
        else
            WriteLine("<b>You</b> see " + Utilities.GetIndefiniteArticle(npc.name, false, true) + ".");
    }
    #endregion

    #region Combat
    public void WriteLine_MeleeAttackCharacter(CharacterManager attacker, CharacterManager target, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, BodyPartType bodyPartHit, int damage, bool attackedFromBehind)
    {
        string attackFromBehindText = "";
        if (attackedFromBehind) attackFromBehindText = " from behind";

        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + "punched " + Utilities.GetPronoun(target, false, true) + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) 
                    + attackFromBehindText + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].GetItemName(1), false, true)
                    + attackFromBehindText + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].GetItemName(1), false, true)
                    + attackFromBehindText + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WriteLine_PenetrateArmorAndClothing_Melee(CharacterManager attacker, CharacterManager target, Wearable armor, Wearable clothing, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, PhysicalDamageType physicalDamageType, BodyPartType bodyPartHit, int damage, bool attackedFromBehind)
    {
        string attackFromBehindText = "";
        if (attackedFromBehind) attackFromBehindText = " from behind";

        switch (generalAttackType)
        {
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].GetItemName(1), false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, armor) + Utilities.GetPronoun(target, false, true) + "<b>" + armor.name + "</b> and <b>" + clothing.name 
                    + "</b>, ignoring defenses and causing <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].GetItemName(1), false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, armor) + Utilities.GetPronoun(target, false, true) + "<b>" + armor.name + "</b> and <b>" + clothing.name 
                    + "</b>, ignoring defenses and causing <b><color=red>" + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WriteLine_PenetrateWearable_Melee(CharacterManager attacker, CharacterManager target, Wearable wearable, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, PhysicalDamageType physicalDamageType, BodyPartType bodyPartHit, int damage, bool attackedFromBehind)
    {
        string attackFromBehindText = "";
        if (attackedFromBehind) attackFromBehindText = " from behind";

        switch (generalAttackType)
        {
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].GetItemName(1), false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, wearable) + Utilities.GetPronoun(target, false, true) + "<b>" + wearable.name + "</b>, ignoring its defenses and causing <b><color=red>" 
                    + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].GetItemName(1), false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, wearable) + Utilities.GetPronoun(target, false, true) + "<b>" + wearable.name + "</b>, ignoring its defenses and causing <b><color=red>" 
                    + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WriteLine_MeleeAttackObject(CharacterManager attacker, Stats targetsStats, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, int damage)
    {
        Debug.Log(damage);
        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + "punched the " + targetsStats.name + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + "the " + targetsStats.name + " with " 
                    + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].GetItemName(1), false, true) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + "the " + targetsStats.name + " with " 
                    + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].GetItemName(1), false, true) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WriteLine_AbsorbedMeleeAttack(CharacterManager attacker, CharacterManager target, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, BodyPartType bodyPartHit, bool attackedFromBehind)
    {
        string attackFromBehindText = "";
        if (attackedFromBehind) attackFromBehindText = " from behind";

        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + "punched " + Utilities.GetPronoun(target, false, true) + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText 
                    + ", but the hit was absorbed by " + Utilities.GetPronoun(target, false, true) + "armor.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].GetItemName(1), false, true) 
                    + ", but the attack was absorbed by " + Utilities.GetPronoun(target, false, true) + "armor.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].GetItemName(1), false, true) 
                    + ", but the attack was absorbed by " + Utilities.GetPronoun(target, false, true) + "armor.");
                break;
            default:
                break;
        }
    }

    public void WriteLine_MissedAttack(CharacterManager attacker, CharacterManager target)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, false) + "attacked " + Utilities.GetPronoun(target, false, false) + "and missed!");
    }

    public void WriteLine_EvadedAttack(CharacterManager attacker, CharacterManager target)
    {
        WriteLine(Utilities.GetPronoun(target, true, false) + "evaded " + Utilities.GetPronoun(attacker, false, true) + "attack!");
    }

    public void WriteLine_BlockedAttack(CharacterManager attacker, CharacterManager target, ItemData weaponOrShieldItemData)
    {
        WriteLine(Utilities.GetPronoun(target, true, false) + "blocked " + Utilities.GetPronoun(attacker, false, true) + "attack with " + Utilities.GetPossessivePronoun(target) + "<b>" 
            + weaponOrShieldItemData.GetItemName(1) + "</b>!");
    }

    public void WriteLine_StickWeapon(CharacterManager attacker, CharacterManager target, BodyPartType bodyPartStuck, ItemData weaponUsedItemData)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, true) + "<b>" + weaponUsedItemData.GetItemName(1) + "</b> embedded into the flesh of " + Utilities.GetPronoun(target, false, true) 
            + Utilities.FormatEnumStringWithSpaces(bodyPartStuck.ToString(), true) + ".");
    }

    public void WriteLine_StickWeapon(CharacterManager attacker, Stats targetsStats, ItemData weaponUsedItemData)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, true) + "<b>" + weaponUsedItemData.GetItemName(1) + "</b> got stuck in the <b>" + targetsStats.name + "</b>.");
    }

    public void WriteLine_UnstickWeapon(CharacterManager attacker, CharacterManager target, BodyPartType bodyPartStuck, ItemData weaponUsedItemData, int damage)
    {
        if (target.status.isDead)
            WriteLine(Utilities.GetPronoun(attacker, true, false) + "pulled the <b>" + weaponUsedItemData.GetItemName(1) + "</b> out of " + Utilities.GetPronoun(target, false, true) + "corpse. Blood spills out of the wound.");
        else
            WriteLine(Utilities.GetPronoun(attacker, true, false) + "pulled the <b>" + weaponUsedItemData.GetItemName(1) + "</b> out of " + Utilities.GetPronoun(target, false, true)
                + Utilities.FormatEnumStringWithSpaces(bodyPartStuck.ToString(), true) + ", causing an additional <b><color=red>" + damage + "</color></b> damage. Blood spurts out of the wound.");
    }

    public void WriteLine_UnstickWeapon(CharacterManager attacker, Stats targetsStats, ItemData weaponUsedItemData, int damage)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, false) + "pulled the <b>" + weaponUsedItemData.GetItemName(1) + "</b> out of the <b>" + targetsStats.name + "</b>.");
    }

    public void WriteLine_SwitchStance(CharacterManager character, ItemData weaponItemData, Weapon weapon)
    {
        if (character.equipmentManager.isTwoHanding)
        {
            // If the character doesn't have the required strength to use the weapon effectively, even two-handed
            if (character.characterStats.strength.GetValue() <= weapon.StrengthRequired_TwoHand())
                WriteLine(Utilities.GetPronoun(character, true, false) + "grip " + Utilities.GetPossessivePronoun(character) + "<b>" + weaponItemData.GetItemName(1) 
                    + "</b> with both hands, yet the weapon still feels too heavy for you to wield it effectively.");
            else
                WriteLine(Utilities.GetPronoun(character, true, false) + "firmly grip " + Utilities.GetPossessivePronoun(character) + "<b>" + weaponItemData.GetItemName(1) + "</b> with both hands.");
        }
        else // If one-handing
        {
            // If the character doesn't have the required strength to effectively wield the weapon with one hand
            if (weapon.IsTwoHanded(character))
                WriteLine(Utilities.GetPronoun(character, true, false) + "hold " + Utilities.GetPossessivePronoun(character) + "<b>" + weaponItemData.GetItemName(1)
                    + "</b> in one hand. Your muscles start to tremble under the weight of the weapon.");
            else
                WriteLine(Utilities.GetPronoun(character, true, false) + "firmly grip " + Utilities.GetPossessivePronoun(character) + "<b>" + weaponItemData.GetItemName(1) + "</b> with one hand.");
        }
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
        if (weapon == null)
            return "hit ";

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
    #endregion

    #region Injuries
    public void WriteLine_Injury(CharacterManager characterManager, Injury injury, BodyPartType bodyPartType)
    {
        StartCoroutine(DelayWriteLine(Utilities.GetPronoun(characterManager, true, false) + "received " + Utilities.GetIndefiniteArticle(injury.name, false, true) + " on " 
            + Utilities.GetPossessivePronoun(characterManager) + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + "."));
    }

    public void WriteLine_Reinjure(CharacterManager characterManager, Injury injury, BodyPartType bodyPartType)
    {
        StartCoroutine(DelayWriteLine("An existing <b>" + injury.name + "</b> on " + Utilities.GetPronoun(characterManager, false, true) 
            + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + " was also hit, reinjuring it.", 2));
    }

    public void WriteLine_Bleed(CharacterManager characterManager, BodyPartType bodyPartType, int damage)
    {
        StartCoroutine(DelayWriteLine(Utilities.GetPronoun(characterManager, true, false) + "bleed from injuries on " + Utilities.GetPronoun(characterManager, false, true)
            + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + ", causing <b><color=red>" + damage + "</color></b> damage.", 3));
    }

    public void WriteLine_ApplyBandage(CharacterManager characterApplying, CharacterManager characterBeingBandaged, Injury injury, ItemData itemData, BodyPartType bodyPartType)
    {
        if (characterApplying == characterBeingBandaged)
            WriteLine(Utilities.GetPronoun(characterApplying, true, false) + "wrapped up the <b>" + injury.name + "</b> on " + Utilities.GetPossessivePronoun(characterBeingBandaged)
                + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(itemData.GetSoilageText() + " " + itemData.GetItemName(1), false, true) + ".");
        else
            WriteLine(Utilities.GetPronoun(characterApplying, true, false) + "wrapped up the <b>" + injury.name + "</b> on " + Utilities.GetPronoun(characterBeingBandaged, false, true)
                + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(itemData.GetSoilageText() + " " + itemData.GetItemName(1), false, true) + ".");
    }

    public void WriteLine_RemoveBandage(CharacterManager characterRemoving, CharacterManager bandagedCharacter, ItemData itemData, BodyPartType bodyPartType)
    {
        if (characterRemoving == bandagedCharacter)
            WriteLine(Utilities.GetPronoun(characterRemoving, true, false) + "removed " + Utilities.GetIndefiniteArticle(itemData.GetSoilageText() + " " + itemData.GetItemName(1), false, true)
                + " from " + Utilities.GetPossessivePronoun(bandagedCharacter) + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + ".");
        else
            WriteLine(Utilities.GetPronoun(characterRemoving, true, false) + "removed " + Utilities.GetIndefiniteArticle(itemData.GetSoilageText() + " " + itemData.GetItemName(1), false, true)
                + " from " + Utilities.GetPronoun(bandagedCharacter, false, true) + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + ".");
    }

    /*string GetInjuryDescription(Injury injury)
    {
        for (int i = 0; i < gm.traumaSystem.lacerations.Length; i++)
        {
            if (gm.traumaSystem.lacerations[i] == injury)
            {
                if (injury.descriptions.Length > 0)
                {
                    int random = UnityEngine.Random.Range(0, injury.descriptions.Length);
                    return injury.descriptions[random];
                }
                else
                    Debug.LogWarning("No descriptions set in the ScriptableObject for <b>" + injury.name + "</b>. Fix me!");
            }
        }
        return "";
    }*/
    #endregion

    #region Inventory
    public void WriteLine_DropItem(CharacterManager character, ItemData itemDropping, int amountDropping)
    {
        if (amountDropping == 1)
            WriteLine(Utilities.GetPronoun(character, true, false) + "dropped " + Utilities.GetPossessivePronoun(character) + "<b>" + itemDropping.GetItemName(1) + "</b>.");
        else
            WriteLine(Utilities.GetPronoun(character, true, false) + "dropped " + amountDropping + " <b>" + itemDropping.GetItemName(amountDropping) + "</b>.");
    }

    public void WriteLine_DroppingPersonalItems()
    {
        WriteLine("<b>You</b> no longer have room for all of the items in your <b>Personal Inventory</b>. You will have to drop some items for now.");
    }

    public void WriteLine_CarryItem(CharacterManager character, ItemData itemData)
    {
        if (itemData.currentStackSize > 1)
            WriteLine(Utilities.GetPronoun(character, true, false) + "picked up and started carrying <b>" + itemData.currentStackSize + " " + itemData.GetItemName(itemData.currentStackSize) + "</b>.");
        else
            WriteLine(Utilities.GetPronoun(character, true, false) + "picked up and starting carrying the <b>" + itemData.GetItemName(itemData.currentStackSize) + "</b>.");
    }

    public void WriteLine_CantCarryItem(CharacterManager character, ItemData itemData, int itemCount)
    {
        BodyPart leftHand = character.status.GetBodyPart(BodyPartType.LeftHand);
        BodyPart rightHand = character.status.GetBodyPart(BodyPartType.RightHand);

        if ((leftHand.isIncapacitated || leftHand.isSevered) && (rightHand.isIncapacitated || rightHand.isSevered))
            WriteLine("<b>You</b> want to pick up and carry the <b>" + itemData.GetItemName(itemCount) + "</b>, but you no longer have use of either of your hands.");
        else
            WriteLine("<b>You</b> try to pick up and carry the <b>" + itemData.GetItemName(itemCount) + "</b>, but you don't have enough room in your hands.");
    }

    public void WriteLine_OverEncumbered()
    {
        Debug.Log("Here");
        WriteLine("<b>You</b> feel weighed down by all of the items and equipment you have on you. Your every action feels sluggish.");
    }

    public void WriteLine_TakeItem(ItemData itemTaking, int amountTaking, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        string invTakingFromName = "";
        string invPuttingInName = "";
        if (inventoryTakingFrom != null)
        {
            if (inventoryTakingFrom == gm.playerManager.personalInventory)
                invTakingFromName = "Personal Inventory";
            else
                invTakingFromName = inventoryTakingFrom.name;
        }

        if (inventoryPuttingIn != null)
        {
            if (inventoryPuttingIn == gm.playerManager.personalInventory)
                invPuttingInName = "Personal Inventory";
            else
                invPuttingInName = inventoryPuttingIn.name;
        }

        if (inventoryPuttingIn == gm.playerManager.keysInventory)
        {
            if (inventoryTakingFrom != null)
                WriteLine("<b>You</b> took " + Utilities.GetIndefiniteArticle(itemTaking.GetItemName(amountTaking), false, true) + " from the <b>" + invTakingFromName + "</b> and put it on your <b>key ring</b>.");
            else
                WriteLine("<b>You</b> picked up " + Utilities.GetIndefiniteArticle(itemTaking.GetItemName(amountTaking), false, true) + " and put it on your <b>key ring</b>.");
        }
        else if (amountTaking == 1)
        {
            if (inventoryTakingFrom != null)
                WriteLine("<b>You</b> took " + Utilities.GetIndefiniteArticle(itemTaking.GetItemName(amountTaking), false, true) + " from the <b>" + invTakingFromName + "</b> and put it in your <b>" + invPuttingInName + "</b>.");
            else
                WriteLine("<b>You</b> picked up " + Utilities.GetIndefiniteArticle(itemTaking.GetItemName(amountTaking), false, true) + " and put it in your <b>" + invPuttingInName + "</b>.");
        }
        else
        {
            if (inventoryTakingFrom != null)
                WriteLine("<b>You</b> took <b>" + amountTaking + " " + itemTaking.GetItemName(amountTaking) + "</b> from the <b>" + invTakingFromName + "</b> and put them in your <b>" + invPuttingInName + "</b>.");
            else
                WriteLine("<b>You</b> picked up <b>" + amountTaking + " " + itemTaking.GetItemName(amountTaking) + "</b>" + " and put them in your <b>" + invPuttingInName + "</b>.");
        }
    }

    public void WriteLine_TransferItem(ItemData itemTaking, int amountTaking, EquipmentManager equipmentManagerTakingFrom, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        string invTakingFromName = "";
        string invPuttingInName = "";
        if (inventoryTakingFrom != null)
        {
            if (inventoryTakingFrom == gm.playerManager.personalInventory)
                invTakingFromName = "Personal Inventory";
            else
                invTakingFromName = inventoryTakingFrom.name;
        }

        if (inventoryPuttingIn != null)
        {
            if (inventoryPuttingIn == gm.playerManager.personalInventory)
                invPuttingInName = "Personal Inventory";
            else
                invPuttingInName = inventoryPuttingIn.name;
        }

        if (amountTaking == 1)
        {
            if (equipmentManagerTakingFrom != null)
                WriteLine("<b>You</b> took " + Utilities.GetIndefiniteArticle(itemTaking.GetItemName(amountTaking), false, true) + " from your <b>Equipped Items</b> and put it in the <b>" 
                    + invPuttingInName + "</b>.");
            else
                WriteLine("<b>You</b> took " + Utilities.GetIndefiniteArticle(itemTaking.GetItemName(amountTaking), false, true) + " from your <b>" + invTakingFromName 
                    + "</b> and put it in the <b>" + invPuttingInName + "</b>.");
        }
        else
        {
            if (equipmentManagerTakingFrom != null)
                WriteLine("<b>You</b> took " + amountTaking + " <b>" + itemTaking.GetItemName(amountTaking) + "</b> from your <b>Equipped Items</b> and put them in the <b>" + invPuttingInName + "</b>.");
            else
                WriteLine("<b>You</b> took " + amountTaking + " <b>" + itemTaking.GetItemName(amountTaking) + "</b> from your <b>" + invTakingFromName
                    + "</b> and put them in the <b>" + invPuttingInName + "</b>.");
        }
    }

    public void WriteLine_ItemTooLarge(CharacterManager characterManager, ItemData itemData, Inventory inventory)
    {
        string inventoryName;
        if (inventory.myItemData != null)
            inventoryName = inventory.myItemData.GetItemName(1);
        else if (inventory == characterManager.personalInventory)
            inventoryName = "Personal Inventory";
        else
            inventoryName = inventory.name;

        WriteLine("<b>You</b> try to put " + Utilities.GetIndefiniteArticle(itemData.GetItemName(itemData.currentStackSize), false, true) + " in your <b>" + inventoryName + "</b>, but it is too large.");
    }

    public void WriteLine_Equip(ItemData itemEquipping, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "equipped " + Utilities.GetIndefiniteArticle(itemEquipping.GetItemName(1), false, true) + ".");
    }
    
    public void WriteLine_Unequip(ItemData itemUnequipping, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "unequipped " + Utilities.GetPossessivePronoun(characterManager) + "<b>" + itemUnequipping.GetItemName(1) + "</b>.");
    }

    public void WriteLine_TryEquipBrokenItem(ItemData itemTryingToEquip, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "move to equip the <b>" + itemTryingToEquip.GetItemName(1) + "</b>, but then " + Utilities.GetPronoun(characterManager, false, false) + "realize it's broken.");
    }

    public void WriteLine_SheatheWeapon(ItemData weaponOne, ItemData weaponTwo = null)
    {
        if (weaponOne != null && weaponTwo != null)
        {
            if (GetSheatheVerb(weaponOne) == GetSheatheVerb(weaponTwo))
                WriteLine("<b>You</b> " + GetSheatheVerb(weaponTwo) + " your <b>" + weaponTwo.GetItemName(1) + "</b> and your <b>" + weaponOne.GetItemName(1) + "</b>.");
            else
                WriteLine("<b>You</b> " + GetSheatheVerb(weaponTwo) + " your <b>" + weaponTwo.GetItemName(1) + "</b> and " + GetSheatheVerb(weaponOne) + " your <b>" + weaponOne.GetItemName(1) + "</b>.");
        }
        else if (weaponOne != null)
            WriteLine("<b>You</b> " + GetSheatheVerb(weaponOne) + " your <b>" + weaponOne.GetItemName(1) + "</b>.");
    }

    public void WriteLine_UnheatheWeapon(ItemData leftWeapon, ItemData rightWeapon)
    {
        if (leftWeapon != null && rightWeapon != null)
        {
            if (GetUnsheatheVerb(leftWeapon) == GetUnsheatheVerb(rightWeapon))
                WriteLine("<b>You</b> " + GetUnsheatheVerb(rightWeapon) + " your <b>" + rightWeapon.GetItemName(1) + "</b> and your <b>" + leftWeapon.GetItemName(1) + "</b>.");
            else
                WriteLine("<b>You</b> " + GetUnsheatheVerb(rightWeapon) + " your <b>" + rightWeapon.GetItemName(1) + "</b> and " + GetUnsheatheVerb(leftWeapon) + " your <b>" + leftWeapon.GetItemName(1) + "</b>.");
        }
        else if (rightWeapon != null)
            WriteLine("<b>You</b> " + GetUnsheatheVerb(rightWeapon) + " your <b>" + rightWeapon.GetItemName(1) + "</b>.");
        else if (leftWeapon != null)
            WriteLine("<b>You</b> " + GetUnsheatheVerb(leftWeapon) + " your <b>" + leftWeapon.GetItemName(1) + "</b>.");
    }

    string GetSheatheVerb(ItemData itemData)
    {
        if (itemData.item.IsWeapon())
        {
            Weapon weapon = (Weapon)itemData.item;
            if (weapon.weaponType == WeaponType.Sword || weapon.weaponType == WeaponType.Dagger)
                return "sheathe";
        }
        return "stow away";
    }

    string GetUnsheatheVerb(ItemData itemData)
    {
        if (itemData.item.IsWeapon())
        {
            Weapon weapon = (Weapon)itemData.item;
            if (weapon.weaponType == WeaponType.Sword || weapon.weaponType == WeaponType.Dagger)
                return "unsheathe";
        }
        return "pull out";
    }
    #endregion

    #region Food/Drink
    public void WriteLine_Consume(CharacterManager characterManager, Consumable consumable, string itemName, float amount)
    {
        switch (consumable.consumableType)
        {
            case ConsumableType.Food:
                if (amount == 1)
                    WriteLine(Utilities.GetPronoun(characterManager, true, false) + "ate " + Utilities.GetIndefiniteArticle(itemName, false, true) + ".");
                else if (amount > 1)
                    WriteLine(Utilities.GetPronoun(characterManager, true, false) + "ate <b>" + amount + " " + itemName + "</b>.");
                else
                    WriteLine(Utilities.GetPronoun(characterManager, true, false) + "ate " + GetFractionalAmount(amount) + "of the <b>" + itemName + "</b>.");
                break;
            case ConsumableType.Drink:
                if (amount == 1)
                    WriteLine(Utilities.GetPronoun(characterManager, true, false) + "drank " + Utilities.GetIndefiniteArticle(itemName, false, true) + ".");
                else if (amount > 1)
                    WriteLine(Utilities.GetPronoun(characterManager, true, false) + "drank <b>" + amount + " " + itemName + "</b>.");
                else
                    WriteLine(Utilities.GetPronoun(characterManager, true, false) + "drank " + GetFractionalAmount(amount) + "of the <b>" + itemName + "</b>.");
                break;
            default:
                break;
        }
    }

    string GetFractionalAmount(float amount)
    {
        if (amount == 0.5f)
            return "half ";
        else if (amount == 0.25f)
            return "a quarter ";
        else if (amount == 0.1f)
            return "a little bit ";
        return "some ";
    }
    #endregion

    #region Status Effects
    public void WriteLine_Vomit(CharacterManager character)
    {
        WriteLine(Utilities.GetPronoun(character, true, true) + "nausea hits a breaking point. " + Utilities.GetPronoun(character, false, false) + "vomit all over the ground.");
    }

    public void WriteLine_DryHeave(CharacterManager character)
    {
        WriteLine(Utilities.GetPronoun(character, true, true) + "nausea hits a breaking point. " + Utilities.GetPronoun(character, true, false) + "start dry heaving, as there's nothing left in " 
            + Utilities.GetPossessivePronoun(character) + "stomach.");
    }
    #endregion

    IEnumerator SetupScrollbar()
    {
        yield return null;
        yield return null;
        scrollbar.value = 0;
    }
}

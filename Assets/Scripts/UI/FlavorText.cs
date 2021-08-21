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
        int numLines = Convert.ToString(stringBuilder).Split('\n').Length;
        if (numLines > maxLines)
            stringBuilder.Remove(0, Convert.ToString(stringBuilder).Split('\n').FirstOrDefault().Length + 1);

        stringBuilder.Append("- " + input + "\n");
        flavorText.text = Convert.ToString(stringBuilder);

        StartCoroutine(SetupScrollbar());
    }

    public IEnumerator DelayWriteLine(string input, int yieldCount = 1)
    {
        for (int i = 0; i < yieldCount; i++) { yield return null; }
        WriteLine(input);
    }

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
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].itemName, false, true)
                    + attackFromBehindText + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].itemName, false, true)
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
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].itemName, false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, armor) + Utilities.GetPronoun(target, false, true) + "<b>" + armor.name + "</b> and <b>" + clothing.name 
                    + "</b>, ignoring defenses and causing <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].itemName, false, true) 
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
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].itemName, false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, wearable) + Utilities.GetPronoun(target, false, true) + "<b>" + wearable.name + "</b>, ignoring its defenses and causing <b><color=red>" 
                    + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].itemName, false, true) + " and " 
                    + GetPenetrateArmorVerb(physicalDamageType, wearable) + Utilities.GetPronoun(target, false, true) + "<b>" + wearable.name + "</b>, ignoring its defenses and causing <b><color=red>" 
                    + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WriteLine_MeleeAttackObject(CharacterManager attacker, Stats targetsStats, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, int damage)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + "punched the " + targetsStats.name + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + "the " + targetsStats.name
                    + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].itemName, false, true) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + "the " + targetsStats.name
                    + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].itemName, false, true) + " for <b><color=red>" + damage + "</color></b> damage.");
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
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem].itemName, false, true) 
                    + ", but the attack was absorbed by " + Utilities.GetPronoun(target, false, true) + "armor.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + attackFromBehindText + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem].itemName, false, true) 
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
        WriteLine(Utilities.GetPronoun(target, true, false) + "blocked " + Utilities.GetPronoun(attacker, false, true) + "attack with " + Utilities.GetPossessivePronoun(target) + "<b>" + weaponOrShieldItemData.itemName + "</b>!");
    }

    public void WriteLine_StickWeapon(CharacterManager attacker, CharacterManager target, BodyPartType bodyPartStuck, ItemData weaponUsedItemData)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, true) + "<b>" + weaponUsedItemData.itemName + "</b> embedded into the flesh of " + Utilities.GetPronoun(target, false, true) 
            + Utilities.FormatEnumStringWithSpaces(bodyPartStuck.ToString(), true) + ".");
    }

    public void WriteLine_StickWeapon(CharacterManager attacker, Stats targetsStats, ItemData weaponUsedItemData)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, true) + "<b>" + weaponUsedItemData.itemName + "</b> got stuck in the <b>" + targetsStats.name + "</b>.");
    }

    public void WriteLine_UnstickWeapon(CharacterManager attacker, CharacterManager target, BodyPartType bodyPartStuck, ItemData weaponUsedItemData, int damage)
    {
        if (target.status.isDead)
            WriteLine(Utilities.GetPronoun(attacker, true, false) + "pulled the <b>" + weaponUsedItemData.itemName + "</b> out of " + Utilities.GetPronoun(target, false, true) + "corpse. Blood spills out of the wound.");
        else
            WriteLine(Utilities.GetPronoun(attacker, true, false) + "pulled the <b>" + weaponUsedItemData.itemName + "</b> out of " + Utilities.GetPronoun(target, false, true)
                + Utilities.FormatEnumStringWithSpaces(bodyPartStuck.ToString(), true) + ", causing an additional <b><color=red>" + damage + "</color></b> damage. Blood spurts out of the wound.");
    }

    public void WriteLine_UnstickWeapon(CharacterManager attacker, Stats targetsStats, ItemData weaponUsedItemData, int damage)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, false) + "pulled the <b>" + weaponUsedItemData.itemName + "</b> out of the <b>" + targetsStats.name + "</b>.");
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
                + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(itemData.GetSoilageText() + " " + itemData.itemName, false, true) + ".");
        else
            WriteLine(Utilities.GetPronoun(characterApplying, true, false) + "wrapped up the <b>" + injury.name + "</b> on " + Utilities.GetPronoun(characterBeingBandaged, false, true)
                + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(itemData.GetSoilageText() + " " + itemData.itemName, false, true) + ".");
    }

    public void WriteLine_RemoveBandage(CharacterManager characterRemoving, CharacterManager bandagedCharacter, ItemData itemData, BodyPartType bodyPartType)
    {
        if (characterRemoving == bandagedCharacter)
            WriteLine(Utilities.GetPronoun(characterRemoving, true, false) + "removed " + Utilities.GetIndefiniteArticle(itemData.GetSoilageText() + " " + itemData.itemName, false, true)
                + " from " + Utilities.GetPossessivePronoun(bandagedCharacter) + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + ".");
        else
            WriteLine(Utilities.GetPronoun(characterRemoving, true, false) + "removed " + Utilities.GetIndefiniteArticle(itemData.GetSoilageText() + " " + itemData.itemName, false, true)
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
    public void WriteLine_DropItem(ItemData itemDropping, int amountDropping)
    {
        if (amountDropping == 1)
            WriteLine(Utilities.GetPronoun(null, true, false) + "dropped " + Utilities.GetIndefiniteArticle(itemDropping.itemName, false, true) + ".");
        else
            WriteLine(Utilities.GetPronoun(null, true, false) + "dropped " + amountDropping + " <b>" + itemDropping.itemName + "s</b>.");
    }

    public void WriteLine_DroppingPersonalItems()
    {
        WriteLine("<b>You</b> no longer have room for all of the items in your <b>Personal Inventory</b>. You will have to drop some items for now." );
    }

    public void WriteLine_CarryItem(ItemData itemData)
    {
        if (itemData.currentStackSize > 1)
            WriteLine("<b>You</b> pick up and carry <b>" + itemData.currentStackSize + " " + itemData.GetPluralName() + "</b>.");
        else
            WriteLine("<b>You</b> pick up and carry the <b>" + itemData.itemName + "</b>.");
    }

    public void WriteLine_CantCarryItem(ItemData itemData, int itemCount)
    {
        BodyPart leftHand = gm.playerManager.status.GetBodyPart(BodyPartType.LeftHand);
        BodyPart rightHand = gm.playerManager.status.GetBodyPart(BodyPartType.RightHand);
        string itemName;
        if (itemCount > 1)
            itemName = itemData.GetPluralName();
        else
            itemName = itemData.itemName;

        if ((leftHand.isIncapacitated || leftHand.isSevered) && (rightHand.isIncapacitated || rightHand.isSevered))
            WriteLine("<b>You</b> want to pick up and carry the <b>" + itemName + "</b>, but you no longer have use of your hands.");
        else
            WriteLine("<b>You</b> try to pick up and carry the <b>" + itemName + "</b>, but you don't have enough room in your hands.");
    }

    public void WriteLine_TakeItem(ItemData itemTaking, int amountTaking, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        string invPuttingInName;
        if (inventoryPuttingIn == gm.playerManager.personalInventory)
            invPuttingInName = "Personal Inventory";
        else
            invPuttingInName = inventoryPuttingIn.name;

        if (inventoryPuttingIn == gm.playerManager.keysInventory)
        {
            if (inventoryTakingFrom != null)
                WriteLine("<b>You</b> took " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " from the <b>" + inventoryTakingFrom.name + "</b> and put it on your <b>key ring</b>.");
            else
                WriteLine("<b>You</b> picked up " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " and put it on your <b>key ring</b>.");
        }
        else if (amountTaking == 1)
        {
            if (inventoryTakingFrom != null)
                WriteLine("<b>You</b> took " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " from the <b>" + inventoryTakingFrom.name + "</b> and put it in your <b>" + invPuttingInName + "</b>.");
            else
                WriteLine("<b>You</b> picked up " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " and put it in your <b>" + invPuttingInName + "</b>.");
        }
        else
        {
            if (inventoryTakingFrom != null)
                WriteLine("<b>You</b> took <b>" + amountTaking + " " + itemTaking.itemName + "s</b> from the <b>" + inventoryTakingFrom.name + "</b> and put them in your <b>" + invPuttingInName + "</b>.");
            else
                WriteLine("<b>You</b> picked up <b>" + amountTaking + " " + itemTaking.itemName + "s</b>" + " and put them in your <b>" + invPuttingInName + "</b>.");
        }
    }

    public void WriteLine_TransferItem(ItemData itemTaking, int amountTaking, EquipmentManager equipmentManagerTakingFrom, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        if (amountTaking == 1)
        {
            if (equipmentManagerTakingFrom != null)
                WriteLine(Utilities.GetPronoun(null, true, false) + "took " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " from your <b>" + equipmentManagerTakingFrom.name 
                    + "</b> and put it in the <b>" + inventoryPuttingIn.name + "</b>.");
            else
                WriteLine(Utilities.GetPronoun(null, true, false) + "took " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " from your <b>" + inventoryTakingFrom.name 
                    + "</b> and put it in the <b>" + inventoryPuttingIn.name + "</b>.");
        }
        else
        {
            if (equipmentManagerTakingFrom != null)
                WriteLine(Utilities.GetPronoun(null, true, false) + "took " + amountTaking + " <b>" + itemTaking.itemName + "s</b> from your <b>" + equipmentManagerTakingFrom.name 
                    + "</b> and put them in the <b>" + inventoryPuttingIn.name + "</b>.");
            else
                WriteLine(Utilities.GetPronoun(null, true, false) + "took " + amountTaking + " <b>" + itemTaking.itemName + "s</b> from your <b>" + inventoryTakingFrom.name 
                    + "</b> and put them in the <b>" + inventoryPuttingIn.name + "</b>.");
        }
    }

    public void WriteLine_ItemTooLarge(CharacterManager characterManager, ItemData itemData, Inventory inventory)
    {
        string inventoryName;
        if (inventory.myItemData != null)
            inventoryName = inventory.myItemData.itemName;
        else if (inventory == characterManager.personalInventory)
            inventoryName = "Personal Inventory";
        else
            inventoryName = inventory.name;

        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "try to put " + Utilities.GetIndefiniteArticle(itemData.itemName, false, true) + " in " + Utilities.GetPossessivePronoun(characterManager) 
            + "<b>" + inventoryName + "</b>, but it is too large.");
    }

    public void WriteLine_Equip(ItemData itemEquipping, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "equipped " + Utilities.GetIndefiniteArticle(itemEquipping.itemName, false, true) + ".");
    }
    
    public void WriteLine_Unequip(ItemData itemUnequipping, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "unequipped " + Utilities.GetPossessivePronoun(characterManager) + "<b>" + itemUnequipping.itemName + "</b>.");
    }

    public void WriteLine_TryEquipBrokenItem(ItemData itemTryingToEquip, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "move to equip the <b>" + itemTryingToEquip.itemName + "</b>, but then " + Utilities.GetPronoun(characterManager, false, false) + "realize it's broken.");
    }

    public void WriteLine_SheatheWeapon(ItemData weaponOne, ItemData weaponTwo = null)
    {
        if (weaponOne != null && weaponTwo != null)
        {
            if (GetSheatheVerb(weaponOne) == GetSheatheVerb(weaponTwo))
                WriteLine("<b>You</b> " + GetSheatheVerb(weaponTwo) + " your <b>" + weaponTwo.itemName + "</b> and your <b>" + weaponOne.itemName + "</b>.");
            else
                WriteLine("<b>You</b> " + GetSheatheVerb(weaponTwo) + " your <b>" + weaponTwo.itemName + "</b> and " + GetSheatheVerb(weaponOne) + " your <b>" + weaponOne.itemName + "</b>.");
        }
        else if (weaponOne != null)
            WriteLine("<b>You</b> " + GetSheatheVerb(weaponOne) + " your <b>" + weaponOne.itemName + "</b>.");
    }

    public void WriteLine_UnheatheWeapon(ItemData leftWeapon, ItemData rightWeapon)
    {
        if (leftWeapon != null && rightWeapon != null)
        {
            if (GetUnsheatheVerb(leftWeapon) == GetUnsheatheVerb(rightWeapon))
                WriteLine("<b>You</b> " + GetUnsheatheVerb(rightWeapon) + " your <b>" + rightWeapon.itemName + "</b> and your <b>" + leftWeapon.itemName + "</b>.");
            else
                WriteLine("<b>You</b> " + GetUnsheatheVerb(rightWeapon) + " your <b>" + rightWeapon.itemName + "</b> and " + GetUnsheatheVerb(leftWeapon) + " your <b>" + leftWeapon.itemName + "</b>.");
        }
        else if (rightWeapon != null)
            WriteLine("<b>You</b> " + GetUnsheatheVerb(rightWeapon) + " your <b>" + rightWeapon.itemName + "</b>.");
        else if (leftWeapon != null)
            WriteLine("<b>You</b> " + GetUnsheatheVerb(leftWeapon) + " your <b>" + leftWeapon.itemName + "</b>.");
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
    public void WriteLine_Consume(Consumable consumable, CharacterManager characterManager)
    {
        switch (consumable.consumableType)
        {
            case ConsumableType.Food:
                WriteLine(Utilities.GetPronoun(characterManager, true, false) + "ate " + Utilities.GetIndefiniteArticle(consumable.name, false, true) + ".");
                break;
            case ConsumableType.Drink:
                WriteLine(Utilities.GetPronoun(characterManager, true, false) + "drank " + Utilities.GetIndefiniteArticle(consumable.name, false, true) + ".");
                break;
            default:
                break;
        }
    }
    #endregion

    IEnumerator SetupScrollbar()
    {
        yield return null;
        yield return null;
        scrollbar.value = 0;
    }
}

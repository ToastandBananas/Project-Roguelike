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
    GameManager gm;

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

    public IEnumerator DelayWriteLine(string input)
    {
        yield return null;
        WriteLine(input);
    }

    #region Combat
    public void WriteMeleeAttackCharacterLine(CharacterManager attacker, CharacterManager target, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, BodyPartType bodyPartHit, int damage)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + "punched " + Utilities.GetPronoun(target, false, true) + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) 
                    + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName, false, true) 
                    + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon].itemName, false, true) 
                    + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WritePenetrateArmorAndClothingLine_Melee(CharacterManager attacker, CharacterManager target, Wearable armor, Wearable clothing, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, PhysicalDamageType physicalDamageType, BodyPartType bodyPartHit, int damage)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName, false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, armor) + Utilities.GetPronoun(target, false, true) + "<b>" + armor.name + "</b> and <b>" + clothing.name 
                    + "</b>, ignoring defenses and causing <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon].itemName, false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, armor) + Utilities.GetPronoun(target, false, true) + "<b>" + armor.name + "</b> and <b>" + clothing.name 
                    + "</b>, ignoring defenses and causing <b><color=red>" + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WritePenetrateWearableLine_Melee(CharacterManager attacker, CharacterManager target, Wearable wearable, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, PhysicalDamageType physicalDamageType, BodyPartType bodyPartHit, int damage)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName, false, true) 
                    + " and " + GetPenetrateArmorVerb(physicalDamageType, wearable) + Utilities.GetPronoun(target, false, true) + "<b>" + wearable.name + "</b>, ignoring its defenses and causing <b><color=red>" 
                    + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon].itemName, false, true) + " and " 
                    + GetPenetrateArmorVerb(physicalDamageType, wearable) + Utilities.GetPronoun(target, false, true) + "<b>" + wearable.name + "</b>, ignoring its defenses and causing <b><color=red>" 
                    + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WriteMeleeAttackObjectLine(CharacterManager attacker, Stats targetsStats, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, int damage)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + "punched the " + targetsStats.name + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + "the " + targetsStats.name
                    + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName, false, true) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + "the " + targetsStats.name
                    + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon].itemName, false, true) + " for <b><color=red>" + damage + "</color></b> damage.");
                break;
            default:
                break;
        }
    }

    public void WriteAbsorbedMeleeAttackLine(CharacterManager attacker, CharacterManager target, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType, BodyPartType bodyPartHit)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + "punched " + Utilities.GetPronoun(target, false, true) + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + ", but the hit was absorbed by " 
                    + Utilities.GetPronoun(target, false, true) + "armor.");
                break;
            case GeneralAttackType.PrimaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetRightWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName, false, true) 
                    + ", but the attack was absorbed by " + Utilities.GetPronoun(target, false, true) + "armor.");
                break;
            case GeneralAttackType.SecondaryWeapon:
                WriteLine(Utilities.GetPronoun(attacker, true, false) + GetMeleeWeaponAttackVerb(attacker.equipmentManager.GetLeftWeapon(), meleeAttackType) + Utilities.GetPronoun(target, false, true) 
                    + Utilities.FormatEnumStringWithSpaces(bodyPartHit.ToString(), true) + " with " + Utilities.GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon].itemName, false, true) 
                    + ", but the attack was absorbed by " + Utilities.GetPronoun(target, false, true) + "armor.");
                break;
            default:
                break;
        }
    }

    public void WriteMissedAttackLine(CharacterManager attacker, CharacterManager target)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, false) + "attacked " + Utilities.GetPronoun(target, false, false) + "and missed!");
    }

    public void WriteEvadedAttackLine(CharacterManager attacker, CharacterManager target)
    {
        WriteLine(Utilities.GetPronoun(target, true, false) + "evaded " + Utilities.GetPronoun(attacker, false, true) + "attack!");
    }

    public void WriteBlockedAttackLine(CharacterManager attacker, CharacterManager target, ItemData weaponOrShieldItemData)
    {
        WriteLine(Utilities.GetPronoun(target, true, false) + "blocked " + Utilities.GetPronoun(attacker, false, true) + "attack with " + Utilities.GetPossessivePronoun(target) + "<b>" + weaponOrShieldItemData.itemName + "</b>!");
    }

    public void WriteStickWeaponLine(CharacterManager attacker, CharacterManager target, BodyPartType bodyPartStuck, ItemData weaponUsedItemData)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, true) + "<b>" + weaponUsedItemData.itemName + "</b> embedded into the flesh of " + Utilities.GetPronoun(target, false, true) 
            + Utilities.FormatEnumStringWithSpaces(bodyPartStuck.ToString(), true) + ".");
    }

    public void WriteStickWeaponLine(CharacterManager attacker, Stats targetsStats, ItemData weaponUsedItemData)
    {
        WriteLine(Utilities.GetPronoun(attacker, true, true) + "<b>" + weaponUsedItemData.itemName + "</b> got stuck in the <b>" + targetsStats.name + "</b>.");
    }

    public void WriteUnstickWeaponLine(CharacterManager attacker, CharacterManager target, BodyPartType bodyPartStuck, ItemData weaponUsedItemData, int damage)
    {
        if (target.status.isDead)
            WriteLine(Utilities.GetPronoun(attacker, true, false) + "pulled the <b>" + weaponUsedItemData.itemName + "</b> out of " + Utilities.GetPronoun(target, false, true) + "corpse. Blood spills out of the wound.");
        else
            WriteLine(Utilities.GetPronoun(attacker, true, false) + "pulled the <b>" + weaponUsedItemData.itemName + "</b> out of " + Utilities.GetPronoun(target, false, true)
                + Utilities.FormatEnumStringWithSpaces(bodyPartStuck.ToString(), true) + ", causing an additional <b><color=red>" + damage + "</color></b> damage. Blood spurts out of the wound.");
    }

    public void WriteUnstickWeaponLine(CharacterManager attacker, Stats targetsStats, ItemData weaponUsedItemData, int damage)
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
    public void WriteInjuryLine(CharacterManager characterManager, Injury injury, BodyPartType bodyPartType)
    {
        StartCoroutine(DelayWriteLine(Utilities.GetPronoun(characterManager, true, false) + "received " + Utilities.GetIndefiniteArticle(injury.name, false, true) + " on " 
            + Utilities.GetPossessivePronoun(characterManager) + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + "."));
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
    public void WriteDropItemLine(ItemData itemDropping, int amountDropping)
    {
        if (amountDropping == 1)
            WriteLine(Utilities.GetPronoun(null, true, false) + "dropped " + Utilities.GetIndefiniteArticle(itemDropping.itemName, false, true) + ".");
        else
            WriteLine(Utilities.GetPronoun(null, true, false) + "dropped " + amountDropping + " <b>" + itemDropping.itemName + "s</b>.");
    }

    public void WriteTakeItemLine(ItemData itemTaking, int amountTaking, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        if (inventoryPuttingIn.name == "Keys")
        {
            if (inventoryTakingFrom != null)
                WriteLine(Utilities.GetPronoun(null, true, false) + "took " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " from the <b>" + inventoryTakingFrom.name + "</b> and put it on your <b>key ring</b>.");
            else
                WriteLine(Utilities.GetPronoun(null, true, false) + "picked up " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " and put it on your <b>key ring</b>.");
        }
        else if (amountTaking == 1)
        {
            if (inventoryTakingFrom != null)
                WriteLine(Utilities.GetPronoun(null, true, false) + "took " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " from the <b>" + inventoryTakingFrom.name + "</b> and put it in your <b>" + inventoryPuttingIn.name + "</b>.");
            else
                WriteLine(Utilities.GetPronoun(null, true, false) + "picked up " + Utilities.GetIndefiniteArticle(itemTaking.itemName, false, true) + " and put it in your <b>" + inventoryPuttingIn.name + "</b>.");
        }
        else
        {
            if (inventoryTakingFrom != null)
                WriteLine(Utilities.GetPronoun(null, true, false) + "took " + amountTaking + " <b>" + itemTaking.itemName + "s</b> from the <b>" + inventoryTakingFrom.name + "</b> and put them in your <b>" + inventoryPuttingIn.name + "</b>.");
            else
                WriteLine(Utilities.GetPronoun(null, true, false) + "picked up " + amountTaking + " <b>" + itemTaking.itemName + "s</b>" + " and put them in your <b>" + inventoryPuttingIn.name + "</b>.");
        }
    }

    public void WriteTransferItemLine(ItemData itemTaking, int amountTaking, EquipmentManager equipmentManagerTakingFrom, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
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

    public void WriteEquipLine(ItemData itemEquipping, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "equipped the <b>" + itemEquipping.itemName + "</b>.");
    }
    
    public void WriteUnquipLine(ItemData itemUnequipping, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "unequipped the <b>" + itemUnequipping.itemName + "</b>.");
    }

    public void WriteTryEquipBrokenItemLine(ItemData itemTryingToEquip, CharacterManager characterManager)
    {
        WriteLine(Utilities.GetPronoun(characterManager, true, false) + "tried to equip the <b>" + itemTryingToEquip.itemName + "</b>, but then " + Utilities.GetPronoun(characterManager, false, false) +  "realize it's broken.");
    }
    #endregion

    #region Food/Drink
    public void WriteConsumeLine(Consumable consumable, CharacterManager characterManager)
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

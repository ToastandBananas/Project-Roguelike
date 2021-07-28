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
        {
            stringBuilder.Remove(0, Convert.ToString(stringBuilder).Split('\n').FirstOrDefault().Length + 1);
        }

        stringBuilder.Append(input + "\n");
        flavorText.text = Convert.ToString(stringBuilder);

        StartCoroutine(SetupScrollbar());
    }

    public void WriteAttackLine(AttackType attackType, CharacterManager attacker, CharacterManager victim, int damage)
    {
        switch (attackType)
        {
            case AttackType.Unarmed:
                WriteLine(GetPronoun(attacker, true) + " punched " + GetPronoun(victim, false) + " for <color=red>" + damage + "</color> damage.");
                break;
            case AttackType.PrimaryWeapon:
                WriteLine(GetPronoun(attacker, true) + " " + GetWeaponAttackVerb(attacker.equipmentManager.GetPrimaryWeapon()) + GetPronoun(victim, false) + " with " 
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon].itemName) + " for <color=red>" + damage + "</color> damage.");
                break;
            case AttackType.SecondaryWeapon:
                WriteLine(GetPronoun(attacker, true) + " " + GetWeaponAttackVerb(attacker.equipmentManager.GetSecondaryWeapon()) + GetPronoun(victim, false) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon].itemName) + " for <color=red>" + damage + "</color> damage.");
                break;
            case AttackType.Ranged:
                WriteLine(GetPronoun(attacker, true) + " " + GetWeaponAttackVerb(attacker.equipmentManager.GetRangedWeapon()) + GetPronoun(victim, false) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.Ranged].itemName) + " for <color=red>" + damage + "</color> damage.");
                break;
            case AttackType.Throwing:
                WriteLine(GetPronoun(attacker, true) + " " + GetWeaponAttackVerb(attacker.equipmentManager.GetRangedWeapon()) + GetPronoun(victim, false) + " with "
                    + GetIndefiniteArticle(attacker.equipmentManager.currentEquipment[(int)EquipmentSlot.Ranged].itemName) + " for <color=red>" + damage + "</color> damage.");
                break;
            case AttackType.Magic:
                break;
            default:
                break;
        }
    }

    public void WriteConsumeLine(Consumable consumable, CharacterManager characterManager)
    {
        switch (consumable.consumableType)
        {
            case ConsumableType.Food:
                WriteLine(GetPronoun(characterManager, true) + " ate " + GetIndefiniteArticle(consumable.name) + ".");
                break;
            case ConsumableType.Drink:
                WriteLine(GetPronoun(characterManager, true) + " drank " + GetIndefiniteArticle(consumable.name) + ".");
                break;
            default:
                break;
        }
    }

    public void WriteDropItemLine(ItemData itemDropping, int amountDropping)
    {
        if (amountDropping == 1)
            WriteLine("You dropped " + GetIndefiniteArticle(itemDropping.itemName) + ".");
        else
            WriteLine("You dropped " + amountDropping + " " + itemDropping.itemName + "s.");
    }

    public void WriteTakeItemLine(ItemData itemTaking, int amountTaking, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        if (inventoryPuttingIn.name == "Keys")
        {
            if (inventoryTakingFrom != null)
                WriteLine("You took " + GetIndefiniteArticle(itemTaking.itemName) + " from the " + inventoryTakingFrom.name + " and put it on your key ring.");
            else
                WriteLine("You picked up " + GetIndefiniteArticle(itemTaking.itemName) + " and put it on your key ring.");
        }
        else if (amountTaking == 1)
        {
            if (inventoryTakingFrom != null)
                WriteLine("You took " + GetIndefiniteArticle(itemTaking.itemName) + " from the " + inventoryTakingFrom.name + " and put it in your " + inventoryPuttingIn.name + ".");
            else
                WriteLine("You picked up " + GetIndefiniteArticle(itemTaking.itemName) + " and put it in your " + inventoryPuttingIn.name + ".");
        }
        else
        {
            if (inventoryTakingFrom != null)
                WriteLine("You took " + amountTaking + " " + itemTaking.itemName + "s from the " + inventoryTakingFrom.name + " and put them in your " + inventoryPuttingIn.name + ".");
            else
                WriteLine("You picked up " + amountTaking + " " + itemTaking.itemName + "s" + " and put them in your " + inventoryPuttingIn.name + ".");
        }
    }

    public void WriteTransferItemLine(ItemData itemTaking, int amountTaking, EquipmentManager equipmentManagerTakingFrom, Inventory inventoryTakingFrom, Inventory inventoryPuttingIn)
    {
        if (amountTaking == 1)
        {
            if (equipmentManagerTakingFrom != null)
                WriteLine("You took " + GetIndefiniteArticle(itemTaking.itemName) + " from your " + equipmentManagerTakingFrom.name + " and put it in the " + inventoryPuttingIn.name + ".");
            else
                WriteLine("You took " + GetIndefiniteArticle(itemTaking.itemName) + " from your " + inventoryTakingFrom.name + " and put it in the " + inventoryPuttingIn.name + ".");
        }
        else
        {
            if (equipmentManagerTakingFrom != null)
                WriteLine("You took " + amountTaking + " " + itemTaking.itemName + "s from your " + equipmentManagerTakingFrom.name + " and put them in the " + inventoryPuttingIn.name + ".");
            else
                WriteLine("You took " + amountTaking + " " + itemTaking.itemName + "s from your " + inventoryTakingFrom.name + " and put them in the " + inventoryPuttingIn.name + ".");
        }
    }

    public void WriteEquipLine(ItemData itemEquipping)
    {
        WriteLine("You equipped the " + itemEquipping.itemName + ".");
    }
    
    public void WriteUnquipLine(ItemData itemUnequipping)
    {
        WriteLine("You unequipped the " + itemUnequipping.itemName + ".");
    }

    IEnumerator SetupScrollbar()
    {
        yield return null;
        yield return null;
        scrollbar.value = 0;
    }

    public string GetPronoun(CharacterManager characterManager, bool uppercase)
    {
        if (characterManager.isNPC == false)
        {
            if (uppercase)
                return "You";
            else
                return "you";
        }
        else
            return characterManager.name;
    }

    public string GetIndefiniteArticle(string noun)
    {
        if ("aeiouAEIOU".IndexOf(noun.Substring(0, 1)) >= 0)
            return "an " + noun;
        else
            return "a " + noun;
    }

    public string GetWeaponAttackVerb(Weapon weapon)
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
            case WeaponType.Polearm:
                return "cleaved ";
            case WeaponType.Sling:
                return "shot ";
            case WeaponType.Bow:
                return "shot ";
            case WeaponType.Crossbow:
                return "shot ";
            case WeaponType.Throwing:
                return "threw and stuck ";
            default:
                return "";
        }
    }
}

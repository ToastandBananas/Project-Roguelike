using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GeneralAttackType { Unarmed, PrimaryWeapon, SecondaryWeapon, DualWield, Ranged, Throwing, Magic }

public class Attack : MonoBehaviour
{
    public int attackRange = 1;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;
    
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public int dualWieldAttackCount = 0;

    readonly int maxBlockChance = 85;

    public virtual void Start()
    {
        canAttack = true;

        gm = GameManager.instance;
        characterManager = GetComponent<CharacterManager>();
    }

    /// <summary>This function determines what type of attack the character should do.</summary>
    public virtual void DetermineAttack(Stats targetsStats)
    {
        // This is just meant to be overridden
    } 

    public virtual void DoAttack(Stats targetsStats, GeneralAttackType attackType, MeleeAttackType meleeAttackType)
    {
        characterManager.movement.FaceForward(targetsStats.transform.position);

        switch (attackType)
        {
            case GeneralAttackType.Unarmed:
                MeleeAttack(targetsStats, attackType, meleeAttackType);
                break;
            case GeneralAttackType.PrimaryWeapon:
                MeleeAttack(targetsStats, attackType, meleeAttackType);
                break;
            case GeneralAttackType.SecondaryWeapon:
                MeleeAttack(targetsStats, attackType, meleeAttackType);
                break;
            case GeneralAttackType.DualWield:
                DualWieldAttack(targetsStats, meleeAttackType);
                break;
            case GeneralAttackType.Ranged:
                break;
            case GeneralAttackType.Throwing:
                break;
            case GeneralAttackType.Magic:
                break;
            default:
                break;
        }

        StartCoroutine(AttackCooldown());
    }

    public void StartMeleeAttack(Stats targetsStats, MeleeAttackType meleeAttackType)
    {
        if (characterManager.equipmentManager.IsDualWielding())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetRightWeapon(), GeneralAttackType.DualWield, meleeAttackType));
        else if (characterManager.equipmentManager.RightWeaponEquipped())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetRightWeapon(), GeneralAttackType.PrimaryWeapon, meleeAttackType));
        else if (characterManager.equipmentManager.LeftWeaponEquipped())
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetLeftWeapon(), GeneralAttackType.SecondaryWeapon, meleeAttackType));
        else // Punch, if no weapons equipped
            StartCoroutine(UseAPAndAttack(targetsStats, null, GeneralAttackType.Unarmed, MeleeAttackType.Unarmed));
    }

    public void StartRandomMeleeAttack(Stats targetsStats)
    {
        if (characterManager.equipmentManager.IsDualWielding())
            StartMeleeAttack(targetsStats, GetRandomMeleeAttackType(characterManager.equipmentManager.GetRightWeapon().defaultMeleeAttackType));
        else if (characterManager.equipmentManager.RightWeaponEquipped())
            StartMeleeAttack(targetsStats, GetRandomMeleeAttackType(characterManager.equipmentManager.GetRightWeapon().defaultMeleeAttackType));
        else if (characterManager.equipmentManager.LeftWeaponEquipped())
            StartMeleeAttack(targetsStats, GetRandomMeleeAttackType(characterManager.equipmentManager.GetLeftWeapon().defaultMeleeAttackType));
        else // Punch, if no weapons equipped
            StartMeleeAttack(targetsStats, MeleeAttackType.Unarmed);
    }

    public void DualWieldAttack(Stats targetsStats, MeleeAttackType meleeAttackType)
    {
        if (dualWieldAttackCount == 0)
        {
            MeleeAttack(targetsStats, GeneralAttackType.PrimaryWeapon, meleeAttackType);
            dualWieldAttackCount++;
            StartCoroutine(UseAPAndAttack(targetsStats, characterManager.equipmentManager.GetLeftWeapon(), GeneralAttackType.DualWield, meleeAttackType));
        }
        else
        {
            MeleeAttack(targetsStats, GeneralAttackType.SecondaryWeapon, meleeAttackType);
            dualWieldAttackCount = 0;
        }
    }

    public void MeleeAttack(Stats targetsStats, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType)
    {
        StartCoroutine(characterManager.movement.BlockedMovement(targetsStats.transform.position));

        ItemData weaponUsedItemData = GetWeaponUsed(generalAttackType);
        Weapon weaponUsed = null;
        PhysicalDamageType physicalDamageType = PhysicalDamageType.Blunt;
        if (weaponUsedItemData != null)
        {
            weaponUsed = (Weapon)weaponUsedItemData.item;
            physicalDamageType = GetMeleeAttacksPhysicalDamageType(weaponUsed.weaponType, meleeAttackType);
        }

        int damage = GetDamage(generalAttackType);

        if (targetsStats.locomotionType != LocomotionType.Inanimate)
        {
            CharacterStats targetCharStats = (CharacterStats)targetsStats;

            if (TryEvade(targetCharStats) == false) // Check if the target evaded the attack (or the attacker missed)
            {
                if (TryBlock(targetCharStats, weaponUsedItemData, damage) == false) // Check if the target blocked the attack with a shield or a weapon
                {
                    BodyPart bodyPartToHit = targetCharStats.GetBodyPartToHit();
                    bool armorPenetrated = false;
                    bool clothingPenetrated = false;

                    // If a weapon was used (unarmed attacks don't ever crush armor)
                    // Check if the target's armor was penetrated, meaning defense values will be ignored (crushed, pierced or cut)
                    Wearable armor = null;
                    Wearable clothing = null;
                    if (weaponUsed != null && targetCharStats.characterManager.equipmentManager != null)
                    {
                        armor = GetLocationalArmor(targetCharStats.characterManager, bodyPartToHit);
                        armorPenetrated = TryPenetrateWearable(targetCharStats.characterManager, characterManager, armor, bodyPartToHit, physicalDamageType);
                        if (armor == null || armorPenetrated)
                        {
                            clothing = GetLocationalClothing(targetCharStats.characterManager, bodyPartToHit);
                            clothingPenetrated = TryPenetrateWearable(targetCharStats.characterManager, characterManager, clothing, bodyPartToHit, physicalDamageType);
                        }
                    }

                    // Damage the target character
                    int finalDamage = targetCharStats.TakeLocationalDamage(damage, bodyPartToHit, targetCharStats.characterManager.equipmentManager, armorPenetrated, clothingPenetrated);

                    // If the character is wearing armor, damage the appropriate piece(s) of armor (moreso if the armor was penetrated)
                    if (targetCharStats.characterManager.equipmentManager != null)
                        DamageLocationalArmorAndClothing(targetCharStats.characterManager, bodyPartToHit, damage, armorPenetrated, clothingPenetrated);

                    // Damage the durability of the weapon used to attack
                    if (weaponUsedItemData != null)
                        weaponUsedItemData.DamageDurability();

                    // Show some flavor text
                    if (finalDamage > 0)
                    {
                        if (armorPenetrated == false && clothingPenetrated == false)
                            gm.flavorText.WriteMeleeAttackCharacterLine(characterManager, targetCharStats.characterManager, generalAttackType, meleeAttackType, bodyPartToHit, finalDamage);
                        else if (armorPenetrated && clothingPenetrated)
                            gm.flavorText.WritePenetrateArmorAndClothingLine_Melee(characterManager, targetCharStats.characterManager, armor, clothing, generalAttackType, meleeAttackType, physicalDamageType, bodyPartToHit, finalDamage);
                        else if (armorPenetrated)
                            gm.flavorText.WritePenetrateWearableLine_Melee(characterManager, targetCharStats.characterManager, armor, generalAttackType, meleeAttackType, physicalDamageType, bodyPartToHit, finalDamage);
                        else if (clothingPenetrated)
                            gm.flavorText.WritePenetrateWearableLine_Melee(characterManager, targetCharStats.characterManager, clothing, generalAttackType, meleeAttackType, physicalDamageType, bodyPartToHit, finalDamage);
                    }
                    else
                        gm.flavorText.WriteAbsorbedMeleeAttackLine(characterManager, targetCharStats.characterManager, generalAttackType, meleeAttackType, bodyPartToHit);
                }
            }
        }
        else
        {
            // Damage the object and show flavor text
            targetsStats.TakeDamage(damage);
            gm.flavorText.WriteMeleeAttackObjectLine(characterManager, targetsStats, meleeAttackType, damage);
        }
    }

    bool TryEvade(CharacterStats targetsCharStats)
    {
        // Evade/miss chance equals the attacker's accuracy plus the target's evasion divided by 2
        float evadePlusMiss = 100 - characterManager.characterStats.meleeAccuracy.GetValue() + targetsCharStats.evasion.GetValue();
        float evadeChance = evadePlusMiss / 2;
        
        if (evadeChance > 0)
        {
            // Determine if the attack hit the target or not
            float random = Random.Range(1f, 100f);
            if (random <= evadeChance)
            {
                // Determine if the attack was evaded or if the attacker just straight up missed
                float percentChanceEvade = targetsCharStats.evasion.GetValue() / evadePlusMiss;

                random = Random.Range(0f, 1f);
                if (random > percentChanceEvade)
                {
                    // Attack missed
                    TextPopup.CreateTextStringPopup(targetsCharStats.transform.position, "Missed");
                    gm.flavorText.WriteMissedAttackLine(characterManager, targetsCharStats.characterManager);
                    return true;
                }
                else
                {
                    // Attack was evaded
                    TextPopup.CreateTextStringPopup(targetsCharStats.transform.position, "Evaded");
                    gm.flavorText.WriteEvadedAttackLine(characterManager, targetsCharStats.characterManager);
                    return true;
                }
            }
        }

        return false;
    }

    bool TryBlock(CharacterStats targetsCharStats, ItemData weaponUsedItemData, int damage)
    {
        // A character can only block if they have a weapon and/or shield equipped
        if (targetsCharStats.characterManager.equipmentManager != null && (targetsCharStats.characterManager.equipmentManager.ShieldEquipped() || targetsCharStats.characterManager.equipmentManager.MeleeWeaponEquipped()))
        {
            // Total up the block chance based off of the character's shield and/or weapon block ability and the block chance multiplier of what they have equipped
            // (Shields generally have a higher chance to block than a weapon)
            float blockChance = 0;
            float leftBlockChance = 0;
            float rightBlockChance = 0;

            // Get the left weapon/shield's block chance
            if (targetsCharStats.characterManager.equipmentManager.LeftWeaponEquipped())
            {
                if (targetsCharStats.characterManager.equipmentManager.GetEquipment(EquipmentSlot.LeftWeapon).IsShield())
                    leftBlockChance = (targetsCharStats.shieldBlock.GetValue() / 2.5f * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftWeapon).blockChanceMultiplier);
                else
                    leftBlockChance = (targetsCharStats.weaponBlock.GetValue() / 3 * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftWeapon).blockChanceMultiplier);

                blockChance += leftBlockChance;
            }

            // Get the right weapon/shield's block chance
            if (targetsCharStats.characterManager.equipmentManager.RightWeaponEquipped())
            {
                if (targetsCharStats.characterManager.equipmentManager.GetEquipment(EquipmentSlot.RightWeapon).IsShield())
                    rightBlockChance = (targetsCharStats.shieldBlock.GetValue() / 2.5f * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightWeapon).blockChanceMultiplier);
                else
                    rightBlockChance = (targetsCharStats.weaponBlock.GetValue() / 3 * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightWeapon).blockChanceMultiplier);

                blockChance += rightBlockChance;
            }

            // Cap the block chance (we don't want anyone being untouchable)
            if (blockChance > maxBlockChance)
                blockChance = maxBlockChance;

            // Determine if the attack was blocked
            float random = Random.Range(1f, 100f);
            if (random <= blockChance)
            {
                // Attack was blocked
                // Determine which shield or weapon blocked the attack and damage its durability
                float percentChanceBlockLeft = leftBlockChance / blockChance;

                random = Random.Range(0f, 1f);
                if (leftBlockChance == 0 || random > leftBlockChance)
                {
                    gm.flavorText.WriteBlockedAttackLine(characterManager, targetsCharStats.characterManager, targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightWeapon));
                    targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightWeapon).DamageDurability(damage, false);
                }
                else
                {
                    gm.flavorText.WriteBlockedAttackLine(characterManager, targetsCharStats.characterManager, targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftWeapon));
                    targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftWeapon).DamageDurability(damage, false);
                }

                if (weaponUsedItemData != null)
                    weaponUsedItemData.DamageDurability();

                TextPopup.CreateTextStringPopup(targetsCharStats.transform.position, "Blocked");
                return true;
            }
        }

        return false;
    }

    bool TryPenetrateWearable(CharacterManager target, CharacterManager attacker, Wearable wearable, BodyPart bodyPartToHit, PhysicalDamageType physicalDamageType)
    {
        if (wearable == null)
            return false;

        float random = Random.Range(1f, 100f);

        switch (wearable.mainMaterial)
        {
            case ItemMaterial.Bone:
                if (random <= GetPenetrateArmorChance(5f, 4f, 1.5f, 2f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Wood:
                if (random <= GetPenetrateArmorChance(3.75f, 6.5f, 2.5f, 2.5f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Bark:
                if (random <= GetPenetrateArmorChance(3.25f, 6f, 2f, 2.25f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Linen:
                if (random <= GetPenetrateArmorChance(7f, 9.5f, 9.5f, 8f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.QuiltedLinen:
                if (random <= GetPenetrateArmorChance(3.5f, 3.5f, 2.8f, 2.25f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Cotton:
                if (random <= GetPenetrateArmorChance(7f, 9.5f, 9.5f, 8f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Wool:
                if (random <= GetPenetrateArmorChance(8.5f, 10f, 10f, 8.5f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.QuiltedWool:
                if (random <= GetPenetrateArmorChance(3.8f, 3.8f, 4f, 2.25f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Silk:
                return true;
            case ItemMaterial.Hemp:
                if (random <= GetPenetrateArmorChance(6f, 8.5f, 8.5f, 7f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Fur:
                if (random <= GetPenetrateArmorChance(5.5f, 8f, 8f, 6.5f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Rawhide:
                if (random <= GetPenetrateArmorChance(3.2f, 3.1f, 2.9f, 2.3f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.SoftLeather:
                if (random <= GetPenetrateArmorChance(3.5f, 3.25f, 3f, 2.8f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.HardLeather:
                if (random <= GetPenetrateArmorChance(2.5f, 2.4f, 2.3f, 2.2f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Chitin:
                if (random <= GetPenetrateArmorChance(3.5f, 3f, 2f, 1.5f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Copper:
                if (random <= GetPenetrateArmorChance(3f, 3f, 2f, 2f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Bronze:
                if (random <= GetPenetrateArmorChance(2.8f, 2.8f, 1.8f, 1.8f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Iron:
                if (random <= GetPenetrateArmorChance(2.4f, 2.4f, 1.4f, 1.4f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Brass:
                if (random <= GetPenetrateArmorChance(1.8f, 1.8f, 0.8f, 0.8f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Steel:
                if (random <= GetPenetrateArmorChance(1.5f, 1.5f, 0.5f, 0.5f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Mithril:
                if (random <= GetPenetrateArmorChance(1.2f, 1.2f, 0.4f, 0.4f, attacker, physicalDamageType))
                    return true;
                break;
            case ItemMaterial.Dragonscale:
                if (random <= GetPenetrateArmorChance(0.8f, 0.8f, 0.2f, 0.2f, attacker, physicalDamageType))
                    return true;
                break;
            default:
                return false;
        }

        return false;
    }

    float GetPenetrateArmorChance(float bluntFactor, float cleaveFactor, float pierceFactor, float slashFactor, CharacterManager attacker, PhysicalDamageType physicalDamageType)
    {
        if (physicalDamageType == PhysicalDamageType.Blunt)
            return bluntFactor * (attacker.characterStats.strength.GetValue() / 4);
        else if (physicalDamageType == PhysicalDamageType.Cleave)
            return cleaveFactor * (attacker.characterStats.strength.GetValue() / 4);
        else if (physicalDamageType == PhysicalDamageType.Pierce)
            return pierceFactor * (attacker.characterStats.strength.GetValue() / 4);
        else if (physicalDamageType == PhysicalDamageType.Slash)
            return slashFactor * (attacker.characterStats.strength.GetValue() / 4);

        return 0;
    }

    int GetDamage(GeneralAttackType generalAttackType)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.Unarmed:
                return characterManager.characterStats.unarmedDamage.GetValue();
            case GeneralAttackType.PrimaryWeapon:
                return characterManager.equipmentManager.GetRightWeaponAttackDamage();
            case GeneralAttackType.SecondaryWeapon:
                return characterManager.equipmentManager.GetLeftWeaponAttackDamage();
            case GeneralAttackType.Ranged:
                return 0;
            case GeneralAttackType.Throwing:
                return 0;
            case GeneralAttackType.Magic:
                return 0;
            default:
                return 0;
        }
    }

    void DamageLocationalArmorAndClothing(CharacterManager target, BodyPart bodyPartHit, int damage, bool armorPenetrated, bool clothingPenetrated)
    {
        switch (bodyPartHit)
        {
            case BodyPart.Torso:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Cape] != null)
                {
                    int random = Random.Range(0, 100);
                    if (random < 50)
                        target.equipmentManager.currentEquipment[(int)EquipmentSlot.Cape].DamageDurability(damage, armorPenetrated);
                }

                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                {
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].DamageDurability(damage, armorPenetrated);
                    if (armorPenetrated)
                    {
                        if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                            target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].DamageDurability(damage, clothingPenetrated);
                    }
                }
                else if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].DamageDurability(damage, clothingPenetrated);
                break;
            case BodyPart.Head:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet].DamageDurability(damage, armorPenetrated);
                break;
            case BodyPart.LeftArm:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Cape] != null)
                {
                    int random = Random.Range(0, 100);
                    if (random < 33)
                        target.equipmentManager.currentEquipment[(int)EquipmentSlot.Cape].DamageDurability(damage, armorPenetrated);
                }

                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                {
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].DamageDurability(damage, armorPenetrated);
                    if (armorPenetrated)
                    {
                        if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                            target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].DamageDurability(damage, clothingPenetrated);
                    }
                }
                else if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].DamageDurability(damage, clothingPenetrated);
                break;
            case BodyPart.RightArm:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Cape] != null)
                {
                    int random = Random.Range(0, 100);
                    if (random < 33)
                        target.equipmentManager.currentEquipment[(int)EquipmentSlot.Cape].DamageDurability(damage, armorPenetrated);
                }

                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                {
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].DamageDurability(damage, armorPenetrated);
                    if (armorPenetrated)
                    {
                        if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                            target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].DamageDurability(damage, clothingPenetrated);
                    }
                }
                else if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].DamageDurability(damage, clothingPenetrated);
                break;
            case BodyPart.LeftLeg:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                {
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].DamageDurability(damage, armorPenetrated);
                    if (armorPenetrated)
                    {
                        if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                            target.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].DamageDurability(damage, clothingPenetrated);
                    }
                }
                else if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].DamageDurability(damage, clothingPenetrated);
                break;
            case BodyPart.RightLeg:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                {
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].DamageDurability(damage, armorPenetrated);
                    if (armorPenetrated)
                    {
                        if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                            target.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].DamageDurability(damage, clothingPenetrated);
                    }
                }
                else if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].DamageDurability(damage, clothingPenetrated);
                break;
            case BodyPart.LeftHand:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].DamageDurability(damage, armorPenetrated);
                break;
            case BodyPart.RightHand:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].DamageDurability(damage, armorPenetrated);
                break;
            case BodyPart.LeftFoot:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].DamageDurability(damage, armorPenetrated);
                break;
            case BodyPart.RightFoot:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].DamageDurability(damage, armorPenetrated);
                break;
            default:
                break;
        }
    }

    ItemData GetWeaponUsed(GeneralAttackType generalAttackType)
    {
        switch (generalAttackType)
        {
            case GeneralAttackType.PrimaryWeapon:
                return characterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.RightWeapon];
            case GeneralAttackType.SecondaryWeapon:
                return characterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftWeapon];
            case GeneralAttackType.Ranged:
                return characterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Ranged];
            case GeneralAttackType.Throwing:
                return characterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Ranged];
            default:
                return null;
        }
    }

    Wearable GetLocationalArmor(CharacterManager targetsCharacterManager, BodyPart bodyPartToHit)
    {
        if (targetsCharacterManager.equipmentManager == null)
            return null;

        switch (bodyPartToHit)
        {
            case BodyPart.Torso:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].item;
                break;
            case BodyPart.Head:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet].item;
                break;
            case BodyPart.LeftArm:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].item;
                break;
            case BodyPart.RightArm:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].item;
                break;
            case BodyPart.LeftLeg:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].item;
                break;
            case BodyPart.RightLeg:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].item;
                break;
            case BodyPart.LeftHand:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].item;
                break;
            case BodyPart.RightHand:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].item;
                break;
            case BodyPart.LeftFoot:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].item;
                break;
            case BodyPart.RightFoot:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].item;
                break;
            default:
                return null;
        }

        return null;
    }

    Wearable GetLocationalClothing(CharacterManager targetsCharacterManager, BodyPart bodyPartToHit)
    {
        if (targetsCharacterManager.equipmentManager == null)
            return null;

        switch (bodyPartToHit)
        {
            case BodyPart.Torso:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].item;
                break;
            case BodyPart.LeftArm:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].item;
                break;
            case BodyPart.RightArm:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].item;
                break;
            case BodyPart.LeftLeg:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].item;
                break;
            default:
                return null;
        }

        return null;
    }

    public IEnumerator UseAPAndAttack(Stats targetsStats, Weapon weapon, GeneralAttackType attackType, MeleeAttackType meleeAttackType)
    {
        characterManager.actionQueued = true;

        while (characterManager.isMyTurn == false || characterManager.movement.isMoving || canAttack == false)
        {
            yield return null;
        }

        if (TargetInAttackRange(targetsStats.transform) == false || targetsStats.isDeadOrDestroyed)
        {
            CancelAttack();
            yield break;
        }

        if (characterManager.remainingAPToBeUsed > 0)
        {
            if (characterManager.remainingAPToBeUsed <= characterManager.characterStats.currentAP)
            {
                characterManager.actionQueued = false;
                characterManager.characterStats.UseAP(characterManager.remainingAPToBeUsed);
                DoAttack(targetsStats, attackType, meleeAttackType);
            }
            else
            {
                characterManager.characterStats.UseAP(characterManager.characterStats.currentAP);
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndAttack(targetsStats, weapon, attackType, meleeAttackType));
            }
        }
        else
        {
            int remainingAP = characterManager.characterStats.UseAPAndGetRemainder(gm.apManager.GetAttackAPCost(characterManager, weapon, attackType));
            if (remainingAP == 0)
            {
                characterManager.actionQueued = false;
                DoAttack(targetsStats, attackType, meleeAttackType);
            }
            else
            {
                characterManager.remainingAPToBeUsed = remainingAP;
                gm.turnManager.FinishTurn(characterManager);
                StartCoroutine(UseAPAndAttack(targetsStats, weapon, attackType, meleeAttackType));
            }
        }
    }

    public bool TargetInAttackRange(Transform target)
    {
        int distX = Mathf.RoundToInt(Mathf.Abs(transform.position.x - target.position.x));
        int distY = Mathf.RoundToInt(Mathf.Abs(transform.position.y - target.position.y));

        if (distX <= attackRange && distY <= attackRange)
            return true;

        return false;
    }

    public void CancelAttack()
    {
        dualWieldAttackCount = 0;
        characterManager.remainingAPToBeUsed = 0;
        characterManager.actionQueued = false;
        if (characterManager.isNPC && characterManager.characterStats.isDeadOrDestroyed == false)
            characterManager.TakeTurn();
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.4f);
        canAttack = true;
    }

    public PhysicalDamageType GetMeleeAttacksPhysicalDamageType(WeaponType weaponType, MeleeAttackType meleeAttackType)
    {
        switch (weaponType)
        {
            case WeaponType.Sword:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Slash;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Cleave;
            case WeaponType.Dagger:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Slash;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Pierce;
            case WeaponType.Axe:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Cleave;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Cleave;
            case WeaponType.SpikedAxe:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Cleave;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Cleave;
            case WeaponType.Club:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.SpikedClub:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.Mace:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.SpikedMace:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.Hammer:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.SpikedHammer:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.Flail:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.SpikedFlail:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Pierce;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Pierce;
            case WeaponType.Staff:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.Spear:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Slash;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Slash;
            case WeaponType.Polearm:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Cleave;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Cleave;
            case WeaponType.BluntPolearm:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.Sling:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.Bow:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.Crossbow:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            case WeaponType.ThrowingKnife:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Slash;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Pierce;
            case WeaponType.ThrowingAxe:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Cleave;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Cleave;
            case WeaponType.ThrowingStar:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Slash;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Pierce;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Slash;
            case WeaponType.ThrowingClub:
                if (meleeAttackType == MeleeAttackType.Swipe)
                    return PhysicalDamageType.Blunt;
                else if (meleeAttackType == MeleeAttackType.Thrust)
                    return PhysicalDamageType.Blunt;
                else // if (meleeAttackType == MeleeAttackType.Overhead)
                    return PhysicalDamageType.Blunt;
            default:
                return PhysicalDamageType.Blunt;
        }
    }

    public MeleeAttackType GetRandomMeleeAttackType(MeleeAttackType defaultMeleeAttackType)
    {
        int random = Random.Range(0, 100);
        if (random < 50)
            return defaultMeleeAttackType;
        else if (random < 75)
        {
            if (MeleeAttackType.Swipe != defaultMeleeAttackType)
                return MeleeAttackType.Swipe;
            else
                return MeleeAttackType.Thrust;
        }
        else
        {
            if (MeleeAttackType.Overhead != defaultMeleeAttackType)
                return MeleeAttackType.Overhead;
            else
                return MeleeAttackType.Thrust;
        }
    }

    /*public void ShowWeaponTrail(HeldWeapon heldWeapon)
    {
        WeaponTrail weaponTrail = weaponTrailObjectPool.GetPooledObject().GetComponent<WeaponTrail>();
        weaponTrail.gameObject.SetActive(true);
        
        weaponTrail.ShowChopTrail(heldWeapon, characterManager.transform);
    }*/
}

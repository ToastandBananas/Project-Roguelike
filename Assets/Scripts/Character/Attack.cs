using System.Collections;
using UnityEngine;

public enum GeneralAttackType { Unarmed, PrimaryWeapon, SecondaryWeapon, DualWield, Ranged, Throwing, Magic }

public class Attack : MonoBehaviour
{
    public int attackRange = 1;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;

    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public int dualWieldAttackCount = 0;

    readonly int maxBlockChance = 85;
    bool cancellingAttacking;

    public virtual void Start()
    {
        canAttack = true;

        gm = GameManager.instance;
        characterManager = GetComponent<CharacterManager>();
    }

    /// <summary>This function determines what type of attack the character should do.</summary>
    public virtual void DetermineAttack(CharacterManager targetsCharacterManager, Stats targetsStats)
    {
        // This is just meant to be overridden
    } 

    public virtual IEnumerator DoAttack(CharacterManager targetsCharacterManager, Stats targetsStats, Weapon weapon, GeneralAttackType attackType, MeleeAttackType meleeAttackType)
    {
        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetAttackAPCost(characterManager, weapon, attackType)));
        isAttacking = true;

        int queueNumber = characterManager.currentQueueNumber + characterManager.actionsQueued;
        while (queueNumber != characterManager.currentQueueNumber || canAttack == false)
        {
            yield return null;
            if (cancellingAttacking)
            {
                cancellingAttacking = false;
                characterManager.TakeTurn();
                yield break;
            }
        }

        if ((targetsCharacterManager != null && targetsCharacterManager.status.isDead) || targetsStats.isDestroyed || TargetInAttackRange(targetsStats.transform) == false)
        {
            characterManager.TakeTurn();
            yield break;
        }

        switch (attackType)
        {
            case GeneralAttackType.Unarmed:
                MeleeAttack(targetsCharacterManager, targetsStats, attackType, meleeAttackType);
                break;
            case GeneralAttackType.PrimaryWeapon:
                MeleeAttack(targetsCharacterManager, targetsStats, attackType, meleeAttackType);
                break;
            case GeneralAttackType.SecondaryWeapon:
                MeleeAttack(targetsCharacterManager, targetsStats, attackType, meleeAttackType);
                break;
            case GeneralAttackType.DualWield:
                DualWieldAttack(targetsCharacterManager, targetsStats, meleeAttackType);
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

        isAttacking = false;
        StartCoroutine(AttackCooldown());
    }

    public void StartMeleeAttack(CharacterManager targetsCharacterManager, Stats targetsStats, MeleeAttackType meleeAttackType)
    {
        if (characterManager.equipmentManager.IsDualWielding())
            StartCoroutine(DoAttack(targetsCharacterManager, targetsStats, characterManager.equipmentManager.GetRightWeapon(), GeneralAttackType.DualWield, meleeAttackType));
        else if (characterManager.equipmentManager.RightHandItemEquipped())
            StartCoroutine(DoAttack(targetsCharacterManager, targetsStats, characterManager.equipmentManager.GetRightWeapon(), GeneralAttackType.PrimaryWeapon, meleeAttackType));
        else if (characterManager.equipmentManager.LeftHandItemEquipped())
            StartCoroutine(DoAttack(targetsCharacterManager, targetsStats, characterManager.equipmentManager.GetLeftWeapon(), GeneralAttackType.SecondaryWeapon, meleeAttackType));
        else // Punch, if no weapons equipped
            StartCoroutine(DoAttack(targetsCharacterManager, targetsStats, null, GeneralAttackType.Unarmed, meleeAttackType));
    }

    public void StartRandomMeleeAttack(CharacterManager targetsCharacterManager, Stats targetsStats)
    {
        if (characterManager.equipmentManager.IsDualWielding())
            StartMeleeAttack(targetsCharacterManager, targetsStats, GetRandomMeleeAttackType(characterManager.equipmentManager.GetRightWeapon().defaultMeleeAttackType));
        else if (characterManager.equipmentManager.RightHandItemEquipped())
            StartMeleeAttack(targetsCharacterManager, targetsStats, GetRandomMeleeAttackType(characterManager.equipmentManager.GetRightWeapon().defaultMeleeAttackType));
        else if (characterManager.equipmentManager.LeftHandItemEquipped())
            StartMeleeAttack(targetsCharacterManager, targetsStats, GetRandomMeleeAttackType(characterManager.equipmentManager.GetLeftWeapon().defaultMeleeAttackType));
        else // Punch, if no weapons equipped
            StartMeleeAttack(targetsCharacterManager, targetsStats, MeleeAttackType.Unarmed);
    }

    public void DualWieldAttack(CharacterManager targetsCharacterManager, Stats targetsStats, MeleeAttackType meleeAttackType)
    {
        if (dualWieldAttackCount == 0)
        {
            // Do a right weapon attack
            MeleeAttack(targetsCharacterManager, targetsStats, GeneralAttackType.PrimaryWeapon, meleeAttackType);
            dualWieldAttackCount++;
            
            // Now start up the left weapon attack
            StartCoroutine(DoAttack(targetsCharacterManager, targetsStats, characterManager.equipmentManager.GetLeftWeapon(), GeneralAttackType.SecondaryWeapon, meleeAttackType));
        }
        else
        {
            // Do a right weapon attack
            MeleeAttack(targetsCharacterManager, targetsStats, GeneralAttackType.SecondaryWeapon, meleeAttackType);
            dualWieldAttackCount = 0;
        }
    }

    public void MeleeAttack(CharacterManager targetsCharacterManager, Stats targetsStats, GeneralAttackType generalAttackType, MeleeAttackType meleeAttackType)
    {
        StartCoroutine(characterManager.movement.BlockedMovement(targetsStats.transform.position));

        ItemData weaponUsedItemData = GetWeaponUsed(generalAttackType);
        Weapon weaponUsed = null;
        PhysicalDamageType mainPhysicalDamageType = PhysicalDamageType.Blunt;
        if (weaponUsedItemData != null)
        {
            weaponUsed = (Weapon)weaponUsedItemData.item;
            mainPhysicalDamageType = weaponUsedItemData.GetMeleeAttacksPhysicalDamageType(meleeAttackType);
        }

        int bluntDamage = characterManager.equipmentManager.GetPhysicalMeleeDamage(weaponUsedItemData, meleeAttackType, PhysicalDamageType.Blunt);
        int pierceDamage = characterManager.equipmentManager.GetPhysicalMeleeDamage(weaponUsedItemData, meleeAttackType, PhysicalDamageType.Pierce);
        int slashDamage = characterManager.equipmentManager.GetPhysicalMeleeDamage(weaponUsedItemData, meleeAttackType, PhysicalDamageType.Slash);
        int cleaveDamage = characterManager.equipmentManager.GetPhysicalMeleeDamage(weaponUsedItemData, meleeAttackType, PhysicalDamageType.Cleave);
        int totalDamage = bluntDamage + pierceDamage + slashDamage + cleaveDamage;

        if (targetsCharacterManager != null) // If the character is attacking another character
        {
            CharacterStats targetCharStats = (CharacterStats)targetsStats;

            if (TryEvade(targetCharStats) == false) // Check if the target evaded the attack (or the attacker missed)
            {
                if (TryBlock(targetCharStats, weaponUsedItemData, totalDamage) == false) // Check if the target blocked the attack with a shield or a weapon
                {
                    BodyPartType bodyPartToHit = targetsCharacterManager.status.GetBodyPartToHit();
                    bool armorPenetrated = false;
                    bool clothingPenetrated = false;

                    // If a weapon was used (unarmed attacks don't ever crush armor)
                    // Check if the target's armor was penetrated, meaning defense values will be ignored (crushed, pierced or cut)
                    Wearable armor = null;
                    Wearable clothing = null;
                    if (weaponUsed != null && targetCharStats.characterManager.equipmentManager != null)
                    {
                        // See if the attack penetrates the character's armor
                        armor = GetLocationalArmor(targetCharStats.characterManager, bodyPartToHit);
                        armorPenetrated = TryPenetrateWearable(targetCharStats.characterManager, characterManager, armor, bodyPartToHit, mainPhysicalDamageType);

                        // If the character isn't wearing armor or their armor was penetrated
                        if (armor == null || armorPenetrated)
                        {
                            // See if the attack penetrates the character's clothing
                            clothing = GetLocationalClothing(targetCharStats.characterManager, bodyPartToHit);
                            clothingPenetrated = TryPenetrateWearable(targetCharStats.characterManager, characterManager, clothing, bodyPartToHit, mainPhysicalDamageType);
                        }
                    }

                    // Damage the target character
                    bool behindTarget = characterManager.movement.IsBehindCharacter(targetsCharacterManager);
                    int finalDamage = targetsCharacterManager.status.TakeLocationalDamage(characterManager, bluntDamage, pierceDamage, slashDamage, cleaveDamage, behindTarget, bodyPartToHit, targetCharStats.characterManager.equipmentManager, armor, clothing, armorPenetrated, clothingPenetrated);

                    // If the character is wearing armor, damage the appropriate piece(s) of armor (moreso if the armor was penetrated)
                    if (targetCharStats.characterManager.equipmentManager != null)
                        DamageLocationalArmorAndClothing(targetCharStats.characterManager, bodyPartToHit, totalDamage, armorPenetrated, clothingPenetrated);

                    // If a weapon was used, damage the durability of the weapon used to attack
                    if (weaponUsedItemData != null)
                        weaponUsedItemData.DamageDurability();

                    // Show some flavor text
                    if (finalDamage > 0)
                    {
                        if (armorPenetrated == false && clothingPenetrated == false)
                            gm.flavorText.WriteMeleeAttackCharacterLine(characterManager, targetCharStats.characterManager, generalAttackType, meleeAttackType, bodyPartToHit, finalDamage, behindTarget);
                        else if (armorPenetrated && clothingPenetrated)
                            gm.flavorText.WritePenetrateArmorAndClothingLine_Melee(characterManager, targetCharStats.characterManager, armor, clothing, generalAttackType, meleeAttackType, mainPhysicalDamageType, bodyPartToHit, finalDamage, behindTarget);
                        else if (armorPenetrated)
                            gm.flavorText.WritePenetrateWearableLine_Melee(characterManager, targetCharStats.characterManager, armor, generalAttackType, meleeAttackType, mainPhysicalDamageType, bodyPartToHit, finalDamage, behindTarget);
                        else if (clothingPenetrated)
                            gm.flavorText.WritePenetrateWearableLine_Melee(characterManager, targetCharStats.characterManager, clothing, generalAttackType, meleeAttackType, mainPhysicalDamageType, bodyPartToHit, finalDamage, behindTarget);

                        // See if the weapon will stick in the opponent's flesh
                        if (weaponUsedItemData != null && weaponUsedItemData.durability > 0 && (armor == null || armorPenetrated) && (clothing == null || clothingPenetrated))
                        {
                            // This can't happen with blunt damage only weapons
                            if (pierceDamage > 0 || slashDamage > 0 || cleaveDamage > 0)
                                TryGetWeaponStuck(targetsStats, targetsCharacterManager, weaponUsedItemData, mainPhysicalDamageType, pierceDamage, slashDamage, cleaveDamage, bodyPartToHit);
                        }
                    }
                    else
                        gm.flavorText.WriteAbsorbedMeleeAttackLine(characterManager, targetCharStats.characterManager, generalAttackType, meleeAttackType, bodyPartToHit, behindTarget);
                }
            }
        }
        else // If the character is attacking an object
        {
            // Damage the object and show flavor text
            targetsStats.TakeDamage(totalDamage);
            gm.flavorText.WriteMeleeAttackObjectLine(characterManager, targetsStats, generalAttackType, meleeAttackType, totalDamage);
            TryGetWeaponStuck(targetsStats, null, weaponUsedItemData, mainPhysicalDamageType, pierceDamage, slashDamage, cleaveDamage);
        }
    }

    bool TryGetWeaponStuck(Stats targetsStats, CharacterManager target, ItemData weaponUsedItemData, PhysicalDamageType mainPhysicalDamageType, int pierceDamage, int slashDamage, int cleaveDamage, BodyPartType bodyPartHit = BodyPartType.Torso)
    {
        // A weapon should only get stuck in a character's head if they died from the hit, otherwise it would be unrealistic to have a weapon get lodged into their head and still have them live
        if ((target != null && bodyPartHit == BodyPartType.Head && target.status.isDead == false) || (target == null && targetsStats.isDestroyed))
            return false;

        int maxHealth = 0;
        if (target != null)
            maxHealth = target.status.GetBodyPart(bodyPartHit).maxHealth.GetValue();
        else
            maxHealth = targetsStats.maxHealth.GetValue();

        float random = Random.Range(0f, 1f);
        float percentDamage = 0f;
        float stickChance = 0f;
        // Debug.Log("Pierce: " + pierceDamage + " | Slash: " + slashDamage + " | Cleave: " + cleaveDamage);

        // Chance to stick is the percent damage done, compared to the body part's max health, times 2
        if (mainPhysicalDamageType == PhysicalDamageType.Pierce)
            percentDamage = (float)pierceDamage / maxHealth;
        else if (mainPhysicalDamageType == PhysicalDamageType.Cleave)
            percentDamage = (float)cleaveDamage / maxHealth;
        else
            percentDamage = (float)slashDamage / maxHealth;

        if (percentDamage <= 0.1f)
            return false;

        stickChance = percentDamage * 2f;
        
        if (random <= stickChance) // If stuck
        {
            // Use up AP until the character can pull the weapon out
            if (target != null)
                StartCoroutine(UnstickWeapon(target, bodyPartHit, weaponUsedItemData, mainPhysicalDamageType, Mathf.RoundToInt(percentDamage * maxHealth), percentDamage)); // If stuck in NPC
            else
                StartCoroutine(UnstickWeapon(targetsStats, weaponUsedItemData, mainPhysicalDamageType, Mathf.RoundToInt(percentDamage * maxHealth), percentDamage)); // If stuck in an object

            // The target will lose a little bit of AP, based off of damage done
            if (target != null)
            {
                gm.apManager.LoseAP(target, gm.apManager.GetStuckWithWeaponAPLoss(target, percentDamage));
                gm.flavorText.WriteStickWeaponLine(characterManager, target, bodyPartHit, weaponUsedItemData);
            }
            else
                gm.flavorText.WriteStickWeaponLine(characterManager, targetsStats, weaponUsedItemData);

            return true;
        }

        return false;
    }

    IEnumerator UnstickWeapon(CharacterManager target, BodyPartType bodyPartStuck, ItemData weaponUsedItemData, PhysicalDamageType mainPhysicalDamageType, int damage, float percentDamage)
    {
        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetWeaponStickAPCost(characterManager, (Weapon)weaponUsedItemData.item, mainPhysicalDamageType, percentDamage)));

        int queueNumber = characterManager.currentQueueNumber + characterManager.actionsQueued;
        while (queueNumber != characterManager.currentQueueNumber)
        {
            yield return null;
            if (characterManager.status.isDead) yield break;
        }

        int newDamage = 0;
        if (target.status.isDead == false)
        {
            target.movement.canMove = true;

            // Damage the target a little more upon removing the weapon
            newDamage = Mathf.RoundToInt(damage * Random.Range(0.2f, 0.4f));
            target.status.TakeLocationalDamage_IgnoreArmor(newDamage, bodyPartStuck);
        }

        // The target also loses a spurt of blood
        target.status.LoseBlood(100f * percentDamage);

        // Write some flavor text
        gm.flavorText.WriteUnstickWeaponLine(characterManager, target, bodyPartStuck, weaponUsedItemData, newDamage);
    }

    IEnumerator UnstickWeapon(Stats targetsStats, ItemData weaponUsedItemData, PhysicalDamageType mainPhysicalDamageType, int damage, float percentDamage)
    {
        StartCoroutine(gm.apManager.UseAP(characterManager, gm.apManager.GetWeaponStickAPCost(characterManager, (Weapon)weaponUsedItemData.item, mainPhysicalDamageType, percentDamage)));

        int queueNumber = characterManager.currentQueueNumber + characterManager.actionsQueued;
        while (queueNumber != characterManager.currentQueueNumber)
        {
            yield return null;
            if (characterManager.status.isDead) yield break;
        }
        
        gm.flavorText.WriteUnstickWeaponLine(characterManager, targetsStats, weaponUsedItemData, damage);
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
            if (targetsCharStats.characterManager.equipmentManager.LeftHandItemEquipped())
            {
                if (targetsCharStats.characterManager.equipmentManager.GetEquipment(EquipmentSlot.LeftHandItem).IsShield())
                    leftBlockChance = (targetsCharStats.shieldBlock.GetValue() / 2.5f * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftHandItem).blockChanceMultiplier);
                else
                    leftBlockChance = (targetsCharStats.weaponBlock.GetValue() / 3 * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftHandItem).blockChanceMultiplier);

                blockChance += leftBlockChance;
            }

            // Get the right weapon/shield's block chance
            if (targetsCharStats.characterManager.equipmentManager.RightHandItemEquipped())
            {
                if (targetsCharStats.characterManager.equipmentManager.GetEquipment(EquipmentSlot.RightHandItem).IsShield())
                    rightBlockChance = (targetsCharStats.shieldBlock.GetValue() / 2.5f * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightHandItem).blockChanceMultiplier);
                else
                    rightBlockChance = (targetsCharStats.weaponBlock.GetValue() / 3 * targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightHandItem).blockChanceMultiplier);

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
                    gm.flavorText.WriteBlockedAttackLine(characterManager, targetsCharStats.characterManager, targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightHandItem));
                    targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.RightHandItem).DamageDurability(damage, false);
                }
                else
                {
                    gm.flavorText.WriteBlockedAttackLine(characterManager, targetsCharStats.characterManager, targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftHandItem));
                    targetsCharStats.characterManager.equipmentManager.GetEquipmentsItemData(EquipmentSlot.LeftHandItem).DamageDurability(damage, false);
                }

                if (weaponUsedItemData != null)
                    weaponUsedItemData.DamageDurability();

                TextPopup.CreateTextStringPopup(targetsCharStats.transform.position, "Blocked");
                return true;
            }
        }

        return false;
    }

    bool TryPenetrateWearable(CharacterManager target, CharacterManager attacker, Wearable wearable, BodyPartType bodyPartToHit, PhysicalDamageType physicalDamageType)
    {
        if (wearable == null)
            return false;

        float random = Random.Range(1f, 100f);

        switch (wearable.mainMaterial)
        {
            case ItemMaterial.Bone:
                return random <= GetPenetrateArmorChance(5f, 4f, 1.5f, 2f, attacker, physicalDamageType);
            case ItemMaterial.Wood:
                return random <= GetPenetrateArmorChance(3.75f, 6.5f, 2.5f, 2.5f, attacker, physicalDamageType);
            case ItemMaterial.Bark:
                return random <= GetPenetrateArmorChance(3.25f, 6f, 2f, 2.25f, attacker, physicalDamageType);
            case ItemMaterial.Linen:
                return random <= GetPenetrateArmorChance(7f, 9.5f, 9.5f, 9.5f, attacker, physicalDamageType);
            case ItemMaterial.QuiltedLinen:
                return random <= GetPenetrateArmorChance(3.5f, 3.5f, 2.8f, 2.25f, attacker, physicalDamageType);
            case ItemMaterial.Cotton:
                return random <= GetPenetrateArmorChance(7f, 9.5f, 9.5f, 9.5f, attacker, physicalDamageType);
            case ItemMaterial.Wool:
                return random <= GetPenetrateArmorChance(8.5f, 10f, 10f, 8.5f, attacker, physicalDamageType);
            case ItemMaterial.QuiltedWool:
                return random <= GetPenetrateArmorChance(3.8f, 3.8f, 4f, 2.25f, attacker, physicalDamageType);
            case ItemMaterial.Silk:
                return true; // Silk provides no penetration protection
            case ItemMaterial.Hemp:
                return random <= GetPenetrateArmorChance(6f, 8.5f, 8.5f, 7f, attacker, physicalDamageType);
            case ItemMaterial.Fur:
                return random <= GetPenetrateArmorChance(5.5f, 8f, 8f, 7f, attacker, physicalDamageType);
            case ItemMaterial.Rawhide:
                return random <= GetPenetrateArmorChance(3.2f, 3.1f, 2.9f, 2.3f, attacker, physicalDamageType);
            case ItemMaterial.SoftLeather:
                return random <= GetPenetrateArmorChance(3.5f, 3.25f, 3f, 2.8f, attacker, physicalDamageType);
            case ItemMaterial.HardLeather:
                return random <= GetPenetrateArmorChance(2.5f, 2.4f, 2.3f, 2.2f, attacker, physicalDamageType);
            case ItemMaterial.Chitin:
                return random <= GetPenetrateArmorChance(3.5f, 3f, 2f, 1.5f, attacker, physicalDamageType);
            case ItemMaterial.Copper:
                return random <= GetPenetrateArmorChance(3f, 3f, 2f, 2f, attacker, physicalDamageType);
            case ItemMaterial.Bronze:
                return random <= GetPenetrateArmorChance(2.8f, 2.8f, 1.8f, 1.8f, attacker, physicalDamageType);
            case ItemMaterial.Iron:
                return random <= GetPenetrateArmorChance(2.4f, 2.4f, 1.4f, 1.4f, attacker, physicalDamageType);
            case ItemMaterial.Brass:
                return random <= GetPenetrateArmorChance(1.8f, 1.8f, 0.8f, 0.8f, attacker, physicalDamageType);
            case ItemMaterial.Steel:
                return random <= GetPenetrateArmorChance(1.5f, 1.5f, 0.5f, 0.5f, attacker, physicalDamageType);
            case ItemMaterial.Mithril:
                return random <= GetPenetrateArmorChance(1.2f, 1.2f, 0.4f, 0.4f, attacker, physicalDamageType);
            case ItemMaterial.Dragonscale:
                return random <= GetPenetrateArmorChance(0.8f, 0.8f, 0.2f, 0.2f, attacker, physicalDamageType);
            default:
                return false;
        }
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

    void DamageLocationalArmorAndClothing(CharacterManager target, BodyPartType bodyPartHit, int damage, bool armorPenetrated, bool clothingPenetrated)
    {
        switch (bodyPartHit)
        {
            case BodyPartType.Torso:
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
            case BodyPartType.Head:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet].DamageDurability(damage, armorPenetrated);
                break;
            case BodyPartType.LeftArm:
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
            case BodyPartType.RightArm:
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
            case BodyPartType.LeftLeg:
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
            case BodyPartType.RightLeg:
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
            case BodyPartType.LeftHand:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].DamageDurability(damage, armorPenetrated);
                break;
            case BodyPartType.RightHand:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].DamageDurability(damage, armorPenetrated);
                break;
            case BodyPartType.LeftFoot:
                if (target.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    target.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].DamageDurability(damage, armorPenetrated);
                break;
            case BodyPartType.RightFoot:
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
                return characterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.RightHandItem];
            case GeneralAttackType.SecondaryWeapon:
                return characterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LeftHandItem];
            case GeneralAttackType.Ranged:
                return characterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Ranged];
            case GeneralAttackType.Throwing:
                return characterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Ranged];
            default:
                return null;
        }
    }

    Wearable GetLocationalArmor(CharacterManager targetsCharacterManager, BodyPartType bodyPartToHit)
    {
        if (targetsCharacterManager.equipmentManager == null)
            return null;

        switch (bodyPartToHit)
        {
            case BodyPartType.Torso:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].item;
                break;
            case BodyPartType.Head:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Helmet].item;
                break;
            case BodyPartType.LeftArm:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].item;
                break;
            case BodyPartType.RightArm:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].item;
                break;
            case BodyPartType.LeftLeg:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].item;
                break;
            case BodyPartType.RightLeg:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.LegArmor].item;
                break;
            case BodyPartType.LeftHand:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].item;
                break;
            case BodyPartType.RightHand:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].item;
                break;
            case BodyPartType.LeftFoot:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].item;
                break;
            case BodyPartType.RightFoot:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Boots].item;
                break;
            default:
                return null;
        }

        return null;
    }

    Wearable GetLocationalClothing(CharacterManager targetsCharacterManager, BodyPartType bodyPartToHit)
    {
        if (targetsCharacterManager.equipmentManager == null)
            return null;

        switch (bodyPartToHit)
        {
            case BodyPartType.Torso:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].item;
                break;
            case BodyPartType.LeftArm:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].item;
                break;
            case BodyPartType.RightArm:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].item;
                break;
            case BodyPartType.LeftLeg:
                if (targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants] != null)
                    return (Wearable)targetsCharacterManager.equipmentManager.currentEquipment[(int)EquipmentSlot.Pants].item;
                break;
            default:
                return null;
        }

        return null;
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
        if (isAttacking)
        {
            Debug.Log("Cancelling attack...");
            cancellingAttacking = true;
            dualWieldAttackCount = 0;
            characterManager.EditActionsQueued(-1);
            characterManager.currentQueueNumber++;
            characterManager.TakeTurn();
        }
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.4f);
        canAttack = true;
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

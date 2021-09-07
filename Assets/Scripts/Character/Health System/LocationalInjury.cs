using System.Collections;
using UnityEngine;

[System.Serializable]
public class LocationalInjury
{
    public Injury injury;
    public BodyPartType injuryLocation;
    public bool onBackOfBodyPart;
    public float injuryHealMultiplier = 1f;

    public bool sterilized;

    public ItemData bandageItemData;
    public MedicalSupply bandage;

    public int injuryTimeRemaining;

    public float damagePerTurn;
    public int bleedTimeRemaining;
    public float bloodLossPerTurn;

    public float agilityModifier;
    public float dexterityModifier;
    public float speedModifier;

    [HideInInspector] public CharacterManager characterManager;

    public LocationalInjury(CharacterManager characterManager, Injury injury, BodyPartType injuryLocation, bool onBackOfBodyPart)
    {
        this.characterManager = characterManager;
        this.injury = injury;
        this.injuryLocation = injuryLocation;
        this.onBackOfBodyPart = onBackOfBodyPart;
        SetupInjuryVariables();
    }

    void SetupInjuryVariables()
    {
        damagePerTurn = Mathf.RoundToInt(Random.Range(injury.damagePerTurn.x, injury.damagePerTurn.y) * 100f) / 100f;

        injuryTimeRemaining = Random.Range(TimeSystem.GetTotalSeconds(injury.minInjuryHealTime), TimeSystem.GetTotalSeconds(injury.maxInjuryHealTime) + 1);

        Vector2Int bleedTimes = injury.BleedTime();
        if (bleedTimes.y > 0)
            bleedTimeRemaining = Random.Range(bleedTimes.x, bleedTimes.y + 1);

        Vector2 bloodLossValues = injury.BloodLossPerTurn();
        if (bloodLossValues.y > 0)
            bloodLossPerTurn = Random.Range(bloodLossValues.x, bloodLossValues.y);

        if (injury.agilityModifier.y != 0)
        {
            if (injury.agilityMod_LowerBodyOnly == false || InjuryLocatedOnLowerBody())
            {
                agilityModifier = (Random.Range(injury.agilityModifier.x, injury.agilityModifier.y) / 100f) * characterManager.characterStats.agility.GetBaseValue();
                characterManager.characterStats.AdjustTotalAgilityMods(agilityModifier);
            }
        }

        if (injury.dexterityModifier.y != 0)
        {
            if (injury.dexterityMod_ArmsOnly == false || InjuryLocatedOnArms())
            {
                dexterityModifier = (Random.Range(injury.dexterityModifier.x, injury.dexterityModifier.y) / 100f) * characterManager.characterStats.dexterity.GetBaseValue();
                characterManager.characterStats.AdjustTotalDexterityMods(dexterityModifier);
            }
        }

        if (injury.speedModifier.y != 0)
        {
            if (injury.speedMod_LowerBodyOnly == false || InjuryLocatedOnLowerBody())
            {
                speedModifier = (Random.Range(injury.speedModifier.x, injury.speedModifier.y) / 100f) * characterManager.characterStats.speed.GetBaseValue();
                characterManager.characterStats.AdjustTotalSpeedMods(speedModifier);
            }
        }
    }

    public IEnumerator ApplyMedicalItem(CharacterManager characterApplying, ItemData itemData, Inventory inventory, InventoryItem invItem)
    {
        if (characterApplying.status.isDead)
            yield break;
        else if (characterManager.status.isDead)
        {
            characterApplying.FinishAction();
            yield break;
        }
        // If the someone else is applying the bandage, lose AP so that this character can't try to perform any actions
        else if(characterApplying != characterManager)
            APManager.instance.LoseAP(characterManager, APManager.instance.GetApplyMedicalItemAPCost((MedicalSupply)itemData.item));

        if (CanApplyBandage(itemData))
            ApplyBandage(characterApplying, itemData);

        itemData.item.Use(characterApplying, inventory, invItem, itemData, 1);

        // If this is the player, update the Health Display
        if (characterManager.isNPC == false)
        {
            HealthDisplay.instance.UpdateHealthHeaderColor(injuryLocation);
            HealthDisplay.instance.UpdateTooltip();
        }

        characterApplying.FinishAction();
    }

    void ApplyBandage(CharacterManager characterApplying, ItemData bandageItemData)
    {
        // Create new ItemData object for the bandage
        ItemData newItemData = UIManager.instance.CreateNewItemDataChild(bandageItemData, null, characterManager.appliedItemsParent, false);

        this.bandageItemData = newItemData;
        this.bandageItemData.currentStackSize = 1;
        bandage = (MedicalSupply)newItemData.item;
        injuryHealMultiplier += bandage.quality;

        if (PlayerManager.instance.CanSee(characterApplying.spriteRenderer))
            FlavorText.instance.WriteLine_ApplyBandage(characterApplying, characterManager, injury, newItemData, injuryLocation);
    }

    public IEnumerator RemoveMedicalItem(CharacterManager characterRemoving, MedicalSupplyType medicalSupplyType)
    {
        if (characterRemoving.status.isDead)
            yield break;
        else if (characterManager.status.isDead)
        {
            characterRemoving.FinishAction();
            yield break;
        }
        // If the someone else is removing the bandage, lose AP so that this character can't try to perform any actions
        else if (characterRemoving != characterManager)
            APManager.instance.LoseAP(characterManager, APManager.instance.GetRemoveMedicalItemAPCost(bandage));

        if (medicalSupplyType == MedicalSupplyType.Bandage)
            RemoveBandage(characterRemoving);

        if (characterManager.isNPC == false)
        {
            HealthDisplay.instance.UpdateHealthHeaderColor(injuryLocation);
            HealthDisplay.instance.UpdateTooltip();
        }

        characterRemoving.FinishAction();
    }

    public void RemoveBandage(CharacterManager characterRemoving)
    {
        if (bandage == null) return;

        injuryHealMultiplier -= bandage.quality;

        // Try to place the bandage in one of the player's inventories. If it won't fit, drop it
        if (characterRemoving.TryAddingItemToInventory(bandageItemData, null, false) == false)
            DropItemController.instance.ForceDropNearest(characterRemoving, bandageItemData, 1, null, null);

        if (PlayerManager.instance.CanSee(characterRemoving.spriteRenderer))
            FlavorText.instance.WriteLine_RemoveBandage(characterRemoving, characterManager, bandageItemData, injuryLocation);

        bandageItemData.ReturnToItemDataObjectPool();
        bandageItemData = null;
        bandage = null;

        if (injuryTimeRemaining <= 0)
            HealthSystem.RemoveInjury(characterManager, this);
    }

    public void SoilBandage(float amount)
    {
        if (bandageItemData != null && bandageItemData.freshness > 0f)
        {
            bandageItemData.freshness -= amount;
            if (bandageItemData.freshness <= 0f)
            {
                bandageItemData.freshness = 0f;
                injuryHealMultiplier -= bandage.quality;
            }
        }
    }

    public bool CanApplyMedicalItem(ItemData itemData)
    {
        if (CanApplyBandage(itemData))
            return true;
        return false;
    }

    public bool CanApplyBandage(ItemData itemData)
    {
        MedicalSupply medSupply = (MedicalSupply)itemData.item;
        if (medSupply.medicalSupplyType == MedicalSupplyType.Bandage && bandage == null && injury.CanBandage())
            return true;
        return false;
    }

    public void Reinjure()
    {
        // If this is an injury that bleeds and it hasn't sealed up yet
        if (bloodLossPerTurn > 0)
        {
            // Start bleeding again or increase bleed time, up to the max value
            Vector2Int bleedTimes = injury.BleedTime();
            bleedTimeRemaining += Random.Range(Mathf.RoundToInt(bleedTimes.x / 1.25f), Mathf.RoundToInt(bleedTimes.y / 1.25f));
            if (bleedTimeRemaining > bleedTimes.y)
                bleedTimeRemaining = bleedTimes.y;

            // Worsen bleeding, up to the max value
            Vector2 bloodLoss = injury.BloodLossPerTurn();
            bloodLossPerTurn *= Random.Range(1.15f, 1.35f);
            if (bloodLossPerTurn > bloodLoss.y)
                bloodLossPerTurn = bloodLoss.y;
        }

        // If this injury affects agility
        if (agilityModifier > 0)
        {
            // Remove the current speed modifier
            characterManager.characterStats.AdjustTotalAgilityMods(-agilityModifier);

            // Increase the speed modifier, up to the max value
            agilityModifier *= Random.Range(1.15f, 1.35f);
            if (speedModifier > injury.agilityModifier.y)
                speedModifier = injury.agilityModifier.y;

            // Re-add the speed modifier
            characterManager.characterStats.AdjustTotalAgilityMods(agilityModifier);
        }

        // If this injury affects dexterity
        if (dexterityModifier > 0)
        {
            // Remove the current speed modifier
            characterManager.characterStats.AdjustTotalDexterityMods(-dexterityModifier);

            // Increase the speed modifier, up to the max value
            dexterityModifier *= Random.Range(1.15f, 1.35f);
            if (dexterityModifier > injury.dexterityModifier.y)
                dexterityModifier = injury.dexterityModifier.y;

            // Re-add the speed modifier
            characterManager.characterStats.AdjustTotalDexterityMods(dexterityModifier);
        }

        // If this injury affects speed
        if (speedModifier > 0)
        {
            // Remove the current speed modifier
            characterManager.characterStats.AdjustTotalSpeedMods(-speedModifier);

            // Increase the speed modifier, up to the max value
            speedModifier *= Random.Range(1.15f, 1.35f);
            if (speedModifier > injury.speedModifier.y)
                speedModifier = injury.speedModifier.y;

            // Re-add the speed modifier
            characterManager.characterStats.AdjustTotalSpeedMods(speedModifier);
        }

        // Add to the total injury time remaining, up to the max value
        Vector2Int injuryTimes = injury.InjuryTimeInSeconds();
        injuryTimeRemaining += Random.Range(injuryTimes.x / 2, (injuryTimes.y + 1) / 2);
        if (injuryTimeRemaining > injuryTimes.y)
            injuryTimeRemaining = injuryTimes.y;

        if (characterManager.isNPC == false)
            HealthDisplay.instance.UpdateHealthHeaderColor(injuryLocation);
    }

    public bool InjuryRemedied()
    {
        if (injury.CanBandage() && bandage != null)
            return true;
        return false;
    }

    public MedicalSupply GetAppliedMedicalSupply(MedicalSupplyType medSupplyType)
    {
        switch (medSupplyType)
        {
            case MedicalSupplyType.Bandage:
                return bandage;
            default:
                return null;
        }
    }

    public string GetBleedSeverity()
    {
        if (bloodLossPerTurn <= 1)
            return "Barely Bleeding";
        else if (bloodLossPerTurn <= 3)
            return "Bleeding Lightly";
        else if (bloodLossPerTurn <= 6)
            return "Bleeding";
        else if (bloodLossPerTurn <= 9.5f)
            return "Bleeding Moderately";
        else if (bloodLossPerTurn <= 13.5f)
            return "Bleeding Heavily";
        else
            return "Bleeding Severely";
    }

    bool InjuryLocatedOnUpperBody()
    {
        if (injuryLocation == BodyPartType.Torso || injuryLocation == BodyPartType.Head || injuryLocation == BodyPartType.LeftArm || injuryLocation == BodyPartType.RightArm
            || injuryLocation == BodyPartType.LeftHand || injuryLocation == BodyPartType.RightHand)
            return true;
        return false;
    }

    bool InjuryLocatedOnArms()
    {
        if (injuryLocation == BodyPartType.LeftArm || injuryLocation == BodyPartType.RightArm || injuryLocation == BodyPartType.LeftHand || injuryLocation == BodyPartType.RightHand)
            return true;
        return false;
    }

    bool InjuryLocatedOnLowerBody()
    {
        if (injuryLocation == BodyPartType.LeftLeg || injuryLocation == BodyPartType.RightLeg || injuryLocation == BodyPartType.LeftFoot || injuryLocation == BodyPartType.RightFoot)
            return true;
        return false;
    }
}

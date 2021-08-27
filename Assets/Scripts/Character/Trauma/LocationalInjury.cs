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

        Vector2Int bleedTimes = injury.GetBleedTime();
        if (bleedTimes.y > 0)
            bleedTimeRemaining = Random.Range(bleedTimes.x, bleedTimes.y + 1);

        Vector2 bloodLossValues = injury.GetBloodLossPerTurn();
        if (bloodLossValues.y > 0)
            bloodLossPerTurn = Random.Range(bloodLossValues.x, bloodLossValues.y);
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

        FlavorText.instance.WriteLine_RemoveBandage(characterRemoving, characterManager, bandageItemData, injuryLocation);

        bandageItemData.ReturnToItemDataObjectPool();
        bandageItemData = null;
        bandage = null;

        if (injuryTimeRemaining <= 0)
            TraumaSystem.RemoveInjury(characterManager, this);
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
        // If this is an injury that bleeds, re-open it or add onto the bleed time
        if (bloodLossPerTurn > 0)
        {
            // Start bleeding again, or increase bleed time up to the max value
            Vector2Int bleedTimes = injury.GetBleedTime();
            bleedTimeRemaining += Random.Range(Mathf.RoundToInt(bleedTimes.x / 1.25f), Mathf.RoundToInt(bleedTimes.y / 1.25f));
            if (bleedTimeRemaining > bleedTimes.y)
                bleedTimeRemaining = bleedTimes.y;

            // Worsen bleeding, up to the max value
            Vector2 bloodLoss = injury.GetBloodLossPerTurn();
            bloodLossPerTurn *= 1.25f;
            if (bloodLossPerTurn > bloodLoss.y)
                bloodLossPerTurn = bloodLoss.y;
        }

        // Add to the total injury time remaining, up to the max value
        Vector2Int injuryTimes = injury.GetInjuryTimesInSeconds();
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
}

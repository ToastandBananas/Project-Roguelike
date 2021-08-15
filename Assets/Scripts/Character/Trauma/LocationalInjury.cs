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

    CharacterManager characterManager;

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

    public IEnumerator ApplyMedicalItem(ItemData itemData, Inventory inventory, InventoryItem invItem)
    {
        MedicalSupply medSupply = (MedicalSupply)itemData.item;
        characterManager.StartCoroutine(APManager.instance.UseAP(characterManager, APManager.instance.GetApplyMedicalItemAPCost(medSupply)));

        int queueNumber = characterManager.currentQueueNumber + characterManager.actionsQueued;
        while (queueNumber != characterManager.currentQueueNumber)
        {
            yield return null;
            if (characterManager.status.isDead) yield break;
        }

        if (CanApplyBandage(itemData))
            ApplyBandage(itemData);

        itemData.item.Use(characterManager, inventory, invItem, itemData, 1);

        if (characterManager.isNPC == false)
        {
            HealthDisplay.instance.UpdateHealthHeaderColor(injuryLocation);
            HealthDisplay.instance.UpdateTooltip();
        }
    }

    void ApplyBandage(ItemData bandageItemData)
    {
        // Create new ItemData object for the bandage
        ItemData tempItemData = ObjectPoolManager.instance.itemDataObjectPool.GetPooledItemData();
        tempItemData.gameObject.SetActive(true);
        tempItemData.TransferData(bandageItemData, tempItemData);
        tempItemData.currentStackSize = 1;
        Debug.Log(tempItemData);

        ItemData newItemData = UIManager.instance.CreateNewItemDataChild(tempItemData, null, HealthDisplay.instance.medicalItemsParent, false);
        tempItemData.ReturnToItemDataObjectPool();

        this.bandageItemData = newItemData;
        bandage = (MedicalSupply)newItemData.item;
        injuryHealMultiplier += bandage.quality;

        FlavorText.instance.WriteApplyBandageLine(characterManager, injury, bandage, injuryLocation);
    }

    public IEnumerator RemoveMedicalItem(MedicalSupplyType medicalSupplyType)
    {
        characterManager.StartCoroutine(APManager.instance.UseAP(characterManager, APManager.instance.GetRemoveMedicalItemAPCost(bandage)));

        int queueNumber = characterManager.currentQueueNumber + characterManager.actionsQueued;
        while (queueNumber != characterManager.currentQueueNumber)
        {
            yield return null;
            if (characterManager.status.isDead) yield break;
        }

        if (medicalSupplyType == MedicalSupplyType.Bandage)
            RemoveBandage();
    }

    public void RemoveBandage()
    {
        if (bandage == null) return;

        if (characterManager.isNPC)
            DropItemController.instance.ForceDropNearest(bandageItemData, 1, null, null);
        else
        {
            // Try to place the bandage in one of the player's inventories. If it won't fit, drop it
            if (PlayerInventoryUI.instance.TryAddingItemToPlayersInventory(bandageItemData, null, false) == false)
                DropItemController.instance.ForceDropNearest(bandageItemData, 1, null, null);
        }

        bandageItemData.ReturnToItemDataObjectPool();
        bandageItemData = null;
        bandage = null;
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
        MedicalSupply medSupply = (MedicalSupply)itemData.item;
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

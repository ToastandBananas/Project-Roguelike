using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenu : MonoBehaviour
{
    [HideInInspector] public InventoryItem contextActiveInvItem;
    [HideInInspector] public InjuryTextButton contextActiveInjuryTextButton;
    [HideInInspector] public CharacterManager contextActiveCharacter;

    [HideInInspector] public bool isActive;

    List<ContextMenuButton> activeContextButtons = new List<ContextMenuButton>();

    Canvas canvas;
    GameManager gm;
    
    readonly float minButtonWidth = 110f;
    float currentWidth;

    #region Singleton
    public static ContextMenu instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogError("More than one instance of ContextMenu. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        gm = GameManager.instance;
    }

    #region Health Display
    public void BuildContextMenu(InjuryTextButton injuryTextButton)
    {
        isActive = true;
        contextActiveInjuryTextButton = injuryTextButton;

        if (injuryTextButton.locationalInjury != null)
        {
            if (injuryTextButton.locationalInjury.InjuryRemedied() == false)
                CreateApplyMedicalItemButtons(injuryTextButton.locationalInjury, false);
            else if (injuryTextButton.locationalInjury.bandage != null)
                CreateRemoveMedicalItemButton(injuryTextButton.locationalInjury.bandageItemData, injuryTextButton.locationalInjury, MedicalSupplyType.Bandage, false);
        }
        else
        {
            DisableContextMenu();
            return;
        }

        SetupContextButton();
    }

    void CreateApplyMedicalItemButtons(LocationalInjury locationalInjury, bool includeInjury)
    {
        List<ItemData> medicalItems = new List<ItemData>();
        
        // If the injury can be bandaged, get all bandages from player's inventories
        if (locationalInjury.injury.CanBandage() && locationalInjury.bandage == null)
            medicalItems = gm.playerManager.GetMedicalSupplies(MedicalSupplyType.Bandage);

        // For each medical item, create a context button
        for (int i = 0; i < medicalItems.Count; i++)
        {
            ItemData medItemData = medicalItems[i];
            MedicalSupply medSupply = (MedicalSupply)medItemData.item;
            ContextMenuButton contextButton = GetNextInactiveButton();
            contextButton.gameObject.SetActive(true);

            // Determine the button's text based off of thes medical supply type
            if (medSupply.medicalSupplyType == MedicalSupplyType.Bandage)
                contextButton.textMesh.text = "Apply " + medicalItems[i].GetSoilageText() + " " + medicalItems[i].itemName;

            if (includeInjury)
                contextButton.textMesh.text += " - " + Utilities.FormatEnumStringWithSpaces(locationalInjury.injuryLocation.ToString(), false) + " - " + locationalInjury.injury.name;

            contextButton.button.onClick.AddListener(delegate { ApplyMedicalItemToInjury_FromInventory(locationalInjury, medItemData, medItemData.GetItemDatasPlayerInventory()); });
        }
    }

    void ApplyMedicalItemToInjury_FromInventory(LocationalInjury locationalInjury, ItemData medicalItemData, Inventory inventory)
    {
        StartCoroutine(locationalInjury.ApplyMedicalItem(gm.playerManager, medicalItemData, inventory, medicalItemData.GetItemDatasInventoryItem()));
        DisableContextMenu();
    }

    void CreateRemoveMedicalItemButton(ItemData medicalItemData, LocationalInjury locationalInjury, MedicalSupplyType medicalSupplyType, bool includeInjury)
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        contextButton.textMesh.text = "Remove " + medicalItemData.itemName;
        if (includeInjury)
            contextButton.textMesh.text += " - " + Utilities.FormatEnumStringWithSpaces(locationalInjury.injuryLocation.ToString(), false) + " - " + locationalInjury.injury.name;

        contextButton.button.onClick.AddListener(delegate { RemoveMedicalItem(locationalInjury, medicalSupplyType); });
    }

    void RemoveMedicalItem(LocationalInjury locationalInjury, MedicalSupplyType medicalSupplyType)
    {
        StartCoroutine(locationalInjury.RemoveMedicalItem(gm.playerManager, medicalSupplyType));
        DisableContextMenu();
    }
    #endregion

    #region Character
    public void BuildContextMenu(CharacterManager characterManager)
    {
        isActive = true;
        contextActiveCharacter = characterManager;
        if (characterManager != null)
        {
            for (int i = 0; i < characterManager.status.bodyParts.Count; i++)
            {
                for (int j = 0; j < characterManager.status.bodyParts[i].injuries.Count; j++)
                {
                    if (characterManager.status.bodyParts[i].injuries[j].InjuryRemedied() == false)
                        CreateApplyMedicalItemButtons(characterManager.status.bodyParts[i].injuries[j], true);
                    else if (characterManager.status.bodyParts[i].injuries[j].bandage != null)
                        CreateRemoveMedicalItemButton(characterManager.status.bodyParts[i].injuries[j].bandageItemData, characterManager.status.bodyParts[i].injuries[j], MedicalSupplyType.Bandage, true);
                }
            }
        }
        else
        {
            DisableContextMenu();
            return;
        }

        SetupContextButton();
    }
    #endregion

    #region Inventory
    public void BuildContextMenu(InventoryItem invItem)
    {
        isActive = true;
        contextActiveInvItem = invItem;
        
        if (contextActiveInvItem.myEquipmentManager != null)
        {
            CreateUnequipButton();

            if (gm.containerInvUI.activeInventory != null)
                CreateTransferButton();
            else
                CreateDropItemButton();
        }
        else if (gm.uiManager.activeInvItem != null)
        {
            if (gm.uiManager.activeInvItem.itemData.item.isUsable)
            {
                if (gm.uiManager.activeInvItem.itemData.item.IsWeapon())
                {
                    Weapon weapon = (Weapon)gm.uiManager.activeInvItem.itemData.item;
                    if (weapon.isTwoHanded == false)
                        CreateEquipLeftWeaponButton();
                }

                CreateUseItemButton();
            }
            else if (gm.uiManager.activeInvItem.itemData.item.IsMedicalSupply())
            {
                CreateApplyMedicalSupplyButtons();
            }

            if (gm.uiManager.activeInvItem.itemData.currentStackSize > 1)
                CreateSplitStackButton();

            // If selecting the player's inventory or equipment menu
            if ((contextActiveInvItem.myInventory != null && contextActiveInvItem.myInventory.myInvUI == gm.playerInvUI) || contextActiveInvItem.myEquipmentManager != null
                || (contextActiveInvItem.parentInvItem != null && contextActiveInvItem.parentInvItem.myInvUI == gm.playerInvUI))
            {
                if (gm.containerInvUI.activeInventory != null)
                    CreateTransferButton();
                else
                    CreateDropItemButton();
            }
            else
                CreateTransferButton();
        }

        SetupContextButton();
    }

    void CreateApplyMedicalSupplyButtons()
    {
        MedicalSupply medSupply = (MedicalSupply)gm.uiManager.activeInvItem.itemData.item;
        for (int i = 0; i < gm.playerManager.status.bodyParts.Count; i++)
        {
            for (int j = 0; j < gm.playerManager.status.bodyParts[i].injuries.Count; j++)
            {
                ContextMenuButton contextButton = GetNextInactiveButton();
                LocationalInjury locationalInjury = gm.playerManager.status.bodyParts[i].injuries[j];
                bool canApplyItem = false;

                // If this is a bandage and the wound is something that can be remedied with a bandage (and it's not already bandaged)
                if (locationalInjury.CanApplyBandage(gm.uiManager.activeInvItem.itemData))
                {
                    canApplyItem = true;
                    contextButton.textMesh.text = "Bandage " + locationalInjury.injury.name + " - " + Utilities.FormatEnumStringWithSpaces(gm.playerManager.status.bodyParts[i].bodyPartType.ToString(), false);
                }

                if (canApplyItem)
                {
                    contextButton.gameObject.SetActive(true);
                    contextButton.button.onClick.AddListener(delegate { ApplyMedicalItemToInjury_InventoryItem(locationalInjury); });
                }
                else
                    activeContextButtons.Remove(contextButton);
            }
        }
    }

    void ApplyMedicalItemToInjury_InventoryItem(LocationalInjury locationalInjury)
    {
        StartCoroutine(locationalInjury.ApplyMedicalItem(gm.playerManager, contextActiveInvItem.itemData, contextActiveInvItem.myInventory, contextActiveInvItem));
        DisableContextMenu();
    }

    void CreateUseItemButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        if (gm.uiManager.activeInvItem.itemData.item.IsEquipment() == false && gm.uiManager.activeInvItem.itemData.item.IsConsumable() == false)
            contextButton.textMesh.text = "Use";
        else if (gm.uiManager.activeInvItem.itemData.item.IsConsumable())
        {
            Consumable consumable = (Consumable)gm.uiManager.activeInvItem.itemData.item;
            if (consumable.consumableType == ConsumableType.Food)
                contextButton.textMesh.text = "Eat";
            else
                contextButton.textMesh.text = "Drink";
        }
        else if (gm.uiManager.activeInvItem.itemData.item.IsWeapon())
        {
            Weapon weapon = (Weapon)gm.uiManager.activeInvItem.itemData.item;
            if (weapon.isTwoHanded == false)
                contextButton.textMesh.text = "Equip Right";
            else
                contextButton.textMesh.text = "Equip";
        }
        else
            contextButton.textMesh.text = "Equip";

        contextButton.button.onClick.AddListener(UseItem);
    }

    void UseItem()
    {
        contextActiveInvItem.UseItem();
        DisableContextMenu();
    }

    void CreateEquipLeftWeaponButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        contextButton.textMesh.text = "Equip Left";

        contextButton.button.onClick.AddListener(EquipLeftWeapon);
    }
    
    void EquipLeftWeapon()
    {
        Weapon weapon = (Weapon)contextActiveInvItem.itemData.item;
        weapon.Use(gm.playerManager, contextActiveInvItem.myInventory, contextActiveInvItem, contextActiveInvItem.itemData, 1, EquipmentSlot.LeftWeapon);
        DisableContextMenu();
    }

    void CreateUnequipButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        contextButton.textMesh.text = "Unequip";

        contextButton.button.onClick.AddListener(UseItem);
    }

    void CreateTransferButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        if ((contextActiveInvItem.myInventory != null && contextActiveInvItem.myInventory.myInvUI == gm.playerInvUI) || contextActiveInvItem.myEquipmentManager != null
            || (contextActiveInvItem.parentInvItem != null && contextActiveInvItem.parentInvItem.myInvUI == gm.playerInvUI))
            contextButton.textMesh.text = "Transfer";
        else
            contextButton.textMesh.text = "Take";

        contextButton.button.onClick.AddListener(TransferItem);
    }

    void TransferItem()
    {
        contextActiveInvItem.TransferItem();
        DisableContextMenu();
    }

    void CreateSplitStackButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        contextButton.textMesh.text = "Split Stack";

        contextButton.button.onClick.AddListener(SplitStack);
    }

    void SplitStack()
    {
        gm.stackSizeSelector.ShowStackSizeSelector(contextActiveInvItem);
        DisableContextMenu();
    }

    void CreateDropItemButton()
    {
        ContextMenuButton contextButton = GetNextInactiveButton();
        contextButton.gameObject.SetActive(true);

        contextButton.textMesh.text = "Drop";

        contextButton.button.onClick.AddListener(TransferItem);
    }
    #endregion

    ContextMenuButton GetNextInactiveButton()
    {
        ContextMenuButton contextButton = gm.objectPoolManager.contextButtonObjectPool.GetPooledContextButton();
        activeContextButtons.Add(contextButton);
        return contextButton;
    }

    public void DisableContextMenu()
    {
        for (int i = 0; i < activeContextButtons.Count; i++)
        {
            activeContextButtons[i].button.onClick.RemoveAllListeners();
            activeContextButtons[i].gameObject.SetActive(false);
        }

        activeContextButtons.Clear();
        isActive = false;
        contextActiveInvItem = null;
        contextActiveInjuryTextButton = null;
        contextActiveCharacter = null;
        gm.uiManager.activeContextMenuButton = null;
    }

    public IEnumerator DelayDisableContextMenu()
    {
        yield return new WaitForSeconds(0.1f);
        if (isActive)
            DisableContextMenu();
    }

    int GetActiveButtonCount()
    {
        return activeContextButtons.Count;
    }

    void SetActiveButtonsWidth()
    {
        currentWidth = minButtonWidth;
        for (int i = 0; i < activeContextButtons.Count; i++)
        {
            activeContextButtons[i].textMesh.ForceMeshUpdate();
            if (currentWidth < activeContextButtons[i].textMesh.textBounds.size.x)
                currentWidth = activeContextButtons[i].textMesh.textBounds.size.x + 40;
        }
        
        for (int i = 0; i < activeContextButtons.Count; i++)
        {
            activeContextButtons[i].rectTransform.sizeDelta = new Vector2(currentWidth, activeContextButtons[i].rectTransform.sizeDelta.y);
        }
    }

    void SetupContextButton()
    {
        SetActiveButtonsWidth();

        float xPosAddon = xPosAddon = (currentWidth - minButtonWidth) / 2; // Account for any resizing of the context button that may have happened
        float yPosAddon = 0;

        // Get the desired position:
        // If the mouse position is too close to the bottom of the screen
        if (Input.mousePosition.y <= 190f)
            yPosAddon = GetActiveButtonCount() * gm.playerInvUI.invItemHeight;

        // If the mouse position is too far to the right of the screen
        if (Input.mousePosition.x >= 1680f)
            xPosAddon = -currentWidth;

        transform.position = Input.mousePosition + new Vector3(xPosAddon, yPosAddon);
    }
}

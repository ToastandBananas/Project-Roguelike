using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemSorter : MonoBehaviour
{
    public InventoryUI myInvUI;
    public TextMeshProUGUI sortButtonText;

    [HideInInspector] public List<ItemData> shownItems = new List<ItemData>();

    [HideInInspector] public bool showArmor = true;
    [HideInInspector] public bool showClothing = true;
    [HideInInspector] public bool showWeapons = true;
    [HideInInspector] public bool showFood = true;
    [HideInInspector] public bool showIngredients = true;
    [HideInInspector] public bool showSeeds = true;
    [HideInInspector] public bool showDrinks = true;
    [HideInInspector] public bool showAmmo = true;
    [HideInInspector] public bool showKeys = true;
    [HideInInspector] public bool showContainers = true;
    [HideInInspector] public bool showReadables = true;

    public void ShowAll()
    {
        shownItems.Clear();

        for (int i = 0; i < myInvUI.activeInventory.items.Count; i++)
        {
            shownItems.Add(myInvUI.activeInventory.items[i]);
        }

        showArmor = true;
        showClothing = true;
        showWeapons = true;
        showFood = true;
        showIngredients = true;
        showSeeds = true;
        showDrinks = true;
        showAmmo = true;
        showKeys = true;
        showContainers = true;
        showReadables = true;
    }

    public void ToggleShowArmor()
    {
        showArmor = !showArmor;
    }

    public void ToggleShowClothing()
    {
        showClothing = !showClothing;
    }

    public void ToggleShowWeapons()
    {
        showWeapons = !showWeapons;
    }

    public void ToggleShowFood()
    {
        showFood = !showFood;
    }

    public void ToggleShowIngredients()
    {
        showIngredients = !showIngredients;
    }

    public void ToggleShowSeeds()
    {
        showSeeds = !showSeeds;
    }

    public void ToggleShowDrinks()
    {
        showDrinks = !showDrinks;
    }

    public void ToggleShowAmmunition()
    {
        showAmmo = !showAmmo;
    }

    public void ToggleShowKeys()
    {
        showKeys = !showKeys;
    }

    public void ToggleShowContainers()
    {
        showContainers = !showContainers;
    }

    public void ToggleShowReadables()
    {
        showReadables = !showReadables;
    }
}
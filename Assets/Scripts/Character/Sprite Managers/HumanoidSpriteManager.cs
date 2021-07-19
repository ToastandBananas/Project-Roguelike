using UnityEngine;

public class HumanoidSpriteManager : CharacterSpriteManager
{
    [Header("Hair")]
    public Sprite hairSprite;
    public SpriteRenderer hair, beard;
    public Color hairColor;

    [Header("Equipment Sprites")]
    public SpriteRenderer leftWeapon;
    public SpriteRenderer rightWeapon, helmet, shirt, bodyArmor, pants, legArmor, boots, gloves, cape;

    void Start()
    {
        if (hair != null)
            hair.color = hairColor;
        if (beard != null)
            beard.color = hairColor;
    }

    public void ShowHair()
    {
        hair.enabled = true;
    }

    public void HideHair()
    {
        hair.enabled = false;
    }

    public void SetHairColor(Color color)
    {
        hairColor = color;
        hair.color = hairColor;
    }

    public void SetupOneHandedWeaponStance(EquipmentManager equipmentManager, CharacterManager characterManager)
    {
        SetToDefaultCharacterSprite(characterManager);

        if (equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
        {
            Equipment equipment = (Equipment)equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].item;
            shirt.sprite = equipment.primaryEquippedSprite;
        }

        if (equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
        {
            Equipment equipment = (Equipment)equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].item;
            bodyArmor.sprite = equipment.primaryEquippedSprite;
        }

        if (equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
        {
            Equipment equipment = (Equipment)equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].item;
            gloves.sprite = equipment.primaryEquippedSprite;
        }

        if (equipmentManager.currentEquipment[(int)EquipmentSlot.Cape] != null)
        {
            Equipment equipment = (Equipment)equipmentManager.currentEquipment[(int)EquipmentSlot.Cape].item;
            cape.sprite = equipment.primaryEquippedSprite;
        }
    }

    public void SetupTwoHandedWeaponStance(EquipmentManager equipmentManager, CharacterManager characterManager)
    {
        SetToSecondaryCharacterSprite(characterManager);

        if (equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt] != null)
        {
            Equipment equipment = (Equipment)equipmentManager.currentEquipment[(int)EquipmentSlot.Shirt].item;
            shirt.sprite = equipment.secondaryEquippedSprite;
        }

        if (equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor] != null)
        {
            Equipment equipment = (Equipment)equipmentManager.currentEquipment[(int)EquipmentSlot.BodyArmor].item;
            bodyArmor.sprite = equipment.secondaryEquippedSprite;
        }

        if (equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves] != null)
        {
            Equipment equipment = (Equipment)equipmentManager.currentEquipment[(int)EquipmentSlot.Gloves].item;
            gloves.sprite = equipment.secondaryEquippedSprite;
        }

        if (equipmentManager.currentEquipment[(int)EquipmentSlot.Cape] != null)
        {
            Equipment equipment = (Equipment)equipmentManager.currentEquipment[(int)EquipmentSlot.Cape].item;
            cape.sprite = equipment.secondaryEquippedSprite;
        }
    }

    public void AssignSprite(EquipmentSlot equipSlot, Equipment equipment, EquipmentManager equipmentManager)
    {
        switch (equipSlot)
        {
            case EquipmentSlot.Helmet:
                helmet.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Shirt:
                if (equipmentManager.TwoHandedWeaponEquipped())
                    shirt.sprite = equipment.secondaryEquippedSprite;
                else
                    shirt.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Pants:
                pants.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Boots:
                boots.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Gloves:
                if (equipmentManager.TwoHandedWeaponEquipped())
                    gloves.sprite = equipment.secondaryEquippedSprite;
                else
                    gloves.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.BodyArmor:
                if (equipmentManager.TwoHandedWeaponEquipped())
                    bodyArmor.sprite = equipment.secondaryEquippedSprite;
                else
                    bodyArmor.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.LegArmor:
                legArmor.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.LeftWeapon:
                leftWeapon.sprite = equipment.secondaryEquippedSprite;
                break;
            case EquipmentSlot.RightWeapon:
                rightWeapon.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Ranged:
                rightWeapon.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Cape:
                if (equipmentManager.TwoHandedWeaponEquipped())
                    cape.sprite = equipment.secondaryEquippedSprite;
                else
                    cape.sprite = equipment.primaryEquippedSprite;
                break;
            default:
                break;
        }
    }

    public void RemoveSprite(EquipmentSlot equipSlot)
    {
        switch (equipSlot)
        {
            case EquipmentSlot.Helmet:
                helmet.sprite = null;
                break;
            case EquipmentSlot.Shirt:
                shirt.sprite = null;
                break;
            case EquipmentSlot.Pants:
                pants.sprite = null;
                break;
            case EquipmentSlot.Boots:
                boots.sprite = null;
                break;
            case EquipmentSlot.Gloves:
                gloves.sprite = null;
                break;
            case EquipmentSlot.BodyArmor:
                bodyArmor.sprite = null;
                break;
            case EquipmentSlot.LegArmor:
                legArmor.sprite = null;
                break;
            case EquipmentSlot.LeftWeapon:
                leftWeapon.sprite = null;
                break;
            case EquipmentSlot.RightWeapon:
                rightWeapon.sprite = null;
                break;
            case EquipmentSlot.Ranged:
                rightWeapon.sprite = null;
                break;
            case EquipmentSlot.Cape:
                cape.sprite = null;
                break;
            default:
                break;
        }
    }
}

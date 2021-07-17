using UnityEngine;

public class EquippedItemsSpriteManager : MonoBehaviour
{
    public SpriteRenderer leftWeapon, rightWeapon, helmet, shirt, bodyArmor, pants, legArmor, boots, gloves;

    public void AssignSprite(EquipmentSlot equipSlot, Equipment equipment)
    {
        switch (equipSlot)
        {
            case EquipmentSlot.Helmet:
                helmet.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Shirt:
                shirt.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Pants:
                pants.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Boots:
                boots.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Gloves:
                gloves.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.BodyArmor:
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
            default:
                break;
        }
    }
}

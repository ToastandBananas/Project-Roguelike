using UnityEngine;

public class EquippedItemsSpriteManager : MonoBehaviour
{
    public SpriteRenderer leftWeapon, rightWeapon, helmet, shirt, bodyArmor, pants, legArmor, boots, gloves;

    public void AssignSprite(EquipmentSlot equipSlot, Equipment equipment)
    {
        switch (equipSlot)
        {
            case EquipmentSlot.Helmet:
                helmet.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.Shirt:
                shirt.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.Pants:
                pants.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.Boots:
                boots.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.Gloves:
                gloves.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.BodyArmor:
                bodyArmor.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.LegArmor:
                legArmor.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.RightWeapon:
                rightWeapon.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.LeftWeapon:
                leftWeapon.sprite = equipment.equippedSprite;
                break;
            case EquipmentSlot.Ranged:
                leftWeapon.sprite = equipment.equippedSprite;
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
            case EquipmentSlot.RightWeapon:
                rightWeapon.sprite = null;
                break;
            case EquipmentSlot.LeftWeapon:
                leftWeapon.sprite = null;
                break;
            case EquipmentSlot.Ranged:
                leftWeapon.sprite = null;
                break;
            default:
                break;
        }
    }
}

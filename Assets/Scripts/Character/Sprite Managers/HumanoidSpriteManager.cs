using System.Collections;
using UnityEngine;

public class HumanoidSpriteManager : SpriteManager
{
    [Header("Facing Arrow")]
    public SpriteRenderer facingArrow;

    [Header("Hair")]
    public Sprite hairSprite;
    public SpriteRenderer hair, beard;
    public Color hairColor;

    [Header("Equipment Sprites")]
    public SpriteRenderer leftHandItem;
    public SpriteRenderer rightHandItem, helmet, shirt, bodyArmor, pants, legArmor, boots, gloves, cape;

    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;

        if (hair != null)
            hair.color = hairColor;
        if (beard != null)
            beard.color = hairColor;
    }

    public override void SetToDeathSprite(SpriteRenderer spriteRenderer)
    {
        base.SetToDeathSprite(spriteRenderer);

        facingArrow.enabled = false;

        if (hair != null)
            hair.sprite = null;
        if (beard != null)
            beard.sprite = null;
        leftHandItem.sprite = null;
        rightHandItem.sprite = null;
        helmet.sprite = null;
        shirt.sprite = null;
        bodyArmor.sprite = null;
        pants.sprite = null;
        legArmor.sprite = null;
        boots.sprite = null;
        gloves.sprite = null;
        cape.sprite = null;
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
        SetToDefaultSprite(characterManager.spriteRenderer);

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

        characterManager.equipmentManager.isTwoHanding = false;
    }

    public void SetupTwoHandedWeaponStance(EquipmentManager equipmentManager, CharacterManager characterManager)
    {
        SetToSecondarySprite(characterManager.spriteRenderer);

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

        characterManager.equipmentManager.isTwoHanding = true;
    }

    public IEnumerator SwapStance(EquipmentManager equipmentManager, CharacterManager characterManager, ItemData weaponItemData)
    {
        if (characterManager.status.isDead) yield break;

        if (equipmentManager.isTwoHanding)
            SetupOneHandedWeaponStance(equipmentManager, characterManager);
        else
            SetupTwoHandedWeaponStance(equipmentManager, characterManager);

        if (characterManager.isNPC == false)
            gm.flavorText.WriteLine_SwitchStance(characterManager, weaponItemData, (Weapon)weaponItemData.item);

        characterManager.FinishAction();
    }

    public void AssignSprite(EquipmentSlot equipSlot, Equipment equipment, EquipmentManager equipmentManager)
    {
        switch (equipSlot)
        {
            case EquipmentSlot.Helmet:
                helmet.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Shirt:
                if (equipmentManager.TwoHandedWeaponEquipped() || equipmentManager.isTwoHanding)
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
                if (equipmentManager.TwoHandedWeaponEquipped() || equipmentManager.isTwoHanding)
                    gloves.sprite = equipment.secondaryEquippedSprite;
                else
                    gloves.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.BodyArmor:
                if (equipmentManager.TwoHandedWeaponEquipped() || equipmentManager.isTwoHanding)
                    bodyArmor.sprite = equipment.secondaryEquippedSprite;
                else
                    bodyArmor.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.LegArmor:
                legArmor.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.LeftHandItem:
                leftHandItem.sprite = equipment.secondaryEquippedSprite;
                break;
            case EquipmentSlot.RightHandItem:
                rightHandItem.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Ranged:
                rightHandItem.sprite = equipment.primaryEquippedSprite;
                break;
            case EquipmentSlot.Cape:
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
            case EquipmentSlot.LeftHandItem:
                leftHandItem.sprite = null;
                break;
            case EquipmentSlot.RightHandItem:
                rightHandItem.sprite = null;
                break;
            case EquipmentSlot.Ranged:
                rightHandItem.sprite = null;
                break;
            case EquipmentSlot.Cape:
                cape.sprite = null;
                break;
            default:
                break;
        }
    }

    public void SetFacingArrowDirection(Direction direction)
    {
        // Debug.Log("Switching arrow to point: " + direction);
        switch (direction)
        {
            case Direction.North:
                facingArrow.transform.localEulerAngles = new Vector3(0, 0, 315);
                break;
            case Direction.South:
                facingArrow.transform.localEulerAngles = new Vector3(0, 0, 135);
                break;
            case Direction.West:
                facingArrow.transform.localEulerAngles = new Vector3(0, 0, 225);
                break;
            case Direction.East:
                facingArrow.transform.localEulerAngles = new Vector3(0, 0, 225);
                break;
            case Direction.Northwest:
                facingArrow.transform.localEulerAngles = new Vector3(0, 0, 270);
                break;
            case Direction.Northeast:
                facingArrow.transform.localEulerAngles = new Vector3(0, 0, 270);
                break;
            case Direction.Southwest:
                facingArrow.transform.localEulerAngles = new Vector3(0, 0, 180);
                break;
            case Direction.Southeast:
                facingArrow.transform.localEulerAngles = new Vector3(0, 0, 180);
                break;
            default:
                break;
        }
    }
}

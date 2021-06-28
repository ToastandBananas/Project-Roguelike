using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public delegate void OnWearableChanged(ItemData newItemData, ItemData oldItemData);
    public OnWearableChanged onWearableChanged;

    public delegate void OnWeaponChanged(ItemData newItemData, ItemData oldItemData);
    public OnWeaponChanged onWeaponChanged;

    public ItemData[] currentEquipment;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;

    public virtual void Start()
    {
        if (gameObject.CompareTag("NPC"))
            characterManager = GetComponent<CharacterManager>();
        else
            characterManager = PlayerManager.instance;

        gm = GameManager.instance;

        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new ItemData[numSlots];
    }

    public virtual void Equip(ItemData newItemData, EquipmentSlot equipmentSlot)
    {
        AssignEquipment(newItemData, equipmentSlot);

        if (gm.playerInvUI.activeInventory == null) // If the equipment inventory is active, show the item in the menu
        {

        }

        // If this is a Wearable item, assign the item's Animator Controller
        if (IsWeapon(equipmentSlot) == false)
            SetWearableSprite(equipmentSlot, newItemData);
        else
            SetWeaponSprite(equipmentSlot, newItemData);
    }

    public virtual void Unequip(EquipmentSlot equipmentSlot, bool shouldAddToInventory)
    {
        if (currentEquipment[(int)equipmentSlot] != null)
        {
            ItemData oldItemData = currentEquipment[(int)equipmentSlot];

            UnassignEquipment(oldItemData, equipmentSlot, shouldAddToInventory);

            // If this is a Wearable Item, set the scriptableObject to null
            if (oldItemData.item.IsWeapon() == false)
                UnequipWearable(equipmentSlot);
            else  // If this is a Weapon Item, get rid of the weapon's gameobject
                UnequipWeapon(equipmentSlot, oldItemData);
        }
    }

    public virtual void AssignEquipment(ItemData newItemData, EquipmentSlot equipmentSlot)
    {
        ItemData oldItemData = null;

        // If there's already an Item in this slot
        if (currentEquipment[(int)equipmentSlot] != null)
        {
            oldItemData = currentEquipment[(int)equipmentSlot];
            Unequip(equipmentSlot, true);
        }

        if (onWearableChanged != null && newItemData.item.IsWeapon() == false)
            onWearableChanged.Invoke(newItemData, oldItemData);
        else if (onWeaponChanged != null && newItemData.item.IsWeapon())
            onWeaponChanged.Invoke(newItemData, oldItemData);

        currentEquipment[(int)equipmentSlot] = newItemData;
    }

    public virtual void UnassignEquipment(ItemData oldItemData, EquipmentSlot equipmentSlot, bool shouldAddToInventory)
    {
        if (shouldAddToInventory)
        {
            if (characterManager.inventory.Add(null, oldItemData, 1, null) == false) // If we can't add it to the Inventory, drop it
                gm.dropItemController.DropEquipment(this, equipmentSlot, characterManager.transform.position, oldItemData, 1);
        }

        currentEquipment[(int)equipmentSlot] = null;

        if (onWearableChanged != null && oldItemData.item.IsWeapon() == false)
            onWearableChanged.Invoke(null, oldItemData);
        else if (onWeaponChanged != null && oldItemData.item.IsWeapon())
            onWeaponChanged.Invoke(null, oldItemData);
    }

    public void UnequipAll(bool shouldAddEquipmentToInventory)
    {
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            Equipment equipment = (Equipment)currentEquipment[i].item;
            Unequip(equipment.equipmentSlot, shouldAddEquipmentToInventory);
        }
    }

    void SetWearableSprite(EquipmentSlot wearableSlot, ItemData wearable)
    {
        // TODO
    }

    void UnequipWearable(EquipmentSlot wearableSlot)
    {
        // TODO
    }

    void SetWeaponSprite(EquipmentSlot weaponSlot, ItemData weapon)
    {
        // TODO
    }

    void UnequipWeapon(EquipmentSlot weaponSlot, ItemData weapon)
    {
        // TODO
    }

    public void SheathWeapon(EquipmentSlot weaponSlot)
    {
        
    }

    public void UnsheathWeapon(EquipmentSlot weaponSlot)
    {
        
    }

    bool IsWeapon(EquipmentSlot equipSlot)
    {
        if (equipSlot == EquipmentSlot.LeftWeapon || equipSlot == EquipmentSlot.RightWeapon || equipSlot == EquipmentSlot.Ranged)
            return true;

        return false;
    }

    #region Old Wearable Animation Method
    /*void SetWearableAnim(EquipmentSlot equipSlot, Wearable newItem)
    {
        switch (equipSlot)
        {
            case EquipmentSlot.Helmet:
                break;
            case EquipmentSlot.Shirt:
                characterManager.charAnimController.SetAnimatorController(characterManager.charAnimController.shirtAnim, newItem.animatorController);
                break;
            case EquipmentSlot.Pants:
                characterManager.charAnimController.SetAnimatorController(characterManager.charAnimController.pantsAnim, newItem.animatorController);
                break;
            case EquipmentSlot.Boots:
                break;
            case EquipmentSlot.Gloves:
                break;
            case EquipmentSlot.BodyArmor:
                break;
            case EquipmentSlot.LegArmor:
                break;
            case EquipmentSlot.Quiver:
                break;
            default:
                break;
        }
    }

    void RemoveWearableAnim(EquipmentSlot equipSlot)
    {
        switch (equipSlot)
        {
            case EquipmentSlot.Helmet:
                break;
            case EquipmentSlot.Shirt:
                characterManager.charAnimController.RemoveAnimController(characterManager.charAnimController.shirtAnim, characterManager.charAnimController.shirtSpriteRenderer);
                break;
            case EquipmentSlot.Pants:
                characterManager.charAnimController.RemoveAnimController(characterManager.charAnimController.pantsAnim, characterManager.charAnimController.pantsSpriteRenderer);
                break;
            case EquipmentSlot.Boots:
                break;
            case EquipmentSlot.Gloves:
                break;
            case EquipmentSlot.BodyArmor:
                break;
            case EquipmentSlot.LegArmor:
                break;
            case EquipmentSlot.Quiver:
                break;
            default:
                break;
        }
    }*/
    #endregion
}

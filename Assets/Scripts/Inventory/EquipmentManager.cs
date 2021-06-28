using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public delegate void OnWearableChanged(ItemData newItemData, ItemData oldItemData);
    public OnWearableChanged onWearableChanged;

    public delegate void OnWeaponChanged(ItemData newItemData, ItemData oldItemData);
    public OnWeaponChanged onWeaponChanged;

    public ItemData[] currentEquipment;

    public float currentWeight, currentVolume;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public CharacterManager characterManager;
    [HideInInspector] public Transform itemsParent;
    [HideInInspector] public bool isPlayer;

    public virtual void Start()
    {
        if (gameObject.CompareTag("NPC"))
        {
            characterManager = GetComponent<CharacterManager>();
            isPlayer = false;
        }
        else
        {
            characterManager = PlayerManager.instance;
            isPlayer = true;
        }

        gm = GameManager.instance;
        itemsParent = transform.Find("Items");

        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        currentEquipment = new ItemData[numSlots];
    }

    public virtual void Equip(ItemData newItemData, EquipmentSlot equipmentSlot)
    {
        ItemData equipmentItemData = gm.objectPoolManager.itemDataObjectPool.GetPooledItemData();
        newItemData.TransferData(newItemData, equipmentItemData);
        equipmentItemData.currentStackSize = 1;
        equipmentItemData.transform.SetParent(itemsParent);
        equipmentItemData.gameObject.SetActive(true);

        #if UNITY_EDITOR
            equipmentItemData.gameObject.name = equipmentItemData.itemName;
        #endif

        currentWeight += Mathf.RoundToInt(equipmentItemData.item.weight * 100f) / 100f;
        currentVolume += Mathf.RoundToInt(equipmentItemData.item.volume * 100f) / 100f;

        AssignEquipment(equipmentItemData, equipmentSlot);

        // If the equipment inventory is active, show the item in the menu
        if (gm.playerInvUI.activeInventory == null && isPlayer)
        {
            gm.playerInvUI.ShowNewInventoryItem(equipmentItemData);
            gm.playerInvUI.UpdateUINumbers();
        }

        // If this is a Wearable item, assign the item's Animator Controller
        if (equipmentItemData.item.IsWeapon() == false)
            SetWearableSprite(equipmentSlot, equipmentItemData);
        else
            SetWeaponSprite(equipmentSlot, equipmentItemData);
    }

    public virtual void Unequip(EquipmentSlot equipmentSlot, bool shouldAddToInventory)
    {
        if (currentEquipment[(int)equipmentSlot] != null)
        {
            ItemData oldItemData = currentEquipment[(int)equipmentSlot];

            UnassignEquipment(oldItemData, equipmentSlot, shouldAddToInventory);

            // If this is a Wearable Item, set the scriptableObject to null
            if (oldItemData.item.IsWeapon() == false)
                RemoveWearableSprite(equipmentSlot);
            else  // If this is a Weapon Item, get rid of the weapon's gameobject
                RemoveWeaponSprite(equipmentSlot, oldItemData);
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

    public EquipmentSlot GetEquipmentSlot(ItemData itemData)
    {
        for (int i = 0; i < currentEquipment.Length; i++)
        {
            if (currentEquipment[i] != null && currentEquipment[i] == itemData)
                return (EquipmentSlot)i;
        }

        return 0;
    }

    void SetWearableSprite(EquipmentSlot wearableSlot, ItemData wearable)
    {
        // TODO
        Debug.Log("Setting wearable sprite");
    }

    void RemoveWearableSprite(EquipmentSlot wearableSlot)
    {
        // TODO
        Debug.Log("Removing wearable sprite");
    }

    void SetWeaponSprite(EquipmentSlot weaponSlot, ItemData weapon)
    {
        // TODO
        Debug.Log("Setting weapon sprite");
    }

    void RemoveWeaponSprite(EquipmentSlot weaponSlot, ItemData weapon)
    {
        // TODO
        Debug.Log("Removing weapon sprite");
    }

    public void SheathWeapon(EquipmentSlot weaponSlot)
    {
        // TODO
        Debug.Log("Sheathing weapon");
    }

    public void UnsheathWeapon(EquipmentSlot weaponSlot)
    {
        // TODO
        Debug.Log("Unsheathing weapon");
    }
}

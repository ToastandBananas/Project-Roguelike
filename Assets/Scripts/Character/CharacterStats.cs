using UnityEngine;
using System.Collections;

public class CharacterStats : MonoBehaviour
{
    public Stat maxHealth;
    public int currentHealth { get; private set; }

    public Stat damage;
    public Stat defense;

    [HideInInspector] public CharacterManager characterManager;
    [HideInInspector] public BoxCollider2D hitCollider;

    [HideInInspector] public bool canTakeDamage = true;

    void Awake()
    {
        currentHealth = maxHealth.GetValue();
    }

    void Start()
    {
        characterManager = GetComponentInParent<CharacterManager>();
        hitCollider = GetComponent<BoxCollider2D>();

        characterManager.equipmentManager.onWearableChanged += OnWearableChanged;
        characterManager.equipmentManager.onWeaponChanged += OnWeaponChanged;
    }

    public void TakeDamage(int damage)
    {
        if (canTakeDamage)
        {
            damage -= defense.GetValue();

            if (damage <= 0)
                damage = 1;

            currentHealth -= damage;
            if (currentHealth <= 0)
                Die();
            //else
                //StartCoroutine(TakeDamageCooldown());
        }
    }

    public IEnumerator TakeDamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(0.25f);
        canTakeDamage = true;
    }

    public virtual void Die()
    {
        Debug.Log(name + " died.");
        characterManager.boxCollider.enabled = false;
        characterManager.vision.visionCollider.enabled = false;
        characterManager.vision.enabled = false;
        hitCollider.enabled = false;

        if (characterManager.stateController != null)
            characterManager.stateController.enabled = false;
    }

    public virtual void OnWearableChanged(ItemData newItemData, ItemData oldItemData)
    {
        if (newItemData != null)
        {
            defense.AddModifier(newItemData.defense);
        }

        if (oldItemData != null)
        {
            defense.RemoveModifier(oldItemData.defense);
        }
    }

    public virtual void OnWeaponChanged(ItemData newItemData, ItemData oldItemData)
    {
        if (newItemData != null)
        {
            damage.AddModifier(newItemData.damage);
        }

        if (oldItemData != null)
        {
            damage.RemoveModifier(oldItemData.damage);
        }
    }
}

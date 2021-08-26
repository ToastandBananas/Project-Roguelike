using UnityEngine;

public class Stats : MonoBehaviour
{
    [HideInInspector] public bool canTakeDamage = true;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public GameManager gm;

    public virtual void Start()
    {
        gm = GameManager.instance;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual int TakeDamage(int damage)
    {
        // Just meant to be overridden
        return damage;
    }

    public virtual int Heal(int healAmount)
    {
        // Just meant to be overridden
        return healAmount;
    }

    public virtual int GetMaxHealth()
    {
        // Just meant to be overridden
        return 1;
    }

    public virtual bool IsDeadOrDestroyed()
    {
        // Just meant to be overridden
        return false;
    }
}

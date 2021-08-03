using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite defaultSprite;
    public Sprite secondarySprite;
    public Sprite deathSprite;
    
    public void SetToDefaultSprite(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.sprite = defaultSprite;
    }

    public void SetToSecondarySprite(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.sprite = secondarySprite;
    }

    public virtual void SetToDeathSprite(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.sprite = deathSprite;
    }

    public virtual void SetToDeathSprite()
    {
        GetComponent<SpriteRenderer>().sprite = deathSprite;
    }
}

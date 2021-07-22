using UnityEngine;

public class CharacterSpriteManager : MonoBehaviour
{
    [Header("Character Sprites")]
    public Sprite defaultCharacterSprite;
    public Sprite secondaryCharacterSprite;
    public Sprite deathSprite;

    public void SetToDefaultCharacterSprite(CharacterManager characterManager)
    {
        characterManager.spriteRenderer.sprite = defaultCharacterSprite;
    }

    public void SetToSecondaryCharacterSprite(CharacterManager characterManager)
    {
        characterManager.spriteRenderer.sprite = secondaryCharacterSprite;
    }

    public virtual void SetToDeathSprite(CharacterManager characterManager)
    {
        characterManager.spriteRenderer.sprite = deathSprite;
    }
}

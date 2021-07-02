using UnityEngine;
using UnityEngine.UI;

public class AddItemEffect : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [SerializeField] Image image;

    public void DoEffect_Left(Sprite sprite, float sideBarYPosition)
    {
        image.sprite = sprite;
        transform.position = new Vector2(-105, sideBarYPosition);
        anim.Play("AddItemLeft");
    }

    public void DoEffect_Right(Sprite sprite, float sideBarYPosition)
    {
        image.sprite = sprite;
        transform.position = new Vector2(105, sideBarYPosition);
        anim.Play("AddItemRight");
    }

    public void DisableEffect()
    {
        image.color = Color.white;
        anim.Play("Inactive");
        gameObject.SetActive(false);
    }
}

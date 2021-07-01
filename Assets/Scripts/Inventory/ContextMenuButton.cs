using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ContextMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public TextMeshProUGUI textMesh;

    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gm.uiManager.activeContextMenuButton = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gm.uiManager.activeContextMenuButton == this)
            gm.uiManager.activeContextMenuButton = null;
    }
}

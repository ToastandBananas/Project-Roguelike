using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryMenuBackground : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public InventoryUI myInvUI;

    UIManager uiManager;

    void Start()
    {
        uiManager = UIManager.instance;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiManager.activeInvUI = myInvUI;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiManager.activeInvUI == myInvUI)
            uiManager.activeInvUI = null;
    }
}

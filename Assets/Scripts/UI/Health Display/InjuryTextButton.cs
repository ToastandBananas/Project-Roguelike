using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class InjuryTextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI injuryText;
    public Button button;

    [HideInInspector] public LocationalInjury locationalInjury;

    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gm.healthDisplay.focusedInjuryTextButton = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gm.healthDisplay.focusedInjuryTextButton == this)
            gm.healthDisplay.focusedInjuryTextButton = null;
    }
}

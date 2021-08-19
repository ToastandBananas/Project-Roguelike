using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HealthDisplay_BodyPart : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image image;
    [SerializeField] BodyPartType bodyPartType;
    [HideInInspector] public BodyPart bodyPart;

    readonly string lightGray = "#4B4B4B";
    readonly string darkGray = "#2E2E2E";
    readonly string red = "#C81236";
    readonly string orange = "#FE6E00";
    readonly string blue = "#0055B3";
    readonly string green = "#387532";

    StringBuilder stringBuilder = new StringBuilder();
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;

        bodyPart = gm.playerManager.status.GetBodyPart(bodyPartType);
    }

    void Update()
    {
        if (GameControls.gamePlayActions.menuSelect.WasPressed && gm.healthDisplay.focusedBodyPart == this && gm.healthDisplay.selectedBodyPart != this)
            gm.healthDisplay.selectedBodyPart = this;
    }

    public void GenerateTooltipTexts()
    {
        gm.healthDisplay.ClearInjuryTextButtons();
        gm.healthDisplay.SetTooltipHeader(bodyPartType);

        if (bodyPart.injuries.Count == 0)
        {
            stringBuilder.Clear();
            stringBuilder.Append("No injuries or ailments.");
            gm.healthDisplay.SetTooltipText(stringBuilder.ToString(), null, false);
        }
        else
        {
            for (int i = 0; i < bodyPart.injuries.Count; i++)
            {
                stringBuilder.Clear();

                // Injury Name
                stringBuilder.Append("<size=22>" + bodyPart.injuries[i].injury.name + "</size>\n"); // If not fully healed
                
                if (bodyPart.injuries[i].injuryTimeRemaining <= 0)
                    stringBuilder.Append("- <color=" + green + ">Fully Healed</color>\n");

                // Bandaged?
                if (bodyPart.injuries[i].bandage != null)
                    stringBuilder.Append("- <color=" + blue + ">" + "Bandaged with " + bodyPart.injuries[i].bandage.name + " (Soilage: " + bodyPart.injuries[i].bandageItemData.GetSoilage().ToString("#0") + "%)</color>\n");

                // If the wound can bleed...
                if (bodyPart.injuries[i].injury.GetBleedTime().y > 0 && bodyPart.injuries[i].injuryTimeRemaining > 0)
                {
                    // Bleed Severity
                    if (bodyPart.injuries[i].bleedTimeRemaining > 0)
                        stringBuilder.Append("- <color=" + red + ">" + bodyPart.injuries[i].GetBleedSeverity() + "</color>");
                    else
                        stringBuilder.Append("- <color=" + orange + ">Bleeding Stopped</color>");
                }

                gm.healthDisplay.SetTooltipText(stringBuilder.ToString(), bodyPart.injuries[i], true);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = Utilities.HexToRGBAColor(darkGray);
        gm.healthDisplay.focusedBodyPart = this;
        GenerateTooltipTexts();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = Utilities.HexToRGBAColor(lightGray);
        if (gm.healthDisplay.focusedBodyPart == this)
        {
            gm.healthDisplay.focusedBodyPart = null;

            if (gm.healthDisplay.selectedBodyPart == null)
                gm.healthDisplay.HideTooltip();
            else
                gm.healthDisplay.selectedBodyPart.GenerateTooltipTexts();
        }
    }
}

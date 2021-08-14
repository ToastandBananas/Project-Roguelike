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
        {
            gm.healthDisplay.selectedBodyPart = this;
        }
    }

    public void GenerateTooltipText()
    {
        stringBuilder.Clear();

        stringBuilder.Append("<size=24>" + Utilities.FormatEnumStringWithSpaces(bodyPartType.ToString(), true) + "</size>\n\n");

        if (bodyPart.injuries.Count == 0)
            stringBuilder.Append("<size=20>No injuries or ailments.</size>");
        else
        {
            for (int i = 0; i < bodyPart.injuries.Count; i++)
            {
                // Injury Name
                stringBuilder.Append("<size=22>" + bodyPart.injuries[i].injury.name + "</size>\n");

                // Bandaged?
                if (bodyPart.injuries[i].bandage != null)
                    stringBuilder.Append("- <color=" + blue + ">" + "Bandaged with " + bodyPart.injuries[i].bandage.name + " (Soilage: " + bodyPart.injuries[i].bandageSoil.ToString("#0") + "%)</color>\n");

                // Bleed Severity
                if (bodyPart.injuries[i].bleedTimeRemaining > 0)
                    stringBuilder.Append("- <color=" + red + ">" + GetBleedSeverity(bodyPart.injuries[i]) + "</color>\n");
                else if (bodyPart.injuries[i].injury.GetBleedTime().y > 0)
                    stringBuilder.Append("- <color=" + orange + ">Bleeding Stopped</color>\n");

                if (i != bodyPart.injuries.Count - 1)
                    stringBuilder.Append("\n");
            }
        }

        gm.healthDisplay.SetTooltipText(stringBuilder.ToString());
    }

    string GetBleedSeverity(LocationalInjury locationalInjury)
    {
        if (locationalInjury.bloodLossPerTurn <= 1)
            return "Barely Bleeding";
        else if (locationalInjury.bloodLossPerTurn <= 3)
            return "Bleeding Lightly";
        else if (locationalInjury.bloodLossPerTurn <= 6)
            return "Bleeding";
        else if (locationalInjury.bloodLossPerTurn <= 9.5f)
            return "Bleeding Moderately";
        else if (locationalInjury.bloodLossPerTurn <= 13.5f)
            return "Bleeding Heavily";
        else
            return "Bleeding Severely";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = Utilities.HexToRGBAColor(darkGray);
        gm.healthDisplay.focusedBodyPart = this;
        GenerateTooltipText();
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
                gm.healthDisplay.selectedBodyPart.GenerateTooltipText();
        }
    }
}

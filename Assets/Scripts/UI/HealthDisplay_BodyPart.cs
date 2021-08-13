using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HealthDisplay_BodyPart : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image image;
    [SerializeField] BodyPartType bodyPartType;
    BodyPart bodyPart;

    string lightGray = "#4B4B4B";
    string darkGray = "#2E2E2E";
    string red = "#C81236";
    string orange = "#FE6E00";

    StringBuilder stringBuilder = new StringBuilder();
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;

        bodyPart = gm.playerManager.status.GetBodyPart(bodyPartType);
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
                stringBuilder.Append("<size=22>" + bodyPart.injuries[i].injury.name + "</size>\n");
                if (bodyPart.injuries[i].bleedTimeRemaining > 0)
                    stringBuilder.Append("<color=" + red + ">" + GetBleedSeverity(bodyPart.injuries[i]) + "</color>");
                else if (bodyPart.injuries[i].injury.GetBleedTime().y > 0)
                    stringBuilder.Append("<color=" + orange + ">" + "Bleeding Stopped</color>");

                if (i != bodyPart.injuries.Count - 1)
                    stringBuilder.Append("\n\n");
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
        gm.healthDisplay.activeBodyPart = this;
        GenerateTooltipText();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = Utilities.HexToRGBAColor(lightGray);
        if (gm.healthDisplay.activeBodyPart == this)
        {
            gm.healthDisplay.activeBodyPart = null;
            gm.healthDisplay.HideTooltip();
        }
    }
}

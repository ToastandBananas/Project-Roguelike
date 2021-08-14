using UnityEngine;
using TMPro;
using System;
using System.Text;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tooltipText;
    [SerializeField] GameObject tooltipParent;

    [Header("Header Texts")]
    [SerializeField] TextMeshProUGUI headHeader;
    [SerializeField] TextMeshProUGUI torsoHeader, leftArmHeader, leftHandHeader, leftLegHeader, leftFootHeader, rightArmHeader, rightHandHeader, rightLegHeader, rightFootHeader;

    [Header("Health Texts")]
    [SerializeField] TextMeshProUGUI headHealthText;
    [SerializeField] TextMeshProUGUI torsoHealthText, leftArmHealthText, leftHandHealthText, leftLegHealthText, leftFootHealthText;
    [SerializeField] TextMeshProUGUI rightArmHealthText, rightHandHealthText, rightLegHealthText, rightFootHealthText;

    [HideInInspector] public HealthDisplay_BodyPart focusedBodyPart;
    [HideInInspector] public HealthDisplay_BodyPart selectedBodyPart;

    StringBuilder stringBuilder = new StringBuilder();
    GameManager gm;

    readonly string red = "#C81236";
    readonly string orange = "#FE6E00";
    readonly string blue = "#0055B3";

    #region Singleton
    public static HealthDisplay instance;
    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Debug.LogWarning("More than one instance of HealthDisplay. Fix me!");
                Destroy(gameObject);
            }
        }
        else
            instance = this;
    }
    #endregion

    void Start()
    {
        gm = GameManager.instance;

        UpdateAllHealthTexts();
        HideTooltip();
    }

    public void UpdateAllHealthTexts()
    {
        for (int i = 0; i < Enum.GetValues(typeof(BodyPartType)).Length; i++)
        {
            UpdateHealthText((BodyPartType)Enum.GetValues(typeof(BodyPartType)).GetValue(i));
        }
    }

    public void UpdateHealthText(BodyPartType bodyPartType)
    {
        BodyPart bodyPart = gm.playerManager.status.GetBodyPart(bodyPartType);

        if (bodyPart != null)
        {
            stringBuilder.Clear();
            stringBuilder.Append(bodyPart.currentHealth + "/" + bodyPart.maxHealth.GetValue());

            switch (bodyPartType)
            {
                case BodyPartType.Torso:
                    torsoHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.Head:
                    headHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.LeftArm:
                    leftArmHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.RightArm:
                    rightArmHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.LeftLeg:
                    leftLegHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.RightLeg:
                    rightLegHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.LeftHand:
                    leftHandHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.RightHand:
                    rightHandHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.LeftFoot:
                    leftFootHealthText.text = stringBuilder.ToString();
                    break;
                case BodyPartType.RightFoot:
                    rightFootHealthText.text = stringBuilder.ToString();
                    break;
                default:
                    break;
            }
        }

        UpdateHealthHeaderColor(bodyPartType, bodyPart);
    }

    public void UpdateHealthHeaderColor(BodyPartType bodyPartType, BodyPart bodyPart = null)
    {
        if (bodyPart == null)
            bodyPart = gm.playerManager.status.GetBodyPart(bodyPartType);

        Color headerColor = Color.white;
        if (bodyPart.IsBleeding())
            headerColor = Utilities.HexToRGBAColor(red);
        else if (bodyPart.injuries.Count > 0)
        {
            bool allInjuriesRemedied = true;
            for (int i = 0; i < bodyPart.injuries.Count; i++)
            {
                if (bodyPart.injuries[i].InjuryRemedied() == false)
                {
                    allInjuriesRemedied = false;
                    break;
                }
            }

            if (allInjuriesRemedied)
                headerColor = Utilities.HexToRGBAColor(blue);
            else
                headerColor = Utilities.HexToRGBAColor(orange);
        }

        switch (bodyPartType)
        {
            case BodyPartType.Torso:
                torsoHeader.color = headerColor;
                break;
            case BodyPartType.Head:
                headHeader.color = headerColor;
                break;
            case BodyPartType.LeftArm:
                leftArmHeader.color = headerColor;
                break;
            case BodyPartType.RightArm:
                rightArmHeader.color = headerColor;
                break;
            case BodyPartType.LeftLeg:
                leftLegHeader.color = headerColor;
                break;
            case BodyPartType.RightLeg:
                rightLegHeader.color = headerColor;
                break;
            case BodyPartType.LeftHand:
                leftHandHeader.color = headerColor;
                break;
            case BodyPartType.RightHand:
                rightHandHeader.color = headerColor;
                break;
            case BodyPartType.LeftFoot:
                leftFootHeader.color = headerColor;
                break;
            case BodyPartType.RightFoot:
                rightFootHeader.color = headerColor;
                break;
            default:
                break;
        }
    }

    public void SetTooltipText(string text)
    {
        tooltipText.text = text;
        ShowTooltip();
    }

    public void ShowTooltip()
    {
        tooltipParent.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipParent.SetActive(false);
        selectedBodyPart = null;
    }

    public void UpdateTooltip()
    {
        if (focusedBodyPart != null)
            focusedBodyPart.GenerateTooltipText();
        else if (selectedBodyPart != null)
            selectedBodyPart.GenerateTooltipText();
    }
}

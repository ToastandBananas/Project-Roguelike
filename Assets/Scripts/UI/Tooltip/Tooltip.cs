using UnityEngine;
using TMPro;
using System.Text;
// using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI textMesh;

    [HideInInspector] public StringBuilder stringBuilder = new StringBuilder();

    public void ShowTooltip()
    {
        gameObject.SetActive(true);
        textMesh.text = stringBuilder.ToString();
        RecalculateTooltipSize();
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
        stringBuilder.Clear();
    }

    void RecalculateTooltipSize()
    {
        // LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}

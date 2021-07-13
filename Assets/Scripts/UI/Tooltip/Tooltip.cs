using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI textMesh;

    [HideInInspector] public StringBuilder stringBuilder = new StringBuilder();

    public void ShowTooltip(Vector2 position)
    {
        gameObject.SetActive(true);
        textMesh.text = stringBuilder.ToString();
        rectTransform.position = position;
        AdjustTooltipPosition();
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
        stringBuilder.Clear();
    }

    public virtual void BuildTooltip(ItemData itemData)
    {
        // This is just meant to be overridden
    }

    void AdjustTooltipPosition()
    {
        float xOffset = 0f;
        float yOffset = 0f;

        if (Input.mousePosition.x <= rectTransform.sizeDelta.x + 25f)
            xOffset = rectTransform.sizeDelta.x;

        if (Input.mousePosition.y >= 1080 - rectTransform.sizeDelta.y)
            yOffset = -rectTransform.sizeDelta.y;
        
        if (xOffset != 0 || yOffset != 0)
            rectTransform.position += new Vector3(xOffset, yOffset);
    }
}

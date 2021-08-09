using TMPro;
using UnityEngine;

public class TextPopup : MonoBehaviour
{
    public TextMeshPro textMesh;
    Color textColor;

    string defaultHitColor = "FF1100"; // Reddish-Orange
    // string criticalHitColor = "FF7700"; // Orangish-Yellow

    Color positiveValueColor = Color.green; // Green
    Color negativeValueColor = Color.red;   // Red

    static int sortingOrder;

    const float DISAPPEAR_TIMER_MAX = 1f;
    float disappearTimer;
    readonly float disappearSpeed = 3f;
    readonly float increaseScaleAmount = 0.2f;
    readonly float decreaseScaleAmount = 0.25f;

    Vector3 moveVector;
    Vector3 defaultMoveVector = new Vector3(1.5f, 1.75f);
    Vector3 offset = new Vector2(0.2f, 0.2f);

    void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 3f * Time.deltaTime;

        if (disappearTimer > DISAPPEAR_TIMER_MAX * 0.5f)
        {
            // First half of the popup's lifetime
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            // Second half of the popup's lifetime
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0f)
        {
            // Start disappearing
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a <= 0f)
                gameObject.SetActive(false);
        }
    }

    // Create a damage popup
    public static TextPopup CreateDamagePopup(Vector3 position, float damageAmount, bool isCriticalHit)
    {
        TextPopup damagePopup = GameManager.instance.objectPoolManager.textPopupObjectPool.GetPooledTextPopup();
        
        damagePopup.SetupDamagePopup(position, damageAmount, isCriticalHit);

        return damagePopup;
    }

    // Create a heal popup
    public static TextPopup CreateHealPopup(Vector3 position, float healAmount)
    {
        TextPopup healPopup = GameManager.instance.objectPoolManager.textPopupObjectPool.GetPooledTextPopup();

        healPopup.SetupHealPopup(position, healAmount);

        return healPopup;
    }

    // Create a text popup with a given string
    public static TextPopup CreateTextStringPopup(Vector3 position, string stringText)
    {
        TextPopup textPopup = GameManager.instance.objectPoolManager.textPopupObjectPool.GetPooledTextPopup();

        textPopup.SetupTextStringPopup(position, stringText);

        return textPopup;
    }

    void SetupDamagePopup(Vector3 position, float damageAmount, bool isCriticalHit)
    {
        ResetPopup();

        textMesh.SetText(damageAmount.ToString());
        textColor = Utilities.HexToRGBAColor(defaultHitColor);
        textMesh.color = textColor;

        if (isCriticalHit)
            textMesh.fontSize = 4f;
        else // Normal Hit
            textMesh.fontSize = 3f;

        sortingOrder++;
        textMesh.sortingOrder = sortingOrder;

        gameObject.SetActive(true);
        transform.position = position;
    }

    void SetupHealPopup(Vector3 position, float healAmount)
    {
        ResetPopup();

        textMesh.SetText(healAmount.ToString());
        textMesh.color = positiveValueColor;
        textMesh.fontSize = 3f;

        sortingOrder++;
        textMesh.sortingOrder = sortingOrder;

        gameObject.SetActive(true);
        transform.position = position;
    }

    void SetupTextStringPopup(Vector3 position, string stringText)
    {
        ResetPopup();

        textMesh.SetText(stringText);
        textMesh.color = Color.white;
        textMesh.fontSize = 2.5f;

        sortingOrder++;
        textMesh.sortingOrder = sortingOrder;

        moveVector = defaultMoveVector;
        gameObject.SetActive(true);
        transform.position = position + offset;
    }

    void ResetPopup()
    {
        // Reset the size, move vector and disappear timer
        transform.localScale = Vector3.one;
        moveVector = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
        disappearTimer = DISAPPEAR_TIMER_MAX;
    }
}

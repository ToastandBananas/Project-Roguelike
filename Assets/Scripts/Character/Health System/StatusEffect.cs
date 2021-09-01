using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Status Effect/Basic")]
public class StatusEffect : ScriptableObject
{
    [Header("Main Stats")]
    [Range(-1f, 1f)] public float speedMultiplier;

    [Header("Stamina")]
    [Range(-1f, 1f)] public float maxStaminaMultiplier;
    [Range(-1f, 1f)] public float staminaRegenModifier;

    [Header("Healthiness")]
    [Range(-1f, 1f)] public float healthinessAdjustmentPerTurn;

    [Header("Nausea")]
    [Range(-100f, 100f)] public float nauseaPerTurn;
}

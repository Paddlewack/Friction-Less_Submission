using UnityEngine;

[CreateAssetMenu(fileName = "AiTargetingSysData", menuName = "Scriptable Objects/AiTargetingSysData")]
public class AiTargetingSysData : ScriptableObject
{
    /// <summary>
    /// The range within which the AI can detect targets.
    /// </summary>
    public float detectionZoneRadius;
    /// <summary>
    /// The angle in degrees within which the AI can detect targets, with the AI's forward direction at the centre of this angle.
    /// </summary>
    public float detctionZoneAngle;
    /// <summary>
    /// Faction which the AI will attempt to detect and target.
    /// </summary>
    public Faction TargetFaction;
    /// <summary>
    /// The speed which the AI turns toward a detected target.
    /// </summary>
    public float targetingSpeed;
    /// <summary>
    /// Time in seconds between checking for targets.
    /// </summary>
    public float scanRate;
    /// <summary>
    /// Time in seconds which the AI remains in active targeting state before returning to passive state after losing sight of a target.
    /// </summary>
    public float targetingDeactivationTime;

    public LayerMask AimPointMask;
    public float preFiringTime;
    public float postFiringTime;
}

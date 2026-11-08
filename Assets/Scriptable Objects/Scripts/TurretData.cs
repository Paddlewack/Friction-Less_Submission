using UnityEngine;

[CreateAssetMenu(fileName = "TurretData", menuName = "Scriptable Objects/TurretData")]
public class TurretData : ScriptableObject
{
    [Header("Speeds (deg/sec)")]
    public float traverseSpeed;
    public float elevationSpeed;
    public float minPitch;
    public float maxPitch;
    public float MaxYawDelta;
    public float AimPointSnapAngle;
    public LayerMask AimPointMask;
}

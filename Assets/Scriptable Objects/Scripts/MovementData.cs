using UnityEngine;

[CreateAssetMenu(fileName = "MovementData", menuName = "Scriptable Objects/MovementData")]
public class MovementData : ScriptableObject
{

    [Header("Throttle Settings")]

    [Tooltip("Whether the tank must be grounded to move")]
    public bool requiesGroundedToMove = true;

    [Tooltip("Rate at which the tank accelerates when forward input is applied (units/s)")]
    public float forwardAcceleration = 18f;

    [Tooltip("Rate at which the tank accelerates when forward input is applied (units/s)")]
    public float reverseAcceleration = 8f;

    [Tooltip("Rate at which the tank can accelerate while drifting (units/s) - intentionally small")]
    public float driftAcceleration = 4f;

    [Tooltip("Maximum forward speed the tank can normally reach (units/s)")]
    public float maxSpeed = 30f;

    [Tooltip("Rate at which the tank decelerates when no input is applied (units/s)")]
    public float freeDeceleration = 8f;

    [Tooltip("Rate at which the tank decelerates when movement input opposite of velocity is applied (units/s)")]
    public float brakingDeceleration = 25f;

    [Tooltip("Rate at which the tank decelerates when braking during a drift (units/s) — intentionally small")]
    public float driftBrakingDeceleration = 6f;

    [Tooltip("Heavy deceleration rate applied when speed exceeds maxSpeed (e.g. after an ability boost) during normal movement (units/s)")]
    public float overSpeedDeceleration = 5f;

    [Tooltip("Throttle reduction applied to throttle input when applying turn input. 0.5 means up to 50% throttle reduction at full turn input.")]
    public float throttleScaleSensitivity = 0.5f;

    [Tooltip("Throttle reduction applied to throttle input when applying turn input while drifting. 0.5 means up to 50% throttle reduction at full turn input.")]
    public float driftThrottleScaleSensitivity = 0.3f;


    [Header("Steering Settings")]

    [Tooltip("Normal strength of steering input response (degrees/s)")]
    public float normalSteerStrength = 800f;

    [Tooltip("Drift strength of steering input response (degrees/s)")]
    public float diftSteerStrength = 800f;

    [Tooltip("Animation curve that modulates turning strength based on forward velocity compared to max speed")]
    public AnimationCurve turningCurve;

    [Tooltip("Coefficient for lateral drag force opposing sideways movement during normal movement")]
    public float normalDragCoefficent = 10f;

    [Tooltip("Coefficient for lateral drag force opposing sideways movement during drift")]
    public float driftDragCoefficent = 0.1f;

    [Tooltip("Steering reduction applied to steering input when applying throttle input. 0.5 means up to 50% steering reduction at full throttle input.")]
    public float steeringScaleSensitivity = 0.5f;

    [Tooltip("Steering reduction applied to steering input when applying throttle input while drifting. 0.5 means up to 50% steering reduction at full throttle input.")]
    public float driftSteeringScaleSensitivity = 0.3f;

    [Header("Drift Specific Settings")]

    [Tooltip("Velocity at which the tank can begin a drift")]
    public float driftStartSpeed = 15f;

    [Tooltip("Velocity at which the tank will be forced out of a drift")]
    public float driftStopSpeed = 5f;

    [Tooltip("Rate at which lateral (sideways) velocity is redirected into the forward direction during a drift (units/s)")]
    public float velocityRedirectionStrength = 15f;


    [Header("Model Posing")]

    [Tooltip("Speed at which the model rotates toward the velocity direction during normal movement")]
    public float normalModelRotationSpeed = 5f;

    [Tooltip("Maximum angle the model can deviate from physics body during normal movement (degrees)")]
    public float normalMaxTiltAngle = 8f;

    [Tooltip("Speed at which the model rotates toward the velocity direction during drifting")]
    public float driftModelRotationSpeed = 5f;

    [Tooltip("Maximum angle the model can deviate from physics body during drifts (degrees)")]
    public float driftMaxTiltAngle = 35f;
}

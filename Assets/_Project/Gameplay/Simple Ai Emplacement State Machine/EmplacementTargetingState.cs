using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EmplacementTargetingState : IEmplacementState
{
    float timeSinceTargetSeen;
    float targetingDeactivationTime;
    EmplacementStateManager emplSM;
    /// <summary>
    /// The points belonging to the emplacements current target most recently visible to the emplacement.
    /// </summary>
    List<Vector3> lastVisiblePoints = new List<Vector3>();
    private LayerMask aimPointMask;
    private float range;

    //For FMOD event
    private float _lastDetectedSoundTime = -99f;
    private const float DetectionSoundCooldown = 8f;
    public static UnityEvent PlayerDetected = new UnityEvent();


    public EmplacementTargetingState(EmplacementStateManager e, float deactivationTime)
    {
        emplSM = e;
        targetingDeactivationTime = deactivationTime;
        aimPointMask = emplSM.targetingSysData.AimPointMask;
        range = emplSM.targetingSysData.detectionZoneRadius;
    }
    public void Enter()
    {
        timeSinceTargetSeen = 0f;

        if (Time.time - _lastDetectedSoundTime >= DetectionSoundCooldown)
        {
            _lastDetectedSoundTime = Time.time;
            PlayerDetected.Invoke();
        }
    }

    public void Exit() { }
    public void Update()
    {
        Debug.Log("TARGETING");
        timeSinceTargetSeen += Time.deltaTime;
        if (lastVisiblePoints.Count > 0)
        {
            Vector3 targetDir = new();
            // Aim at the average of targetable points where target last seen.
            foreach (var point in lastVisiblePoints)
            {
                targetDir += (point - emplSM.cannonPivot.position).normalized;
            }
            targetDir = targetDir.normalized;
            emplSM.SetTurretTargetDir(targetDir);
            // Fire if the turret is pointed at the target.
            Debug.DrawRay(emplSM.cannonTip.position, emplSM.emplacementController.cannonPivot.transform.forward * range, Color.purple);
            if (Physics.Raycast(emplSM.cannonTip.position, emplSM.emplacementController.cannonPivot.transform.forward * range, out RaycastHit turretForwardHit, aimPointMask))
            {
                if (turretForwardHit.collider.transform.root.gameObject == emplSM.GetCurrentTarget() && emplSM.emplacementController.lastFireTime + emplSM.emplacementController.weaponSystem.CurrentWeapon.FireRate < Time.time)
                {
                    emplSM.SwitchTo(EmplacementStateId.Firing);
                }
            }
        }
        
        if (timeSinceTargetSeen > targetingDeactivationTime)
        {
            emplSM.SwitchTo(EmplacementStateId.Passive);
        }
    }
    /// <summary>
    /// Sets the last visible points in the state manager to the visible points associated with the current target given by targeting system.
    /// </summary>
    /// <param name="visibleTargetPoints">Passed from AI Targeting System. Key: target game object. Value: List of targetable points on the game object.</param>
    public void OnTargetSeen(Dictionary<GameObject, List<Transform>> visibleTargetPoints)
    {
        if (visibleTargetPoints.ContainsKey(emplSM.GetCurrentTarget()))
        {
            timeSinceTargetSeen = 0;
            lastVisiblePoints.Clear();
            foreach (Transform point in visibleTargetPoints[emplSM.GetCurrentTarget()])
            {
                lastVisiblePoints.Add(point.position);
            }
        }
    }
    void Fire()
    {
        
    }
}

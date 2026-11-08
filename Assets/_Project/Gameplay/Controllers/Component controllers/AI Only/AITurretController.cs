using UnityEngine;

public class AITurretController : TurretController
{
    public AITurretController(AimingSystem aimSys, GameObject turret, Transform cannonPivot, Transform cannonTip, Transform parentTransform, TurretData d) : base(aimSys, turret, cannonPivot, cannonTip, parentTransform, d)
    {
    }

    public override void Update()
    {
        
    }

    public override void LateUpdate()
    {
        // --- Turret traverse ---
        trackedTurretWorldYaw = Mathf.MoveTowardsAngle(
            trackedTurretWorldYaw,
            aimingSystem.Yaw,
            traverseSpeed * Time.deltaTime
        );

        // Clamp to MaxYawDelta arc around the tank's forward
        if (data.MaxYawDelta > 0)
        {
            float tankWorldYaw = parentTransform.eulerAngles.y;
            float delta = Mathf.DeltaAngle(tankWorldYaw, trackedTurretWorldYaw);
            delta = Mathf.Clamp(delta, -data.MaxYawDelta, data.MaxYawDelta);
            trackedTurretWorldYaw = tankWorldYaw + delta;
        }

        // Get parent rotation
        Quaternion actualParentRot = turretTransform.parent != null
            ? turretTransform.parent.rotation
            : Quaternion.identity;

        // Build target rotation using quaternion composition:
        // Start with parent's rotation (inherited pitch/roll on bumps)
        // Apply independent yaw rotation around the inherited up vector
        Quaternion yawRotation = Quaternion.Euler(0f, trackedTurretWorldYaw, 0f);
        Quaternion targetWorldRot = yawRotation * Quaternion.Inverse(Quaternion.Euler(0f, actualParentRot.eulerAngles.y, 0f));
        targetWorldRot = actualParentRot * targetWorldRot;

        // Convert to local space
        turretTransform.localRotation = Quaternion.Inverse(actualParentRot) * targetWorldRot;

        // --- Cannon elevation ---
        Quaternion targetCannonRot = Quaternion.Euler(aimingSystem.Pitch, 0f, 0f);
        cannonPivotTransform.localRotation = Quaternion.RotateTowards(
            cannonPivotTransform.localRotation,
            targetCannonRot,
            elevationSpeed * Time.deltaTime
        );
    }
}
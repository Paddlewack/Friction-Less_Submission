using UnityEngine;

public class PlayerTurretController : TurretController
{
    public PlayerTurretController(AimingSystem aimSys, GameObject turret, Transform cannonPivot, Transform cannonTip, Transform parentTransform, TurretData d) : base(aimSys, turret, cannonPivot, cannonTip, parentTransform, d)
    {
    }

    public override void Update()
    {
        // Calculate target angles based on aim point (if using world point system)
        float targetYaw = aimingSystem.Yaw;
        float targetPitch = aimingSystem.Pitch;

        if (aimingSystem is PlayerAimingSystem playerAimingSystem)
        {
            Vector3 directionToAimPoint = playerAimingSystem.CameraAimPoint - cannonTipTransform.position;
            Vector3 flatDirection = playerAimingSystem.CameraAimPoint - turretTransform.position;
            flatDirection.y = 0f;

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                targetYaw = Mathf.Atan2(
                    flatDirection.x,
                    flatDirection.z
                ) * Mathf.Rad2Deg;
            }

            Vector3 localDir = turretTransform.InverseTransformDirection(
                directionToAimPoint.normalized
            );

            targetPitch = Mathf.Atan2(
                -localDir.y,
                localDir.z
            ) * Mathf.Rad2Deg;

            // Clamp elevation limits
            targetPitch = Mathf.Clamp(
                targetPitch,
                data.minPitch,
                data.maxPitch
            );
        }

        // --- Turret traverse ---
        trackedTurretWorldYaw = Mathf.MoveTowardsAngle(
            trackedTurretWorldYaw,
            targetYaw,
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
        Quaternion targetCannonRot = Quaternion.Euler(targetPitch, 0f, 0f);
        cannonPivotTransform.localRotation = Quaternion.RotateTowards(
            cannonPivotTransform.localRotation,
            targetCannonRot,
            elevationSpeed * Time.deltaTime
        );

        Debug.DrawRay(
            cannonTipTransform.position,
            cannonTipTransform.forward * 200f,
            Color.red);
    }
    public override void LateUpdate()
    {
    }
}
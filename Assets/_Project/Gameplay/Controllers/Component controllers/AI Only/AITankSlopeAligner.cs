using UnityEngine;

public class AITankSlopeAligner : MonoBehaviour
{
    [SerializeField] private Transform hullVisual;
    [SerializeField] private Transform colliders;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayLength = 3f;
    [SerializeField] private float tiltSharpness = 8f;
    [SerializeField] private float maxTiltAngle = 35f;

    private Vector3 _smoothedNormal = Vector3.up;

    private void LateUpdate()
    {
        Vector3 targetNormal = Vector3.up;
        Vector3 origin = transform.position + Vector3.up * 1f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, groundMask))
            targetNormal = hit.normal;

        // Don't tilt past a limit (steep walls, props, etc.)
        if (Vector3.Angle(Vector3.up, targetNormal) > maxTiltAngle)
            targetNormal = Vector3.RotateTowards(Vector3.up, targetNormal,
                                                 maxTiltAngle * Mathf.Deg2Rad, 0f);

        // Frame-rate independent smoothing
        float t = 1f - Mathf.Exp(-tiltSharpness * Time.deltaTime);
        _smoothedNormal = Vector3.Slerp(_smoothedNormal, targetNormal, t).normalized;

        // Keep the heading from the root, tilt it onto the slope
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, _smoothedNormal);
        if (fwd.sqrMagnitude < 0.0001f) return;

        hullVisual.rotation = Quaternion.LookRotation(fwd, _smoothedNormal);
        colliders.rotation = Quaternion.LookRotation(fwd, _smoothedNormal);
    }
}
using UnityEngine;

/// <summary>
/// Defines a patrol route with multiple waypoints.
/// Assign patrol points in the Inspector.
/// </summary>
public class AIPatrolRoute : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Patrol points for the AI to follow in order")]
    private Transform[] patrolPoints = new Transform[0];

    [SerializeField]
    [Tooltip("Loop back to first point after reaching the last")]
    private bool loopRoute = true;

    [SerializeField]
    [Tooltip("Wait time at each patrol point (seconds)")]
    private float waitTimeAtPoint = 0f;

    private int currentPointIndex = 0;

    public Vector3 GetCurrentPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("[AIPatrolRoute] No patrol points assigned!");
            return transform.position;
        }
        return patrolPoints[currentPointIndex].position;
    }

    public Vector3 GetNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return transform.position;

        currentPointIndex++;
        if (currentPointIndex >= patrolPoints.Length)
        {
            if (loopRoute)
            {
                currentPointIndex = 0;
            }
            else
            {
                currentPointIndex = patrolPoints.Length - 1;
            }
        }

        return patrolPoints[currentPointIndex].position;
    }

    public int GetPatrolPointCount()
    {
        return patrolPoints.Length;
    }

    public int GetCurrentPointIndex()
    {
        return currentPointIndex;
    }

    public void ResetRoute()
    {
        currentPointIndex = 0;
    }

    public Transform[] GetAllPatrolPoints()
    {
        return patrolPoints;
    }

    public float GetWaitTimeAtPoint()
    {
        return waitTimeAtPoint;
    }

    public bool IsLooping()
    {
        return loopRoute;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Draw patrol points as yellow spheres
        Gizmos.color = Color.yellow;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;

            Gizmos.DrawSphere(patrolPoints[i].position, 1.5f);

            // Draw point number
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(patrolPoints[i].position + Vector3.up, $"P{i}");
            #endif
        }

        // Draw path lines between points
        Gizmos.color = Color.green;
        for (int i = 0; i < patrolPoints.Length - 1; i++)
        {
            if (patrolPoints[i] == null || patrolPoints[i + 1] == null) continue;
            Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
        }

        // Draw loop line if enabled
        if (loopRoute && patrolPoints.Length > 1)
        {
            if (patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
            }
        }

        // Highlight current patrol point in red
        if (patrolPoints.Length > 0 && patrolPoints[currentPointIndex] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(patrolPoints[currentPointIndex].position, 2f);
        }
    }
#endif
}
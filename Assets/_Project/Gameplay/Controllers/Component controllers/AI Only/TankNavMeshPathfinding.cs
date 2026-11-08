using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles NavMesh pathfinding for AI tanks.
/// Converts NavMesh paths into waypoints for custom tank movement.
/// </summary>
public class TankNavMeshPathfinding : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    [SerializeField]
    private float waypointStoppingDistance = 2f;
    [SerializeField]
    private float pathRecalculateDistance = 5f;
    [SerializeField]
    private int navMeshAreaMask = NavMesh.AllAreas;

    private NavMeshPath _currentPath;
    private Vector3[] _pathCorners = new Vector3[0];
    private int _currentCornerIndex = 0;
    private Vector3 _lastPathStart = Vector3.zero;
    private bool _hasValidPath = false;

    public bool HasValidPath => _hasValidPath;
    public Vector3 CurrentTarget => GetCurrentWaypoint();
    public int PathProgress => _currentCornerIndex;
    public int PathLength => _pathCorners.Length;

    private void Awake()
    {
        _currentPath = new NavMeshPath();
    }

    /// <summary>
    /// Calculates a new path from start to target position.
    /// </summary>
    public bool CalculatePath(Vector3 startPosition, Vector3 targetPosition)
    {
        // Only recalculate if target has moved significantly
        if (Vector3.Distance(startPosition, _lastPathStart) < pathRecalculateDistance && _hasValidPath)
        {
            return true;
        }

        _lastPathStart = startPosition;
        _currentCornerIndex = 0;

        // Calculate path on NavMesh
        bool pathFound = NavMesh.CalculatePath(startPosition, targetPosition, navMeshAreaMask, _currentPath);

        if (pathFound && _currentPath.status == NavMeshPathStatus.PathComplete)
        {
            _pathCorners = _currentPath.corners;
            _hasValidPath = true;
            return true;
        }
        else if (pathFound && _currentPath.status == NavMeshPathStatus.PathPartial)
        {
            // Path is partial but still valid (can reach as close as possible)
            _pathCorners = _currentPath.corners;
            _hasValidPath = true;
            return false; // Indicate that we couldn't reach the exact target
        }
        else
        {
            _hasValidPath = false;
            _pathCorners = new Vector3[0];
            return false;
        }
    }

    /// <summary>
    /// Gets the current waypoint the tank should move toward.
    /// </summary>
    public Vector3 GetCurrentWaypoint()
    {
        if (_pathCorners.Length == 0)
            return transform.position;

        return _pathCorners[Mathf.Min(_currentCornerIndex, _pathCorners.Length - 1)];
    }

    /// <summary>
    /// Updates the waypoint progress based on tank position.
    /// Returns true if the final waypoint has been reached.
    /// </summary>
    public bool UpdateWaypoint(Vector3 tankPosition)
    {
        if (_pathCorners.Length == 0)
            return false;

        Vector3 currentWaypoint = GetCurrentWaypoint();
        float distanceToWaypoint = Vector3.Distance(tankPosition, currentWaypoint);

        // Move to next waypoint if close enough
        if (distanceToWaypoint < waypointStoppingDistance)
        {
            _currentCornerIndex++;

            // Check if we've reached the final waypoint
            if (_currentCornerIndex >= _pathCorners.Length)
            {
                _currentCornerIndex = _pathCorners.Length - 1;
                return true; // Path complete
            }
        }

        return false; // Path not yet complete
    }

    /// <summary>
    /// Gets the direction vector toward the current waypoint in world space.
    /// </summary>
    public Vector3 GetPathDirection(Vector3 tankPosition)
    {
        Vector3 waypoint = GetCurrentWaypoint();
        return (waypoint - tankPosition).normalized;
    }

    /// <summary>
    /// Resets the path progress (used when stopping a patrol).
    /// </summary>
    public void ResetPath()
    {
        _currentCornerIndex = 0;
        _hasValidPath = false;
        _pathCorners = new Vector3[0];
    }

    /// <summary>
    /// Gets a random point on the NavMesh within a given radius.
    /// </summary>
    public bool GetRandomNavMeshPoint(Vector3 centerPosition, float searchRadius, out Vector3 randomPoint)
    {
        randomPoint = centerPosition;

        for (int i = 0; i < 10; i++) // Try up to 10 times to find a valid point
        {
            Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
            Vector3 randomPosition = centerPosition + randomDirection;

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, searchRadius, navMeshAreaMask))
            {
                randomPoint = hit.position;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Draws the current path in the Scene view for debugging.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (_pathCorners.Length == 0)
            return;

        Gizmos.color = Color.green;
        for (int i = 0; i < _pathCorners.Length - 1; i++)
        {
            Gizmos.DrawLine(_pathCorners[i], _pathCorners[i + 1]);
        }

        // Draw waypoint spheres
        Gizmos.color = Color.yellow;
        foreach (Vector3 corner in _pathCorners)
        {
            Gizmos.DrawSphere(corner, 0.5f);
        }

        // Highlight current target
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetCurrentWaypoint(), 0.8f);
    }
}
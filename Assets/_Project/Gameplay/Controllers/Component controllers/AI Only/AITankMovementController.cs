using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI tank movement controller using Unity's NavMeshAgent.
/// Handles patrol point following and player chasing behavior with smooth approach.
/// </summary>
public class AITankMovementController : MonoBehaviour
{
    public enum MovementState
    {
        Patrolling,
        Chasing
    }

    [Header("Movement References")]
    [SerializeField]
    private AITankController aiTankController;

    private NavMeshAgent _navMeshAgent;
    private AIPatrolRoute _patrolRoute;
    private MovementState _currentState = MovementState.Patrolling;

    [Header("Agent Settings")]
    [SerializeField]
    private float moveSpeed = 20f;
    [SerializeField]
    private float rotationSpeed = 8f;
    [SerializeField]
    private float _rotationSharpness = 8f;
    [SerializeField]
    private float _minSpeedToRotate = 0.3f;
    [SerializeField]
    private float stoppingDistance = 1f;


    [Header("Chase Settings")]
    [SerializeField]
    private float chaseStoppingDistance = 5f;
    [SerializeField]
    private float pathUpdateInterval = 0.3f;
    [SerializeField]
    private float slowdownDistance = 15f;
    [SerializeField]
    private float minChaseSpeed = 5f;

    private float _waitTimer = 0f;
    private float _pathUpdateTimer = 0f;

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _patrolRoute = aiTankController.PatrolRoute;
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
            rb.isKinematic = true;

        if (_navMeshAgent != null)
        {
            _navMeshAgent.speed = moveSpeed;
            _navMeshAgent.stoppingDistance = stoppingDistance;
            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
        }

        if (_patrolRoute == null)
            Debug.LogError($"[AITankMovementController] No patrol route assigned to {gameObject.name}");

        MoveToNextPatrolPoint();
    }

    private void Update()
    {
        if (aiTankController == null || !aiTankController.IsAlive)
            return;

        _pathUpdateTimer -= Time.deltaTime;

        // Check if player is in sight
        if (aiTankController.PlayerInSight)
        {
            ChangeState(MovementState.Chasing);
        }
        else if (_currentState == MovementState.Chasing)
        {
            ChangeState(MovementState.Patrolling);
        }

        // Update behavior based on current state
        switch (_currentState)
        {
            case MovementState.Patrolling:
                UpdatePatrolling();
                break;
            case MovementState.Chasing:
                UpdateChasing();
                break;
        }
    }

    private Vector3 _smoothedDir;

    private void LateUpdate()
    {
        if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.hasPath)
            return;

        transform.position = _navMeshAgent.nextPosition;

        // Only rotate while actually moving; avoids spinning near the destination
        if (_navMeshAgent.desiredVelocity.magnitude < _minSpeedToRotate)
            return;

        // steeringTarget is the next path corner: more stable than desiredVelocity
        Vector3 dir = _navMeshAgent.steeringTarget - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            return;

        dir.Normalize();

        // Low-pass filter the direction so small flips don't reach the rotation
        float t = 1f - Mathf.Exp(-_rotationSharpness * Time.deltaTime);
        _smoothedDir = Vector3.Slerp(
            _smoothedDir.sqrMagnitude < 0.001f ? dir : _smoothedDir, dir, t);

        transform.rotation = Quaternion.LookRotation(_smoothedDir);
    }

    private void UpdatePatrolling()
    {
        if (_patrolRoute == null || _patrolRoute.GetPatrolPointCount() == 0)
            return;

        // Wait at current point
        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            _navMeshAgent.ResetPath();
            return;
        }

        // Check if reached current waypoint
        if (!_navMeshAgent.hasPath || _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            if (!_navMeshAgent.hasPath || !_navMeshAgent.pathPending)
            {
                // Reached point, wait then move to next
                _waitTimer = _patrolRoute.GetWaitTimeAtPoint();
                _patrolRoute.GetNextPatrolPoint();
                MoveToNextPatrolPoint();
            }
        }
    }

    private void UpdateChasing()
    {
        GameObject player = aiTankController.DetectedPlayer;

        if (player == null)
            return;

        Vector3 playerPosition = player.transform.position;
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);

        // Update path to player periodically
        if (_pathUpdateTimer <= 0f)
        {
            _navMeshAgent.SetDestination(playerPosition);
            _pathUpdateTimer = pathUpdateInterval;
        }

        // Adjust speed based on distance to player (smooth slowdown)
        float adjustedSpeed = moveSpeed;
        if (distanceToPlayer < slowdownDistance)
        {
            // Lerp between minChaseSpeed and moveSpeed based on distance
            float speedRatio = distanceToPlayer / slowdownDistance;
            adjustedSpeed = Mathf.Lerp(minChaseSpeed, moveSpeed, speedRatio);
        }


        // When very close to player, stop pathfinding but continue looking at player
        if (distanceToPlayer <= chaseStoppingDistance)
        {
            _navMeshAgent.speed = 0;
        }
        else
        {
            _navMeshAgent.speed = adjustedSpeed;
        }
    }

    private void ChangeState(MovementState newState)
    {
        if (_currentState == newState)
            return;

        _currentState = newState;
        _pathUpdateTimer = 0f;
        Debug.Log($"[AITankMovementController] {gameObject.name} changed state to {newState}");
    }

    private void MoveToNextPatrolPoint()
    {
        if (_patrolRoute == null || _patrolRoute.GetPatrolPointCount() == 0)
            return;

        Vector3 targetPoint = _patrolRoute.GetCurrentPatrolPoint();
        _navMeshAgent.SetDestination(targetPoint);
    }
}
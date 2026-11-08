using UnityEngine;
using UnityEngine.Events;

public class AITankController : TankController
{
    [Header("AI Tank Settings")]
    [SerializeField]
    private AiTargetingSysData targetingData;
    [SerializeField]
    private Transform eyePos;
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private float turretFireOffsetDeadzone = 8f;

    [Header("Patrol Settings")]
    [SerializeField]
    private AIPatrolRoute patrolRoute;

    [Header("VFX")]
    public MuzzleFlashVFX muzzleFlashVFX;
    public ParticleSystem deathVFX;
    public GameObject projectileTrailPrefab;
    public ParticleSystem deadLoopVFXSmoke;
    public ParticleSystem deadLoopVFXStatic;

    private ModelController _modelController;
    private GameObject _detectedPlayer;
    private bool _playerInSight = false;

    public GameObject DetectedPlayer => _detectedPlayer;
    public bool PlayerInSight => _playerInSight;
    public LayerMask PlayerLayer => playerLayer;
    public AIPatrolRoute PatrolRoute => patrolRoute;

    protected override void Awake()
    {
        base.Awake();
        _modelController = new ModelController(this);

    }

    protected override void Start()
    {
        aimingSystem = new AIAimingSystem(turret.transform, tank, targetingData, eyePos, turret.transform);
        turretController = new AITurretController(aimingSystem, turret, cannonPivot.transform, cannonTip.transform, tank.transform, turretData);
        base.Start();

        //VFX
        if (muzzleFlashVFX != null)
            firingSystem.onFire.AddListener(muzzleFlashVFX.Play);

        firingSystem.onProjectileFired.AddListener(SpawnTrail);
    }

    protected override void Update()
    {
        if (!IsAlive) return;

        weaponSystem?.Update(Time.deltaTime);
        aimingSystem?.Update();
        UpdateAim();
        healthSystem?.OnUpdate();
    }

    protected override void LateUpdate()
    {
        if (!IsAlive) return;
        turretController?.LateUpdate();
    }

    private void UpdateAim()
    {
        if (aimingSystem == null) return;

        if (TryGetTargetPoint(out Vector3 aimPoint, out GameObject target))
        {
            _playerInSight = true;
            _detectedPlayer = target;

            Vector3 toTarget = aimPoint - turret.transform.position;

            aimingSystem.Yaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;

            Vector3 local = turret.transform.InverseTransformDirection(toTarget.normalized);
            float horizontal = new Vector2(local.x, local.z).magnitude;
            aimingSystem.Pitch = -Mathf.Atan2(local.y, horizontal) * Mathf.Rad2Deg;

            // Angle between where the barrel actually points and where the target is.
            // Measure from the muzzle so it matches where the shot really leaves.
            Vector3 muzzleToTarget = aimPoint - cannonTip.transform.position;
            float aimError = Vector3.Angle(cannonPivot.transform.forward, muzzleToTarget);
            Debug.DrawLine(cannonPivot.transform.position, aimPoint, Color.red);

            if (aimError <= turretFireOffsetDeadzone)
            {
                TryFire();
            }
        }
        else
        {
            _playerInSight = false;

            Vector3 f = Vector3.ProjectOnPlane(tank.transform.forward, Vector3.up);
            aimingSystem.Yaw = Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
            aimingSystem.Pitch = 0f;
        }
    }

    private bool TryGetTargetPoint(out Vector3 vectorPoint, out GameObject targetPoint)
    {
        vectorPoint = default;
        targetPoint = null;
        var visible = ((AIAimingSystem)aimingSystem).visibleTargetPoints;
        foreach (var kvp in visible)
        {
            if (kvp.Value.Count == 0) continue;
            vectorPoint = kvp.Value[0].position;
            targetPoint = kvp.Value[0].gameObject;
            return true;
        }
        return false;
    }

    protected override void FixedUpdate()
    {
        if (!IsAlive) return;

        _modelController.Update();
    }

    private void TryFire()
    {
        weaponSystem.TryFire(cannonPivot.transform.forward);
    }

    protected override void HandleDeath()
    {
        base.HandleDeath();

        FMODManager.Instance.PlayATDeathSound(transform.position);

        FMODManager.Instance.PlayDopamineKillNoise();

        if (deathVFX != null)
        {
            deathVFX.Play();
        }

        if (deadLoopVFXStatic != null)
        {
            deadLoopVFXStatic.gameObject.SetActive(true);
            deadLoopVFXStatic.Play();
        }

        if (deadLoopVFXSmoke != null)
        {
            deadLoopVFXSmoke.gameObject.SetActive(true);
            deadLoopVFXSmoke.Play();
        }

        if (deadLoopVFXStatic != null)
        {
            deadLoopVFXStatic.gameObject.SetActive(true);
            deadLoopVFXStatic.Play();
        }

        if (deadLoopVFXSmoke != null)
        {
            deadLoopVFXSmoke.gameObject.SetActive(true);
            deadLoopVFXSmoke.Play();
        }
    }

    void SpawnTrail(Vector3 origin, Vector3 destination)
    {
        if (projectileTrailPrefab == null) return;
        Vector3 gunForward = (destination - origin).normalized;
        GameObject trail = Instantiate(projectileTrailPrefab, origin, Quaternion.LookRotation(gunForward));
        ProjectileTrail pt = trail.GetComponent<ProjectileTrail>();
        if (pt != null)
            pt.Init(origin, destination, gunForward, 500f);
        FMODManager.Instance.PlayATShotSound(transform.position);
    }

}
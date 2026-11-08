using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class TankController : MonoBehaviour, IDamageable
{
    [Header("Tank turret object references")]
    [Tooltip("Child object containing all visuals and colliders for the tank model")]
    public Transform visuals;
    public GameObject tank;
    public GameObject turret;
    public GameObject cannon;
    public GameObject cannonPivot;
    public GameObject cannonTip;
    public AmmoType initialAmmoType;
    public WeaponTypeData primaryWeapon, secondaryWeapon, auxillaryWeapon;

    [Header("Model Controller Settings")]
    public Transform[] wheelBones;
    public Wheel[] wheels;
    public Transform[] wheelModels;
    public float maxBoneMovePerFrame = 0.02f;
    public Vector3 bonePositionOffset = Vector3.zero;
    public Vector3 modelPositionOffset = Vector3.zero;

    [Header("VFX Settings")]
    public float trailFadeTime = 2f;

    [Header("Tank Data Objects")]
    public MovementData movementData;
    public TurretData turretData;
    public HealthSysData healthSysData;
    private HealthSystem _healthSystem;

    [Header("Events")]
    public UnityEvent DriftStarted = new();
    public UnityEvent DriftStopped = new();

    [HideInInspector]
    public TurretController turretController;
    [HideInInspector]
    public TankMovementController tankMovementController;
    [HideInInspector]
    public AimingSystem aimingSystem;
    [HideInInspector]
    public WeaponSystem weaponSystem;
    [HideInInspector]
    public FiringSystem firingSystem;
    [HideInInspector]
    public bool initialised;
    [HideInInspector]
    public Rigidbody rb;

    private bool isAlive = true;
    public bool IsAlive { get { return isAlive; } }
    public HealthSystem healthSystem
    {
        get { return _healthSystem; }
        set { _healthSystem = value; }
    }


    protected virtual void Awake()
    {
        tankMovementController = new(this, movementData);
        tankMovementController.onDriftStart.AddListener(HandleDriftStart);
        tankMovementController.onDriftStop.AddListener(HandleDriftStop);

        // Initialize weapon system with all weapon slots
        weaponSystem = new WeaponSystem(primaryWeapon, secondaryWeapon, auxillaryWeapon);

        healthSystem = new(healthSysData);
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        if (turretController == null)
        {
            Debug.LogError("[Tank Controller - ERROR] Turret Controller not assigned!");
        }

        // Create FiringSystem (note: AmmoType parameter removed)
        firingSystem = new(turretController, weaponSystem);

        // Initialize weapon system with firing system reference
        weaponSystem.InitializeFiringSystem(firingSystem);

        healthSystem.onDeath.AddListener(HandleDeath);
    }

    protected virtual void Update()
    {
        if (!isAlive) return;

        weaponSystem.Update(Time.deltaTime);
        tankMovementController.Update();
        aimingSystem.Update();
        turretController.Update();
        healthSystem.OnUpdate();
    }
    protected virtual void LateUpdate()
    {
        turretController.LateUpdate();
        aimingSystem.LateUpdate();
    }


    protected virtual void FixedUpdate()
    {
        tankMovementController.FixedUpdate();
    }

    public void Kill()
    {
        healthSystem.Kill();
    }

    protected void HandleDriftStart()
    {
        Debug.Log($"Drift on rank controller");
        DriftStarted.Invoke();
    }
    protected void HandleDriftStop()
    {
        DriftStopped.Invoke();
    }
    protected virtual void HandleDeath()
    {
        isAlive = false;
        tankMovementController.disableMovement();
        IDamageable.onUnitDeath.Invoke(this);
    }
}
using UnityEngine;

public class AiEmplacementController : MonoBehaviour, IDamageable
{
    [Header("Emplacement object references")]
    public GameObject emplacement;
    public GameObject turret;
    public GameObject cannon;
    public GameObject cannonPivot;
    public GameObject cannonTip;
    public Transform eyePos;
    public WeaponTypeData primaryWeapon;
    public AmmoType initialAmmoType;

    [Header("Emplacement Data Objects")]
    public TurretData turretData;
    public AiTargetingSysData aiTargetingData;
    public HealthSysData healthSysData;

    [Header("VFX")]
    public MuzzleFlashVFX muzzleFlashVFX;
    public ParticleSystem deathVFX;
    public GameObject projectileTrailPrefab;
    public ParticleSystem deadLoopVFXSmoke;
    public ParticleSystem deadLoopVFXStatic;

    public WeaponSystem weaponSystem;
    public AIAimingSystem aimingSystem;
    public AITurretController turretController;
    EmplacementStateManager emplacementStateManager;
    public FiringSystem firingSystem;
    public float lastFireTime;
    public HealthSystem healthSystem { get; set; }

   
    private bool isAlive = true;
    public bool IsAlive { get { return isAlive; } }
    public bool isActive = true; // For use in spawner for animation locking

    private void Start()
    {
        // WeaponSystem now instantiates WeaponInstance from the template
        weaponSystem = new WeaponSystem(primaryWeapon, null, null);

        aimingSystem = new AIAimingSystem(turret.transform, emplacement, aiTargetingData, eyePos);
        turretController = new AITurretController(aimingSystem, turret, cannonPivot.transform, cannonTip.transform, emplacement.transform, turretData);
        emplacementStateManager = new(this);
        emplacementStateManager.firingState.onFire.AddListener(onFire);

        // Create FiringSystem
        firingSystem = new(turretController, weaponSystem);

        // Initialize weapon system with firing system reference
        weaponSystem.InitializeFiringSystem(firingSystem);

        healthSystem = new(healthSysData);
        Debug.Log($"Registered on instance: {healthSystem.GetHashCode()}");

        //VFX
        if (muzzleFlashVFX != null)
            firingSystem.onFire.AddListener(muzzleFlashVFX.Play);

        firingSystem.onProjectileFired.AddListener(SpawnTrail);

        healthSystem.onDeath.AddListener(HandleDeath);
    }

    private void Update()
    {
        if (!isAlive || !isActive) return;

        weaponSystem.Update(Time.deltaTime);
        aimingSystem.Update();
        emplacementStateManager.Update();
        healthSystem.OnUpdate();
        Debug.DrawRay(cannonTip.transform.position, cannonPivot.transform.forward * 100);
    }

    private void LateUpdate()
    {
        if (!isAlive) return;
        turretController.LateUpdate();
    }

    private void onFire()
    {
        if (!isActive) return;
        // Fire command now goes through WeaponSystem
        weaponSystem.TryFire(emplacementStateManager.GetEmplacementCurrentDir());
    }

    private void HandleDeath()
    {
        Debug.Log("HandleDeath fired!");
        isAlive = false;

        
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

        IDamageable.onUnitDeath.Invoke(this);
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
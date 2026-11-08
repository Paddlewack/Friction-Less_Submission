using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using IngameDebugConsole;
using Unity.Mathematics;

public class PlayerTankController : TankController
{
    [Header("Player tank object references")]
    public Camera thirdPersonCam;
    public Camera firstPersonCam;

    [Header("Aiming System Settings")]
    public LayerMask aimpointMask;

    [Header("Scriptable Object data")]
    public PlayerTankData playerTankData;
    public PlayerCameraData playerCameraData;

    [Header("UI")]
    public CrosshairUI crosshairUI;

    private InputSystem_Actions.PlayerActions _playerActions;
    private ModelController modelController;
    private bool inputRegistered = false;

    [Header("VFX")]
    public MuzzleFlashVFX muzzleFlashVFX;
    public GameObject projectileTrailPrefab;
    public ParticleSystem[] deathVFX;

    [Header("Controls")]
    [SerializeField]
    private bool canMove = true;
    [SerializeField]
    private bool canDrift = true;
    [SerializeField]
    private bool canLook = true;
    [SerializeField]
    private bool canAttack = true;
    [SerializeField]
    private bool canZoom = true;
    [SerializeField]
    private bool canFreelook = true;

    // FMOD
    private Rigidbody _rb;
    private bool _moveSoundActive = false;
    private float _turretSoundStartTime = -999f;
    private const float TurretSoundClipLength = 1.5f;
    private const float TurretStartThreshold = 0.5f;
    private const float TurretStopThreshold = 0.2f;
    private bool _turretSoundPlaying = false;

    // Turret lock
    private bool isHoldFreelook = false;
    private bool isToggleFreelook = false;

    protected override void Awake()
    {
        base.Awake();
    }

    void OnDisable()
    {
        UnregisterInputCallbacks();
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= HandleHealthChanged;
    }

    protected override void Start()
    {
        aimingSystem = new PlayerAimingSystem(this);
        turretController = new PlayerTurretController(aimingSystem, turret, cannonPivot.transform, cannonTip.transform, transform, turretData);
        _playerActions = GameManager.Instance.Actions.Player;
        base.Start();
        modelController = new ModelController(this);
        tankMovementController = new PlayerTankMovementController(this, movementData);

        _rb = GetComponent<Rigidbody>();

        tankMovementController.onDriftStart.AddListener(HandleDriftStart);
        tankMovementController.onDriftStop.AddListener(HandleDriftStop);

        firingSystem.onFire.AddListener(() => FMODManager.Instance.PlayTankShot(transform.position));
        firingSystem.onFire.AddListener(() => StartCoroutine(PlayReloadAfterDelay()));

        if (muzzleFlashVFX != null)
            firingSystem.onFire.AddListener(muzzleFlashVFX.Play);

        firingSystem.onProjectileFired.AddListener(SpawnTrail);

        weaponSystem.onReloadStart.AddListener(HandleReloadStart);
        weaponSystem.onReloadComplete.AddListener(HandleReloadComplete);
        weaponSystem.onWeaponSwitched.AddListener(HandleWeaponSwitched);
        weaponSystem.onAmmoChanged.AddListener(HandleAmmoChanged);

        // Initialize weapon UI after UIManager.Start() has run
        StartCoroutine(InitializeWeaponUINextFrame());

        // Only play music if not already playing
        if (!FMODManager.Instance.IsMusicPlaying())
            FMODManager.Instance.PlayMusicTrackOne();

        StartCoroutine(InitialisationCheck());

        healthSystem.OnHealthChanged += HandleHealthChanged;
        healthSystem.ResetHealth();
    }

    private IEnumerator InitializeWeaponUINextFrame()
    {
        yield return null; // Wait one frame for UIManager.Start() to complete
        if (UIManager.Instance != null)
            UIManager.Instance.InitializeWeaponUI(this);
    }

    protected override void Update()
    {
        if (!initialised) return;
        base.Update();
        modelController.Update();

        float speed = _rb.linearVelocity.magnitude;

        if (speed > 0.05f)
        {
            FMODManager.Instance.PlayEngineNoise(transform.position);
            FMODManager.Instance.PlayTankTrackRattle(transform.position);
            _moveSoundActive = true;
        }
        else if (_moveSoundActive)
        {
            FMODManager.Instance.StopEngineNoise();
            FMODManager.Instance.StopTankTrackRattle();
            _moveSoundActive = false;
        }

        Vector3 origin = cannonTip.transform.position;
        Vector3 target = (aimingSystem as PlayerAimingSystem).CameraAimPoint;

        Vector3 direction = (target - origin).normalized;
    }

    public Vector3 GetBarrelAimPoint(float distance = 500f)
    {
        if (Physics.Raycast(cannonTip.transform.position, cannonTip.transform.forward, out RaycastHit hit, distance))
            return hit.point;
        return cannonTip.transform.position + cannonTip.transform.forward * distance;
    }


    private IEnumerator InitialisationCheck()
    {
        while (true)
        {
            yield return null;
            if (aimingSystem != null && turretController != null && modelController != null && tankMovementController != null)
            {
                initialised = true;

                // Initialize crosshair UI
                if (crosshairUI != null)
                    crosshairUI.Initialize(this, thirdPersonCam.isActiveAndEnabled ? thirdPersonCam : firstPersonCam);

                if (!inputRegistered)
                {
                    RegisterInputCallbacks();
                    inputRegistered = true;
                }
                break;
            }
        }
    }

    #region VFX
    void SpawnTrail(Vector3 origin, Vector3 destination)
    {
        if (projectileTrailPrefab == null) return;
        Vector3 gunForward = (destination - origin).normalized;
        GameObject trail = Instantiate(projectileTrailPrefab, origin, Quaternion.LookRotation(gunForward));
        ProjectileTrail pt = trail.GetComponent<ProjectileTrail>();
        if (pt != null)
            pt.Init(origin, destination, gunForward, 500f);
    }
    #endregion

    #region FMOD
    private IEnumerator PlayReloadAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        FMODManager.Instance.PlayReloadTankShell(transform.position);
    }
    #endregion

    #region Input Callback Results
    void OnMove(InputAction.CallbackContext ctx)
    {
        if (!canMove || tankMovementController == null) return;
        (tankMovementController as PlayerTankMovementController).OnMove(ctx);
    }

    void OnDrift(InputAction.CallbackContext ctx)
    {
        if (!canDrift || tankMovementController == null) return;
        (tankMovementController as PlayerTankMovementController).OnDrift(ctx);

        if (ctx.performed)
        {
            FMODManager.Instance.PlayTankDriftStart(transform.position);
            FMODManager.Instance.PlayTankDriftHum(transform.position);
        }
        else if (ctx.canceled)
        {
            FMODManager.Instance.StopTankDriftHum();
        }
    }

    void OnZoom(InputAction.CallbackContext ctx)
    {
        if (!canZoom || aimingSystem == null) return;
        (aimingSystem as PlayerAimingSystem).camController.camSM.SwitchCamera();
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        if (!canLook || aimingSystem == null) return;

        if (CheckFreelook())
            (aimingSystem as PlayerAimingSystem).OnLookCameraOnly(ctx);
        else
            (aimingSystem as PlayerAimingSystem).OnLook(ctx);

        Vector2 lookInput = ctx.ReadValue<Vector2>();
        float speed = lookInput.magnitude / Time.deltaTime;

        bool clipFinished = Time.time - _turretSoundStartTime >= TurretSoundClipLength;

        if (!_turretSoundPlaying && speed >= TurretStartThreshold)
        {
            _turretSoundPlaying = true;
            _turretSoundStartTime = Time.time;
            FMODManager.Instance.PlayTurretRotation(transform.position);
        }
        else if (_turretSoundPlaying && clipFinished && speed >= TurretStartThreshold)
        {
            _turretSoundStartTime = Time.time;
            FMODManager.Instance.PlayTurretRotation(transform.position);
        }
        else if (_turretSoundPlaying && clipFinished && speed < TurretStopThreshold)
        {
            FMODManager.Instance.StopTurretRotation();
            _turretSoundPlaying = false;
        }
    }

    void OnToggleFreelook(InputAction.CallbackContext ctx)
    {
        if (!canFreelook) return;
        isToggleFreelook = !isToggleFreelook;
        if (!isToggleFreelook && !CheckFreelook())
            (aimingSystem as PlayerAimingSystem).camController.camSM.ResetFreeLook();
    }
    void OnHoldFreelookStart(InputAction.CallbackContext ctx)
    {
        if (!canFreelook) return;
        isHoldFreelook = true;
    }
    void OnHoldFreelookStop(InputAction.CallbackContext ctx)
    {
        if (!canFreelook) return;
        isHoldFreelook = false;
        if (!CheckFreelook())
            (aimingSystem as PlayerAimingSystem).camController.camSM.ResetFreeLook();
    }

    bool CheckFreelook()
    {
        bool shouldFreeLook = isHoldFreelook || isToggleFreelook;
        return shouldFreeLook;
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!canAttack || weaponSystem == null) return;
        // Fire command now goes through WeaponSystem first
        weaponSystem.TryFire((aimingSystem as PlayerAimingSystem).CameraAimPoint - cannonTip.transform.position);
    }

    void OnSwitchWeaponPrimary(InputAction.CallbackContext ctx)
    {
        if (weaponSystem != null)
            weaponSystem.SwitchToWeaponSlot(WeaponSlot.Primary);
    }

    void OnSwitchWeaponSecondary(InputAction.CallbackContext ctx)
    {
        if (weaponSystem != null)
            weaponSystem.SwitchToWeaponSlot(WeaponSlot.Secondary);
    }

    void OnSwitchWeaponAuxiliary(InputAction.CallbackContext ctx)
    {
        if (weaponSystem != null)
            weaponSystem.SwitchToWeaponSlot(WeaponSlot.Auxiliary);
    }

    #endregion

    #region Input Callback Registration
    private void RegisterInputCallbacks()
    {
        _playerActions.Move.performed += OnMove;
        _playerActions.Move.canceled += OnMove;
        _playerActions.Drift.performed += OnDrift;
        _playerActions.Drift.canceled += OnDrift;
        _playerActions.Zoom.performed += OnZoom;
        _playerActions.Fire.performed += OnAttack;
        _playerActions.Look.performed += OnLook;
        _playerActions.Look.canceled += OnLook;
        _playerActions.ToggleFreelook.started += OnToggleFreelook;
        _playerActions.HoldFreelook.started += OnHoldFreelookStart;
        _playerActions.HoldFreelook.canceled += OnHoldFreelookStop;
    }

    private void UnregisterInputCallbacks()
    {
        if (!inputRegistered) return;
        _playerActions.Move.performed -= OnMove;
        _playerActions.Move.canceled -= OnMove;
        _playerActions.Drift.performed -= OnDrift;
        _playerActions.Drift.canceled -= OnDrift;
        _playerActions.Zoom.performed -= OnZoom;
        _playerActions.Fire.performed -= OnAttack;
        _playerActions.Look.performed -= OnLook;
        _playerActions.Look.canceled -= OnLook;
        _playerActions.ToggleFreelook.started -= OnToggleFreelook;
        _playerActions.HoldFreelook.started -= OnHoldFreelookStart;
        _playerActions.HoldFreelook.canceled -= OnHoldFreelookStop;
        inputRegistered = false;
    }

    #endregion

    #region Event Handling
    protected override void HandleDeath()
    {
        if (deathVFX != null)
        {
            foreach (var vfx in deathVFX)
            vfx.Play();
        }

        FMODManager.Instance.StopEngineNoise();
        FMODManager.Instance.StopTankTrackRattle();
        FMODManager.Instance.StopTurretRotation();

        canAttack = false;
        canDrift = false;
        canMove = false;
        canZoom = false;

        base.HandleDeath();
    }
    private void HandleHealthChanged(float hullCurrent, float hullMax, float shieldCurrent, float shieldMax)
    {
        UIManager.Instance.UpdateHealthBars(hullCurrent, hullMax, shieldCurrent, shieldMax);
    }

    private void HandleReloadStart(float reloadTime)
    {
        Debug.Log($"Reload started for {reloadTime} seconds");
    }

    private void HandleReloadComplete()
    {
        Debug.Log("Reload complete");
    }

    private void HandleWeaponSwitched(WeaponInstance newWeapon)
    {
        Debug.Log($"Switched to {newWeapon.WeaponName}");
        // Update UI with new weapon info
    }

    private void HandleAmmoChanged(int current, int max)
    {
        Debug.Log($"Ammo: {current}/{max}");
        // Update ammo UI
    }

    #endregion
}
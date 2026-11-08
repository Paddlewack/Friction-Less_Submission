using UnityEngine;
using UnityEngine.Events;

public class WeaponSystem
{
    private WeaponInstance currentWeapon;
    private WeaponInstance primaryWeapon;
    private WeaponInstance secondaryWeapon;
    private WeaponInstance auxiliaryWeapon;

    private float _reloadTimer;
    private bool _isReloading;
    private FiringSystem _firingSystem;

    public UnityEvent<int, int> onAmmoChanged = new();
    public UnityEvent<float> onReloadStart = new();
    public UnityEvent onReloadComplete = new();
    public UnityEvent<WeaponInstance> onWeaponSwitched = new();

    // Firing events exposed so listeners can subscribe directly
    public UnityEvent onFire => _firingSystem?.onFire;
    public UnityEvent<Vector3, Vector3> onProjectileFired => _firingSystem?.onProjectileFired;

    public int AmmoCount => currentWeapon != null ? currentWeapon.AmmoCurrent : 0;
    public int MaxAmmo => currentWeapon != null ? currentWeapon.AmmoMax : 0;
    public bool IsReloading => _isReloading;
    public bool CanFire => currentWeapon != null && currentWeapon.AmmoCurrent > 0 && !_isReloading;
    public WeaponInstance CurrentWeapon => currentWeapon;

    public WeaponSystem(WeaponTypeData primaryTemplate, WeaponTypeData secondaryTemplate = null, WeaponTypeData auxiliaryTemplate = null)
    {
        // Instantiate weapon data from templates so each unit has independent ammo pools
        primaryWeapon = primaryTemplate != null ? new WeaponInstance(primaryTemplate) : null;
        secondaryWeapon = secondaryTemplate != null ? new WeaponInstance(secondaryTemplate) : null;
        auxiliaryWeapon = auxiliaryTemplate != null ? new WeaponInstance(auxiliaryTemplate) : null;

        // Set primary weapon as current
        SetWeapon(primaryWeapon);
    }

    public void InitializeFiringSystem(FiringSystem firingSystem)
    {
        _firingSystem = firingSystem;
    }

    public void Update(float deltaTime)
    {
        if (_isReloading)
        {
            _reloadTimer -= deltaTime;
            if (_reloadTimer <= 0)
            {
                CompleteReload();
            }
        }
    }

    public void SetWeapon(WeaponInstance newWeapon)
    {
        if (newWeapon == null) return;

        // Cancel current reload when switching
        _isReloading = false;
        _reloadTimer = 0;

        currentWeapon = newWeapon;
        onWeaponSwitched.Invoke(currentWeapon);
        onAmmoChanged.Invoke(currentWeapon.AmmoCurrent, currentWeapon.AmmoMax);
    }

    public void SwitchToWeaponSlot(WeaponSlot slot)
    {
        WeaponInstance weaponToSwitch = slot switch
        {
            WeaponSlot.Primary => primaryWeapon,
            WeaponSlot.Secondary => secondaryWeapon,
            WeaponSlot.Auxiliary => auxiliaryWeapon,
            _ => null
        };

        if (weaponToSwitch != null && weaponToSwitch != currentWeapon)
        {
            SetWeapon(weaponToSwitch);
        }
    }

    /// <summary>
    /// Entry point for all firing commands. WeaponSystem validates, then delegates to FiringSystem.
    /// </summary>
    public void TryFire(Vector3 aimDirection)
    {
        if (_firingSystem == null)
        {
            Debug.LogError("[WeaponSystem] FiringSystem not initialized!");
            return;
        }

        // Validate ammo and reload state 
        
        if (!CanFire)
        {
            // DISABLED ALL LOGS CAUSE AI USE IT OFTEN, RE-ENABLE FOR DEBUGGING
            /*if (currentWeapon == null)
               /Debug.LogError("[WeaponSystem] No weapon equipped"); 
            else if (_isReloading)
                Debug.Log("[WeaponSystem] Weapon is reloading");
            else
                Debug.Log("[WeaponSystem] No ammo available");*/
            return;
        }

        // Delegate to FiringSystem with aim direction
        _firingSystem.TryFire(aimDirection);
    }

    /// <summary>
    /// Called by FiringSystem after it fires to consume ammo.
    /// </summary>
    public void OnWeaponFired()
    {
        if (currentWeapon == null) return;

        currentWeapon.AmmoCurrent--;
        onAmmoChanged.Invoke(currentWeapon.AmmoCurrent, currentWeapon.AmmoMax);

        if (currentWeapon.AmmoCurrent <= 0)
        {
            StartReload();
        }
    }

    public void StartReload()
    {
        if (_isReloading || currentWeapon == null) return;

        _isReloading = true;
        _reloadTimer = currentWeapon.ReloadTime;
        onReloadStart.Invoke(currentWeapon.ReloadTime);
    }

    private void CompleteReload()
    {
        _isReloading = false;
        if (currentWeapon != null)
        {
            currentWeapon.AmmoCurrent = currentWeapon.AmmoMax;
            onAmmoChanged.Invoke(currentWeapon.AmmoCurrent, currentWeapon.AmmoMax);
            onReloadComplete.Invoke();
        }
    }

    public void RefillAmmo()
    {
        if (currentWeapon == null) return;
        currentWeapon.RefillAmmo();
        onAmmoChanged.Invoke(currentWeapon.AmmoCurrent, currentWeapon.AmmoMax);
    }

    public void ForceReloadComplete()
    {
        CompleteReload();
    }

    public WeaponInstance GetWeaponBySlot(WeaponSlot slot)
    {
        return slot switch
        {
            WeaponSlot.Primary => primaryWeapon,
            WeaponSlot.Secondary => secondaryWeapon,
            WeaponSlot.Auxiliary => auxiliaryWeapon,
            _ => null
        };
    }
}

public enum WeaponSlot
{
    Primary,
    Secondary,
    Auxiliary
}

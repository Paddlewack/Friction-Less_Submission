using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class FiringSystem
{
    Transform gun;
    Transform gunTip;
    float lastFireTime;
    Vector3 aimDirection;
    float aimPointSnapAngle;
    LayerMask aimPointMask;
    WeaponSystem weaponSystem;

    public UnityEvent onFire = new();
    public UnityEvent<Vector3, Vector3> onProjectileFired = new();

    public FiringSystem(TurretController turretController, WeaponSystem weaponSystem)
    {
        gun = turretController.cannonPivotTransform;
        gunTip = turretController.cannonTipTransform;
        this.weaponSystem = weaponSystem;
        onFire.AddListener(Fire);
        lastFireTime = -GetCurrentFireRate();
        aimPointSnapAngle = turretController.aimPointSnapAngle;
        aimPointMask = turretController.aimPointMask;

        // Listen for weapon switches to reset fire rate timing
        weaponSystem.onWeaponSwitched.AddListener(OnWeaponSwitched);
    }

    private void OnWeaponSwitched(WeaponInstance newWeapon)
    {
        // Reset fire rate timer when switching weapons
        lastFireTime = Time.time - GetCurrentFireRate();
    }

    private float GetCurrentFireRate()
    {
        if (weaponSystem.CurrentWeapon == null) return 1f;
        return weaponSystem.CurrentWeapon.FireRate;
    }

    /// <summary>
    /// Called by WeaponSystem after validation. Only checks fire rate here.
    /// </summary>
    public void TryFire(Vector3 aimDir)
    {
        aimDirection = aimDir;

        // Only check fire rate - WeaponSystem already validated ammo/reload
        if (lastFireTime + GetCurrentFireRate() <= Time.time)
        {
            onFire.Invoke();
            lastFireTime = Time.time;
        }
    }

    /// <summary>
    /// Internal fire logic. Only called after all validation passes.
    /// </summary>
    private void Fire()
    {
        // Notify WeaponSystem that we fired so it can consume ammo
        weaponSystem.OnWeaponFired();

        // Get ammo data from current weapon
        AmmoTypeData ammoData = AmmoTypes.GetAmmoData(weaponSystem.CurrentWeapon.AmmoType);
        if (ammoData == null) return;

        if (ammoData.isProjectile) FireProjectile(ammoData);
        else FireRay(ammoData);
    }

    void FireRay(AmmoTypeData ammoData)
    {
        RayAmmoData rayAmmoData = (RayAmmoData)ammoData;
        Vector3 dir = gun.transform.forward;
        Ray ray = new(gunTip.position, dir * rayAmmoData.range);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            OnHit(hit, ammoData);
        }
    }

    void FireProjectile(AmmoTypeData ammoData)
    {
        Debug.Log($"fired!");
        ProjectileAmmoData projectileAmmoData = (ProjectileAmmoData)ammoData;

        // Hard coded method of fixing weird bug. Firing a projectile when turning the tank hull would cause,
        // the shell to shoot towards a point off the aimed point.
        Vector3 direction;
        if (Vector3.Angle(gunTip.forward, aimDirection) < aimPointSnapAngle)
        {
            direction = aimDirection;
        }
        else
        {
            direction = gunTip.forward;
        }

        Vector3 position = gunTip.position;
        Vector3 velocity = direction * projectileAmmoData.muzzleVelocity;

        float stepTime = 0.02f;
        Vector3 destination = gunTip.position + gun.transform.forward * 500f; // fallback

        for (int i = 0; i < 500; i++)
        {
            Vector3 nextPosition = position + velocity * stepTime;

            if (Physics.Raycast(position, nextPosition - position, out RaycastHit hit, (nextPosition - position).magnitude, aimPointMask))
            {
                OnHit(hit, ammoData);
                destination = hit.point; // actually store the hit point
                break;
            }

            velocity += Physics.gravity * stepTime;
            position = nextPosition;
        }

        onProjectileFired.Invoke(gunTip.position, destination);
    }

    void OnHit(RaycastHit hit, AmmoTypeData ammoData)
    {
        IDamageable damagable = hit.collider.GetComponentInParent<IDamageable>();
        if (damagable != null)
        {
            damagable.healthSystem.TakeDamage(ammoData.damageAmount, ammoData.damageType);
            if (damagable.healthSystem.faction == Faction.Good)
                FMODManager.Instance.PlayTankDamageNoise();
        }
        PlayHitEffects(hit);
    }

    void PlayHitEffects(RaycastHit hit)
    {
        Debug.Log($"Hit {hit.collider}. Play hit visuals and audio at point {hit.point}");
    }
}
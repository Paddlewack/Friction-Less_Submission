using UnityEngine;

/// <summary>
/// Instance-specific weapon data. Created from a WeaponTypeData template.
/// Each unit gets its own WeaponInstance so they don't share ammo pools.
/// </summary>
public class WeaponInstance
{
    public string WeaponName { get; private set; }
    public int AmmoMax { get; private set; }
    public int AmmoCurrent { get; set; }
    public float FireRate { get; private set; }
    public float ReloadTime { get; private set; }
    public int Damage { get; private set; }
    public AmmoType AmmoType { get; private set; }
    public Sprite WeaponIcon { get; private set; }

    public WeaponInstance(WeaponTypeData template)
    {
        if (template == null)
        {
            Debug.LogError("[WeaponInstance] Template WeaponTypeData is null!");
            return;
        }

        WeaponName = template.weaponName;
        AmmoMax = template.ammoMax;
        AmmoCurrent = template.ammoMax;
        FireRate = template.fireRate;
        ReloadTime = template.reloadTime;
        Damage = template.damage;
        AmmoType = template.ammoType;
        WeaponIcon = template.weaponIcon;
    }

    public void RefillAmmo()
    {
        AmmoCurrent = AmmoMax;
    }

    public bool TryConsumeAmmo()
    {
        if (AmmoCurrent <= 0) return false;
        AmmoCurrent--;
        return true;
    }
}
using UnityEngine;
public abstract class AmmoTypeData : ScriptableObject
{
    public AmmoType ammoType;
    public DamageType damageType;
    public float damageAmount;    
    public float fireRate;
    public bool isProjectile;
}

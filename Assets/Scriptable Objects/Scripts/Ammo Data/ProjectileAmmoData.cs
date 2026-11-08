using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileAmmoData", menuName = "Scriptable Objects/ProjectileAmmoData")]
public class ProjectileAmmoData : AmmoTypeData
{
    public float muzzleVelocity;
    public float dragForce;
}

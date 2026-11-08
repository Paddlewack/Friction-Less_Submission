using UnityEngine;

[CreateAssetMenu(fileName = "WeaponTypeData", menuName = "Scriptable Objects/WeaponTypeData")]
public class WeaponTypeData : ScriptableObject
{
    public string weaponName;
    public int ammoMax;
    public int ammoCurrent;
    public float fireRate;
    public float reloadTime;
    public int damage;

    public AmmoType ammoType;
    public Sprite weaponIcon;


}

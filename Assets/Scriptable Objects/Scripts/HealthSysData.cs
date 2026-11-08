using UnityEngine;

[CreateAssetMenu(fileName = "HealthSysData", menuName = "Scriptable Objects/HealthSysData")]
public class HealthSysData : ScriptableObject
{
    public Faction Faction;
    public float StartingMaxHealth;
    public float StartingCurrentHealth;
    public float StartingMaxShield;
    public float StartingCurrentShield;
    public float ShieldRechargeTickRate;
    public float ShieldRechargePerTick;
    public float ShieldRechargeDelay;
    public float Armour;
}

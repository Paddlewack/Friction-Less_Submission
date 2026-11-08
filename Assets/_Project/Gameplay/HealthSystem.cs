using System;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Needs to be attached to any damagable entity to manage its health.
/// </summary>
public class HealthSystem
{
    public event Action<float, float, float, float> OnHealthChanged; // (current, max)
    public UnityEvent onDeath = new();

    float ShieldLastRechargeTick;
    float ShieldLastDamaged;
    bool DamageImmune = false;
    bool Dead = false;
    public Faction faction;
    float MaxHull;
    float CurrentHull;
    float MaxShield;
    float CurrentShield;
    float ShieldRechargeTickRate;
    float ShieldRechargePerTick;
    float ShieldRechargeDelay;
    float Armour;
    public HealthSystem(HealthSysData d)
    {
        MaxHull = d.StartingMaxHealth;
        CurrentHull = d.StartingCurrentHealth;
        MaxShield = d.StartingMaxShield;
        CurrentShield = d.StartingCurrentShield;
        faction = d.Faction;
        ShieldRechargeTickRate = d.ShieldRechargeTickRate;
        ShieldRechargePerTick = d.ShieldRechargePerTick;
        ShieldRechargeDelay = d.ShieldRechargeDelay;
        Armour = d.Armour;
        ShieldLastDamaged = ShieldRechargeDelay;
        ShieldLastRechargeTick = ShieldRechargeTickRate;
    }

    public void OnUpdate()
    {
        if (Dead) return;

        if (CurrentShield < MaxShield)
        {
            ShieldLastDamaged += Time.deltaTime;
            if (ShieldLastDamaged >= ShieldRechargeDelay)
            {
                ShieldLastRechargeTick += Time.deltaTime;

                // Regenerate shield in ticks
                if (ShieldLastRechargeTick >= ShieldRechargeTickRate)
                {
                    ShieldLastRechargeTick = 0f;
                    AddShield(ShieldRechargePerTick);
                }
            }
        }
        else
        {
            // Reset timers when shield is full
            ShieldLastDamaged = 0f;
            ShieldLastRechargeTick = 0f;
        }
    }

    /// <summary>
    /// Reduces the current health/shields of the entity based on the damage dealt to it (NOT IMPLEMENTED: also based on damage type).
    /// </summary>
    /// <param name="dmg">The ammount damage dealt to the entity.</param>
    /// <param name="type">The type of damage dealt to the entity (NOT IMPLEMENTED).</param>
    public void TakeDamage(float dmg, DamageType type)
    {
        if (DamageImmune || Dead) return;
        Debug.Log($"TakeDamage — dmg: {dmg}, health: {CurrentHull}, shield: {CurrentShield}, dead: {Dead}");

        // Reset shield recharge delay when taking damage
        ShieldLastDamaged = 0f;
        ShieldLastRechargeTick = 0f;

        if (CurrentShield <= dmg)
        {
            float postShieldDmg = dmg - CurrentShield;
            CurrentShield = 0;
            float postArmourDmg = postShieldDmg * (100 / (100 + Armour));
            CurrentHull -= postArmourDmg;
            if (CurrentHull <= 0)
                Die();
        }
        else
        {
            CurrentShield -= dmg;
        }

        OnHealthChanged?.Invoke(CurrentHull, MaxHull, CurrentShield, MaxShield);
    }
    /// <summary>
    /// Increases the current health of the entity by the amount healed, up to the max health.
    /// </summary>
    /// <param name="hp">The amount to add to the current health of the entity.</param>
    public void AddHull(float hp)
    {
        if (Dead) return;
        float newHp = CurrentHull + hp;
        if (newHp >= MaxHull)
        {
            CurrentHull = MaxHull;
        }
        else
        {
            CurrentHull = newHp;
        }
        OnHealthChanged?.Invoke(CurrentHull, MaxHull, CurrentShield, MaxShield);
    }
    public void AddShield(float shield)
    {
        if (Dead) return;
        float newShield = CurrentShield + shield;
        if (newShield >= MaxShield)
        {
            CurrentShield = MaxShield;
        }
        else
        {
            CurrentShield = newShield;
        }
        OnHealthChanged?.Invoke(CurrentHull, MaxHull, CurrentShield, MaxShield);
    }
    /// <summary>
    /// Sets the entity to dead state and triggers on death events.
    /// </summary>
    public void Die()
    {
        if (Dead) return;
        Dead = true;
        Debug.Log($"Die() on instance: {this.GetHashCode()}");
        onDeath.Invoke();
    }
    public void Kill()
    {
        TakeDamage(CurrentHull + CurrentShield, DamageType.Physical);
    }

    public void ResetHealth()
    {
        CurrentHull = MaxHull;
        CurrentShield = MaxShield;
        Dead = false;
        OnHealthChanged?.Invoke(CurrentHull, MaxHull, CurrentShield, MaxShield);
    }
}

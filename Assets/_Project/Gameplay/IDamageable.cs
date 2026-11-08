using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    public static UnityEvent<IDamageable> onUnitDeath = new();
    public bool IsAlive { get; }
    public HealthSystem healthSystem { get; set; }
}

using UnityEngine;

public abstract class AimingSystem
{
    public float Yaw;
    public float Pitch;
    public virtual void Update() { }
    public virtual void LateUpdate() { }
}

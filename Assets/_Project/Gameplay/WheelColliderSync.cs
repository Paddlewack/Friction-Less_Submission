using UnityEngine;

public class WheelColliderSync : MonoBehaviour
{
    public WheelCollider master;
    public void SyncWheels()
    {
        if (master == null) return;

        WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();

        foreach (WheelCollider wc in wheels)
        {
            if (wc == master) continue;

            wc.mass = master.mass;
            wc.radius = master.radius;
            wc.suspensionDistance = master.suspensionDistance;
            wc.forceAppPointDistance = master.forceAppPointDistance;

            wc.suspensionSpring = master.suspensionSpring;
            wc.forwardFriction = master.forwardFriction;
            wc.sidewaysFriction = master.sidewaysFriction;
        }
    }

    // Runs automatically when values change in the inspector
    void OnValidate()
    {
        SyncWheels();
    }
}
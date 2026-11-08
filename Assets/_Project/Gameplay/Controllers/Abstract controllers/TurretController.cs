using Unity.VisualScripting;
using UnityEngine;

public abstract class TurretController
{
    #region Field Declaration
    public GameObject turret;
    public Transform turretTransform;
    public Transform cannonPivotTransform;
    public Transform cannonTipTransform;
    public float aimPointSnapAngle;
    public LayerMask aimPointMask;

    protected float traverseSpeed;
    protected float elevationSpeed;
    protected float trackedTurretWorldYaw;
    protected Transform parentTransform;
    protected AimingSystem aimingSystem;
    protected FiringSystem firingSystem;
    protected TurretData data;


    #endregion
    public TurretController(AimingSystem aimSys, GameObject turret, Transform cannonPivot, Transform cannonTip, Transform parentTransform, TurretData d)
    {
        aimingSystem = aimSys;
        this.turret = turret;
        turretTransform = turret.transform;
        cannonPivotTransform = cannonPivot;
        cannonTipTransform = cannonTip;
        traverseSpeed = d.traverseSpeed;
        elevationSpeed = d.elevationSpeed;
        this.parentTransform = parentTransform;
        trackedTurretWorldYaw = turret.transform.eulerAngles.y;
        aimPointSnapAngle = d.AimPointSnapAngle;
        data = d;
        aimPointMask = d.AimPointMask;
    }

    public abstract void Update();
    public abstract void LateUpdate();
}
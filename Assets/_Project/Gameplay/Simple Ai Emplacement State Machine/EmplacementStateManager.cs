using System.Collections.Generic;
using UnityEngine;
public class EmplacementStateManager
{
    /// <summary>
    /// The rotaatable turret part of the emplacement.
    /// </summary>
    GameObject emplacementTurret;
    /// <summary>
    /// The entire emplacement.
    /// </summary>
    GameObject emplacement;
    Vector3 turretFrontDir;
    Vector3 turretTargetDir;
    float turretSpeed;
    IEmplacementState currentState;
    GameObject currentTarget;
    public AiTargetingSysData targetingSysData;
    public AiEmplacementController emplacementController;
    public EmplacementPassiveState passiveState;
    public EmplacementTargetingState targetingState;
    public EmplacementFiringState firingState;
    public Transform cannonTip;
    public Transform cannonPivot;
    

    public EmplacementStateManager(AiEmplacementController emplacementController)
    {
        
        this.emplacementController = emplacementController;
        emplacement = emplacementController.emplacement;
        emplacementTurret = emplacementController.turret;
        turretFrontDir = emplacementTurret.transform.forward;
        turretTargetDir = turretFrontDir;        
        targetingSysData = emplacementController.aiTargetingData;
        emplacementController.aimingSystem.onDetect.AddListener(TargetSeenAction);
        turretSpeed = targetingSysData.targetingSpeed;
        passiveState = new(this);
        targetingState = new(this, targetingSysData.targetingDeactivationTime);
        firingState = new(this);
        currentState = passiveState;
        cannonTip = emplacementController.cannonTip.transform;
        cannonPivot = emplacementController.cannonPivot.transform;
    }
    public void SwitchTo(EmplacementStateId id)
    {
        Exit();
        currentState = id switch
        {
            EmplacementStateId.Passive => passiveState,
            EmplacementStateId.Targeting => targetingState,
            EmplacementStateId.Firing => firingState,
            _ => passiveState
        };
        Enter();
    }
    void Enter()
    {
        currentState.Enter();
    }
    public void Update()
    {
        Debug.DrawRay(emplacementController.cannonPivot.transform.position, turretTargetDir * 100, Color.purple);
        currentState.Update();

        emplacementController.aimingSystem.Yaw = Mathf.Atan2(turretTargetDir.x, turretTargetDir.z) * Mathf.Rad2Deg;

        emplacementController.aimingSystem.Pitch = -Mathf.Asin(Mathf.Clamp(turretTargetDir.y, -1f, 1f)) * Mathf.Rad2Deg;
    }
    void Exit() { 
        currentState.Exit();
    }
    void OnTargetSeen(Dictionary<GameObject, List<Transform>> visibleTargetPoints)
    {
        currentState.OnTargetSeen(visibleTargetPoints);
    }
    public void TargetSeenAction()
    {
        OnTargetSeen(emplacementController.aimingSystem.visibleTargetPoints);
    }
    public void SetCurrentTarget(GameObject target = null)
    {
        currentTarget = target;
    }
    public void SetTurretTargetDir(Vector3 dir)
    {
        turretTargetDir = dir;
    }
    public void SetTurretTargetDirToFront()
    {
        turretTargetDir = turretFrontDir;
    }
    public Vector3 GetEmplacementPos()
    {
        return emplacement.transform.position;
    }
    public Vector3 GetEmplacementForwardDir()
    {
        return emplacement.transform.forward;
    }
    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }
    public Vector3 GetEmplacementCurrentDir()
    {
        return emplacement.transform.forward;
    }
    public Vector3 GetTurretPos()
    {
        return emplacementTurret.transform.position;
    }
}

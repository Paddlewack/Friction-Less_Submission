using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmplacementPassiveState : IEmplacementState
{
    EmplacementStateManager emplSM;
    public EmplacementPassiveState(EmplacementStateManager e)
    {
        emplSM = e;
    }
    public void Enter()
    {
        emplSM.SetTurretTargetDirToFront();
        emplSM.SetCurrentTarget(null);
    }
    public void Exit() { }
    public void Update() { }
    /// <summary>
    /// Sets emplacement target and enteres targeting state.
    /// </summary>
    /// <param name="visibleTargetPoints">Passed from AI Targeting System. Key: target game object. Value: List of targetable points on the game object.</param>
    public void OnTargetSeen(Dictionary<GameObject, List<Transform>> visibleTargetPoints)
    {
        emplSM.SetCurrentTarget(visibleTargetPoints.First().Key);
        emplSM.SwitchTo(EmplacementStateId.Targeting);
    }
}

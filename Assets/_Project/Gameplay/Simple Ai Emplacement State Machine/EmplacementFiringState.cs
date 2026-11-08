using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EmplacementFiringState :IEmplacementState
{
    EmplacementStateManager emplSM;
    public UnityEvent onFire = new UnityEvent();
    public UnityEvent onEnterFiring = new UnityEvent();
    bool fired;
    float preFireDelay;
    float preFireTimer;
    float postFireDelay;
    float postFireTimer;
    public EmplacementFiringState(EmplacementStateManager e)
    {
        emplSM = e;
        preFireDelay = emplSM.emplacementController.aiTargetingData.preFiringTime;
        postFireDelay = emplSM.emplacementController.aiTargetingData.postFiringTime;
    }
    public void Enter()
    {
        onEnterFiring.Invoke();
        preFireTimer = 0;
        postFireTimer = 0;
        fired = false;
        Debug.Log("starting firing");
    }
    public void Exit() { Debug.Log("ending firing"); }
    public void Update()
    {
        if (!fired)
        {
            preFireTimer += Time.deltaTime;
            if (preFireTimer >= preFireDelay)
            {
                onFire.Invoke();
                fired = true;
            }
        }
        if (fired)
        {
            postFireTimer += Time.deltaTime;
            if (postFireTimer >= postFireDelay)
            {
                emplSM.SwitchTo(EmplacementStateId.Targeting);
            }
        }
    }
    public void OnTargetSeen(Dictionary<GameObject, List<Transform>> visibleTargetPoints) { }
}

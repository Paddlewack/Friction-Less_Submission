using System.Collections.Generic;
using UnityEngine;
public enum EmplacementStateId { Passive, Targeting, Firing }
public interface IEmplacementState
{
    void Enter();
    void Update();
    void Exit();
    void OnTargetSeen(Dictionary<GameObject, List<Transform>> visibleTargetPoints);
}

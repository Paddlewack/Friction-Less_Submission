using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour, ITriggerable
{
    public bool isActivated = false;
    public int TriggeredCount {  get; private set; }
    public static UnityEvent<Checkpoint> onCheckpointActivated = new();

    public void Trigger()
    {
        if (isActivated) return;
        isActivated = true;
        Debug.Log($"[Checkpoint] Checkpoint unlocked: {gameObject.name}");
        onCheckpointActivated.Invoke(this);
        TriggeredCount++;
    }
}
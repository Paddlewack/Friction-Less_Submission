using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReachAreaTask : BaseTask
{
    public override TaskType taskType => TaskType.ReachArea;
    public override void Activate()
    {
        base.Activate();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCollider"))
            UpdateProgress(1);
    }
}
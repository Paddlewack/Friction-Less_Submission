using UnityEngine;
using UnityEngine.Events;

public class TaskGroup : MonoBehaviour
{
    [Header("On Group Complete")]
    public UnityEvent onGroupComplete;

    private BaseTask[] tasks;
    private int completedCount = 0;

    public void Activate()
    {
        tasks = GetComponentsInChildren<BaseTask>();
        completedCount = 0;
        Debug.Log($"[TaskGroup] Activating group: {gameObject.name} ({tasks.Length} tasks found)");

        foreach (var task in tasks)
        {
            task.onComplete.AddListener(OnTaskComplete);
            task.Activate();
        }
    }

    public void Deactivate()
    {
        foreach (var task in tasks)
            task.Deactivate();
    }

    private void OnTaskComplete()
    {
        completedCount++;
        Debug.Log($"[TaskGroup] {completedCount}/{tasks.Length} tasks complete in {gameObject.name}");

        if (completedCount >= tasks.Length)
        {
            Debug.Log($"[TaskGroup] Group complete: {gameObject.name}");
            Deactivate();
            onGroupComplete.Invoke();
        }
    }

    public void ForceComplete()
    {
        foreach (var task in tasks)
            task.ForceComplete();
    }
}
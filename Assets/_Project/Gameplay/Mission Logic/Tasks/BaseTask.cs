using UnityEngine;
using UnityEngine.Events;

public abstract class BaseTask : MonoBehaviour
{
    public enum Visibility
    {
        AlwaysVisible,
        StartHidden,
        AlwaysHidden
    }
    public enum TaskState
    {
        Deactivated,
        Active,
        InProgress,
        Completed
    }
    public enum TaskType
    {
        Default,
        ReachArea,
        CaptureZone,
        Elimination,
        // Add new types here
    }

    [Header("Task Info")]
    public abstract TaskType taskType { get; }
    public string taskName;
    [TextArea] public string description;
    [SerializeField]
    private Visibility visibility = Visibility.AlwaysVisible;

    [Header("Task Events")]
    public UnityEvent onActivate = new();
    public UnityEvent onUpdate = new();
    public UnityEvent onProgress = new();
    public UnityEvent onComplete = new();

    [Header("Task State - DO NOT EDIT")]
    public bool isVisibleInUI;
    [SerializeField]
    protected TaskState currentState = TaskState.Deactivated;
    [SerializeField]
    protected float progress = 0;

    public virtual void Activate()
    {
        currentState = TaskState.Active;
        isVisibleInUI = visibility == Visibility.AlwaysVisible;
        Debug.Log($"[Task] Started: {taskName}");
        onActivate.Invoke();
        onUpdate.Invoke();
    }

    public virtual void Deactivate()
    {
        if (currentState == TaskState.Deactivated) return;
        currentState = TaskState.Deactivated;
        onComplete.RemoveAllListeners();
        Debug.Log($"[Task] Deactivated: {taskName}");
        onUpdate.Invoke();
    }
    protected void Complete()
    {
        if (!IsActiveOrInProgress()) return;
        progress = 1;
        currentState = TaskState.Completed;
        Debug.Log($"[Task] Completed: {taskName}");
        onUpdate.Invoke();
        onComplete.Invoke();
        Deactivate();
    }

    protected void UpdateProgress(float amount)
    {
        if (!IsActiveOrInProgress()) return;
        progress = Mathf.Clamp01(progress + amount);
        if (progress >= 1) Complete();
        else
        {
            currentState = TaskState.InProgress;
            Debug.Log($"[Task] Progress updated: {taskName} - {progress * 100}%");
            onProgress.Invoke();
            onUpdate.Invoke();
        }
    }

    public void ShowHiddenTask()
    {
        if (isVisibleInUI || visibility != Visibility.StartHidden) return;
        isVisibleInUI = true;
        Debug.Log($"[Task] Shown in UI: {taskName}");
        onUpdate.Invoke();
    }
    public void HideTask()
    {
        if (!isVisibleInUI || visibility != Visibility.StartHidden) return;
        isVisibleInUI = false;
        Debug.Log($"[Task] Hidden in UI: {taskName}");
        onUpdate.Invoke();
    }

    public virtual void ForceComplete()
    {
        if (currentState == TaskState.Completed) return;
        Debug.Log($"[Task] Force completed: {taskName}");
        Complete();
    }

    protected bool IsActiveOrInProgress()
    {
        return currentState == TaskState.Active || currentState == TaskState.InProgress;
    }

    public virtual GameObject[] GetTargets()
    {
        GameObject[] targets = { gameObject };
        return targets;
    }

    /// <summary>
    /// Get current task state. Used by icon system and UI.
    /// </summary>
    public TaskState GetCurrentState() => currentState;

    /// <summary>
    /// Get current progress (0-1).
    /// </summary>
    public float GetProgress() => progress;
}
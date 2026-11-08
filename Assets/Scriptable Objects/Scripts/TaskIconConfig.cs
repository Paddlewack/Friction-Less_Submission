using UnityEngine;

/// <summary>
/// Configuration for how a specific task type's icon should appear and behave.
/// Create one of these per task type (or reuse for multiple types).
/// 
/// Examples:
/// - TaskIconConfig_ReachArea
/// - TaskIconConfig_CaptureZone
/// - TaskIconConfig_Elimination
/// </summary>
[CreateAssetMenu(fileName = "TaskIconConfig_", menuName = "Task/Icon Config")]
public class TaskIconConfig : ScriptableObject
{
    [Header("Task Type")]
    [Tooltip("The task type this config applies to (e.g., ReachAreaTask, CaptureZoneTask)")]
    public BaseTask.TaskType taskType;

    [Header("Appearance")]
    public Sprite icon;
    public Color color = Color.white;
    [Range(0.5f, 3f)]
    public float scale = 1f;
    public float heightOffset = 0f;

    [Header("Animation")]
    public bool bobUpAndDown = true;
    [Range(0.5f, 5f)]
    public float bobSpeed = 2f;
    [Range(0.1f, 2f)]
    public float bobHeight = 0.5f;

    [Header("Behavior")]
    public bool billboardToCamera = true;
}
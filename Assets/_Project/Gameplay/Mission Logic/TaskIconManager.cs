using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages all task icons in the scene. Handles:
/// - Icon spawning for active tasks
/// - Icon configuration based on task type
/// - LOS visibility updates
/// 
/// Place on an empty GameObject in your scene and assign the icon prefab and configs.
/// </summary>
public class TaskIconManager : MonoBehaviour
{
    public static TaskIconManager Instance { get; private set; }

    [Header("Icon Setup")]
    [SerializeField] private GameObject iconPrefab;

    [Header("Task Icon Configs")]
    [SerializeField] private TaskIconConfig[] iconConfigs;

    [Header("LOS Detection")]
    [SerializeField] private float losCheckDistance = 200f;
    [SerializeField] private LayerMask occlusionLayerMask = -1;
    [SerializeField] private float updateFrequency = 0.1f;

    private Transform playerTransform;
    private Dictionary<BaseTask, TaskIcon[]> activeIcons = new Dictionary<BaseTask, TaskIcon[]>();
    private Dictionary<BaseTask.TaskType, TaskIconConfig> configMap = new Dictionary<BaseTask.TaskType, TaskIconConfig>();
    private float timeSinceLastUpdate = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build taskType -> config mapping
        BuildConfigMap();
    }

    private void BuildConfigMap()
    {
        configMap.Clear();

        if (iconConfigs == null || iconConfigs.Length == 0)
        {
            Debug.LogWarning("[TaskIconManager] No icon configs assigned!");
            return;
        }

        foreach (TaskIconConfig config in iconConfigs)
        {
            if (config == null) continue;
            configMap[config.taskType] = config;
            Debug.Log($"[TaskIconManager] Registered icon config for: {config.taskType}");
        }
    }

    private void Start()
    {
        // Find player camera or transform
        playerTransform = Camera.main?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("[TaskIconManager] Could not find player camera. Ensure Camera is tagged as MainCamera.");
            enabled = false;
        }

        if (iconPrefab == null)
        {
            Debug.LogError("[TaskIconManager] Icon prefab not assigned!");
            enabled = false;
        }
    }

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateFrequency)
        {
            UpdateAllIconVisibility();
            timeSinceLastUpdate = 0f;
        }
    }

    /// <summary>
    /// Called by TaskGroup when it activates. Registers all tasks for icon display.
    /// </summary>
    public void RegisterTaskGroup(BaseTask[] tasks)
    {
        foreach (var task in tasks)
        {
            if (!activeIcons.ContainsKey(task))
            {
                SpawnIconForTask(task);
                task.onUpdate.AddListener(() => OnTaskStateChanged(task));
            }
        }
    }

    private void SpawnIconForTask(BaseTask task)
    {
        List<TaskIcon> icons = new List<TaskIcon>();
        GameObject[] taskTargets = task.GetTargets();
        foreach (GameObject target in taskTargets)
        {
            GameObject iconGO = Instantiate(iconPrefab, target.transform.position, Quaternion.identity);
            TaskIcon icon = iconGO.GetComponent<TaskIcon>();

            if (icon == null)
            {
                Debug.LogError("[TaskIconManager] Icon prefab missing TaskIcon component!");
                Destroy(iconGO);
                return;
            }

            // Get config and initialize icon
            TaskIconConfig config = GetConfigForTask(task);
            icon.Initialize(task, config, target);
            icon.gameObject.name = $"TaskIcon for {target.name}";
            icons.Add(icon);
        }

        activeIcons[task] = icons.ToArray();

        bool visible = CheckTaskVisibility(task);
        Array.ForEach(activeIcons[task], icon => icon.SetVisible(visible));

        Debug.Log($"[TaskIconManager] Spawned icon for task: {task.taskName} ({task.taskType})");
    }

    /// <summary>
    /// Get the icon config for a specific task.
    /// Returns config if found, otherwise null.
    /// </summary>
    public TaskIconConfig GetConfigForTask(BaseTask task)
    {
        if (configMap.TryGetValue(task.taskType, out var config))
            return config;

        Debug.LogWarning($"[TaskIconManager] No icon config found for task type: {task.taskType}. Using default settings.");
        return null;
    }

    private void OnTaskStateChanged(BaseTask task)
    {
        if (!activeIcons.ContainsKey(task)) return;

        bool visible = CheckTaskVisibility(task);
        foreach (TaskIcon icon in activeIcons[task])
        {
            GameObject[] newTargets = task.GetTargets();
            if (!newTargets.Contains(icon.GetLinkedTarget()))
            {
                // Hide icon when associated task no longer has the linked target
                icon.SetVisible(false);
            }
            else
            {
                icon.SetVisible(visible);
            }
        }

        // Destroy icon when task is completed
        if (task.GetCurrentState() == BaseTask.TaskState.Completed)
        {
            Array.ForEach(activeIcons[task], icon => Destroy(icon.gameObject));
            activeIcons.Remove(task);
        }
    }
    private bool CheckTaskVisibility(BaseTask task)
    {
        // Hide icon if task is no longer active/in progress or not visible in UI
        bool shouldShow = task.isVisibleInUI &&
                         (task.GetCurrentState() == BaseTask.TaskState.Active ||
                          task.GetCurrentState() == BaseTask.TaskState.InProgress);
        return shouldShow;
    }


    private void UpdateAllIconVisibility()
    {
        if (playerTransform == null) return;

        foreach (TaskIcon[] iconArray in activeIcons.Values)
        {
            if (iconArray == null) continue;
            foreach(TaskIcon icon in iconArray)
            {
                bool hasLineOfSight = CheckLineOfSight(playerTransform.position, icon.transform.position);
                icon.SetLineOfSightAlpha(hasLineOfSight);
            }
        }
    }

    private bool CheckLineOfSight(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector3 direction = (toPosition - fromPosition).normalized;
        float distance = Vector3.Distance(fromPosition, toPosition);

        // Don't raycast if target is too far
        if (distance > losCheckDistance)
            return false;

        // Raycast to check if anything blocks the view
        if (Physics.Raycast(fromPosition, direction, distance, occlusionLayerMask))
        {
            return false; // Something is blocking
        }

        return true; // Clear line of sight
    }

    public void Clear()
    {
        foreach (TaskIcon[] iconArray in activeIcons.Values)
        {
            foreach (TaskIcon icon in iconArray)
            {
                if (icon != null)
                Destroy(icon.gameObject);
            }
        }
        activeIcons.Clear();
    }

    private void OnDestroy()
    {
        Clear();
    }
}
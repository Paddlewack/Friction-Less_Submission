using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using IngameDebugConsole;
using System.Collections;


public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("References")]
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private Material captureZoneMaterial;

    [Header("Live Data Viewer - DO NOT EDIT")]
    [SerializeField]
    private Checkpoint[] checkpoints;
    [SerializeField]
    private Checkpoint currentCheckpoint;
    [SerializeField]
    private TaskGroup[] taskGroups;
    [SerializeField]
    private int currentGroupIndex = -1;
    [SerializeField]
    private GameObject currentPlayer;

    private PlayerTankController ptc;
    private bool isRespawning = false;

    public static UnityEvent playerRespawn = new UnityEvent();


    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        GameObject taskGroupHolder = GameObject.FindGameObjectWithTag("TaskGroupHolder");
        if (taskGroupHolder == null)
        {
            Debug.LogError("No GameObject with tag 'TaskGroupHolder' found in scene!");
            return;
        }
        taskGroups = taskGroupHolder.GetComponentsInChildren<TaskGroup>();

        GameObject checkpointHolder = GameObject.FindGameObjectWithTag("CheckpointHolder");
        if (checkpointHolder == null)
        {
            Debug.LogError("No GameObject with tag 'CheckpointHolder' found in scene!");
            return;
        }
        checkpoints = checkpointHolder.GetComponentsInChildren<Checkpoint>();

        DebugLogConsole.AddCommand("respawn", "Respawns player tank at checkpoint", LoadCheckpoint);
        DebugLogConsole.AddCommand("restart", "Restarts the current game scene", RestartCurrentScene);
        DebugLogConsole.AddCommand("nextTask", "Force completes the current task group", ForceCompleteCurrentGroup);

        currentCheckpoint = GetCurrentCheckpoint();
        IDamageable.onUnitDeath.AddListener(OnUnitDeath);
        Checkpoint.onCheckpointActivated.AddListener(OnCheckpointActivated);

        if (currentPlayer = GameObject.FindGameObjectWithTag("Player"))
        {
            Debug.Log("Player already exists, skipping instantiation.");
        }
        else
        {
            Debug.Log("No player found, instantiating new player.");
            SpawnPlayerAtCheckpoint();
        }

        AdvanceTaskGroup();
    }

    void OnUnitDeath(IDamageable unit)
    {
        if (unit is PlayerTankController)
        {
            HandlePlayerDeath();
        }
    }
    void OnCheckpointActivated(Checkpoint checkpoint)
    {
        currentCheckpoint = GetCurrentCheckpoint();
        Debug.Log($"Current checkpoint updated to: {currentCheckpoint.gameObject.name}");
    }

    void HandlePlayerDeath()
    {
        StartCoroutine(RespawnAfterDelay(4f));
    }
    IEnumerator RespawnAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        LoadCheckpoint();
    }

    Checkpoint GetCurrentCheckpoint()
    {
        Checkpoint highestCheckpoint = checkpoints[0]; // Default to first checkpoint if none activated
        foreach (var checkpoint in checkpoints)
        {
            if (checkpoint.isActivated)
            {
                highestCheckpoint = checkpoint;
            }
        }
        return highestCheckpoint;
    }

    /// <summary>
    /// Spawns player with proper cleanup of previous player.
    /// Handles async nature of Destroy() by waiting a frame.
    /// </summary>
    void SpawnPlayerAtCheckpoint()
    {
        Transform spawnLocation = currentCheckpoint.transform;
        StartCoroutine(SpawnPlayerAsync(spawnLocation));
    }

    IEnumerator SpawnPlayerAsync(Transform spawnLocation)
    {
        isRespawning = true;

        GameObject oldPlayer = currentPlayer;
        Transform parentObject = oldPlayer != null ? oldPlayer.transform.parent : null;

        // disable player
        oldPlayer.tag = "Untagged";
        foreach (var col in oldPlayer.GetComponentsInChildren<Collider>())
            col.enabled = false;
        if (oldPlayer.TryGetComponent(out Rigidbody rb))
            rb.detectCollisions = false;

        // create new player
        GameObject newPlayer = Instantiate(playerPrefab, spawnLocation.position, spawnLocation.rotation, parentObject);
        currentPlayer = newPlayer;
        ptc = newPlayer.GetComponent<PlayerTankController>();

        yield return null;

        // destroy player after new one takes over
        if (oldPlayer != null)
            Destroy(oldPlayer);

        playerRespawn.Invoke();
        isRespawning = false;
    }

    void LoadCheckpoint()
    {
        if (isRespawning)
        {
            Debug.Log("Already respawning, ignoring respawn request.");
            return;
        }
        SpawnPlayerAtCheckpoint();
    }

    public void RestartCurrentScene()
    {
        IDamageable.onUnitDeath.RemoveAllListeners();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    void AdvanceTaskGroup()
    {
        currentGroupIndex++;
        if (currentGroupIndex >= taskGroups.Length)
        {
            Debug.Log("[Mission] All task groups complete!");
            return;
        }

        TaskGroup group = taskGroups[currentGroupIndex];
        group.onGroupComplete.AddListener(AdvanceTaskGroup);
        group.Activate();

        // Register tasks with icon system
        if (TaskIconManager.Instance != null)
        {
            BaseTask[] tasks = group.GetComponentsInChildren<BaseTask>();
            TaskIconManager.Instance.RegisterTaskGroup(tasks);
        }
    }

    void ForceCompleteCurrentGroup()
    {
        if (currentGroupIndex < 0 || currentGroupIndex >= taskGroups.Length)
        {
            Debug.LogWarning("[Mission] No active task group to force complete.");
            return;
        }
        taskGroups[currentGroupIndex].ForceComplete();
    }

    public Material GetCaptureZoneMaterial()
    {
        return captureZoneMaterial;
    }

    public void TriggerCompleteMission()
    {
        CompleteMission();
    }

    public void ExitMission()
    {
        GameManager.Instance.ExitMission();
    }

    private void CompleteMission()
    {
        ExitMission();
    }
}
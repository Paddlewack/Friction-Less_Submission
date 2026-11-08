using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual task icon component. Shows in world space and adjusts transparency based on line of sight.
/// This should be placed on a prefab with an Image component (or Canvas with Image child).
/// </summary>
[RequireComponent(typeof(Canvas))]
public class TaskIcon : MonoBehaviour
{
    [Header("Base Settings ")]
    [SerializeField] private float iconScale = 0.4f;
    [SerializeField] private float spawnHeight = 4f;
    [SerializeField] private float visibleAlpha = 1f;
    [SerializeField] private float occludedAlpha = 0.5f;

    [Header("Appearance")]
    [SerializeField] private bool billboardToCamera = true;
    [SerializeField] private float heightOffset = 0f;

    [Header("Animation")]
    [SerializeField] private bool bobUpAndDown = true;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;

    private BaseTask linkedTask;
    private GameObject linkedTarget;
    private Canvas canvas;
    private Image image;
    private CanvasGroup canvasGroup;
    private Vector3 initialPosition;
    private bool isVisible = true;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        image = GetComponentInChildren<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Setup canvas for world space rendering
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;

            // Scale canvas appropriately for world space
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 100);
        }

        initialPosition = transform.position;
    }

    public void Initialize(BaseTask task, TaskIconConfig config, GameObject target)
    {
        // Apply config if provided
        if (config != null)
        {
            ApplyConfig(config);
        }
        else
        {
            // Use defaults from inspector
            ApplyDefaultAppearance();
        }

        linkedTask = task;
        linkedTarget = target;
        initialPosition = target.transform.position + new Vector3(0, spawnHeight + heightOffset, 0);
        transform.position = initialPosition;
        
        image.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
    }

    private void ApplyConfig(TaskIconConfig config)
    {
        // Spawn behaviour
        heightOffset = config.heightOffset;

        // Icon appearance
        if (image != null)
        {
            if (config.icon != null)
                image.sprite = config.icon;
            image.color = config.color;
        }

        // Scale
        transform.localScale = Vector3.one * config.scale;

        // Animation settings
        bobUpAndDown = config.bobUpAndDown;
        bobSpeed = config.bobSpeed;
        bobHeight = config.bobHeight;

        // Behavior
        billboardToCamera = config.billboardToCamera;
    }

    private void ApplyDefaultAppearance()
    {
        // Use inspector-assigned defaults if no config provided
        if (image != null)
            image.color = Color.white;
    }

    private void Update()
    {
        if (linkedTask == null)
        {
            Destroy(gameObject);
            return;
        }

        // Billboard to camera
        if (billboardToCamera && Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - Camera.main.transform.position
            );
        }

        // Bob up and down animation
        if (bobUpAndDown)
        {
            float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = initialPosition + Vector3.up * yOffset;
        }
        else
        {
            transform.position = initialPosition;
        }
    }

    /// <summary>
    /// Set whether the icon should be visible based on task state.
    /// </summary>
    public void SetVisible(bool visible)
    {
        isVisible = visible;

        if (!visible)
        {
            canvasGroup.alpha = 0;
        }
        else
        {
            // Will be updated by LOS system
            canvasGroup.alpha = visibleAlpha;
        }
    }

    /// <summary>
    /// Called by TaskIconManager to update alpha based on line of sight.
    /// </summary>
    public void SetLineOfSightAlpha(bool hasLineOfSight)
    {
        if (!isVisible)
        {
            canvasGroup.alpha = 0;
            return;
        }

        canvasGroup.alpha = hasLineOfSight ? visibleAlpha : occludedAlpha;
    }

    public BaseTask GetLinkedTask() => linkedTask;
    public GameObject GetLinkedTarget() => linkedTarget;
}
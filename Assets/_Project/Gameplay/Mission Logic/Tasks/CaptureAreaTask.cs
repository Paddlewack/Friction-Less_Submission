using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CaptureZoneTask : BaseTask
{
    public override TaskType taskType => TaskType.CaptureZone;
    [Header("Capture Zone Config")]
    public float captureTime = 5f;

    [Header("Visual Indicator - Alpha set by material")]
    public bool showCaptureZone = true;
    public float visualHeight = 0.1f;
    public Color activeColor = new Color(1, 0, 0);
    public Color inProgressColor = new Color(1, 1, 0);
    public Color completeColor = new Color(0, 0, 1);

    [Header("VFX")]
    public ParticleSystem[] zoneVFX = new ParticleSystem[4];

    public static UnityEvent PlayerStartCapturing = new UnityEvent();
    public static UnityEvent PlayerStoppedCapturing = new UnityEvent();

    private bool playerInZone = false;
    private bool playerStartedCapturing = false;
    private float timeInZone = 0f;
    private CapsuleCollider captureCollider;
    private GameObject cylinderVisual;
    private Material cylinderMaterial;
    private Renderer cylinderRenderer;

    void Start()
    {
        cylinderMaterial = MissionManager.Instance.GetCaptureZoneMaterial();
        captureCollider = GetComponent<CapsuleCollider>();
        CreateCylinderVisual();

        this.onUpdate.AddListener(() => {
            if (currentState == TaskState.Active)
                ChangeVisualsColor(activeColor);
            else if (currentState == TaskState.InProgress)
                ChangeVisualsColor(inProgressColor);
            else if (currentState == TaskState.Completed)
                ChangeVisualsColor(completeColor);
        });
    }

    void CreateCylinderVisual()
    {
        cylinderVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinderVisual.name = "CaptureAreaVisual";

        Collider visualCollider = cylinderVisual.GetComponent<Collider>();
        DestroyImmediate(visualCollider);

        cylinderVisual.transform.SetParent(transform);
        cylinderVisual.transform.localPosition = Vector3.zero;

        float radius = captureCollider.radius;
        cylinderVisual.transform.localScale = new Vector3(radius * 2, visualHeight / 2, radius * 2);
        cylinderVisual.transform.localPosition = new Vector3(0, visualHeight / 2, 0);

        cylinderRenderer = cylinderVisual.GetComponent<MeshRenderer>();
        cylinderVisual.SetActive(false);
    }

    void Update()
    {
        if (!IsActiveOrInProgress()) return;
        if (!playerInZone)
        {
            if (playerStartedCapturing) PlayerStoppedCapturing.Invoke();
            return;
        }
        if (!playerStartedCapturing)
        {
            playerStartedCapturing = true;
            PlayerStartCapturing.Invoke();
        }

        timeInZone += Time.deltaTime;

        float safeCaptureTime = Mathf.Max(0.0001f, captureTime);
        float deltaProgress = Time.deltaTime / safeCaptureTime;

        UpdateProgress(deltaProgress);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCollider"))
            playerInZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCollider"))
        {
            playerInZone = false;
            timeInZone = 0f;
        }
    }

    private void ChangeVisualsColor(Color newColor)
    {
        if (cylinderRenderer == null) return;

        cylinderRenderer.material = cylinderMaterial;
        Color matColor = cylinderRenderer.material.color;
        matColor.r = newColor.r;
        matColor.g = newColor.g;
        matColor.b = newColor.b;
        cylinderRenderer.material.color = matColor;
    }

    private void PlayZoneVFX()
    {
        foreach (var vfx in zoneVFX)
        {
            if (vfx != null) vfx.Play();
        }
    }

    private void StopZoneVFX()
    {
        foreach (var vfx in zoneVFX)
        {
            if (vfx != null) vfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public override void Activate()
    {
        base.Activate();
        timeInZone = 0f;
        if (cylinderVisual != null)
            cylinderVisual.SetActive(showCaptureZone);
        ChangeVisualsColor(activeColor);
        PlayZoneVFX();
    }

    public override void Deactivate()
    {
        base.Deactivate();
        if (cylinderVisual != null)
            cylinderVisual.SetActive(false);
        StopZoneVFX();
    }

    public void SetCaptureZoneVisible(bool visible)
    {
        showCaptureZone = visible;
        if (cylinderVisual != null)
            cylinderVisual.SetActive(visible);
    }

    void OnDestroy()
    {
        if (cylinderVisual != null)
            Destroy(cylinderVisual);
    }
}
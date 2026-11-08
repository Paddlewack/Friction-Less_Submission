using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Camera Crosshair Settings")]
    [SerializeField] private RectTransform cameraCrosshairImage;
    [SerializeField] private Color cameraCrosshairColor = Color.white;

    [Header("Cannon Crosshair Settings")]
    [SerializeField] private RectTransform cannonCrosshairImage;
    [SerializeField] private Color cannonCrosshairColor = Color.white;

    [Header("Distance Scaling")]
    [Tooltip("Distance (world units) at which the crosshair renders at crosshairSize")]
    [SerializeField] private float baseCrosshairSize = 40f;
    [SerializeField] private float referenceDistance = 50f;
    [SerializeField] private float minSize = 10f;
    [SerializeField] private float maxSize = 120f;

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    private PlayerTankController playerTankController;
    private Image _crosshairImageComponent;

    private void OnEnable()
    {
        if (cameraCrosshairImage != null)
        {
            _crosshairImageComponent = cameraCrosshairImage.GetComponent<Image>();
            if (_crosshairImageComponent != null)
                _crosshairImageComponent.color = cameraCrosshairColor;
        }
        if (cannonCrosshairImage != null)
        {
            _crosshairImageComponent = cannonCrosshairImage.GetComponent<Image>();
            if (_crosshairImageComponent != null)
                _crosshairImageComponent.color = cannonCrosshairColor;
        }
    }

    public void Initialize(PlayerTankController ptc, Camera cam)
    {
        playerTankController = ptc;
        targetCamera = cam;
    }

    private void Update()
    {
        if (playerTankController?.aimingSystem == null || targetCamera == null)
            return;
        PlayerAimingSystem aimingSystem = playerTankController.aimingSystem as PlayerAimingSystem;
        if (aimingSystem == null)
            return;

        // Get the world point from aiming system
        Vector3 cameraAimPoint = aimingSystem.CameraAimPoint;
        Vector3 cannonAimPoint = aimingSystem.CannonAimPoint;
        UpdateCameraCrosshair(cameraAimPoint);
        UpdateCannonCrosshair(cannonAimPoint);
    }

    private void UpdateCameraCrosshair(Vector3 crosshairPoint)
    {
        // Convert world point to screen position
        Vector3 screenPoint = targetCamera.WorldToScreenPoint(crosshairPoint);

        cameraCrosshairImage.position = new Vector3(screenPoint.x, screenPoint.y, 0f);

        // Scale by inverse distance so closer targets = bigger crosshair.
        // At exactly referenceDistance the size equals crosshairSize.
        float distance = Mathf.Max(Vector3.Distance(targetCamera.transform.position, crosshairPoint), 0.01f);
        float scaledSize = Mathf.Clamp(baseCrosshairSize * (referenceDistance / distance), minSize, maxSize);
        cameraCrosshairImage.sizeDelta = new Vector2(scaledSize, scaledSize);

        // Only update if the point is in front of the camera
        if (screenPoint.z > 0 && cameraCrosshairImage != null)
        {
            
        }
        Debug.DrawRay(targetCamera.gameObject.transform.position, (crosshairPoint - targetCamera.gameObject.transform.position).normalized* 1000, Color.green);
    }

    private void UpdateCannonCrosshair(Vector3 crosshairPoint)
    {
        // Convert world point to screen position
        Vector3 screenPoint = targetCamera.WorldToScreenPoint(crosshairPoint);

        cannonCrosshairImage.position = new Vector3(screenPoint.x, screenPoint.y, 0f);

        // Scale by inverse distance so closer targets = bigger crosshair.
        // At exactly referenceDistance the size equals crosshairSize.
        float distance = Mathf.Max(Vector3.Distance(targetCamera.transform.position, crosshairPoint), 0.01f);
        float scaledSize = Mathf.Clamp(baseCrosshairSize * (referenceDistance / distance), minSize, maxSize);
        cannonCrosshairImage.sizeDelta = new Vector2(scaledSize, scaledSize);

        // Only update if the point is in front of the camera
        if (screenPoint.z > 0 && cannonCrosshairImage != null)
        {

        }
        Debug.DrawRay(targetCamera.gameObject.transform.position, (crosshairPoint - targetCamera.gameObject.transform.position).normalized * 1000, Color.pink);
    }

    public void SetCrosshairColor(Color color)
    {
        cameraCrosshairColor = color;
        if (_crosshairImageComponent != null)
            _crosshairImageComponent.color = color;
    }

    public void SetCameraCrosshairSize(float size)
    {
        baseCrosshairSize = size;
        if (cameraCrosshairImage != null)
            cameraCrosshairImage.sizeDelta = new Vector2(size, size);
    }
}
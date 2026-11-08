using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimingSystem : AimingSystem
{
    PlayerTankController playerTankController;
    Transform tankTransform;

    //Sensitivity
    float sensX;
    float sensY;

    //Elevation Limits (degrees)
    public float minPitch;
    public float maxPitch;

    //Aim Distance
    public float aimDistance = 50000f;
    public Vector3 CameraAimPoint { get; private set; }
    public Vector3 CannonAimPoint { get; private set; }
    private Vector2 _lookDelta;
    private Vector2 _cameraOnlyDelta;
    public PlayerCameraController camController;

    // Camera raycast aiming
    private LayerMask aimpointMask;
    private Camera _activeCamera;
    private float _raycastDistance = 1000f;

    public PlayerAimingSystem(PlayerTankController ptc)
    {
        playerTankController = ptc;
        PlayerTankData data = playerTankController.playerTankData;
        sensX = data.mouseSensX;
        sensY = data.mouseSensY;
        minPitch = data.yMinMax.x;
        maxPitch = data.yMinMax.y;
        tankTransform = playerTankController.tank.transform;
        Yaw = tankTransform.eulerAngles.y;
        Pitch = 0f;
        camController = new(playerTankController);
        aimpointMask = ptc.aimpointMask;
    }

    public override void Update()
    {
        // Update active camera reference
        _activeCamera = playerTankController.thirdPersonCam.isActiveAndEnabled
            ? playerTankController.thirdPersonCam
            : playerTankController.firstPersonCam;
    }

    public override void LateUpdate()
    {
        camController.LateUpdate();
        // Calculate world point from camera raycast at screen center
        UpdateCameraAimPoint();
        UpdateCannonAimPoint();
        _cameraOnlyDelta = Vector2.zero;
        _lookDelta = Vector2.zero;
    }

    private void UpdateCameraAimPoint()
    {
        if (_activeCamera == null) return;

        // Raycast from center of screen
        Ray ray = _activeCamera.ScreenPointToRay(_activeCamera.pixelRect.center);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, aimpointMask))
        {
            CameraAimPoint = hit.point;
        }
        else
        {
            // If no hit, use point at max distance along ray
            CameraAimPoint = ray.origin + ray.direction * _raycastDistance;
        }
    }

    private void UpdateCannonAimPoint()
    {
        if (_activeCamera == null) return;

        Transform cannonTip = (playerTankController.turretController as PlayerTurretController).cannonTipTransform;

        // Ray from cannon tip forward
        Ray ray = new Ray(cannonTip.position, cannonTip.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, aimpointMask))
        {
            CannonAimPoint = hit.point;
        }
        else
        {
            CannonAimPoint = ray.origin + ray.direction * _raycastDistance;
        }

        Debug.DrawRay(ray.origin, ray.direction * _raycastDistance, Color.blue);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        _lookDelta = ctx.ReadValue<Vector2>();
        // Pass input to camera controller for handling
        camController.AddLookDelta(_lookDelta);
    }

    public void OnLookCameraOnly(InputAction.CallbackContext ctx)
    {
        _cameraOnlyDelta = ctx.ReadValue<Vector2>();
        // During freelook, only camera moves
        camController.AddLookDelta(_cameraOnlyDelta);
    }
}
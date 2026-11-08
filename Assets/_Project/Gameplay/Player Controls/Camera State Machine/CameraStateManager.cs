using UnityEngine;
using UnityEngine.InputSystem;

public class CameraStateManager
{
    private InputSystem_Actions _actions;

    #region Field Declaration
    public Transform followTarget;
    public GameObject cameraTP;
    public GameObject cameraFP;

    [Header("Camera States")]
    ThirdPersonState thirdPerson;
    FirstPersonState firstPerson;
    public ICameraState cameraState;
    public GameObject playerTurret;
    public Transform cannonPivot;
    public Transform cannonTip;

    [Header("Speed")]
    public float trackSpeed = 8f;

    public float currentYaw;
    public float currentPitch;
    public PlayerTankController playerTankController;

    private PlayerCameraData playerCameraData;
    private float _lookYaw = 0f;
    private float _lookPitch = 0f;
    private float _freeLookYaw = 0f;
    private float _freeLookPitch = 0f;
    #endregion

    public CameraStateManager(PlayerTankController ptc)
    {
        playerTankController = ptc;
        playerCameraData = ptc.playerCameraData;
        followTarget = playerTankController.turret.transform;
        cameraTP = playerTankController.thirdPersonCam.gameObject;
        cameraFP = playerTankController.firstPersonCam.gameObject;
        playerTurret = playerTankController.turret;
        cannonPivot = playerTankController.cannonPivot.transform;
        cannonTip = playerTankController.cannonTip.transform;

        thirdPerson = new(this, playerCameraData.distance);
        firstPerson = new(this);
        cameraState = thirdPerson;

        _lookPitch = Mathf.Clamp(6f, playerCameraData.minPitch, playerCameraData.maxPitch);
        _lookYaw = playerTankController.transform.eulerAngles.y;
        currentPitch = _lookPitch;
        currentYaw = _lookYaw;

        cameraState.Enter();
    }

    public void LateUpdate()
    {
        // Target is direct input + freelook offset
        float targetYaw = _lookYaw + _freeLookYaw;
        float targetPitch = Mathf.Clamp(_lookPitch + _freeLookPitch, playerCameraData.minPitch, playerCameraData.maxPitch);

        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, trackSpeed * Time.deltaTime);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, trackSpeed * Time.deltaTime);
        cameraState.Update();
    }

    public void AddLookDelta(Vector2 delta)
    {
        PlayerTankData data = playerTankController.playerTankData;
        _lookYaw += delta.x * data.mouseSensX;
        _lookPitch -= delta.y * data.mouseSensY;
        _lookPitch = Mathf.Clamp(_lookPitch, playerCameraData.minPitch, playerCameraData.maxPitch);
    }

    public void ResetFreeLook()
    {
        _freeLookYaw = 0f;
        _freeLookPitch = 0f;
    }

    public void SwitchCamera()
    {
        cameraState.Exit();
        if (cameraState == thirdPerson)
            cameraState = firstPerson;
        else
            cameraState = thirdPerson;
        cameraState.Enter();
    }
}
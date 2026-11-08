using UnityEngine;

public class ThirdPersonState : ICameraState
{
    CameraStateManager camSM;

    public float distance;
    public float heightOffset = 5f;

    public ThirdPersonState(CameraStateManager c, float thirdPersonDistance)
    {
        camSM = c;
        distance = thirdPersonDistance;
    }
    public void Enter()
    {
        camSM.cameraTP.SetActive(true);
    }
    public void Exit()
    {
        camSM.cameraTP.SetActive(false);
    }


    public void Update()
    {
        // Build camera position from angles
        Quaternion rotation = Quaternion.Euler(camSM.currentPitch, camSM.currentYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, heightOffset, -distance);
        camSM.cameraTP.transform.position = camSM.followTarget.position + offset;
        camSM.cameraTP.transform.rotation = rotation;
    }
}

using UnityEngine;
public class FirstPersonState : ICameraState
{
    CameraStateManager camSM;
    public FirstPersonState(CameraStateManager c)
    {
        camSM = c;
    }
    public void Enter()
    {
        camSM.cameraFP.SetActive(true);
        camSM.playerTurret.GetComponent<Renderer>().enabled = false;
        foreach (Renderer r in camSM.playerTurret.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
    }
    public void Exit()
    {
        camSM.cameraFP.SetActive(false);
        camSM.playerTurret.GetComponent<Renderer>().enabled = true;
        foreach (Renderer r in camSM.playerTurret.GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
        
    }
    public void Update()
    {
        camSM.cameraFP.transform.rotation = Quaternion.Euler(camSM.currentPitch, camSM.currentYaw, 0f);
    }
}

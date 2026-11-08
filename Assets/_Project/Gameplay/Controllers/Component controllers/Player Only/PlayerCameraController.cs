using UnityEngine;

public class PlayerCameraController
{
    PlayerTankController playerTankController;
    public CameraStateManager camSM;

    public PlayerCameraController(PlayerTankController ptc)
    {
        playerTankController = ptc;
        camSM = new(playerTankController);
    }

    public void LateUpdate()
    {
        camSM.LateUpdate();
    }

    public void AddLookDelta(Vector2 delta)
    {
        camSM.AddLookDelta(delta);
    }
}
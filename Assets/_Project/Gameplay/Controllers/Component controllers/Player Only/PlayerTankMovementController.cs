using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTankMovementController : TankMovementController
{

    public PlayerTankMovementController(TankController tankController, MovementData movementData) : base(tankController, movementData)
    {

    }

    #region Input Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveVector = context.ReadValue<Vector2>();
        Move(moveVector);
    }
    public void OnDrift(InputAction.CallbackContext context)
    {
        bool driftInput = context.ReadValueAsButton();
        Drift(driftInput);
    }
    #endregion

}
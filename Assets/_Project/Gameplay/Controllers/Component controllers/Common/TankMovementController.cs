using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TankMovementController
{
    private TankController tc;
    private MovementData md;

    private Vector3 currentTankLocalVelocity = Vector3.zero;
    private float tankVelocityRatio = 0;
    private float currentTiltAmount = 0f;

    // Minimum angular velocity to overcome static friction when turning.
    // A value lower than 8 will cause the tank to stick when trying to turn from standstill.
    private float minimumAngularVelocity = 10f; 

    private InputSystem_Actions _input;
    private float throttleInput = 0f;
    private float steerInput = 0f;
    private bool driftInput = false;
    private bool driftAvailable = false;
    private Dictionary<WheelCollider, int> wheelsGrounded = new Dictionary<WheelCollider, int>();
    private bool isGrounded = false;
    private bool isMovementEnabled = true;

    public UnityEvent onDriftStart = new();
    public UnityEvent onDriftStop = new();


    public enum State
    {
        Normal,
        Drifting
    }
    private State tankState = State.Normal;

    public TankMovementController(TankController tankController, MovementData movementData)
    {
        tc = tankController;
        md = movementData;
    }

    public void Update()
    {
        // Input and status checks in Update (variable rate is fine)
        CheckGrounded();
        CalculateTankVelocity();
    }

    public void FixedUpdate()
    {
        // All physics calculations in FixedUpdate (fixed timestep)
        Movement();
        UpdateModelPose();
    }

    public void enableMovement()
    {
        isMovementEnabled = true;
    }
    public void disableMovement()
    {
        isMovementEnabled = false;
    }

    #region Input Callbacks
    /// <summary>
    /// Sets movement input for both player and AI controllers.
    /// </summary>
    /// <param name="moveInput">Movement vector where X is steering and Y is throttle (-1 to 1 range)</param>
    public void SetMovementInput(Vector2 moveInput)
    {
        throttleInput = Mathf.Clamp(moveInput.y, -1f, 1f);
        steerInput = Mathf.Clamp(moveInput.x, -1f, 1f);
    }

    /// <summary>
    /// Sets drift input for both player and AI controllers.
    /// </summary>
    public void SetDriftInput(bool shouldDrift)
    {
        driftInput = shouldDrift;
    }

    /// <summary>
    /// Legacy input callback for player controller compatibility.
    /// </summary>
    protected void Move(Vector2 moveInput)
    {
        SetMovementInput(moveInput);
    }

    /// <summary>
    /// Legacy input callback for player controller compatibility.
    /// </summary>
    protected void Drift(bool driftInput)
    {
        SetDriftInput(driftInput);
    }
    #endregion


    #region Tank Status Check
    private void CheckGrounded()
    {
        if (tc.wheels.Any(wheel => wheel.CheckWheelGrounded()))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CalculateTankVelocity()
    {
        currentTankLocalVelocity = tc.transform.InverseTransformDirection(tc.rb.linearVelocity);
        float forwardSpeed = currentTankLocalVelocity.z;
        tankVelocityRatio = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / md.maxSpeed); // Calculate velocity ratio (0 to 1) based on max speed
    }

    #endregion


    #region Movement

    private void Movement()
    {
        if (!isMovementEnabled) return;
        DriftCheck();

        if (isGrounded || !md.requiesGroundedToMove)
        {
            ForwardMovement();
        }
        Turn();
        SidewaysDrag();
        ApplyAngularDrag();
        if (tankState == State.Drifting)
            MaintainDrift();
    }

    private void DriftCheck()
    {
        float horizontalSpeed = new Vector3(currentTankLocalVelocity.x, 0, currentTankLocalVelocity.z).magnitude;
        if (tankState == State.Normal) // Check if drift is available during normal movement
            driftAvailable = horizontalSpeed >= md.driftStartSpeed;
        else // Check if drift is still avilable during drifting
            driftAvailable = horizontalSpeed >= md.driftStopSpeed;

        if (driftInput && tankState == State.Normal && driftAvailable) // Try drifting
            StartDrift();
        else if (driftInput && driftAvailable) // Continue Drifting
            //Continue drifting
            return;
        else if (!driftInput && tankState == State.Drifting) // Try stopping drift
            StopDrift();
        else if (driftInput && tankState == State.Drifting && !driftAvailable) // Force stop drift
            StopDrift();
    }
    private void StartDrift()
    {
        
        tankState = State.Drifting;
        onDriftStart.Invoke();
    }
    private void StopDrift()
    {
        tankState = State.Normal;
        onDriftStop.Invoke();
    }
    private void MaintainDrift()
    {
        RedirectDriftVelocity();
    }
    private void RedirectDriftVelocity() // Essentially allows more steering during drift without affecting total velocity.
    {
        // Each FixedUpdate, bleed a portion of lateral (sideways) velocity into the forward or backward
        // direction depending on moveInput. This gives the player agency over where the drift carries
        // them rather than lateral velocity being purely eaten by DriftDragCoefficent.
        float lateralSpeed = currentTankLocalVelocity.x; // positive = sliding right, negative = sliding left
        float forwardSpeed = currentTankLocalVelocity.z;
        if (Mathf.Abs(lateralSpeed) < 0.01f) return;

        // Record horizontal speed before redirect — redirection must never add energy, only redistribute it.
        // Without this, wiggling left/right generates new lateral speed each frame which all gets
        // converted to forward speed, allowing infinite acceleration.
        float horizontalSpeedBefore = new Vector3(tc.rb.linearVelocity.x, 0f, tc.rb.linearVelocity.z).magnitude;

        float redirectAmount = Mathf.Sign(lateralSpeed) * Mathf.Min(Mathf.Abs(lateralSpeed), md.velocityRedirectionStrength * Time.deltaTime);
        float driveDirection = Mathf.Sign(forwardSpeed);

        // Remove the redirected component from lateral velocity and add it to the drive direction
        Vector3 lateralRemoval = -tc.transform.right * redirectAmount;
        Vector3 forwardAddition = tc.transform.forward * driveDirection * Mathf.Abs(redirectAmount);

        tc.rb.linearVelocity += lateralRemoval + forwardAddition;

        // Clamp horizontal speed back to what it was before — redirection only changes direction, not magnitude
        Vector3 horizontalVelocity = new Vector3(tc.rb.linearVelocity.x, 0f, tc.rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > horizontalSpeedBefore)
        {
            horizontalVelocity = horizontalVelocity.normalized * horizontalSpeedBefore;
            tc.rb.linearVelocity = new Vector3(horizontalVelocity.x, tc.rb.linearVelocity.y, horizontalVelocity.z);
        }
    }

    private void ForwardMovement()
    {
        Vector3 forward = tc.transform.forward;
        float forwardSpeed = Vector3.Dot(tc.rb.linearVelocity, forward);
        float acceleration = throttleInput >= 0f ? md.forwardAcceleration : md.reverseAcceleration;

        float moveResult = throttleInput * GetThrottleScale(); // Reduce move input when applying turn input
        float targetSpeed = moveResult * md.maxSpeed;       // Target is a fraction of maxSpeed
        float speedDifference = targetSpeed - forwardSpeed;


        bool aboveMaxSpeed = Mathf.Abs(forwardSpeed) > md.maxSpeed; // Whether an external force has pushed tank past the normal speed cap
        float appliedAccel;

        // Drifting State: small acceleration and deceleation allowed
        if (tankState == State.Drifting)
        {
            if (throttleInput == 0f) // Coasting
            {
                appliedAccel = 0f;
            }
            else if (throttleInput * forwardSpeed < 0f) // Braking
            {
                appliedAccel = Mathf.Clamp(speedDifference, -md.driftBrakingDeceleration * Time.deltaTime, md.driftBrakingDeceleration * Time.deltaTime);
            }
            else // Accelerating
            {
                float accelCap = aboveMaxSpeed ? 0f : md.driftAcceleration * Time.deltaTime;
                appliedAccel = Mathf.Clamp(speedDifference, 0f, accelCap);
            }
        }
        else // Normal state
        {
            if (aboveMaxSpeed)
            {
                // Overspeed deceleration pulls speed back toward maxSpeed
                float targetAtMax = Mathf.Sign(forwardSpeed) * md.maxSpeed;
                appliedAccel = Mathf.Clamp(targetAtMax - forwardSpeed,
                    -md.overSpeedDeceleration * Time.deltaTime, md.overSpeedDeceleration * Time.deltaTime);
            }
            else if (throttleInput == 0f) // Coasting
            {
                appliedAccel = Mathf.Clamp(speedDifference, -md.freeDeceleration * Time.deltaTime, md.freeDeceleration * Time.deltaTime);
            }
            else if (throttleInput * forwardSpeed < 0f) // Braking (when input opposes current motion)
            {
                appliedAccel = Mathf.Clamp(speedDifference, -md.brakingDeceleration * Time.deltaTime, md.brakingDeceleration * Time.deltaTime);
            }
            else
            // Normal acceleration
            {
                appliedAccel = Mathf.Clamp(speedDifference, -acceleration * Time.deltaTime, acceleration * Time.deltaTime);
            }
        }
        tc.rb.linearVelocity += forward * appliedAccel;
    }
    private void Turn()
    {
        float steerStrength = tankState == State.Normal ? md.normalSteerStrength : md.diftSteerStrength;
        float steerResult = steerInput * GetSteeringScale(); // Reduce turning input when applying move input
        float turnPower = steerStrength * steerResult * Time.deltaTime;
        if(tankState == State.Normal)
            turnPower *= md.turningCurve.Evaluate(tankVelocityRatio); // Modulate turn power based on velocity ratio (0-1) compared to max speed

        tc.rb.AddTorque(turnPower * tc.transform.up, ForceMode.Acceleration);
    }

    #region Cross-Axis Scaling
    // Returns a 0-1 scale factor for an input axis, reduced by how much the opposing axis is pressed.
    // e.g. sensitivity 0.5 means the axis loses up to 50% authority at full opposing input.
    private float GetCrossAxisScale(float opposingInput, float sensitivity)
    {
        return Mathf.Clamp01(1f - (Mathf.Abs(opposingInput) * sensitivity));
    }
    private float GetSteeringScale()
    {
        // Movement input reduces steering authority (less precise turning at full throttle/reverse)
        float sensitivity = tankState == State.Drifting ? md.driftSteeringScaleSensitivity : md.steeringScaleSensitivity;
        return GetCrossAxisScale(throttleInput, sensitivity);
    }
    private float GetThrottleScale()
    {
        // Steer input reduces throttle authority (less acceleration through tight turns)
        float sensitivity = tankState == State.Drifting ? md.driftThrottleScaleSensitivity : md.throttleScaleSensitivity;
        return GetCrossAxisScale(steerInput, sensitivity);
    }
    #endregion

    private void SidewaysDrag()
    {
        float dragCoefficent = tankState == State.Normal ? md.normalDragCoefficent : md.driftDragCoefficent;
        float currentSidewaysSpeed = currentTankLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * dragCoefficent;
        Vector3 dragForce = tc.transform.right * dragMagnitude;

        tc.rb.AddForce(dragForce, ForceMode.Acceleration);
    }

    private void ApplyAngularDrag()
    {
        // Debug.Log($"Current Angular Velocity: {currentAngularVelocityY} FPS: {1f / Time.deltaTime:F2}");
        // Get current angular velocity around Y axis
        float currentAngularVelocityY = tc.rb.angularVelocity.y;


        // Only boost angular velocity to overcome static friction when there is turn input
        if (steerInput != 0f && Mathf.Abs(currentAngularVelocityY) < minimumAngularVelocity && currentAngularVelocityY != 0f)
        {
            float boostDirection = Mathf.Sign(steerInput);
            float velocityBoost = (minimumAngularVelocity - Mathf.Abs(currentAngularVelocityY)) * boostDirection * Time.deltaTime;
            tc.rb.angularVelocity += Vector3.up * velocityBoost;
        }
    }

    #endregion


    #region Model Pose
    private void UpdateModelPose()
    {
        if (tc.visuals == null) return;
        float rotationSpeed = (tankState == State.Normal ? md.normalModelRotationSpeed : md.driftModelRotationSpeed);
        float maxTiltAngle = (tankState == State.Normal ? md.normalMaxTiltAngle : md.driftMaxTiltAngle);

        // Calculate model tilt based on steering input (turn amount)
        float targetTiltAmount = steerInput * GetSteeringScale();
        currentTiltAmount = Mathf.Lerp(currentTiltAmount, targetTiltAmount, rotationSpeed * Time.deltaTime);

        // Rotate model toward velocity direction during movement
        Quaternion targetRotation = tc.rb.rotation;

        // Apply tilt based on steering input
        if (Mathf.Abs(currentTiltAmount) > 0.01f)
        {
            float tiltAngle = currentTiltAmount * maxTiltAngle;
            targetRotation *= Quaternion.Euler(0, tiltAngle, 0);
        }

        tc.visuals.rotation = Quaternion.Lerp(tc.visuals.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    #endregion
}
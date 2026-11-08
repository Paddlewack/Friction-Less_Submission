using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class FreeCamera : MonoBehaviour
{
    public bool useFixedUpdate = false;
    public bool invert = true;
    public float sensitivity = 20f;
    public float speed = 20f;
    public float modSpeed = 60f;
    public float zoomFov = 20.0f;
    public bool visible = true;

    private Camera cam;
    private float shift = 1f;
    private float lastX, lastY = 0f;
    private float x, y;
    private float startingFov;
    private bool useModSpeed = false;

    private InputAction attack;
    private InputAction zoom;
    private InputAction pan;

    private void Awake()
    {
        attack = InputSystem.actions.FindAction("Attack", throwIfNotFound: true);
        zoom = InputSystem.actions.FindAction("Zoom", throwIfNotFound: true);
        pan = InputSystem.actions.FindAction("Pan", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        attack.Enable();
        zoom.Enable();
        pan.Enable();
    }

    private void OnDisable()
    {
        attack.Disable();
        zoom.Disable();
        pan.Disable();
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        startingFov = cam.fieldOfView;
    }

    private void Update()
    {
        if (attack.ReadValue<float>() > 0.5f)
        {
            visible = !visible;
        }

        if (zoom.ReadValue<float>() > 0.5f)
        {
            useModSpeed = !useModSpeed;
        }

        if (cam != null)
        {
            float targetFov = startingFov;
            if (pan.ReadValue<float>() > 0.5f)
                targetFov = zoomFov;

            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, 10.0f * Time.deltaTime);
        }

        if (!useFixedUpdate)
            MouseLookMovement();
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
            MouseLookMovement();
    }

    private void MouseLookMovement()
    {
        float trueSpeed = useModSpeed ? modSpeed : speed;

        if (!visible)
        {
            x = Mouse.current.delta.x.ReadValue() + lastX * 0.9f;
            y = invert ? -Mouse.current.delta.y.ReadValue() + lastY * 0.9f : Mouse.current.delta.y.ReadValue() + lastY * 0.9f;

            float targetShift = Keyboard.current.leftShiftKey.isPressed ? 2f : 1f;
            shift = Mathf.Lerp(shift, targetShift, trueSpeed * Time.deltaTime);

            lastX = x;
            lastY = y;

            transform.Rotate(sensitivity * y * Time.deltaTime, sensitivity * x * Time.deltaTime, 0f);
        }

        Vector3 upright = transform.eulerAngles;
        upright.z = 0f;
        transform.eulerAngles = upright;

        transform.Translate((trueSpeed * shift * (Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 0 : 0)) * Time.deltaTime,
                            0f,
                            (trueSpeed * shift * (Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0)) * Time.deltaTime);
    }
}

using Unity.VisualScripting;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    [Header("Script exists to check grounded state")]
    public WheelCollider wheelCollider;

    private void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    public bool CheckWheelGrounded()
    {
        return wheelCollider.isGrounded;
    }
}

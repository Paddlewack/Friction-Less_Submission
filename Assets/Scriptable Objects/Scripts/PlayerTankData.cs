using UnityEngine;

[CreateAssetMenu(fileName = "PlayerTankData", menuName = "Scriptable Objects/PlayerTankData")]
public class PlayerTankData : ScriptableObject
{
    public float mouseSensX;
    public float mouseSensY;
    public Vector2 yMinMax;
    public float cameraDistance;
    public float cameraVerticalOffset;
    public float aimPointVerticalOffset;
}

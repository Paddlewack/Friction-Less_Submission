using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCameraData", menuName = "Scriptable Objects/PlayerCameraData")]
public class PlayerCameraData : ScriptableObject
{
    public float distance = 13f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
}

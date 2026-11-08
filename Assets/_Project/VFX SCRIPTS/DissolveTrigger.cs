using UnityEngine;

public class DissolveTrigger : MonoBehaviour
{
    [Header("Walls to dissolve")]
    public WallDissolveController[] walls;

    [Header("Only trigger for this tag (leave blank for any)")]
    public string playerTag = "Player";

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;

        triggered = true;
        foreach (WallDissolveController wall in walls)
        {
            if (wall != null)
                wall.StartDissolve();
        }
    }
}
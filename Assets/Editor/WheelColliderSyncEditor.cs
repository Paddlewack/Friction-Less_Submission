using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WheelColliderSync))]
public class WheelColliderSyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WheelColliderSync script = (WheelColliderSync)target;
        if (script == null) return;
        GUILayout.Space(10);

        if (GUILayout.Button("Sync Wheel Colliders"))
        {
            script.SyncWheels();
        }
    }
}
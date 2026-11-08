using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>DebugText class
/// AUTHOR: Beard or Die  (more or less)
/// BASED ON AND THANKS TO: Video and code by Unity3d.college: https://www.youtube.com/watch?v=uFFexymk3FY
/// DATE: 2018-01-11
/// COPYRIGHT: None; even remove this text if you want.
/// PURPOSE:
/// Adds a customizable text viewable within the Scene tab, even when not in Play mode
/// You can choose between the built-in data types to be shown, or customize the debug data. 
///
/// INSTRUCTIONS:
/// If using Override mode to set custom debug text, in order to prevent
/// "OverrideText method not yet called. See instructions." being shown, 
/// in any another script attached to the same GameObject, use the following code:
///
/// private void OnGUI()
/// {
/// 	if(gameObject.GetComponent<DebugText>()!=null) gameObject.GetComponent<DebugText>().OverrideText("myVar1: " + myVar1 + "\n" + "myVar2: " + myVar2);
/// }
/// </summary>

public class DebugText : MonoBehaviour
{
    [HideInInspector] public enum DebugDataTypes { TransformPosition, GameObjectName, GameObjectToString, Override }
    [Tooltip("Choose type of data to show")] public DebugDataTypes useData = DebugDataTypes.TransformPosition;
    [Tooltip("Adjust position of debug text relative to object")] public Vector3 heightOffset = Vector3.up;
    public Color color = Color.green;

    private string overrideText = "Debug Txt";


    public void OverrideText(string newText)
    {
        overrideText = newText;
    }



    public GUIStyle style;

#if UNITY_EDITOR
    private void OnGUI()
    {
        DrawText();
    }
#endif

#if UNITY_EDITOR
    private void DrawText()
    {
        string text = " ";
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleCenter;
        style.fixedWidth = 300f;
        switch (useData)
        {
            case DebugDataTypes.GameObjectName:
                text = gameObject.name;
                break;
            case DebugDataTypes.TransformPosition:
                text = gameObject.transform.position.ToString();
                break;
            case DebugDataTypes.GameObjectToString:
                text = gameObject.ToString();
                break;
            case DebugDataTypes.Override:
                if (overrideText != null)
                {
                    text = overrideText;
                }
                else
                {
                    text = "Debug Txt"; // to prevent this, see instructions
                }
                break;
            default:
                text = gameObject.ToString();
                break;
        }


        UnityEditor.Handles.Label(gameObject.transform.position + heightOffset, text, style);

    }
#endif

#if UNITY_EDITOR
    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        DrawText();
    }
#endif
}
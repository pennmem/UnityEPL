using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugLogHandler))]
[CanEditMultipleObjects]
public class DebugLogHanderEditor : DataHandlerEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
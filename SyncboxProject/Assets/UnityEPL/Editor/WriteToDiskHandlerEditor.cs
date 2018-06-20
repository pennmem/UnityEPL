using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WriteToDiskHandler))]
[CanEditMultipleObjects]
public class WriteToDiskHanderEditor : DataHandlerEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WriteToDiskHandler currentTarget = (WriteToDiskHandler)target;

        //how often to write to disk
        currentTarget.SetWriteAutomatically(EditorGUILayout.Toggle("Write to disk automatically", currentTarget.WriteAutomatically()));
        if (currentTarget.WriteAutomatically())
        {
            EditorGUI.indentLevel++;
            currentTarget.SetFramesPerWrite(EditorGUILayout.IntField("Frames per write", currentTarget.GetFramesPerWrite()));
        }
    }
}
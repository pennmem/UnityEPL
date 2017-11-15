using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundRecorder))]
[CanEditMultipleObjects]
public class SoundRecorderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
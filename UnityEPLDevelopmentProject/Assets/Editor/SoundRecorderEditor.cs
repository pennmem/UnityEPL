using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundRecorder))]
[CanEditMultipleObjects]
public class SoundRecorderEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();

		SoundRecorder currentTarget = (SoundRecorder)target;

		if (GUILayout.Button ("Select Path"))
		{
			currentTarget.outputPath = EditorUtility.OpenFolderPanel ("Select path", "", "");
		}
	}
}
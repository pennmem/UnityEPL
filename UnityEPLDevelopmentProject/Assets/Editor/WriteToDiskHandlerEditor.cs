using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WriteToDiskHandler))]
[CanEditMultipleObjects]
public class WriteToDiskHanderEditor : DataHandlerEditor 
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();

		WriteToDiskHandler currentTarget = (WriteToDiskHandler)target;

		currentTarget.SetRootDirectory (EditorGUILayout.TextField ("Root directory", currentTarget.GetRootDirectory()));
		if (GUILayout.Button ("Select directory"))
			currentTarget.SetRootDirectory(EditorUtility.OpenFolderPanel ("Select directory", "", ""));

		currentTarget.SetUseDirectoryStructure (EditorGUILayout.Toggle ("Use automatic directory structure", currentTarget.UseDirectoryStructure()));
		if (currentTarget.UseDirectoryStructure ())
		{
			EditorGUI.indentLevel++;
			currentTarget.SetParticipantFirst (EditorGUILayout.Toggle ("Participant/Experiment/SessionNumber.xxx", currentTarget.ParticipantFirst ()));
			currentTarget.SetParticipantFirst (!EditorGUILayout.Toggle ("Experiment/Participant/SessionNumber.xxx", !currentTarget.ParticipantFirst ()));
			EditorGUI.indentLevel--;
		}

		currentTarget.SetWriteAutomatically (EditorGUILayout.Toggle ("Write to disk automatically", currentTarget.WriteAutomatically ()));
		if (currentTarget.WriteAutomatically ())
		{
			EditorGUI.indentLevel++;
			currentTarget.SetFramesPerWrite(EditorGUILayout.IntField("Frames per write", currentTarget.GetFramesPerWrite()));
		}
	}
}
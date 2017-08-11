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

		//where to write to disk
		currentTarget.SetRootDirectory (EditorGUILayout.TextField ("Root ouput directory", currentTarget.GetRootDirectory()));
		if (GUILayout.Button ("Select directory"))
			currentTarget.SetRootDirectory(EditorUtility.OpenFolderPanel ("Select directory", "", ""));

		//should I create directories for you
		currentTarget.SetUseDirectoryStructure (EditorGUILayout.Toggle ("Use automatic directory structure", currentTarget.UseDirectoryStructure()));
		if (currentTarget.UseDirectoryStructure ())
		{
			EditorGUI.indentLevel++;
			currentTarget.SetParticipantFirst (EditorGUILayout.Toggle ("Participant/Experiment/SessionNumber.xxx", currentTarget.ParticipantFirst ()));
			currentTarget.SetParticipantFirst (!EditorGUILayout.Toggle ("Experiment/Participant/SessionNumber.xxx", !currentTarget.ParticipantFirst ()));
			EditorGUI.indentLevel--;
		}

		//how often to write to disk
		currentTarget.SetWriteAutomatically (EditorGUILayout.Toggle ("Write to disk automatically", currentTarget.WriteAutomatically ()));
		if (currentTarget.WriteAutomatically ())
		{
			EditorGUI.indentLevel++;
			currentTarget.SetFramesPerWrite(EditorGUILayout.IntField("Frames per write", currentTarget.GetFramesPerWrite()));
		}
	}
}
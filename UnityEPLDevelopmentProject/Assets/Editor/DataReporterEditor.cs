using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataReporter))]
[CanEditMultipleObjects]
public class DataReporterEditor : Editor 
{
	void OnEnable()
	{
		DataReporter currentTarget = (DataReporter)target;
		//give the DataReporter a unique ID if none has been set manually
		if (currentTarget.reportingID.Equals("Object ID not set."))
			GenerateDefaultName ();
	}

	public override void OnInspectorGUI()
	{
		DataReporter currentTarget = (DataReporter)target;
		currentTarget.reportingID = EditorGUILayout.TextField ("Reporting ID", currentTarget.reportingID);
		if (GUILayout.Button("New unique ID"))
			GenerateDefaultName();

		currentTarget.reportTransform = EditorGUILayout.Toggle ("Report transform data", currentTarget.reportTransform);
		if (currentTarget.reportTransform)
		{
			int input = EditorGUILayout.IntField ("Frames per transform report", currentTarget.framesPerTransformReport);
			if (input > 0)
				currentTarget.framesPerTransformReport = input;
		}

		currentTarget.reportEntersView = EditorGUILayout.Toggle("Report upon entering view", currentTarget.reportEntersView);
		currentTarget.reportLeavesView = EditorGUILayout.Toggle ("Report upon leaving view", currentTarget.reportLeavesView);
	}
		
	private void GenerateDefaultName()
	{
		DataReporter currentTarget = (DataReporter)target;
		currentTarget.reportingID = currentTarget.name + System.Guid.NewGuid();
	}
}